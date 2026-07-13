using csDronLink;
using GMap.NET;
using GMap.NET.MapProviders;
using GMap.NET.WindowsForms;
using GMap.NET.WindowsForms.Markers;
using SimpleExample.csDronLink;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace SimpleExample
{
    public class PedidosForm : FormBaseMercadrona
    {
        private readonly FuncionesPedidos _funciones;
        private readonly Escenario _escenario;

        private GMapControl mapa;
        private GMapOverlay overlayMarcadores;
        private GMapOverlay overlayZona;
        private GMapOverlay overlayRutas;

        private ComboBox cmbProductos;
        private ComboBox cmbClientes;
        private TextBox txtPrecio, txtPeso;
        private Button btnFinalizarPedido;

        private Dictionary<Host, List<Cliente>> asignacionesHost;
        private class ProductoInfo
        {
            public string Nombre { get; set; }
            public double Peso { get; set; }
            public double Precio { get; set; }
            public double Latitud { get; set; }
            public double Longitud { get; set; }
            public bool EsInfinito { get; set; }
        }

        public PedidosForm(FuncionesPedidos funciones, Escenario escenario)
        {
            _funciones = funciones;
            _escenario = escenario;

            this.Text = $"Crear Pedido - {_escenario.Nombre}";
            this.WindowState = FormWindowState.Maximized;

            InicializarControles();
            InicializarMapa();
            AsignarClientesAHosts();
            ActualizarMarcadoresYComboBox();
            SeleccionarClientePorDefecto();
        }
        private List<ProductoInfo> ObtenerPacksInfinitos()
        {
            double latCentro = _escenario.ZonaPermitida.Average(p => p.Lat);
            double lngCentro = _escenario.ZonaPermitida.Average(p => p.Lng);

            return new List<ProductoInfo>
            {
                new ProductoInfo { Nombre = "Pack 1", Peso = 1.0, Precio = 10.0, Latitud = latCentro, Longitud = lngCentro, EsInfinito = true },
                new ProductoInfo { Nombre = "Pack 2", Peso = 3.0, Precio = 20.0, Latitud = latCentro, Longitud = lngCentro, EsInfinito = true },
                new ProductoInfo { Nombre = "Pack 3", Peso = 5.0, Precio = 30.0, Latitud = latCentro, Longitud = lngCentro, EsInfinito = true }
            };
        }
        private List<ProductoInfo> ObtenerTodosLosProductos()
        {
            var normales = _funciones.ObtenerProductos().Select(p => new ProductoInfo
            {
                Nombre = p.Nombre,
                Peso = p.Peso,
                Precio = p.Precio,
                Latitud = p.Latitud,
                Longitud = p.Longitud,
                EsInfinito = false
            }).ToList();

            var packs = ObtenerPacksInfinitos();
            return packs.Concat(normales).ToList();
        }

        private void InicializarControles()
        {
            cmbProductos = new ComboBox { Location = new Point(20, 45), Width = 200, DropDownStyle = ComboBoxStyle.DropDownList };
            cmbProductos.SelectedIndexChanged += (s, e) => ActualizarRuta();

            cmbClientes = new ComboBox { Location = new Point(20, 115), Width = 200, DropDownStyle = ComboBoxStyle.DropDownList };
            cmbClientes.SelectedIndexChanged += (s, e) => ActualizarRuta();

            txtPrecio = new TextBox { Location = new Point(20, 185), Width = 200, ReadOnly = true };
            txtPeso = new TextBox { Location = new Point(20, 245), Width = 200, ReadOnly = true };

            btnFinalizarPedido = new Button { Text = "Finalizar Pedido", Location = new Point(20, 310), Width = 200 };
            btnFinalizarPedido.Click += BtnFinalizarPedido_Click;

            Controls.AddRange(new Control[]
            {
                new Label{Text="Productos:",Location=new Point(20,20)},
                cmbProductos,
                new Label{Text="Clientes:",Location=new Point(20,90)},
                cmbClientes,
                new Label{Text="Precio (€):",Location=new Point(20,160)},
                txtPrecio,
                new Label{Text="Peso (kg):",Location=new Point(20,220)},
                txtPeso,
                btnFinalizarPedido
            });
        }

        private void InicializarMapa()
        {
            mapa = new GMapControl
            {
                Location = new Point(250, 20),
                Size = new Size(1600, 800),
                MapProvider = GoogleSatelliteMapProvider.Instance,
                MinZoom = 5,
                MaxZoom = 20,
                DragButton = MouseButtons.Left
            };

            GMaps.Instance.Mode = AccessMode.ServerAndCache;

            overlayMarcadores = new GMapOverlay("marcadores");
            overlayZona = new GMapOverlay("zonas");
            overlayRutas = new GMapOverlay("rutas");

            mapa.Overlays.Add(overlayZona);
            mapa.Overlays.Add(overlayRutas);
            mapa.Overlays.Add(overlayMarcadores);
            mapa.OnMapClick += Mapa_OnMapClick;

            overlayZona.Polygons.Add(new GMapPolygon(_escenario.ZonaPermitida, "Permitida")
            {
                Fill = new SolidBrush(Color.FromArgb(40, Color.LightGreen)),
                Stroke = new Pen(Color.Green, 2)
            });

            foreach (var zona in _escenario.ZonasProhibidas)
            {
                overlayZona.Polygons.Add(new GMapPolygon(zona, "Prohibida")
                {
                    Fill = new SolidBrush(Color.FromArgb(80, Color.Red)),
                    Stroke = new Pen(Color.DarkRed, 2)
                });
            }

            foreach (var host in _escenario.Hosts)
            {
                overlayMarcadores.Markers.Add(new GMarkerGoogle(host.Posicion, GMarkerGoogleType.yellow_dot)
                {
                    ToolTipText = host.Nombre,
                    ToolTipMode = MarkerTooltipMode.Always
                });
            }

            Controls.Add(mapa);

            List<PointLatLng> todosPuntos = _escenario.Clientes.Select(c => c.Posicion).ToList();
            todosPuntos.Add(new PointLatLng(_escenario.ZonaPermitida.Average(p => p.Lat), _escenario.ZonaPermitida.Average(p => p.Lng)));

            if (todosPuntos.Count > 0)
            {
                double maxLat = todosPuntos.Max(p => p.Lat);
                double minLat = todosPuntos.Min(p => p.Lat);
                double maxLng = todosPuntos.Max(p => p.Lng);
                double minLng = todosPuntos.Min(p => p.Lng);

                PointLatLng centro = new PointLatLng((maxLat + minLat) / 2, (maxLng + minLng) / 2);
                mapa.Position = centro;
                mapa.SetZoomToFitRect(new RectLatLng(maxLat, minLng, maxLng - minLng, maxLat - minLat));
            }

            if (_escenario.Nombre.ToUpper().Contains("MERCADRONA"))
            {
                double latCentro = _escenario.ZonaPermitida.Average(p => p.Lat);
                double lngCentro = _escenario.ZonaPermitida.Average(p => p.Lng);
                PointLatLng posLabel = new PointLatLng(latCentro, lngCentro);

                Label txtMercadrona = new Label()
                {
                    Text = "MERCADRONA",
                    BackColor = Color.LightYellow,
                    BorderStyle = BorderStyle.FixedSingle,
                    AutoSize = true,
                    Font = new Font("Arial", 10, FontStyle.Bold),
                    Padding = new Padding(5)
                };

                mapa.Controls.Add(txtMercadrona);

                void ReposicionarEtiqueta()
                {
                    var puntoLocal = mapa.FromLatLngToLocal(posLabel);
                    txtMercadrona.Location = new Point((int)puntoLocal.X, (int)puntoLocal.Y);
                }

                mapa.OnMapZoomChanged += new GMap.NET.MapZoomChanged(ReposicionarEtiqueta);
                mapa.OnPositionChanged += (p) => ReposicionarEtiqueta();
                this.Shown += (s, e) => ReposicionarEtiqueta();

                var centroEscenario = new PointLatLng(latCentro, lngCentro);

                var lineaIndicador = new GMapRoute(new List<PointLatLng> { posLabel, centroEscenario }, "IndicadorMerca")
                {
                    Stroke = new Pen(Color.Orange, 3) { DashStyle = System.Drawing.Drawing2D.DashStyle.Dash }
                };
                overlayRutas.Routes.Add(lineaIndicador);
            }
        }

        private void AsignarClientesAHosts()
        {
            asignacionesHost = new Dictionary<Host, List<Cliente>>();

            foreach (var host in _escenario.Hosts)
                asignacionesHost[host] = new List<Cliente>();

            foreach (var cliente in _escenario.Clientes)
            {
                var hostMasCercano = _escenario.Hosts
                    .OrderBy(h => RutaDron.Dist(cliente.Posicion, h.Posicion))
                    .First();

                asignacionesHost[hostMasCercano].Add(cliente);
            }
        }

        private void Mapa_OnMapClick(PointLatLng clickPos, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left) return;

            double maxDist = 0.001;
            Cliente clienteCercano = null;
            double distMin = double.MaxValue;

            foreach (var cliente in _escenario.Clientes)
            {
                double dist = RutaDron.Dist(clickPos, cliente.Posicion);
                if (dist < distMin && dist < maxDist)
                {
                    distMin = dist;
                    clienteCercano = cliente;
                }
            }

            if (clienteCercano != null)
            {
                cmbClientes.SelectedIndex = _escenario.Clientes.IndexOf(clienteCercano);
                ActualizarRuta();
            }
        }

        private void ActualizarMarcadoresYComboBox()
        {
            cmbClientes.Items.Clear();
            cmbProductos.Items.Clear();
            var marcadoresAConservar = overlayMarcadores.Markers.OfType<GMarkerGoogle>().Where(m => m.Type == GMarkerGoogleType.yellow_dot).ToList();
            overlayMarcadores.Markers.Clear();
            foreach (var m in marcadoresAConservar) overlayMarcadores.Markers.Add(m);

            foreach (var cliente in _escenario.Clientes)
            {
                overlayMarcadores.Markers.Add(new GMarkerGoogle(cliente.Posicion, GMarkerGoogleType.green_small)
                {
                    ToolTipText = cliente.Nombre,
                    ToolTipMode = MarkerTooltipMode.Always
                });
                cmbClientes.Items.Add(cliente.Nombre);
            }

            var todosLosProductos = ObtenerTodosLosProductos();
            bool dibujadoMarcadorPacks = false;

            foreach (var p in todosLosProductos)
            {
                cmbProductos.Items.Add(p.Nombre);

                if (p.EsInfinito)
                {
                    if (!dibujadoMarcadorPacks)
                    {
                        overlayMarcadores.Markers.Add(new GMarkerGoogle(new PointLatLng(p.Latitud, p.Longitud), GMarkerGoogleType.red_dot)
                        {
                            ToolTipText = "Packs Centrales",
                            ToolTipMode = MarkerTooltipMode.OnMouseOver
                        });
                        dibujadoMarcadorPacks = true;
                    }
                }
                else
                {
                    overlayMarcadores.Markers.Add(new GMarkerGoogle(new PointLatLng(p.Latitud, p.Longitud), GMarkerGoogleType.blue_dot)
                    {
                        ToolTipText = p.Nombre,
                        ToolTipMode = MarkerTooltipMode.Always
                    });
                }
            }

            if (cmbProductos.Items.Count > 0)
                cmbProductos.SelectedIndex = 0;
        }

        private List<PointLatLng> CalcularRutaCompleta(PointLatLng origen, Host host, Cliente cliente)
        {
            var nodos = RutaDron.CrearNodos(origen, host.Posicion, _escenario.ZonaPermitida, _escenario.ZonasProhibidas);
            var grafo = RutaDron.CrearGrafo(nodos, _escenario.ZonasProhibidas);
            var ruta = RutaDron.AStar(origen, host.Posicion, grafo);

            if (ruta == null) return null;

            ruta.Add(cliente.Posicion);
            return ruta;
        }

        private void ActualizarRuta()
        {
            var rutasAconservar = overlayRutas.Routes.Where(r => r.Name == "IndicadorMerca").ToList();
            overlayRutas.Routes.Clear();
            foreach (var rc in rutasAconservar) overlayRutas.Routes.Add(rc);

            if (cmbProductos.SelectedIndex < 0) return;

            var prod = ObtenerTodosLosProductos().FirstOrDefault(p => p.Nombre == cmbProductos.Text);
            if (prod == null) return;

            var origen = new PointLatLng(prod.Latitud, prod.Longitud);

            foreach (var host in _escenario.Hosts)
            {
                var clientesHost = asignacionesHost[host];
                foreach (var cliente in clientesHost)
                {
                    var nodos = RutaDron.CrearNodos(origen, host.Posicion, _escenario.ZonaPermitida, _escenario.ZonasProhibidas);
                    var grafo = RutaDron.CrearGrafo(nodos, _escenario.ZonasProhibidas);
                    var rutaHost = RutaDron.AStar(origen, host.Posicion, grafo);
                    if (rutaHost == null) continue;

                    var rutaCompleta = new List<PointLatLng>(rutaHost) { cliente.Posicion };
                    overlayRutas.Routes.Add(new GMapRoute(rutaCompleta, $"Ruta_{host.Nombre}_{cliente.Nombre}")
                    {
                        Stroke = new Pen(Color.Black, 3)
                    });
                }
            }

            if (cmbClientes.SelectedIndex >= 0)
            {
                var clienteSel = _escenario.Clientes.First(c => c.Nombre == cmbClientes.Text);
                var hostSel = asignacionesHost.First(h => h.Value.Contains(clienteSel)).Key;
                var rutaAzul = CalcularRutaCompleta(origen, hostSel, clienteSel);

                if (rutaAzul != null)
                {
                    overlayRutas.Routes.Add(new GMapRoute(rutaAzul, "Seleccionada")
                    {
                        Stroke = new Pen(Color.Blue, 4)
                    });

                    double distanciaKm = 0;
                    for (int i = 0; i < rutaAzul.Count - 1; i++)
                        distanciaKm += RutaDron.Dist(rutaAzul[i], rutaAzul[i + 1]);

                    txtPrecio.Text = (prod.Precio + distanciaKm * 100).ToString("F2");
                    txtPeso.Text = prod.Peso.ToString("F2");
                }
            }

            mapa.Refresh();
        }

        private void BtnFinalizarPedido_Click(object sender, EventArgs e)
        {
            var todosLosProductos = ObtenerTodosLosProductos();

            if (todosLosProductos.Count == 0 || cmbProductos.SelectedIndex < 0 || cmbClientes.SelectedIndex < 0)
            {
                MessageBox.Show("Seleccione producto y cliente.");
                return;
            }

            var prodInfo = todosLosProductos.FirstOrDefault(p => p.Nombre == cmbProductos.Text);
            var cliente = _escenario.Clientes.FirstOrDefault(c => c.Nombre == cmbClientes.Text);

            if (prodInfo == null || cliente == null) return;

            var origen = new PointLatLng(prodInfo.Latitud, prodInfo.Longitud);
            var host = asignacionesHost.First(h => h.Value.Contains(cliente)).Key;
            var ruta = CalcularRutaCompleta(origen, host, cliente);

            if (ruta == null)
            {
                MessageBox.Show("No se puede calcular ruta válida.");
                return;
            }

            double distanciaKm = 0;
            for (int i = 0; i < ruta.Count - 1; i++)
                distanciaKm += RutaDron.Dist(ruta[i], ruta[i + 1]);

            double precioTotal = prodInfo.Precio + distanciaKm * 100;

            var productosPedido = new List<(string nombre, int cantidad, double peso, double precio, double lat, double lon)>
            {
                (prodInfo.Nombre, 1, prodInfo.Peso, prodInfo.Precio, prodInfo.Latitud, prodInfo.Longitud)
            };

            Pedido pedido = new Pedido();
            pedido.CrearPedido(
                id: _funciones.ObtenerPedidos().Count + 1,
                productos: productosPedido,
                direccion_coord: cliente.Posicion,
                destinatario: cliente.Nombre,
                precio_total: precioTotal,
                peso_total: prodInfo.Peso
            );

            _funciones.InsertarPedido(pedido);
            if (!prodInfo.EsInfinito)
            {
                var productoReal = _funciones.ObtenerProductos().FirstOrDefault(p => p.Nombre == prodInfo.Nombre);
                if (productoReal != null)
                {
                    _funciones.EliminarProducto(productoReal);
                }
            }

            MessageBox.Show($"Pedido creado para {cliente.Nombre}\nPrecio total: {precioTotal:F2}€");

            var rutasAconservar = overlayRutas.Routes.Where(r => r.Name == "IndicadorMerca").ToList();
            overlayRutas.Routes.Clear();
            foreach (var rc in rutasAconservar) overlayRutas.Routes.Add(rc);

            AsignarClientesAHosts();
            ActualizarMarcadoresYComboBox();
            SeleccionarClientePorDefecto();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            this.ClientSize = new System.Drawing.Size(284, 261);
            this.Name = "PedidosForm";
            this.Load += new System.EventHandler(this.PedidosForm_Load);
            this.ResumeLayout(false);
        }

        private void PedidosForm_Load(object sender, EventArgs e)
        {
        }

        private void SeleccionarClientePorDefecto()
        {
            if (cmbClientes.Items.Count > 0)
                cmbClientes.SelectedIndex = 0;
        }
    }
}