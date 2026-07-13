using GMap.NET;
using GMap.NET.MapProviders;
using GMap.NET.WindowsForms;
using GMap.NET.WindowsForms.Markers;
using GMap.NET.WindowsForms.ToolTips;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace SimpleExample
{
    public class SeleccionEscenarioForm : FormBaseMercadrona
    {
        public Escenario EscenarioSeleccionado
        {
            get;
            private set;
        }

        private FlowLayoutPanel flow;
        private List<Escenario> listaEscenarios;

        public SeleccionEscenarioForm(List<Escenario> escenarios)
        {
            this.listaEscenarios = escenarios; 

            Text = "Seleccionar Escenario";
            WindowState = FormWindowState.Maximized;

            flow = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                BackColor = Color.FromArgb(240, 240, 240),
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = true,
                Padding = new Padding(20),
            };

            Controls.Add(flow);

            foreach (var esc in escenarios)
            {
                CrearPreviewEscenario(esc);
            }

            flow.Resize += (s, e) => Centrar();
            Centrar();
        }

        private void Centrar()
        {
            int totalWidth = 0;

            foreach (Control c in flow.Controls)
                totalWidth += c.Width + c.Margin.Horizontal;

            int available = flow.ClientSize.Width;

            int paddingLeft = Math.Max(20, (available - totalWidth) / 2);

            flow.Padding = new Padding(paddingLeft, 20, 20, 20);
        }

        private void CrearPreviewEscenario(Escenario escenario)
        {
            Panel panel = new Panel
            {
                Width = 600,
                Height = 460, 
                BackColor = Color.White,
                Margin = new Padding(20),
                Padding = new Padding(2),
                BorderStyle = BorderStyle.FixedSingle,
                Cursor = Cursors.Hand
            };

            panel.Paint += (s, e) =>
            {
                ControlPaint.DrawBorder(
                    e.Graphics,
                    panel.ClientRectangle,
                    Color.LightGray,
                    ButtonBorderStyle.Solid);
            };

            Label lblNombre = new Label
            {
                Text = escenario.Nombre,
                Dock = DockStyle.Top,
                Height = 35,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = Color.FromArgb(40, 40, 40),
                TextAlign = ContentAlignment.MiddleCenter
            };

            Button btnEliminar = new Button
            {
                Text = "Eliminar Escenario",
                Dock = DockStyle.Bottom,
                Height = 35,
                BackColor = Color.FromArgb(220, 53, 69), 
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Tag = "Ignorar" 
            };
            btnEliminar.FlatAppearance.BorderSize = 0;

            btnEliminar.Click += (s, e) =>
            {

                var resultado = MessageBox.Show(
                    $"¿Estás seguro de que deseas eliminar permanentemente el escenario '{escenario.Nombre}'?",
                    "Confirmar eliminación",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning);

                if (resultado == DialogResult.Yes)
                {
                    listaEscenarios.Remove(escenario);

                    flow.Controls.Remove(panel);
                    panel.Dispose();

                    Centrar();
                }
            };

            GMapControl mapa = new GMapControl
            {
                Dock = DockStyle.Fill, 
                MapProvider = GoogleSatelliteMapProvider.Instance,
                MinZoom = 1,
                MaxZoom = 24,
                DragButton = MouseButtons.Left
            };

            GMaps.Instance.Mode = AccessMode.ServerAndCache;
            AjustarVistaCompleta(mapa, escenario);
            DibujarEscenario(mapa, escenario);

            panel.Controls.Add(lblNombre);
            panel.Controls.Add(btnEliminar);
            panel.Controls.Add(mapa);

            mapa.SendToBack();

            panel.MouseEnter += (s, e) => panel.BackColor = Color.FromArgb(225, 235, 245);
            panel.MouseLeave += (s, e) => panel.BackColor = Color.White;

            panel.Click += (s, e) => SeleccionarEscenario(escenario);
            mapa.Click += (s, e) => SeleccionarEscenario(escenario);
            lblNombre.Click += (s, e) => SeleccionarEscenario(escenario);

            flow.Controls.Add(panel);
        }

        private void AjustarVistaCompleta(GMapControl mapa, Escenario escenario)
        {
            double minLat = double.MaxValue;
            double maxLat = double.MinValue;
            double minLng = double.MaxValue;
            double maxLng = double.MinValue;

            void RevisarPunto(PointLatLng p)
            {
                if (p.Lat < minLat) minLat = p.Lat;
                if (p.Lat > maxLat) maxLat = p.Lat;
                if (p.Lng < minLng) minLng = p.Lng;
                if (p.Lng > maxLng) maxLng = p.Lng;
            }

            foreach (var p in escenario.ZonaPermitida)
                RevisarPunto(p);

            foreach (var zona in escenario.ZonasProhibidas)
            {
                foreach (var p in zona)
                    RevisarPunto(p);
            }

            foreach (var h in escenario.Hosts)
                RevisarPunto(h.Posicion);

            RevisarPunto(escenario.BaseDron);

            RectLatLng rect = RectLatLng.FromLTRB(maxLng, maxLat, minLng, minLat);
            mapa.SetZoomToFitRect(rect);
            mapa.Zoom -= 0.3;
        }

        private void SeleccionarEscenario(Escenario escenario)
        {
            EscenarioSeleccionado = escenario;
            DialogResult = DialogResult.OK;
            Close();
        }

        private void DibujarEscenario(GMapControl mapa, Escenario escenario)
        {
            GMapOverlay overlay = new GMapOverlay("escenario");

            if (escenario.ZonaPermitida.Count > 0)
            {
                GMapPolygon permitida = new GMapPolygon(escenario.ZonaPermitida, "permitida");
                permitida.Fill = new SolidBrush(Color.FromArgb(50, Color.Green));
                permitida.Stroke = new Pen(Color.Lime, 2);
                overlay.Polygons.Add(permitida);
            }

            foreach (var zona in escenario.ZonasProhibidas)
            {
                GMapPolygon prohibida = new GMapPolygon(zona, "prohibida");
                prohibida.Fill = new SolidBrush(Color.FromArgb(70, Color.Red));
                prohibida.Stroke = new Pen(Color.Red, 2);
                overlay.Polygons.Add(prohibida);
            }

            foreach (var host in escenario.Hosts)
            {
                GMarkerGoogle marker = new GMarkerGoogle(host.Posicion, GMarkerGoogleType.blue_dot);
                marker.ToolTipText = host.Nombre;
                overlay.Markers.Add(marker);
            }

            GMarkerGoogle baseMarker = new GMarkerGoogle(escenario.BaseDron, GMarkerGoogleType.green_big_go);
            baseMarker.ToolTipText = "Base Dron";
            overlay.Markers.Add(baseMarker);

            mapa.Overlays.Add(overlay);
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            this.ClientSize = new System.Drawing.Size(284, 261);
            this.Name = "SeleccionEscenarioForm";
            this.Load += new System.EventHandler(this.SeleccionEscenarioForm_Load);
            this.ResumeLayout(false);
        }

        private void SeleccionEscenarioForm_Load(object sender, EventArgs e)
        {
        }
    }
}