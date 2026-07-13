using GMap.NET;
using GMap.NET.MapProviders;
using GMap.NET.WindowsForms;
using GMap.NET.WindowsForms.Markers;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Net;
using System.Web.Script.Serialization;

namespace SimpleExample
{
    public class MapaClientesForm : FormBaseMercadrona
    {
        private readonly Escenario _escenario;
        private readonly GestionarClientes _formLista;

        private GMapControl mapa;
        private GMapOverlay overlayClientes;
        private GMapOverlay overlayZona;

        private TextBox txtNombreCliente;
        private TextBox txtDireccion;
        private Button btnRegistrar;
        private Button btnBuscarDireccion;

        private List<PointLatLng> zonaPermitida;
        private PointLatLng ubicacionSeleccionada = PointLatLng.Empty;
        private GMapMarker marcadorSeleccionado = null;

        public MapaClientesForm(Escenario escenario, GestionarClientes formLista)
        {
            _escenario = escenario;
            _formLista = formLista;

            this.Text = "Ubicación en Mapa - Registrar Nuevo Cliente";
            this.ClientSize = new Size(1000, 600);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.WindowState = FormWindowState.Maximized;

            InicializarControles();
            InicializarMapa();
        }

        private void InicializarControles()
        {
            Panel panel = new Panel
            {
                Location = new Point(10, 20),
                Size = new Size(250, 620),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Bottom
            };

            Label lblNombre = new Label { Text = "Nombre del nuevo cliente:", Location = new Point(10, 10), AutoSize = true };
            txtNombreCliente = new TextBox { Location = new Point(10, 30), Width = 220 };

            Label lblDireccion = new Label
            {
                Text = "Buscar Dirección (Ej: Carrer Barcelona 66, Rubí):",
                Location = new Point(10, 75),
                AutoSize = true,
                MaximumSize = new Size(220, 0) 
            };
            txtDireccion = new TextBox { Location = new Point(10, 115), Width = 220 };

            btnBuscarDireccion = new Button
            {
                Text = "🔍 Localizar Dirección",
                Location = new Point(10, 150),
                Size = new Size(220, 35),
                BackColor = Color.LightBlue,
                FlatStyle = FlatStyle.Flat
            };
            btnBuscarDireccion.Click += BtnBuscarDireccion_Click;
            btnRegistrar = new Button
            {
                Text = "📍 Registrar Cliente",
                Location = new Point(10, 210),
                Size = new Size(220, 45),
                BackColor = Color.LightGreen,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9, FontStyle.Bold)
            };
            btnRegistrar.Click += BtnRegistrar_Click;
            panel.Controls.AddRange(new Control[] { lblNombre, txtNombreCliente, lblDireccion, txtDireccion, btnBuscarDireccion, btnRegistrar });
            Controls.Add(panel);
        }

        private void InicializarMapa()
        {
            mapa = new GMapControl
            {
                Location = new Point(270, 20),
                Size = new Size(this.ClientSize.Width - 370, this.ClientSize.Height - 120),
                MapProvider = GMapProviders.GoogleSatelliteMap,
                MinZoom = 2,
                MaxZoom = 20,
                Zoom = _escenario.ZoomInicial,
                CanDragMap = true,
                DragButton = MouseButtons.Left,
                ShowCenter = false,
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
            };

            GMaps.Instance.Mode = AccessMode.ServerAndCache;
            overlayClientes = new GMapOverlay("clientes");
            overlayZona = new GMapOverlay("zona");
            mapa.Overlays.Add(overlayClientes);
            mapa.Overlays.Add(overlayZona);

            zonaPermitida = _escenario.ZonaPermitida;
            overlayZona.Polygons.Add(new GMapPolygon(zonaPermitida, "ZonaPermitida")
            {
                Fill = new SolidBrush(Color.FromArgb(60, Color.LightGreen)),
                Stroke = new Pen(Color.Green, 2)
            });

            RefrescarMarcadoresMapa();

            mapa.Position = CalcularCentro(zonaPermitida);
            mapa.OnMapClick += Mapa_OnMapClick;

            Controls.Add(mapa);
        }

        private void RefrescarMarcadoresMapa()
        {
            overlayClientes.Markers.Clear();
            foreach (var c in _escenario.Clientes)
            {
                string hostAsignado = c.ObtenerHostCercano(_escenario.Hosts);
                var m = new GMarkerGoogle(new PointLatLng(c.Latitud, c.Longitud), GMarkerGoogleType.green_small)
                {
                    ToolTipText = $"{c.Nombre} ({hostAsignado})"
                };
                overlayClientes.Markers.Add(m);
            }
            if (ubicacionSeleccionada != PointLatLng.Empty)
            {
                marcadorSeleccionado = new GMarkerGoogle(ubicacionSeleccionada, GMarkerGoogleType.yellow_small);
                overlayClientes.Markers.Add(marcadorSeleccionado);
            }
            mapa.Refresh();
        }

