using csDronLink;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SimpleExample.csDronLink // Ajustado a tu namespace
{
    public class GestorTrafico
    {
        private List<Dron> drones;
        private bool monitoreando;

        // Distancias de seguridad
        private double distanciaSeguridadHorizontal = 15.0; // metros
        private double distanciaSeguridadVertical = 5.0; // metros

        // Radio de la Tierra en metros para Haversine
        private const double R = 6371e3;

        // Diccionario para recordar qué drones están esquivando
        private Dictionary<int, bool> dronesPausados = new Dictionary<int, bool>();

        public GestorTrafico(List<Dron> listaDrones)
        {
            this.drones = listaDrones;
        }

        public void IniciarMonitoreo()
        {
            monitoreando = true;
            Task.Run(() => BucleMonitoreo());
            Console.WriteLine("Gestor de tráfico iniciado.");
        }

        public void DetenerMonitoreo()
        {
            monitoreando = false;
        }

        private async Task BucleMonitoreo()
        {
            while (monitoreando)
            {
                EvaluarConflictos();
                await Task.Delay(1000); // Evaluar cada 1 segundo
            }
        }

        private void EvaluarConflictos()
        {
            if (drones == null || drones.Count < 2) return;

            for (int i = 0; i < drones.Count; i++)
            {
                for (int j = i + 1; j < drones.Count; j++)
                {
                    Dron dronA = drones[i];
                    Dron dronB = drones[j];

                    // Filtrar lecturas GPS inválidas
                    if (dronA.GetLat() == 0 || dronB.GetLat() == 0) continue;

                    // Ajuste por si las coordenadas vienen multiplicadas por 10^7
                    float latA = dronA.GetLat() > 1000 || dronA.GetLat() < -1000 ? dronA.GetLat() / 10000000f : dronA.GetLat();
                    float lonA = dronA.GetLon() > 1000 || dronA.GetLon() < -1000 ? dronA.GetLon() / 10000000f : dronA.GetLon();
                    float latB = dronB.GetLat() > 1000 || dronB.GetLat() < -1000 ? dronB.GetLat() / 10000000f : dronB.GetLat();
                    float lonB = dronB.GetLon() > 1000 || dronB.GetLon() < -1000 ? dronB.GetLon() / 10000000f : dronB.GetLon();

                    double distHorizontal = CalcularDistancia(latA, lonA, latB, lonB);
                    double distVertical = Math.Abs(dronA.GetAlt() - dronB.GetAlt());

                    if (distHorizontal < distanciaSeguridadHorizontal && distVertical < distanciaSeguridadVertical)
                    {
                        ResolverConflicto(dronA, dronB);
                    }
                    else
                    {
                        ComprobarReanudacion(dronA, dronB, distHorizontal, distVertical);
                    }
                }
            }
        }

        private void ResolverConflicto(Dron dronA, Dron dronB)
        {
            // El dron con ID mayor cede el paso y sube
            Dron dronCedePaso = dronA.GetID() > dronB.GetID() ? dronA : dronB;

            if (!dronesPausados.ContainsKey(dronCedePaso.GetID()) || !dronesPausados[dronCedePaso.GetID()])
            {
                Console.WriteLine($"[GESTOR] Conflicto detectado. Dron {dronCedePaso.GetID()} esquivando.");
                dronCedePaso.EsquivarHaciaArriba(1.5f);
                dronesPausados[dronCedePaso.GetID()] = true;
            }
        }

        private void ComprobarReanudacion(Dron dronA, Dron dronB, double distH, double distV)
        {
            // Margen de 5m extra para dejarles reanudar
            if (distH > (distanciaSeguridadHorizontal + 5) || distV > (distanciaSeguridadVertical + 2))
            {
                ReanudarSiEstabaPausado(dronA);
                ReanudarSiEstabaPausado(dronB);
            }
        }

        private async void ReanudarSiEstabaPausado(Dron dron)
        {
            if (dronesPausados.ContainsKey(dron.GetID()) && dronesPausados[dron.GetID()])
            {
                dronesPausados[dron.GetID()] = false; // Lo marcamos falso inmediatamente para no repetir comandos

                Console.WriteLine($"[GESTOR] Conflicto resuelto. Dron {dron.GetID()} bajando.");
                dron.VolverAltitudOriginal();

                await Task.Delay(4000); // 4 segundos para que baje físicamente

                Console.WriteLine($"[GESTOR] Dron {dron.GetID()} reanuda misión.");
                dron.CambiarModoVuelo(3); // 3 = AUTO
            }
        }

        private double CalcularDistancia(float lat1, float lon1, float lat2, float lon2)
        {
            var phi1 = lat1 * Math.PI / 180;
            var phi2 = lat2 * Math.PI / 180;
            var deltaPhi = (lat2 - lat1) * Math.PI / 180;
            var deltaLambda = (lon2 - lon1) * Math.PI / 180;

            var a = Math.Sin(deltaPhi / 2) * Math.Sin(deltaPhi / 2) +
                    Math.Cos(phi1) * Math.Cos(phi2) *
                    Math.Sin(deltaLambda / 2) * Math.Sin(deltaLambda / 2);
            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

            return R * c;
        }
    }
}