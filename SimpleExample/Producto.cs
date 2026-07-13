using csDronLink;
using GMap.NET;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows.Forms;

namespace SimpleExample.csDronLink
{
    public class Producto
    {
        public string Nombre { get; set; }
        public double Peso { get; set; }
        public double Latitud { get; set; }
        public double Longitud { get; set; }
        public double Precio { get; set; }

        public Producto() { }

    }

    public class Pedido
    {
        private int id;

        private List<(string nombre, int cantidad, double peso, double precio, double lat, double lon)> productos
            = new List<(string, int, double, double, double, double)>();

        private PointLatLng direccion_coord = new PointLatLng();
        private string destinatario;
        private double precio_total;
        private double peso_total;
        private PointLatLng base_coords = new PointLatLng(0, 0);

        public void CrearPedido(int id,
                                List<(string nombre, int cantidad, double peso, double precio, double lat, double lon)> productos,
                                PointLatLng direccion_coord, string destinatario,
                                double precio_total, double peso_total)
        {
            this.id = id;
            this.productos = new List<(string, int, double, double, double, double)>(productos);
            this.direccion_coord = direccion_coord;
            this.destinatario = destinatario;
            this.precio_total = precio_total;
            this.peso_total = peso_total;
        }

        public (double lat, double lon) GetPosProducto(int index = 0)
        {
            if (productos.Count == 0) return (0, 0);
            var p = productos[index];
            return (p.lat, p.lon);
        }

        public Dron AsignarPedido(List<Dron> drones)
        {
            var disponibles = drones.Where(d => d.GetEstado() == "disponible").ToList();
            if (!disponibles.Any())
            {
                MessageBox.Show("No hay drones disponibles");
                return null;
            }

            foreach (var d in disponibles)
            {
                if (d.GetCargaMax() >= peso_total)
                {
                    d.SetEstado("ocupado");
                    d.SetPedido_id(id);
                    return d;
                }
            }

            MessageBox.Show("No hay drones con suficiente carga");
            return null;
        }

        public void AsignarPedidoEnCola(List<Dron> drones)
        {
            var disponibles = drones.Where(d => d.GetEstado() == "disponible").ToList();
            if (disponibles.Any())
            {
                foreach (var d in disponibles)
                    d.SetDist_base(CalculaDist(d, base_coords));

                var mejor = disponibles.OrderBy(d => d.GetDist_base()).First();
                mejor.SetEstado("ocupado");
                mejor.SetPedido_id(id);
            }
            else
            {
                var dronCola = drones.OrderBy(d => d.GetPedidos_en_cola()).First();
                dronCola.SetEstado("ocupado");
                dronCola.SetPedido_id(id);
            }
        }

        public float CalculaDist(Dron dron, PointLatLng coordenadas)
        {
            double dx = Math.Abs(coordenadas.Lat - dron.GetLat());
            double dy = Math.Abs(coordenadas.Lng - dron.GetLon());
            return Convert.ToSingle(Math.Sqrt(dx * dx + dy * dy));
        }

     
        public PointLatLng GetDireccion() => direccion_coord;
        public string GetDestinatario() => destinatario;
        public double GetPesoTotal() => peso_total;
        public double GetPrecioTotal() => precio_total;

      
    }


    public class FuncionesPedidos
    {
        private List<Pedido> pedidos = new List<Pedido>();
        private List<Producto> productos = new List<Producto>();
        private DataTable table = new DataTable();
        public List<Pedido> ObtenerPedidosPendientes()
        {
            return pedidos;
        }
        public void InsertarProducto(Producto p) => productos.Add(p);
        public List<Producto> ObtenerProductos() => productos;

        public void SetPedidos(List<Pedido> pedidos) => this.pedidos = pedidos;
        public List<Pedido> ObtenerPedidos() => pedidos;
        public void InsertarPedido(Pedido pedido) => pedidos.Add(pedido);

        internal void EliminarProducto(Producto producto) => productos.Remove(producto);
    }
}