        private void Mapa_OnMapClick(PointLatLng pointClick, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left) return;

            if (EstaDentroDePoligono(pointClick, _escenario.ZonaPermitida))
            {
                MessageBox.Show("No se puede registrar dentro de la zona permitida.");
                return;
            }

            ubicacionSeleccionada = pointClick;
            RefrescarMarcadoresMapa();
        }

        private void BtnRegistrar_Click(object sender, EventArgs e)
        {
            string nombre = txtNombreCliente.Text.Trim();

            if (string.IsNullOrEmpty(nombre) || ubicacionSeleccionada == PointLatLng.Empty)
            {
                MessageBox.Show("Asigna un nombre e indica la ubicación cliqueando en el mapa.");
                return;
            }

            var nuevoCliente = new Cliente
            {
                Nombre = nombre,
                Latitud = ubicacionSeleccionada.Lat,
                Longitud = ubicacionSeleccionada.Lng,
                Direccion = txtDireccion.Text.Trim() 
            };

            _escenario.Clientes.Add(nuevoCliente);

            _formLista.CargarClientes();

            txtNombreCliente.Clear();
            txtDireccion.Clear();
            ubicacionSeleccionada = PointLatLng.Empty;
            RefrescarMarcadoresMapa();

            MessageBox.Show("Cliente añadido con éxito.");
        }
        public class ArcGisResponse { public ArcGisCandidate[] candidates { get; set; } }
        public class ArcGisCandidate { public ArcGisLocation location { get; set; } }
        public class ArcGisLocation { public double x { get; set; } public double y { get; set; } }
        private void BtnBuscarDireccion_Click(object sender, EventArgs e)
        {
            string direccion = txtDireccion.Text.Trim();
            if (string.IsNullOrEmpty(direccion)) return;

            try
            {
                System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12;

                string url = $"https://geocode.arcgis.com/arcgis/rest/services/World/GeocodeServer/findAddressCandidates?singleLine={Uri.EscapeDataString(direccion)}&f=json&maxLocations=1";

                using (System.Net.WebClient client = new System.Net.WebClient())
                {
                    client.Headers.Add("User-Agent", "MercadronaApp_Busqueda");

                    string json = client.DownloadString(url);
                    System.Web.Script.Serialization.JavaScriptSerializer js = new System.Web.Script.Serialization.JavaScriptSerializer();

                    ArcGisResponse resultado = js.Deserialize<ArcGisResponse>(json);

                    if (resultado != null && resultado.candidates != null && resultado.candidates.Length > 0)
                    {
                        double lon = resultado.candidates[0].location.x;
                        double lat = resultado.candidates[0].location.y;

                        GMap.NET.PointLatLng punto = new GMap.NET.PointLatLng(lat, lon);
                        mapa.Position = punto;
                        mapa.Zoom = 19; 
                        ubicacionSeleccionada = punto;
                        RefrescarMarcadoresMapa();
                    }
                    else
                    {
                        MessageBox.Show("No se encontró el portal exacto. Prueba a añadir la ciudad (Ej: Carrer Barcelona 66, Rubí).", "Sin resultados");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al buscar la dirección: " + ex.Message, "Error");
            }
        }

        private bool EstaDentroDePoligono(PointLatLng p, List<PointLatLng> poligono)
        {
            bool inside = false;
            for (int i = 0, j = poligono.Count - 1; i < poligono.Count; j = i++)
            {
                if (((poligono[i].Lat > p.Lat) != (poligono[j].Lat > p.Lat)) &&
                    (p.Lng < (poligono[j].Lng - poligono[i].Lng) * (p.Lat - poligono[i].Lat) / (poligono[j].Lat - poligono[i].Lat) + poligono[i].Lng))
                    inside = !inside;
            }
            return inside;
        }

        private PointLatLng CalcularCentro(List<PointLatLng> puntos) => new PointLatLng(puntos.Average(p => p.Lat), puntos.Average(p => p.Lng));

        public class NominatimResult { public string lat { get; set; } public string lon { get; set; } }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            
            this.ClientSize = new System.Drawing.Size(284, 261);
            this.Name = "MapaClientesForm";
            this.Load += new System.EventHandler(this.MapaClientesForm_Load);
            this.ResumeLayout(false);

        }

        private void MapaClientesForm_Load(object sender, EventArgs e)
        {

        }
    }
}