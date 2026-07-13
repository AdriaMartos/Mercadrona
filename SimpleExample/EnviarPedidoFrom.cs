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
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SimpleExample
{
    public partial class EnviarPedidoForm : FormBaseMercadrona
    {
        public enum FaseVuelo { EnTierra, Despegando, Volando, Aterrizando, PausadoConflicto, EvasionElevada, Emergencia, SmartRTL }

        private readonly FuncionesPedidos funciones;
        private readonly Escenario _escenario;

        private List<Pedido> pedidosPendientes;

        private Label lblInfo;
        private Button btnConectar;
        private Button btnEjecutar;
        private Button btnIdentificar;
        private FlowLayoutPanel panelBotonesParada;

        private GMapControl mapa;
        private GMapOverlay overlayZonas;
        private GMapOverlay overlayRutas;
        private GMapOverlay overlayMarkers;
        private GMapOverlay overlayDrones;
        private Dictionary<Dron, CancellationTokenSource> cancelSourcesDrones = new Dictionary<Dron, CancellationTokenSource>();
        private Dictionary<Dron, FaseVuelo> faseDrones = new Dictionary<Dron, FaseVuelo>();
        private Dictionary<Dron, FaseVuelo> fasePreviaConflicto = new Dictionary<Dron, FaseVuelo>();
        private readonly Dictionary<Dron, Panel> cuadrosEstadoDrones = new Dictionary<Dron, Panel>();
        private readonly Dictionary<Dron, FaseVuelo> ultimaFaseCuadro = new Dictionary<Dron, FaseVuelo>();
        private readonly ToolTip toolTipEstados = new ToolTip();
        private PointLatLng baseDron => _escenario.BaseDron;
        private List<Cliente> clientes => _escenario.Clientes;
        private List<Host> hosts => _escenario.Hosts;

        private System.Windows.Forms.Timer timerTelemetria;

        private Dictionary<Dron, GMarkerGoogle> markersDrones = new Dictionary<Dron, GMarkerGoogle>();
        private Dictionary<Dron, bool> paqueteEntregadoPorDron = new Dictionary<Dron, bool>();

        private Dictionary<Dron, List<PointLatLng>> rutaRecorrida = new Dictionary<Dron, List<PointLatLng>>();

        public FormHistorialVuelo formHistorial = new FormHistorialVuelo();

        public EnviarPedidoForm(
            List<Pedido> pedidos,
            FuncionesPedidos funciones,
            Escenario escenario)
        {
            this.funciones = funciones;
            this._escenario = escenario;

            pedidosPendientes = pedidos ?? funciones.ObtenerPedidosPendientes();

            Text = "Centro Control Envíos - " + escenario.Nombre;
            WindowState = FormWindowState.Maximized;
            StartPosition = FormStartPosition.CenterParent;

            InicializarControles();
            DibujarEscenario();
            DibujarIndicadores();
            DibujarTodasLasRutasPreview();
        }
        private void InicializarControles()
        {
            this.AutoScaleMode = AutoScaleMode.Font;
            this.MinimumSize = new Size(1200, 640);
 
            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 3,
                Padding = new Padding(10)
            };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 40f));  
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));    
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 175f));   
            Controls.Add(layout);

            lblInfo = new Label
            {
                Font = new Font("Segoe UI", 11),
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                Text = "Centro listo."
            };
            layout.Controls.Add(lblInfo, 0, 0);

            mapa = new GMapControl
            {
                Dock = DockStyle.Fill,
                MapProvider = GoogleSatelliteMapProvider.Instance,
                MinZoom = 1,
                MaxZoom = 20,
                Zoom = _escenario.ZoomInicial,
                DragButton = MouseButtons.Left
            };
            GMaps.Instance.Mode = AccessMode.ServerAndCache;
            layout.Controls.Add(mapa, 0, 1);

            overlayZonas = new GMapOverlay("zonas");
            overlayRutas = new GMapOverlay("rutas");
            overlayMarkers = new GMapOverlay("markers");
            overlayDrones = new GMapOverlay("drones");
            mapa.Overlays.Add(overlayZonas);
            mapa.Overlays.Add(overlayRutas);
            mapa.Overlays.Add(overlayMarkers);
            mapa.Overlays.Add(overlayDrones);
            var grupoCentrado = new TableLayoutPanel
            {
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                Anchor = AnchorStyles.None,
                ColumnCount = 3,          
                RowCount = 1,
                Margin = new Padding(0)
            };
            grupoCentrado.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            grupoCentrado.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            grupoCentrado.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));  
            layout.Controls.Add(grupoCentrado, 0, 2);


            var botonesPrincipales = new FlowLayoutPanel
            {
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                Anchor = AnchorStyles.None,   
                Margin = new Padding(0)
            };
            grupoCentrado.Controls.Add(botonesPrincipales, 0, 0);

            btnConectar = new Button
            {
                Text = "Conectar Drones",
                Size = new Size(200, 40),
                Margin = new Padding(8)
            };
            btnConectar.Click += BtnConectar_Click;
            botonesPrincipales.Controls.Add(btnConectar);

            btnIdentificar = new Button
            {
                Text = "Identificador de Drones",
                Size = new Size(165, 40),
                Font = new Font("Segoe UI", 8),
                Margin = new Padding(8),
                Visible = false,          
                Tag = "Ignorar"
            };
            btnIdentificar.Click += BtnIdentificar_Click;
            botonesPrincipales.Controls.Add(btnIdentificar);

            btnEjecutar = new Button
            {
                Text = "Ejecutar Pedidos",
                Size = new Size(200, 40),
                Margin = new Padding(8)
            };
            btnEjecutar.Click += BtnEjecutar_Click;
            botonesPrincipales.Controls.Add(btnEjecutar);

            Button btnCancelarTodo = new Button
            {
                Text = "ABORTAR TODAS LAS MISIONES",
                Size = new Size(200, 40),
                BackColor = Color.DarkRed,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Margin = new Padding(8),
                Tag = "Ignorar"
            };
            btnCancelarTodo.Click += BtnCancelarTodo_Click;
            botonesPrincipales.Controls.Add(btnCancelarTodo);

            Button btnVerHistorial = new Button
            {
                Text = "Ver Historial",
                Size = new Size(200, 40),
                Margin = new Padding(8)
            };
            btnVerHistorial.Click += btnVerHistorial_Click;
            botonesPrincipales.Controls.Add(btnVerHistorial);
           

            panelBotonesParada = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                AutoScroll = true,
                Size = new Size(270, 170),          
                Margin = new Padding(15, 0, 0, 0)
            };
            grupoCentrado.Controls.Add(panelBotonesParada, 1, 0);
            grupoCentrado.Controls.Add(CrearLeyendaEstados(), 2, 0);   



            foreach (var dron in GestorDron.Instancia.Drones)
            {
                faseDrones.Add(dron, FaseVuelo.EnTierra);

               
                var fila = new FlowLayoutPanel
                {
                    FlowDirection = FlowDirection.LeftToRight,
                    WrapContents = false,
                    AutoSize = true,
                    AutoSizeMode = AutoSizeMode.GrowAndShrink,
                    Margin = new Padding(0, 0, 0, 4),
                    Tag = "Ignorar"
                };

                var cuadroEstado = new Panel
                {
                    Width = 24,
                    Height = 24,
                    BorderStyle = BorderStyle.FixedSingle,
                    BackColor = ColorPorFase(FaseVuelo.EnTierra),
                    Margin = new Padding(0, 7, 6, 0),
                    Tag = "Ignorar"
                };

                Button btnParar = new Button
                {
                    Text = $"Detener Dron {dron.GetID()}",
                    Size = new Size(190, 38),
                    BackColor = Color.Orange,
                    ForeColor = Color.White,
                    Margin = new Padding(0),
                    Tag = "Ignorar"
                };

                btnParar.Click += (s, e) =>
                {
                    DetenerDron(dron.GetID());
                    btnParar.Enabled = false;
                };

                fila.Controls.Add(cuadroEstado);
                fila.Controls.Add(btnParar);
                panelBotonesParada.Controls.Add(fila);

                cuadrosEstadoDrones.Add(dron, cuadroEstado);
                ultimaFaseCuadro.Add(dron, FaseVuelo.EnTierra);
                toolTipEstados.SetToolTip(cuadroEstado, $"Dron {dron.GetID()}: {TextoFase(FaseVuelo.EnTierra)}");

                GMarkerGoogleType tipoMarcador = dron.GetID() == 1 ? GMarkerGoogleType.green_dot :
                                                 dron.GetID() == 2 ? GMarkerGoogleType.red_dot :
                                                 dron.GetID() == 3 ? GMarkerGoogleType.blue_dot :
                                                 GMarkerGoogleType.orange_dot;

                var marker = new DronMarker(ObtenerBaseDron(dron.GetID()), tipoMarcador, dron.GetID())
                {
                    ToolTipText = $"¡Dron {dron.GetID()} en vuelo!",
                    IsVisible = false
                };

                overlayDrones.Markers.Add(marker);
                markersDrones.Add(dron, marker);
                paqueteEntregadoPorDron.Add(dron, false);
                rutaRecorrida.Add(dron, new List<PointLatLng>());
                cancelSourcesDrones.Add(dron, new CancellationTokenSource());
            }

            timerTelemetria = new System.Windows.Forms.Timer();
            timerTelemetria.Interval = 200;
            timerTelemetria.Tick += TimerTelemetria_Tick;
        }
        private Color ColorPorFase(FaseVuelo fase)
        {
            switch (fase)
            {
                case FaseVuelo.Despegando: return Color.Gold;     
                case FaseVuelo.Volando: return Color.RoyalBlue;   
                case FaseVuelo.Aterrizando: return Color.LimeGreen;  
                case FaseVuelo.PausadoConflicto: return Color.Red;         
                case FaseVuelo.EvasionElevada: return Color.OrangeRed;   
                case FaseVuelo.Emergencia: return Color.DarkRed;     
                case FaseVuelo.SmartRTL: return Color.MediumPurple;
                default: return Color.Gray;        
            }
        }
        private void BtnIdentificar_Click(object sender, EventArgs e)
        {
            int total = GestorDron.Instancia.Drones.Count;

            using (var popup = new Form
            {
                Text = "Identificación de Drones",
                FormBorderStyle = FormBorderStyle.FixedDialog,
                StartPosition = FormStartPosition.CenterParent,
                MaximizeBox = false,
                MinimizeBox = false,
                ClientSize = new Size(280, Math.Min(600, 24 + total * 34))
            })
            {
                var panel = new FlowLayoutPanel
                {
                    Dock = DockStyle.Fill,
                    FlowDirection = FlowDirection.TopDown,
                    WrapContents = false,
                    AutoScroll = true,
                    Padding = new Padding(12)
                };
                popup.Controls.Add(panel);

                foreach (var dron in GestorDron.Instancia.Drones)
                {
                    var (color, nombre) = ColorMarcadorDron(dron.GetID());

                    var fila = new FlowLayoutPanel
                    {
                        FlowDirection = FlowDirection.LeftToRight,
                        WrapContents = false,
                        AutoSize = true,
                        AutoSizeMode = AutoSizeMode.GrowAndShrink,
                        Margin = new Padding(0, 2, 0, 2)
                    };

                    fila.Controls.Add(new Panel
                    {
                        Width = 20,
                        Height = 20,
                        BorderStyle = BorderStyle.FixedSingle,
                        BackColor = color,
                        Margin = new Padding(0, 2, 8, 0)
                    });

                    fila.Controls.Add(new Label
                    {
                        Text = $"Dron {dron.GetID()}: marcador {nombre}",
                        AutoSize = true,
                        Font = new Font("Segoe UI", 10),
                        Margin = new Padding(0, 4, 0, 0)
                    });

                    panel.Controls.Add(fila);
                }

                popup.ShowDialog(this);
            }
        }
        private Control CrearLeyendaEstados()
        {
            var cont = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                Margin = new Padding(15, 0, 0, 0),
                Tag = "Ignorar"
            };

            cont.Controls.Add(new Label
            {
                Text = "Estado del dron:",
                AutoSize = true,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Margin = new Padding(0, 0, 0, 4),
                Tag = "Ignorar"
            });

            FaseVuelo[] fases =
            {
        FaseVuelo.EnTierra,
        FaseVuelo.Despegando,
        FaseVuelo.Volando,
        FaseVuelo.Aterrizando,
        FaseVuelo.PausadoConflicto,
        FaseVuelo.EvasionElevada,
        FaseVuelo.SmartRTL,
        FaseVuelo.Emergencia
    };

            foreach (var fase in fases)
            {
                var filaLeyenda = new FlowLayoutPanel
                {
                    FlowDirection = FlowDirection.LeftToRight,
                    WrapContents = false,
                    AutoSize = true,
                    AutoSizeMode = AutoSizeMode.GrowAndShrink,
                    Margin = new Padding(0, 1, 0, 1),
                    Tag = "Ignorar"
                };

                filaLeyenda.Controls.Add(new Panel
                {
                    Width = 14,
                    Height = 14,
                    BorderStyle = BorderStyle.FixedSingle,
                    BackColor = ColorPorFase(fase),
                    Margin = new Padding(0, 2, 6, 0),
                    Tag = "Ignorar"
                });

                filaLeyenda.Controls.Add(new Label
                {
                    Text = TextoFase(fase),
                    AutoSize = true,
                    Font = new Font("Segoe UI", 8),
                    Margin = new Padding(0, 3, 0, 0),
                    Tag = "Ignorar"
                });

                cont.Controls.Add(filaLeyenda);
            }

            return cont;
        }
        private string TextoFase(FaseVuelo fase)
        {
            switch (fase)
            {
                case FaseVuelo.Despegando: return "Despegando";
                case FaseVuelo.Volando: return "En ruta";
                case FaseVuelo.Aterrizando: return "Aterrizando";
                case FaseVuelo.PausadoConflicto: return "Conflicto (pausado)";
                case FaseVuelo.EvasionElevada: return "Evasión (subiendo)";
                case FaseVuelo.Emergencia: return "Emergencia";
                case FaseVuelo.SmartRTL: return "Regresando (RTL)";
                default: return "En tierra";
            }
        }
        private async void BtnCancelarTodo_Click(object sender, EventArgs e)
        {
            lblInfo.Text = "ABORTO GLOBAL INICIADO...";
            formHistorial?.AgregarMensaje(0, "--- ABORTO GLOBAL ---");

            GestorDron.Instancia.VaciarCola();

            if (sender is Button btnAbortar)
                btnAbortar.Enabled = false;

            var dronesActivos = GestorDron.Instancia.Drones
                .Where(d => faseDrones.ContainsKey(d)
              && faseDrones[d] != FaseVuelo.EnTierra
              && !GestorDron.Instancia.EstaDesactivado(d))   
                 .ToList();

            await Task.WhenAll(dronesActivos.Select(d => ProcesarAbortoDronAsync(d)));
        }
        private async Task ProcesarAbortoDronAsync(Dron dron)
        {
            int idDron = dron.GetID();
            GestorDron.Instancia.DesactivarDron(dron);

            foreach (Control c in panelBotonesParada.Controls)
            {
                if (c is Button btn && btn.Text.Contains(idDron.ToString()))
                {
                    if (btn.InvokeRequired) btn.Invoke(new Action(() => btn.Enabled = false));
                    else btn.Enabled = false;
                }
            }

            if (paqueteEntregadoPorDron.ContainsKey(dron) && !paqueteEntregadoPorDron[dron])
            {
                formHistorial?.AgregarMensaje(idDron, "MISIÓN ABORTADA. Activando Smart RTL...");

                faseDrones[dron] = FaseVuelo.SmartRTL;
                if (cancelSourcesDrones.ContainsKey(dron))
                    cancelSourcesDrones[dron].Cancel();

                await SmartRTLAsync(dron);

                if (faseDrones[dron] == FaseVuelo.Emergencia)
                {
                    formHistorial?.AgregarMensaje(idDron, "Emergencia durante el regreso. Aterrizaje forzoso en curso.");
                    return;
                }

                faseDrones[dron] = FaseVuelo.Aterrizando;
                await AterrizarAsync(dron, CancellationToken.None);
                await EsperarAterrizajeEnBase(dron);
                faseDrones[dron] = FaseVuelo.EnTierra;
                formHistorial?.AgregarMensaje(idDron, "Aterrizaje completado (Aborto). Dron en espera.");
            }
            else
            {
                faseDrones[dron] = FaseVuelo.SmartRTL;
                formHistorial?.AgregarMensaje(idDron, "El dron ya entregó. Terminará su ruta normal.");
                await EsperarAterrizajeEnBase(dron);
                faseDrones[dron] = FaseVuelo.EnTierra;
                formHistorial?.AgregarMensaje(idDron, "Aterrizaje estándar completado. Dron en espera.");
            }
        }
        private PointLatLng ObtenerBaseDron(int idDron)
        {
            int totalDrones = GestorDron.Instancia.Drones.Count;

            if (totalDrones <= 1) return baseDron;

            int indice = idDron - 1;


            double aristaMetros = (_escenario.Nombre == "DroneLab") ? 2.5 : 8.0;

            double radioMetros = (totalDrones == 2)
                ? (aristaMetros / 2.0)
                : (aristaMetros / (2.0 * Math.Sin(Math.PI / totalDrones)));

            double anguloOffset = (totalDrones % 2 == 0) ? (Math.PI / totalDrones) : (Math.PI / 2.0);
            double anguloRadianes = (2.0 * Math.PI * indice / totalDrones) + anguloOffset;

            double offsetXMetros = radioMetros * Math.Cos(anguloRadianes);
            double offsetYMetros = radioMetros * Math.Sin(anguloRadianes);

            double gradosPorMetroLat = 1.0 / 111132.92;
            double gradosPorMetroLng = 1.0 / (111132.92 * Math.Cos(baseDron.Lat * Math.PI / 180.0));

            double latitudFinal = baseDron.Lat + (offsetYMetros * gradosPorMetroLat);
            double longitudFinal = baseDron.Lng + (offsetXMetros * gradosPorMetroLng);

            return new PointLatLng(latitudFinal, longitudFinal);
        }
        private void DibujarEscenario()
        {
            overlayZonas.Polygons.Clear();

            if (_escenario.ZonaPermitida?.Count > 0)
            {
                overlayZonas.Polygons.Add(new GMapPolygon(_escenario.ZonaPermitida, "Permitida")
                {
                    Fill = new SolidBrush(Color.FromArgb(60, Color.LightGreen)),
                    Stroke = new Pen(Color.Green, 2)
                });
                mapa.Position = _escenario.ZonaPermitida[0];
            }

            if (_escenario.ZonasProhibidas != null)
            {
                foreach (var zona in _escenario.ZonasProhibidas)
                {
                    overlayZonas.Polygons.Add(new GMapPolygon(zona, "Prohibida")
                    {
                        Fill = new SolidBrush(Color.FromArgb(60, Color.Red)),
                        Stroke = new Pen(Color.Red, 2)
                    });
                }
            }
        }
        private void DibujarIndicadores()
        {

            foreach (var dron in GestorDron.Instancia.Drones)
            {
                PointLatLng baseAsignada = ObtenerBaseDron(dron.GetID());
                overlayMarkers.Markers.Add(
                    new GMarkerGoogle(baseAsignada, GMarkerGoogleType.yellow_dot)
                    { ToolTipText = $"Base Asignada - Dron {dron.GetID()}" });
            }

            foreach (var c in clientes)
                overlayMarkers.Markers.Add(
                    new GMarkerGoogle(new PointLatLng(c.Latitud, c.Longitud), GMarkerGoogleType.red_small)
                    { ToolTipText = c.Nombre });

            foreach (var h in hosts)
                overlayMarkers.Markers.Add(
                    new GMarkerGoogle(new PointLatLng(h.Lat, h.Lng), GMarkerGoogleType.orange_dot)
                    { ToolTipText = h.Nombre });
        }
        private void DibujarTodasLasRutasPreview()
        {
            overlayRutas.Routes.Clear();
            Random rnd = new Random();

            foreach (var pedido in pedidosPendientes)
            {
                var cliente = pedido.GetDireccion();
                var host = ObtenerHostCliente(cliente);

                var ruta1 = RutaDron.AStar(
                    baseDron, host,
                    RutaDron.CrearGrafo(RutaDron.CrearNodos(baseDron, host, new List<PointLatLng>(), _escenario.ZonasProhibidas), _escenario.ZonasProhibidas));

                var ruta2 = RutaDron.AStar(
                    host, cliente,
                    RutaDron.CrearGrafo(RutaDron.CrearNodos(host, cliente, new List<PointLatLng>(), _escenario.ZonasProhibidas), _escenario.ZonasProhibidas));

                Color color = Color.FromArgb(rnd.Next(50, 255), rnd.Next(50, 255), rnd.Next(50, 255));

                if (ruta1 != null)
                    overlayRutas.Routes.Add(new GMapRoute(ruta1, pedido.GetDestinatario()) { Stroke = new Pen(color, 3) });

                if (ruta2 != null)
                    overlayRutas.Routes.Add(new GMapRoute(ruta2, pedido.GetDestinatario()) { Stroke = new Pen(color, 3) });
            }

            mapa.Invalidate();
        }

        private PointLatLng ObtenerHostCliente(PointLatLng cliente)
        {
            Host mejor = null;
            double min = double.MaxValue;

            foreach (var h in hosts)
            {
                double d = Math.Pow(cliente.Lat - h.Lat, 2) + Math.Pow(cliente.Lng - h.Lng, 2);
                if (d < min)
                {
                    min = d;
                    mejor = h;
                }
            }
            return new PointLatLng(mejor.Lat, mejor.Lng);
        }
        private async void BtnConectar_Click(object sender, EventArgs e)
        {
            btnConectar.Enabled = false;
            lblInfo.Text = "Conectando drones...";

            var tareasConexion = new List<Task>();
            foreach (var dron in GestorDron.Instancia.Drones)
            {
                formHistorial.AgregarMensaje(dron.GetID(), "Conectando al simulador...");
                tareasConexion.Add(Task.Run(() => dron.Conectar("simulacion")));
            }

            await Task.WhenAll(tareasConexion);

            foreach (var dron in GestorDron.Instancia.Drones)
            {
                formHistorial.AgregarMensaje(dron.GetID(), "¡Conexión establecida con éxito!");
            }

            foreach (var marker in markersDrones.Values)
            {
                marker.IsVisible = true;
            }
            timerTelemetria.Start();
            btnIdentificar.Visible = true;   
            lblInfo.Text = $"Conectados {GestorDron.Instancia.Drones.Count} drones ";

            _ = MonitorDeConflictosAsync();
        }

        private async void BtnEjecutar_Click(object sender, EventArgs e)
        {
            var gestor = GestorDron.Instancia;

            foreach (var pedido in pedidosPendientes)
            {
                var cliente = pedido.GetDireccion();
                var hostAsignado = ObtenerHostCliente(cliente);

                await gestor.EncolarPedidoAsync(pedido, hostAsignado, async (dronAsignado, p) =>
                {
                    if (cancelSourcesDrones[dronAsignado].Token.IsCancellationRequested) return;
                    await EjecutarPedido(dronAsignado, p);
                });
            }

            lblInfo.Text = "Todos los pedidos han sido encolados.";
        }

        private async void DetenerDron(int idDron)
        {
            var dron = GestorDron.Instancia.Drones.FirstOrDefault(d => d.GetID() == idDron);
            if (dron == null) return;

            
            if (GestorDron.Instancia.EstaDesactivado(dron))
            {
                formHistorial.AgregarMensaje(idDron, "El dron ya estaba parándose. Orden ignorada.");
                return;
            }

            btnEjecutar.Enabled = false;
            GestorDron.Instancia.DesactivarDron(dron);

           
            if (faseDrones[dron] == FaseVuelo.EnTierra)
            {
                lblInfo.Text = $"Dron {idDron} retirado (estaba en tierra).";
                formHistorial.AgregarMensaje(idDron, "Dron retirado. No recibirá más pedidos.");
                return;
            }

            if (!paqueteEntregadoPorDron[dron])
            {
                lblInfo.Text = $"Abortando misión Dron {idDron}... Iniciando Smart RTL.";
                formHistorial.AgregarMensaje(idDron, "MISIÓN ABORTADA POR USUARIO. Activando Smart RTL...");

                faseDrones[dron] = FaseVuelo.SmartRTL;
                if (cancelSourcesDrones.ContainsKey(dron))
                    cancelSourcesDrones[dron].Cancel();

                await Task.Delay(1500);

                await SmartRTLAsync(dron);

               
                if (faseDrones[dron] == FaseVuelo.Emergencia)
                {
                    formHistorial.AgregarMensaje(idDron, "Emergencia durante el regreso. Aterrizaje forzoso en curso.");
                    return;
                }

                faseDrones[dron] = FaseVuelo.Aterrizando;
                await AterrizarAsync(dron, CancellationToken.None);
                await EsperarAterrizajeEnBase(dron);

                faseDrones[dron] = FaseVuelo.EnTierra;
                lblInfo.Text = $"Dron {idDron} detenido en base (En espera).";
                formHistorial.AgregarMensaje(idDron, "Aterrizaje completado. Dron en espera.");
            }
            else
            {
                lblInfo.Text = $"Dron {idDron} terminará su ruta y quedará inactivo.";
                formHistorial.AgregarMensaje(idDron, "Orden de parada recibida. El dron terminará su ruta.");
                await EsperarAterrizajeEnBase(dron);

                faseDrones[dron] = FaseVuelo.EnTierra;
                lblInfo.Text = $"Dron {idDron} llegó a base y ha sido retirado.";
                formHistorial.AgregarMensaje(idDron, "Aterrizaje estándar completado. Dron en espera.");
            }
        }

        private void TimerTelemetria_Tick(object sender, EventArgs e)
        {
            
            foreach (var kvp in markersDrones)
            {
                var dron = kvp.Key;
                var marker = kvp.Value;

                double latReal = NormalizarCoord(dron.GetLat());
                double lonReal = NormalizarCoord(dron.GetLon());

                if (latReal != 0 && lonReal != 0)
                {
                    marker.Position = new PointLatLng(latReal, lonReal);
                }
            }

         
            foreach (var kvp in cuadrosEstadoDrones)
            {
                var dron = kvp.Key;
                var cuadro = kvp.Value;

                if (!faseDrones.TryGetValue(dron, out var fase)) continue;

                if (!ultimaFaseCuadro.TryGetValue(dron, out var faseAnt) || faseAnt != fase)
                {
                    ultimaFaseCuadro[dron] = fase;
                    cuadro.BackColor = ColorPorFase(fase);
                    toolTipEstados.SetToolTip(cuadro, $"Dron {dron.GetID()}: {TextoFase(fase)}");
                }
            }

            mapa.Invalidate();
        }

        private double NormalizarCoord(float coord)
        {
            return coord > 1000 || coord < -1000 ? coord / 10000000.0 : coord;
        }

        private double CalcularDistancia3D(Dron d1, Dron d2)
        {
            double dist2D = CalcularDistancia(
                NormalizarCoord(d1.GetLat()), NormalizarCoord(d1.GetLon()),
                NormalizarCoord(d2.GetLat()), NormalizarCoord(d2.GetLon())
            );
            double difAlt = Math.Abs(d1.GetAlt() - d2.GetAlt());
            return Math.Sqrt(Math.Pow(dist2D, 2) + Math.Pow(difAlt, 2));
        }

        private float CalcularPrioridadGlobal(Dron dron)
        {
            var fase = faseDrones[dron];

            float scoreId = 1.0f / dron.GetID();

            if (fase == FaseVuelo.Emergencia) return 100f + scoreId;
            if (fase == FaseVuelo.SmartRTL) return 95f + scoreId;
            if (fase == FaseVuelo.Aterrizando) return 90f + scoreId;
            if (fase == FaseVuelo.Despegando) return 80f + scoreId;

            if (fase == FaseVuelo.Volando) return 70f + scoreId;

            if (fase == FaseVuelo.EvasionElevada) return 60f + scoreId;

            if (fase == FaseVuelo.PausadoConflicto) return 50f + scoreId;

            return 0f;
        }

        private async Task MonitorDeConflictosAsync()
        {
            while (true)
            {
                var dronesActivos = GestorDron.Instancia.Drones
                    .Where(d => faseDrones.ContainsKey(d) && faseDrones[d] != FaseVuelo.EnTierra)
                    .ToList();

                foreach (var d in dronesActivos.Where(x =>
                    faseDrones[x] == FaseVuelo.PausadoConflicto ||
                    faseDrones[x] == FaseVuelo.EvasionElevada))
                {
                    bool areaDespejada = true;
                    float miPrioridad = CalcularPrioridadGlobal(d);

                    foreach (var otro in dronesActivos)
                    {
                        if (d == otro) continue;
                        if (CalcularDistancia3D(d, otro) < 12.0)
                        {
                            if (CalcularPrioridadGlobal(otro) > miPrioridad)
                            {
                                areaDespejada = false;
                                break;
                            }
                        }
                    }

                    if (areaDespejada)
                    {
                        var fasePrev = fasePreviaConflicto.ContainsKey(d)
                            ? fasePreviaConflicto[d]
                            : FaseVuelo.Volando;
                        fasePreviaConflicto.Remove(d);
                        faseDrones[d] = fasePrev;
                        formHistorial.AgregarMensaje(d.GetID(), "Área despejada. Reanudando vuelo.");
                    }
                }

                for (int i = 0; i < dronesActivos.Count; i++)
                {
                    for (int j = i + 1; j < dronesActivos.Count; j++)
                    {
                        var d1 = dronesActivos[i];
                        var d2 = dronesActivos[j];
                        double dist3D = CalcularDistancia3D(d1, d2);

                        if (dist3D < 2.0)
                        {
                            if (faseDrones[d1] != FaseVuelo.Emergencia &&
                                faseDrones[d2] != FaseVuelo.Emergencia &&
                                !(EstaSobreSuBase(d1) && EstaSobreSuBase(d2)))
                            {
                                ManejarEmergencia(d1); ManejarEmergencia(d2);
                            }
                            continue;
                        }

                        if (dist3D < 10.0)
                        {
                            if (faseDrones[d1] == FaseVuelo.Emergencia || faseDrones[d2] == FaseVuelo.Emergencia) continue;

                            float prio1 = CalcularPrioridadGlobal(d1);
                            float prio2 = CalcularPrioridadGlobal(d2);

                            Dron principal = prio1 >= prio2 ? d1 : d2;
                            Dron secundario = prio1 < prio2 ? d1 : d2;

                            if (faseDrones[secundario] == FaseVuelo.PausadoConflicto ||
                                faseDrones[secundario] == FaseVuelo.EvasionElevada) continue;

                            if (faseDrones[secundario] == FaseVuelo.Despegando)
                            {
                                if (secundario.GetAlt() >= 1.0f)
                                {
                                    if (!fasePreviaConflicto.ContainsKey(secundario))
                                        fasePreviaConflicto[secundario] = faseDrones[secundario];

                                    faseDrones[secundario] = FaseVuelo.PausadoConflicto;
                                    secundario.IrAlPunto(
                                        (float)NormalizarCoord(secundario.GetLat()),
                                        (float)NormalizarCoord(secundario.GetLon()),
                                        secundario.GetAlt(), false, null);
                                    formHistorial.AgregarMensaje(secundario.GetID(),
                                        $"Congelando ascenso por Dron {principal.GetID()}. Altura segura alcanzada.");
                                }
                            }
                            else if (faseDrones[secundario] == FaseVuelo.Volando ||
                                     faseDrones[secundario] == FaseVuelo.SmartRTL)
                            {
                                if (faseDrones[principal] == FaseVuelo.Aterrizando ||
                                    faseDrones[principal] == FaseVuelo.Despegando)
                                {
                                    if (!fasePreviaConflicto.ContainsKey(secundario))
                                        fasePreviaConflicto[secundario] = faseDrones[secundario];

                                    faseDrones[secundario] = FaseVuelo.PausadoConflicto;
                                    secundario.IrAlPunto(
                                        (float)NormalizarCoord(secundario.GetLat()),
                                        (float)NormalizarCoord(secundario.GetLon()),
                                        secundario.GetAlt(), false, null);
                                    formHistorial.AgregarMensaje(secundario.GetID(),
                                        $"Cediendo paso a maniobra del Dron {principal.GetID()}. Detenido en el aire.");
                                }
                                else
                                {
                                    if (!fasePreviaConflicto.ContainsKey(secundario))
                                        fasePreviaConflicto[secundario] = faseDrones[secundario];

                                    faseDrones[secundario] = FaseVuelo.EvasionElevada;
                                    float nuevaAlt = secundario.GetAlt() + 1.5f;
                                    formHistorial.AgregarMensaje(secundario.GetID(),
                                        $"Evadiendo a Dron {principal.GetID()}. Elevando 1.5m.");
                                    secundario.IrAlPunto(
                                        (float)NormalizarCoord(secundario.GetLat()),
                                        (float)NormalizarCoord(secundario.GetLon()),
                                        nuevaAlt, false, null);
                                }
                            }
                        }
                    }
                }
                await Task.Delay(250); 
            }
        }
        private bool EstaSobreSuBase(Dron dron)
        {
            var b = ObtenerBaseDron(dron.GetID());
            double dist = CalcularDistancia(
                NormalizarCoord(dron.GetLat()), NormalizarCoord(dron.GetLon()),
                b.Lat, b.Lng);
            return dist < 3.0;
        }
        private void ManejarEmergencia(Dron dron)
        {
            if (faseDrones[dron] != FaseVuelo.Emergencia)
            {
                faseDrones[dron] = FaseVuelo.Emergencia;

                if (cancelSourcesDrones.ContainsKey(dron)) cancelSourcesDrones[dron].Cancel();

                formHistorial.AgregarMensaje(dron.GetID(), "¡EMERGENCIA! Distancia < 2m. Aterrizaje forzoso iniciado.");

                dron.Aterrizar(false, null);
                _ = EsperarSueloEmergencia(dron);
            }
        }
        private async Task EsperarSueloEmergencia(Dron dron)
        {
            while (dron.GetAlt() > 0.3f) await Task.Delay(500);
            faseDrones[dron] = FaseVuelo.EnTierra;
            fasePreviaConflicto.Remove(dron);
            formHistorial.AgregarMensaje(dron.GetID(), "Aterrizaje de emergencia completado. Dron retirado.");
        }
        private async Task EjecutarPedido(Dron dronQueVuela, Pedido pedido)
        {
            var token = cancelSourcesDrones[dronQueVuela].Token;
            paqueteEntregadoPorDron[dronQueVuela] = false;

            try
            {
                var cliente = pedido.GetDireccion();
                var host = ObtenerHostCliente(cliente);
                var baseAsignada = ObtenerBaseDron(dronQueVuela.GetID());

                if (!rutaRecorrida.ContainsKey(dronQueVuela))
                    rutaRecorrida[dronQueVuela] = new List<PointLatLng>();
                rutaRecorrida[dronQueVuela].Clear();
                rutaRecorrida[dronQueVuela].Add(baseAsignada);

                float altCrucero = 5f;
                float altRecogida = 2f;

                faseDrones[dronQueVuela] = FaseVuelo.Despegando;
                formHistorial.AgregarMensaje(dronQueVuela.GetID(), $"Despegando desde base. Destino: {pedido.GetDestinatario()}");

                await DespegarAsync(dronQueVuela, altRecogida, token);
                await Task.Delay(4000, token);

                if (faseDrones[dronQueVuela] != FaseVuelo.PausadoConflicto)
                {
                    faseDrones[dronQueVuela] = FaseVuelo.Volando;
                }
                formHistorial.AgregarMensaje(dronQueVuela.GetID(), "Volando hacia coordenadas de destino...");

                float latActual = dronQueVuela.GetLat() > 1000 || dronQueVuela.GetLat() < -1000 ? dronQueVuela.GetLat() / 10000000f : dronQueVuela.GetLat();
                float lonActual = dronQueVuela.GetLon() > 1000 || dronQueVuela.GetLon() < -1000 ? dronQueVuela.GetLon() / 10000000f : dronQueVuela.GetLon();
                await VolarAlPuntoAsync(dronQueVuela, new PointLatLng(latActual, lonActual), altCrucero, token);

                var rutaBaseHost = await Task.Run(() => RutaDron.AStar(baseAsignada, host, RutaDron.CrearGrafo(RutaDron.CrearNodos(baseAsignada, host, new List<PointLatLng>(), _escenario.ZonasProhibidas), _escenario.ZonasProhibidas)));
                var rutaHostCliente = await Task.Run(() => RutaDron.AStar(host, cliente, RutaDron.CrearGrafo(RutaDron.CrearNodos(host, cliente, new List<PointLatLng>(), _escenario.ZonasProhibidas), _escenario.ZonasProhibidas)));

                foreach (var p in rutaBaseHost.Skip(1)) { await VolarAlPuntoAsync(dronQueVuela, p, altCrucero, token); }
                foreach (var p in rutaHostCliente.Skip(1)) { await VolarAlPuntoAsync(dronQueVuela, p, altCrucero, token); }

                faseDrones[dronQueVuela] = FaseVuelo.Aterrizando;
                formHistorial.AgregarMensaje(dronQueVuela.GetID(), "En destino. Iniciando maniobra de entrega.");
                await VolarAlPuntoAsync(dronQueVuela, cliente, altRecogida, token);
                await Task.Delay(4000, token);
                paqueteEntregadoPorDron[dronQueVuela] = true;  

                faseDrones[dronQueVuela] = FaseVuelo.Despegando;
                await VolarAlPuntoAsync(dronQueVuela, cliente, altCrucero, token);
                faseDrones[dronQueVuela] = FaseVuelo.Volando;
                formHistorial.AgregarMensaje(dronQueVuela.GetID(), "Paquete entregado con éxito.");

                var rutaClienteHost = await Task.Run(() => RutaDron.AStar(cliente, host, RutaDron.CrearGrafo(RutaDron.CrearNodos(cliente, host, new List<PointLatLng>(), _escenario.ZonasProhibidas), _escenario.ZonasProhibidas)));
                var rutaHostBase = await Task.Run(() => RutaDron.AStar(host, baseAsignada, RutaDron.CrearGrafo(RutaDron.CrearNodos(host, baseAsignada, new List<PointLatLng>(), _escenario.ZonasProhibidas), _escenario.ZonasProhibidas)));

                foreach (var p in rutaClienteHost.Skip(1)) { await VolarAlPuntoAsync(dronQueVuela, p, altCrucero, token); }
                foreach (var p in rutaHostBase.Skip(1)) { await VolarAlPuntoAsync(dronQueVuela, p, altCrucero, token); }

                await VolarAlPuntoAsync(dronQueVuela, baseAsignada, altCrucero, token);

                faseDrones[dronQueVuela] = FaseVuelo.Aterrizando;
                formHistorial.AgregarMensaje(dronQueVuela.GetID(), "Procediendo a aterrizar en base.");

                await AterrizarAsync(dronQueVuela, token);
                await Task.Delay(9000, token);

                faseDrones[dronQueVuela] = FaseVuelo.EnTierra;
                formHistorial.AgregarMensaje(dronQueVuela.GetID(), "Aterrizaje completado. Fin de la misión.");
            }
            catch (OperationCanceledException)
            {
                if (faseDrones[dronQueVuela] == FaseVuelo.Emergencia ||
                    faseDrones[dronQueVuela] == FaseVuelo.SmartRTL) return;

                formHistorial.AgregarMensaje(dronQueVuela.GetID(), "Misión interrumpida desde el panel.");
                faseDrones[dronQueVuela] = FaseVuelo.EnTierra;
            }
        }

        private async Task EsperarAterrizajeEnBase(Dron dron)
        {
            var baseAsignada = ObtenerBaseDron(dron.GetID());
            while (true)
            {
                float altActual = dron.GetAlt();
                double latReal = NormalizarCoord(dron.GetLat());
                double lonReal = NormalizarCoord(dron.GetLon());

                double distBase = CalcularDistancia(latReal, lonReal, baseAsignada.Lat, baseAsignada.Lng);

                if (altActual <= 0.5f && distBase < 5.0)
                {
                    faseDrones[dron] = FaseVuelo.EnTierra;
                    break;
                }
                await Task.Delay(1000);
            }
        }

        private async Task DespegarAsync(Dron dronQueVuela, float altura, CancellationToken token)
        {
            var tcs = new TaskCompletionSource<bool>();
            dronQueVuela.Despegar((int)altura, false, (id, o) => tcs.TrySetResult(true));
            await Task.WhenAny(tcs.Task, Task.Delay(1000));

            while (true)
            {
                token.ThrowIfCancellationRequested();

                if (faseDrones[dronQueVuela] == FaseVuelo.Emergencia ||
                    faseDrones[dronQueVuela] == FaseVuelo.SmartRTL)
                    throw new OperationCanceledException("Despegue interrumpido");

                if (faseDrones[dronQueVuela] == FaseVuelo.PausadoConflicto)
                {
                    while (faseDrones[dronQueVuela] == FaseVuelo.PausadoConflicto)
                        await Task.Delay(100, token);

                    dronQueVuela.IrAlPunto(
                        (float)NormalizarCoord(dronQueVuela.GetLat()),
                        (float)NormalizarCoord(dronQueVuela.GetLon()),
                        altura, false, null);
                }

                if (dronQueVuela.GetAlt() >= altura - 0.5f) return;

                await Task.Delay(100, token);
            }
        }

        private async Task VolarAlPuntoAsync(Dron dronQueVuela, PointLatLng punto, float alturaObjetivo, CancellationToken token)
        {
            while (true)
            {
                token.ThrowIfCancellationRequested();

                if (faseDrones[dronQueVuela] == FaseVuelo.Emergencia)
                {
                    throw new OperationCanceledException("Aterrizaje de Emergencia");
                }

                if (faseDrones[dronQueVuela] == FaseVuelo.PausadoConflicto)
                {

                    dronQueVuela.IrAlPunto((float)NormalizarCoord(dronQueVuela.GetLat()), (float)NormalizarCoord(dronQueVuela.GetLon()), dronQueVuela.GetAlt(), false, null);
                    while (faseDrones[dronQueVuela] == FaseVuelo.PausadoConflicto) await Task.Delay(100, token);
                    continue;
                }

                if (faseDrones[dronQueVuela] == FaseVuelo.EvasionElevada)
                {

                    while (faseDrones[dronQueVuela] == FaseVuelo.EvasionElevada) await Task.Delay(100, token);
                    continue;
                }


                var tcs = new TaskCompletionSource<bool>();
                using (token.Register(() => tcs.TrySetCanceled()))
                {
                    dronQueVuela.IrAlPunto((float)punto.Lat, (float)punto.Lng, alturaObjetivo, false, (id, o) => tcs.TrySetResult(true));
                    await Task.WhenAny(tcs.Task, Task.Delay(1000));
                }


                while (faseDrones[dronQueVuela] == FaseVuelo.Volando || faseDrones[dronQueVuela] == FaseVuelo.Despegando || faseDrones[dronQueVuela] == FaseVuelo.Aterrizando || faseDrones[dronQueVuela] == FaseVuelo.SmartRTL)
                {
                    token.ThrowIfCancellationRequested();

                    double latReal = NormalizarCoord(dronQueVuela.GetLat());
                    double lonReal = NormalizarCoord(dronQueVuela.GetLon());
                    float altActual = dronQueVuela.GetAlt();

                    if (latReal != 0 && lonReal != 0)
                    {
                        double distanciaMts = CalcularDistancia(latReal, lonReal, punto.Lat, punto.Lng);
                        float diferenciaAlt = Math.Abs(altActual - alturaObjetivo);

                        if (distanciaMts < 2.0 && diferenciaAlt < 0.5f)
                        {

                            if (faseDrones[dronQueVuela] != FaseVuelo.SmartRTL &&
                                rutaRecorrida.ContainsKey(dronQueVuela))
                            {
                                rutaRecorrida[dronQueVuela].Add(punto);
                            }
                            return;
                        }
                    }

                    await Task.Delay(100, token); 
                }
            }
        }

        private async Task AterrizarAsync(Dron dronQueVuela, CancellationToken token)
        {
            var tcs = new TaskCompletionSource<bool>();
            using (token.Register(() => tcs.TrySetCanceled()))
            {
                dronQueVuela.Aterrizar(false, (id, o) => tcs.TrySetResult(true));
                await Task.WhenAny(tcs.Task, Task.Delay(1000));

                while (true)
                {
                    token.ThrowIfCancellationRequested();
                    if (dronQueVuela.GetAlt() <= 0.3f) break;
                    await Task.Delay(1000, token);
                }
            }
        }

        private double CalcularDistancia(double lat1, double lon1, double lat2, double lon2)
        {
            var r = 6371e3;
            var f1 = lat1 * Math.PI / 180;
            var f2 = lat2 * Math.PI / 180;
            var df = (lat2 - lat1) * Math.PI / 180;
            var dl = (lon2 - lon1) * Math.PI / 180;
            var a = Math.Sin(df / 2) * Math.Sin(df / 2) + Math.Cos(f1) * Math.Cos(f2) * Math.Sin(dl / 2) * Math.Sin(dl / 2);
            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            return r * c;
        }
        private (Color color, string nombre) ColorMarcadorDron(int idDron)
        {
            switch (idDron)
            {
                case 1: return (Color.LimeGreen, "verde");
                case 2: return (Color.Red, "rojo");
                case 3: return (Color.RoyalBlue, "azul");
                default: return (Color.Orange, "naranja");
            }
        }
        private async Task SmartRTLAsync(Dron dron)
        {
            PointLatLng baseAsignada = ObtenerBaseDron(dron.GetID());

            List<PointLatLng> caminoVuelta = null;

            if (rutaRecorrida.ContainsKey(dron) && rutaRecorrida[dron].Count > 0)
            {
                caminoVuelta = rutaRecorrida[dron].AsEnumerable().Reverse().ToList();
            }
            else
            {
                PointLatLng posActual = new PointLatLng(NormalizarCoord(dron.GetLat()), NormalizarCoord(dron.GetLon()));
                caminoVuelta = await Task.Run(() => RutaDron.AStar(posActual, baseAsignada,
                    RutaDron.CrearGrafo(RutaDron.CrearNodos(posActual, baseAsignada, new List<PointLatLng>(), _escenario.ZonasProhibidas), _escenario.ZonasProhibidas)));
            }

            try
            {
                if (caminoVuelta != null && caminoVuelta.Count > 0)
                {
                    foreach (var punto in caminoVuelta)
                    {
                        await VolarAlPuntoAsync(dron, punto, 5f, CancellationToken.None);
                    }
                }

                await VolarAlPuntoAsync(dron, baseAsignada, 5f, CancellationToken.None);
            }
            catch (OperationCanceledException)
            {

            }

            await VolarAlPuntoAsync(dron, baseAsignada, 5f, CancellationToken.None);   // <-- BORRA ESTA
        
        }

        private void btnVerHistorial_Click(object sender, EventArgs e)
        {
            if (formHistorial.IsDisposed)
            {
                formHistorial = new FormHistorialVuelo();
            }
            formHistorial.Show();
        }

        public class DronMarker : GMarkerGoogle
        {
            private string numero;

            public DronMarker(PointLatLng p, GMarkerGoogleType type, int id) : base(p, type)
            {
                this.numero = id.ToString();
            }
        }

        private void InitializeComponent()
        {
            ((System.ComponentModel.ISupportInitialize)(this.pictureLogo)).BeginInit();
            this.SuspendLayout();
            
            this.pictureLogo.Location = new System.Drawing.Point(214, 191);
            
            this.ClientSize = new System.Drawing.Size(284, 261);
            this.Name = "EnviarPedidoForm";
            this.Load += new System.EventHandler(this.EnviarPedidoForm_Load);
            ((System.ComponentModel.ISupportInitialize)(this.pictureLogo)).EndInit();
            this.ResumeLayout(false);

        }

        private void EnviarPedidoForm_Load(object sender, EventArgs e)
        {

        }
    }
}