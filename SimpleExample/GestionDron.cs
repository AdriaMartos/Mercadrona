using csDronLink;
using GMap.NET;
using SimpleExample.csDronLink;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SimpleExample
{
    public class GestorDron
    {
        public static GestorDron Instancia { get; } = new GestorDron();

        private readonly List<(Pedido pedido, PointLatLng host, Func<Dron, Pedido, Task> callback)> cola
            = new List<(Pedido, PointLatLng, Func<Dron, Pedido, Task>)>();

        private readonly HashSet<PointLatLng> hostsEnUso = new HashSet<PointLatLng>();

        public List<Dron> Drones { get; } = new List<Dron>();

        private readonly Dictionary<Dron, bool> estadoDrones = new Dictionary<Dron, bool>();
        private readonly HashSet<Dron> dronesDesactivados = new HashSet<Dron>();
        private readonly SemaphoreSlim semaforo = new SemaphoreSlim(1, 1);

        
        private GestorDron()
        {
        }

        
        public void InicializarDrones(int cantidad)
        {
            Drones.Clear();
            estadoDrones.Clear();
            dronesDesactivados.Clear();

            for (byte i = 1; i <= cantidad; i++)
            {
                var nuevoDron = new Dron(i);
                Drones.Add(nuevoDron);
                estadoDrones.Add(nuevoDron, false);
            }
        }

        public int ColaPedidosCount => cola.Count;

        public void VaciarCola()
        {
            cola.Clear();
        }

        public void DesactivarDron(Dron dron)
        {
            dronesDesactivados.Add(dron);
        }

        public async Task EncolarPedidoAsync(Pedido pedido, PointLatLng host, Func<Dron, Pedido, Task> callback)
        {
            await semaforo.WaitAsync();
            try
            {
                cola.Add((pedido, host, callback));
            }
            finally
            {
                semaforo.Release();
            }

            _ = ProcesarColaAsync();
        }
        // Nuevo campo (junto a los demás diccionarios)
        private readonly Dictionary<Dron, double> capacidadesDrones = new Dictionary<Dron, double>();

        // Se llama DESPUÉS de InicializarDrones
        public void ConfigurarCapacidades(IDictionary<int, double> capacidadesPorId)
        {
            capacidadesDrones.Clear();
            if (capacidadesPorId == null) return;
            foreach (var dron in Drones)
                if (capacidadesPorId.TryGetValue(dron.GetID(), out double cap))
                    capacidadesDrones[dron] = cap;
        }

        public double ObtenerCapacidad(Dron dron)
        {
            if (capacidadesDrones.TryGetValue(dron, out double c)) return c;

            // Fallback a tu lógica antigua por si no se configuró
            int id = dron.GetID();
            if (id == 1) return 1.0;
            if (id == 2) return 3.0;
            return 5.0;
        }

        // Lo usaremos también para el bug del "cancelar todo"
        public bool EstaDesactivado(Dron dron) => dronesDesactivados.Contains(dron);
        private async Task ProcesarColaAsync()
        {
            await semaforo.WaitAsync();
            try
            {
                var dronesLibres = estadoDrones
                    .Where(d => d.Value == false && !dronesDesactivados.Contains(d.Key))
                    .Select(d => d.Key)
                    .ToList();

                if (dronesLibres.Count == 0 || cola.Count == 0) return;

                foreach (var dron in dronesLibres)
                {
                    int indexAsignar = -1;

                    for (int i = 0; i < cola.Count; i++)
                    {
                        double pesoPedido = cola[i].pedido.GetPesoTotal();
                        PointLatLng hostPedido = cola[i].host;

                        if (hostsEnUso.Contains(hostPedido)) continue;

                        // La capacidad ahora viene de la configuración por dron
                        bool puedeCargarPeso = pesoPedido <= ObtenerCapacidad(dron);

                        if (puedeCargarPeso)
                        {
                            indexAsignar = i;
                            break;
                        }
                    }

                    if (indexAsignar != -1)
                    {
                        var item = cola[indexAsignar];

                        estadoDrones[dron] = true;
                        hostsEnUso.Add(item.host);
                        cola.RemoveAt(indexAsignar);

                        _ = EjecutarMisionConDronAsync(dron, item.pedido, item.host, item.callback);

                        if (cola.Count == 0) break;
                    }
                }
            }
            finally
            {
                semaforo.Release();
            }
        }

        private async Task EjecutarMisionConDronAsync(Dron dron, Pedido pedido, PointLatLng hostUsado, Func<Dron, Pedido, Task> callback)
        {
            try
            {
                if (callback != null)
                {
                    await callback(dron, pedido);
                }
            }
            finally
            {
                await semaforo.WaitAsync();
                try
                {
                    estadoDrones[dron] = false;
                    hostsEnUso.Remove(hostUsado);
                }
                finally
                {
                    semaforo.Release();
                }

                _ = ProcesarColaAsync();
            }
        }
    }
}