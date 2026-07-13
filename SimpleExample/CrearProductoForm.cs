using GMap.NET;
using GMap.NET.MapProviders;
using GMap.NET.WindowsForms;
using GMap.NET.WindowsForms.Markers;
using SimpleExample.csDronLink;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace SimpleExample
{
    public class CrearProducto : FormBaseMercadrona
    {
        private readonly FuncionesPedidos _funciones;
        private readonly Escenario _escenario;

        private GMapControl gmapProducto;
        private GMapOverlay capaMarcadores;
        private GMapOverlay capaZona;

        private TextBox txtLatitud, txtLongitud, txtNombre, txtPeso, txtPrecio;
        private PointLatLng ubicacionSeleccionada = PointLatLng.Empty;

        private Panel panelLateral;

        public CrearProducto(FuncionesPedidos funciones, Escenario escenario)
        {
            _funciones = funciones;
            _escenario = escenario;

            this.WindowState = FormWindowState.Maximized;
            this.Text = $"Crear Producto - {_escenario.Nombre}";
            this.MinimumSize = new Size(800, 600); 

            InicializarPanelLateral();
            InicializarMapa();
        }

        private void InicializarPanelLateral()
        {
            panelLateral = new Panel
            {
                Dock = DockStyle.Right,
                Width = 260,
                BackColor = SystemColors.Control,
                AutoScroll = true 
            };
            this.Controls.Add(panelLateral);

            Label lblNombre = new Label { Text = "Nombre del producto", Location = new Point(20, 20), Width = 200 };
            txtNombre = new TextBox { Location = new Point(20, 45), Width = 200 };

            Label lblPeso = new Label { Text = "Peso (kg)", Location = new Point(20, 85), Width = 200 };
            txtPeso = new TextBox { Location = new Point(20, 110), Width = 200 };

            Label lblPrecio = new Label { Text = "Precio (€)", Location = new Point(20, 145), Width = 200 };
            txtPrecio = new TextBox { Location = new Point(20, 170), Width = 200 };

            Label lblLat = new Label { Text = "Latitud", Location = new Point(20, 205), Width = 200 };
            txtLatitud = new TextBox { Location = new Point(20, 230), Width = 200 };

            Label lblLng = new Label { Text = "Longitud", Location = new Point(20, 265), Width = 200 };
            txtLongitud = new TextBox { Location = new Point(20, 290), Width = 200 };

            panelLateral.Controls.AddRange(new Control[] {
                lblNombre, txtNombre, lblPeso, txtPeso, lblPrecio, txtPrecio, lblLat, txtLatitud, lblLng, txtLongitud
            });

            Button btnGuardar = new Button { Text = "Guardar producto", Location = new Point(20, 340), Width = 200, Height = 30 };
            btnGuardar.Click += BtnGuardar_Click;
            panelLateral.Controls.Add(btnGuardar);

            Button btnListar = new Button { Text = "Listar productos", Location = new Point(20, 385), Width = 200, Height = 30 };
            btnListar.Click += BtnListar_Click;
            panelLateral.Controls.Add(btnListar);

            Button btnEjemplo = new Button { Text = "Ejemplo Productos", Location = new Point(20, 430), Width = 200, Height = 30 };
            btnEjemplo.Click += BtnEjemplo_Click;
            panelLateral.Controls.Add(btnEjemplo);
        }

        private void InicializarMapa()
        {
            gmapProducto = new GMapControl
            {
                Dock = DockStyle.Fill,
                MapProvider = GoogleSatelliteMapProvider.Instance,
                MinZoom = 2,
                MaxZoom = 20,
                Zoom = _escenario.ZoomInicial,
                CanDragMap = true,
                DragButton = MouseButtons.Left,
                ShowCenter = false
            };

            GMaps.Instance.Mode = AccessMode.ServerAndCache;

            capaMarcadores = new GMapOverlay("markers");
            capaZona = new GMapOverlay("zona");
            gmapProducto.Overlays.Add(capaMarcadores);
            gmapProducto.Overlays.Add(capaZona);

            if (_escenario.ZonaPermitida.Count > 0)
            {
                var poligono = new GMapPolygon(_escenario.ZonaPermitida, "ZonaPermitida")
                {
                    Fill = new SolidBrush(Color.FromArgb(80, Color.LightGreen)),
                    Stroke = new Pen(Color.Green, 2)
                };
                capaZona.Polygons.Add(poligono);
            }

            double latSum = 0, lngSum = 0;
            foreach (var punto in _escenario.ZonaPermitida)
            {
                latSum += punto.Lat;
                lngSum += punto.Lng;
            }
            gmapProducto.Position = new PointLatLng(latSum / _escenario.ZonaPermitida.Count, lngSum / _escenario.ZonaPermitida.Count);

            gmapProducto.MouseClick += GmapProducto_MouseClick;

            this.Controls.Add(gmapProducto);
        }

        private void GmapProducto_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left) return;

            var p = gmapProducto.FromLocalToLatLng(e.X, e.Y);

            if (!EstaDentroDePoligono(p, _escenario.ZonaPermitida))
            {
                MessageBox.Show("La ubicación debe estar dentro de la zona permitida.");
                return;
            }

            ubicacionSeleccionada = p;
            txtLatitud.Text = p.Lat.ToString();
            txtLongitud.Text = p.Lng.ToString();

            capaMarcadores.Markers.Clear();
            var marker = new GMarkerGoogle(p, GMarkerGoogleType.red)
            {
                ToolTipText = "Ubicación seleccionada",
                ToolTipMode = MarkerTooltipMode.Always
            };
            capaMarcadores.Markers.Add(marker);
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            this.ClientSize = new System.Drawing.Size(1024, 720);
            this.Name = "CrearProducto";
            this.Load += new System.EventHandler(this.CrearProducto_Load);
            this.ResumeLayout(false);
        }

        private void CrearProducto_Load(object sender, EventArgs e) { }

        private void BtnEjemplo_Click(object sender, EventArgs e)
        {
            capaMarcadores.Markers.Clear();
            var zona = _escenario.ZonaPermitida;
            if (zona.Count == 0)
            {
                MessageBox.Show("No hay zona permitida definida en este escenario.");
                return;
            }

            Random rnd = new Random();
            List<Producto> productosEjemplo = new List<Producto>();

            double minLat = double.MaxValue, maxLat = double.MinValue;
            double minLng = double.MaxValue, maxLng = double.MinValue;

            foreach (var p in zona)
            {
                if (p.Lat < minLat) minLat = p.Lat;
                if (p.Lat > maxLat) maxLat = p.Lat;
                if (p.Lng < minLng) minLng = p.Lng;
                if (p.Lng > maxLng) maxLng = p.Lng;
            }

            for (int i = 1; i <= 3; i++)
            {
                PointLatLng puntoValido;
                int intentos = 0;

                do
                {
                    double lat = minLat + 0.05 * (maxLat - minLat) + rnd.NextDouble() * 0.9 * (maxLat - minLat);
                    double lng = minLng + 0.05 * (maxLng - minLng) + rnd.NextDouble() * 0.9 * (maxLng - minLng);
                    puntoValido = new PointLatLng(lat, lng);

                    intentos++;
                    if (intentos > 100)
                    {
                        puntoValido = zona[0];
                        break;
                    }

                } while (!EstaDentroDePoligono(puntoValido, zona));

                Producto prod = new Producto
                {
                    Nombre = $"Producto {i}",
                    Peso = i,
                    Precio = i,
                    Latitud = puntoValido.Lat,
                    Longitud = puntoValido.Lng
                };

                productosEjemplo.Add(prod);
                _funciones.InsertarProducto(prod);

                var marker = new GMarkerGoogle(puntoValido, GMarkerGoogleType.red)
                {
                    ToolTipText = prod.Nombre,
                    ToolTipMode = MarkerTooltipMode.Always
                };
                capaMarcadores.Markers.Add(marker);
            }

            MessageBox.Show("Se han creado 3 productos de ejemplo dentro de la zona, evitando bordes y solapamientos.");
        }

        private void BtnGuardar_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtNombre.Text) ||
                string.IsNullOrWhiteSpace(txtPeso.Text) ||
                string.IsNullOrWhiteSpace(txtPrecio.Text) ||
                string.IsNullOrWhiteSpace(txtLatitud.Text) ||
                string.IsNullOrWhiteSpace(txtLongitud.Text))
            {
                MessageBox.Show("Rellena todos los campos.");
                return;
            }

            if (!double.TryParse(txtPeso.Text, out double peso) ||
                !double.TryParse(txtLatitud.Text, out double lat) ||
                !double.TryParse(txtLongitud.Text, out double lon) ||
                !double.TryParse(txtPrecio.Text, out double precio))
            {
                MessageBox.Show("Valores inválidos. Revisa números y coordenadas.");
                return;
            }

            var punto = new PointLatLng(lat, lon);
            if (!EstaDentroDePoligono(punto, _escenario.ZonaPermitida))
            {
                MessageBox.Show("Ubicación inválida: debe estar dentro de la zona permitida.");
                return;
            }

            Producto p = new Producto
            {
                Nombre = txtNombre.Text,
                Peso = peso,
                Precio = precio,
                Latitud = lat,
                Longitud = lon
            };

            _funciones.InsertarProducto(p);

            capaMarcadores.Markers.Clear();
            var marker = new GMarkerGoogle(punto, GMarkerGoogleType.red)
            {
                ToolTipText = p.Nombre,
                ToolTipMode = MarkerTooltipMode.Always
            };
            capaMarcadores.Markers.Add(marker);

            MessageBox.Show("Producto guardado correctamente.");
            LimpiarCampos();
        }

        private void BtnListar_Click(object sender, EventArgs e)
        {
            var productos = _funciones.ObtenerProductos();
            if (productos.Count == 0)
            {
                MessageBox.Show("No hay productos guardados.");
                return;
            }

            string mensaje = "Productos guardados:\n";
            foreach (var p in productos)
                mensaje += $"{p.Nombre} - {p.Peso}kg - {p.Precio}€ - ({p.Latitud:F6}, {p.Longitud:F6})\n";

            MessageBox.Show(mensaje);
        }

        private void LimpiarCampos()
        {
            txtNombre.Clear();
            txtPeso.Clear();
            txtPrecio.Clear();
            txtLatitud.Clear();
            txtLongitud.Clear();
            ubicacionSeleccionada = PointLatLng.Empty;
        }

        private bool EstaDentroDePoligono(PointLatLng p, List<PointLatLng> poligono)
        {
            bool inside = false;
            int count = poligono.Count;
            for (int i = 0, j = count - 1; i < count; j = i++)
            {
                if (((poligono[i].Lat > p.Lat) != (poligono[j].Lat > p.Lat)) &&
                    (p.Lng < (poligono[j].Lng - poligono[i].Lng) *
                    (p.Lat - poligono[i].Lat) / (poligono[j].Lat - poligono[i].Lat) + poligono[i].Lng))
                    inside = !inside;
            }
            return inside;
        }
    }
}