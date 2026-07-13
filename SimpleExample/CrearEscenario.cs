using GMap.NET;
using GMap.NET.MapProviders;
using GMap.NET.WindowsForms;
using GMap.NET.WindowsForms.Markers;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace SimpleExample
{
    public class CrearEscenarioForm : FormBaseMercadrona
    {
        private enum ModoEdicion
        {
            Ninguno,
            ZonaValida,
            ZonaProhibida,
            Hosts,
            Borrar
        }

        private Escenario nuevoEscenario;
        private ModoEdicion modoActual = ModoEdicion.Ninguno;

       
        private List<PointLatLng> puntosPoligonoTemp = new List<PointLatLng>();

        private GMapControl mapa;
        private GMapOverlay overlayPoligonos;
        private GMapOverlay overlayMarcadores;
        private GMapOverlay overlayTemporal; 

      
        private TextBox txtNombreEscenario;
        private Button btnZonaValida, btnZonaProhibida, btnHosts, btnBorrar, btnCerrarPoligono, btnGuardar;
        private Label lblEstado;

        private const double RADIO_SELECCION_METROS = 15;

        public CrearEscenarioForm()
        {
            nuevoEscenario = new Escenario();

            this.Text = "Creador de Escenarios";
            this.ClientSize = new Size(1000, 600);
            this.StartPosition = FormStartPosition.CenterScreen;

            InicializarControles();
            InicializarMapa();
        }

        private void InicializarControles()
        {
            Panel panel = new Panel { Location = new Point(10, 10), Size = new Size(250, 580) };

            panel.Controls.Add(new Label { Text = "Nombre del Escenario:", Location = new Point(10, 10), AutoSize = true });
            txtNombreEscenario = new TextBox { Location = new Point(10, 30), Width = 220 };
            panel.Controls.Add(txtNombreEscenario);

            btnZonaValida = CrearBoton("1. Crear Zona Válida", 70);
            btnZonaProhibida = CrearBoton("2. Crear Zona Prohibida", 110);

            btnCerrarPoligono = CrearBoton("Finalizar Polígono", 150);
            btnCerrarPoligono.BackColor = Color.LightYellow;
            btnCerrarPoligono.Visible = false;
            btnCerrarPoligono.Click += BtnCerrarPoligono_Click;

            btnHosts = CrearBoton("3. Añadir Hosts", 200);
            btnBorrar = CrearBoton("Borrar Elemento", 240);
            btnGuardar = CrearBoton("GUARDAR ESCENARIO", 300); 
            btnGuardar.BackColor = Color.LightGreen;
            btnGuardar.Click += BtnGuardar_Click;

            lblEstado = new Label { Text = "Modo: Ninguno", Location = new Point(10, 360), AutoSize = true, Font = new Font("Arial", 10, FontStyle.Bold) };

            btnZonaValida.Click += (s, e) => CambiarModo(ModoEdicion.ZonaValida, btnZonaValida);
            btnZonaProhibida.Click += (s, e) => CambiarModo(ModoEdicion.ZonaProhibida, btnZonaProhibida);
            btnHosts.Click += (s, e) => CambiarModo(ModoEdicion.Hosts, btnHosts);
            btnBorrar.Click += (s, e) => CambiarModo(ModoEdicion.Borrar, btnBorrar);

            panel.Controls.Add(btnZonaValida);
            panel.Controls.Add(btnZonaProhibida);
            panel.Controls.Add(btnCerrarPoligono);
            panel.Controls.Add(btnHosts);
            panel.Controls.Add(btnBorrar);
            panel.Controls.Add(btnGuardar);
            panel.Controls.Add(lblEstado);

            Controls.Add(panel);
        }

        private Button CrearBoton(string texto, int y)
        {
            return new Button { Text = texto, Location = new Point(10, y), Size = new Size(220, 35) };
        }

        private void InicializarMapa()
        {
            mapa = new GMapControl
            {
                Location = new Point(270, 10),
                Size = new Size(710, 510),
                MapProvider = GoogleSatelliteMapProvider.Instance,
                MinZoom = 2,
                MaxZoom = 20,
                Zoom = 15,
                CanDragMap = true,
                DragButton = MouseButtons.Right,
                ShowCenter = false,
                Position = new PointLatLng(41.276407, 1.988615)
            };

            GMaps.Instance.Mode = AccessMode.ServerAndCache;

            overlayPoligonos = new GMapOverlay("poligonos");
            overlayMarcadores = new GMapOverlay("marcadores");
            overlayTemporal = new GMapOverlay("temporal");

            mapa.Overlays.Add(overlayPoligonos);
            mapa.Overlays.Add(overlayTemporal);
            mapa.Overlays.Add(overlayMarcadores);

            mapa.MouseClick += Mapa_MouseClick;

            Controls.Add(mapa);
        }

        private void CambiarModo(ModoEdicion nuevoModo, Button botonActivo)
        {
            if (modoActual == nuevoModo)
            {
                modoActual = ModoEdicion.Ninguno;
            }
            else
            {
                modoActual = nuevoModo;
            }

            
            btnZonaValida.BackColor = btnZonaProhibida.BackColor = btnHosts.BackColor = btnBorrar.BackColor = SystemColors.Control;
            btnZonaValida.ForeColor = btnZonaProhibida.ForeColor = btnHosts.ForeColor = btnBorrar.ForeColor = SystemColors.ControlText;

          
            puntosPoligonoTemp.Clear();
            overlayTemporal.Polygons.Clear();
            overlayTemporal.Markers.Clear();
            btnCerrarPoligono.Visible = false;

            if (modoActual != ModoEdicion.Ninguno)
            {
                botonActivo.BackColor = (modoActual == ModoEdicion.Borrar) ? Color.DarkRed : Color.SteelBlue;
                botonActivo.ForeColor = Color.White;
                lblEstado.Text = $"Modo: {modoActual}";
                if (modoActual == ModoEdicion.ZonaValida || modoActual == ModoEdicion.ZonaProhibida)
                {
                    btnCerrarPoligono.Visible = true;
                }
            }
            else
            {
                lblEstado.Text = "Modo: Ninguno";
            }

            mapa.Refresh();
        }

        private string ObtenerNombreDialogo(string tipoElemento)
        {
            Form prompt = new Form()
            {
                Width = 300,
                Height = 150,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                Text = $"Añadir {tipoElemento}",
                StartPosition = FormStartPosition.CenterScreen,
                MaximizeBox = false,
                MinimizeBox = false
            };

            Label textLabel = new Label() { Left = 20, Top = 20, Text = $"Introduce el nombre del {tipoElemento}:", AutoSize = true };
            TextBox textBox = new TextBox() { Left = 20, Top = 50, Width = 240 };
            Button confirmation = new Button() { Text = "Aceptar", Left = 160, Width = 100, Top = 80, DialogResult = DialogResult.OK };

            prompt.AcceptButton = confirmation;
            prompt.Controls.Add(textLabel);
            prompt.Controls.Add(textBox);
            prompt.Controls.Add(confirmation);

            return prompt.ShowDialog() == DialogResult.OK ? textBox.Text.Trim() : "";
        }

        private void Mapa_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left) return;

            PointLatLng clickPos = mapa.FromLocalToLatLng(e.X, e.Y);

            switch (modoActual)
            {
                case ModoEdicion.ZonaValida:
                case ModoEdicion.ZonaProhibida:
                    puntosPoligonoTemp.Add(clickPos);
                    DibujarPoligonoTemporal();
                    break;

                case ModoEdicion.Hosts:
                    string nombreHost = ObtenerNombreDialogo("Host");
                    if (!string.IsNullOrWhiteSpace(nombreHost))
                    {
                        nuevoEscenario.Hosts.Add(new Host { Nombre = nombreHost, Lat = clickPos.Lat, Lng = clickPos.Lng });
                        ActualizarMapa();
                    }
                    break;

                case ModoEdicion.Borrar:
                    IntentarBorrarElemento(clickPos);
                    break;
            }
        }

        private void DibujarPoligonoTemporal()
        {
            overlayTemporal.Polygons.Clear();
            overlayTemporal.Markers.Clear();

            foreach (var p in puntosPoligonoTemp)
            {
                overlayTemporal.Markers.Add(new GMarkerGoogle(p, GMarkerGoogleType.black_small));
            }

            if (puntosPoligonoTemp.Count > 1)
            {
                var color = modoActual == ModoEdicion.ZonaValida ? Color.Green : Color.Red;
                overlayTemporal.Polygons.Add(new GMapPolygon(puntosPoligonoTemp, "temp")
                {
                    Fill = new SolidBrush(Color.FromArgb(40, color)),
                    Stroke = new Pen(color, 2) { DashStyle = System.Drawing.Drawing2D.DashStyle.Dash }
                });
            }
            mapa.Refresh();
        }

        private void BtnCerrarPoligono_Click(object sender, EventArgs e)
        {
            if (puntosPoligonoTemp.Count < 3)
            {
                MessageBox.Show("Un polígono necesita al menos 3 puntos.");
                return;
            }

            var poligonoFinal = new List<PointLatLng>(puntosPoligonoTemp);
            poligonoFinal.Add(puntosPoligonoTemp.First());

            if (modoActual == ModoEdicion.ZonaValida)
            {
                if (nuevoEscenario.ZonaPermitida.Count > 0)
                {
                    if (MessageBox.Show("Ya existe una Zona Válida. ¿Deseas reemplazarla?", "Atención", MessageBoxButtons.YesNo) == DialogResult.Yes)
                    {
                        nuevoEscenario.ZonaPermitida = poligonoFinal;
                    }
                }
                else
                {
                    nuevoEscenario.ZonaPermitida = poligonoFinal;
                }
            }
            else if (modoActual == ModoEdicion.ZonaProhibida)
            {
                nuevoEscenario.ZonasProhibidas.Add(poligonoFinal);
            }

            puntosPoligonoTemp.Clear();
            overlayTemporal.Polygons.Clear();
            overlayTemporal.Markers.Clear();
            ActualizarMapa();
        }

        private void CrearEscenarioForm_Load(object sender, EventArgs e)
        {
        }

        private void IntentarBorrarElemento(PointLatLng clickPos)
        {
            var host = nuevoEscenario.Hosts.FirstOrDefault(h => CalcularDistanciaMetros(clickPos, h.Posicion) < RADIO_SELECCION_METROS);
            if (host != null)
            {
                if (MessageBox.Show($"¿Borrar {host.Nombre}?", "Confirmar", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    nuevoEscenario.Hosts.Remove(host);
                    ActualizarMapa();
                }
                return;
            }

            var zonaProhibida = nuevoEscenario.ZonasProhibidas.FirstOrDefault(z => EstaDentroDePoligono(clickPos, z));
            if (zonaProhibida != null)
            {
                if (MessageBox.Show("¿Borrar esta Zona Prohibida?", "Confirmar", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    nuevoEscenario.ZonasProhibidas.Remove(zonaProhibida);
                    ActualizarMapa();
                }
                return;
            }

            if (nuevoEscenario.ZonaPermitida.Count > 0 && EstaDentroDePoligono(clickPos, nuevoEscenario.ZonaPermitida))
            {
                if (MessageBox.Show("¿Borrar la Zona Válida?", "Confirmar", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    nuevoEscenario.ZonaPermitida.Clear();
                    ActualizarMapa();
                }
                return;
            }
        }

        private void ActualizarMapa()
        {
            overlayPoligonos.Polygons.Clear();
            overlayMarcadores.Markers.Clear();

            if (nuevoEscenario.ZonaPermitida.Count > 0)
            {
                overlayPoligonos.Polygons.Add(new GMapPolygon(nuevoEscenario.ZonaPermitida, "Permitida")
                {
                    Fill = new SolidBrush(Color.FromArgb(40, Color.LightGreen)),
                    Stroke = new Pen(Color.Green, 2)
                });
            }

            foreach (var zona in nuevoEscenario.ZonasProhibidas)
            {
                overlayPoligonos.Polygons.Add(new GMapPolygon(zona, "Prohibida")
                {
                    Fill = new SolidBrush(Color.FromArgb(60, Color.Red)),
                    Stroke = new Pen(Color.DarkRed, 2)
                });
            }

            foreach (var h in nuevoEscenario.Hosts)
            {
                overlayMarcadores.Markers.Add(new GMarkerGoogle(h.Posicion, GMarkerGoogleType.yellow_dot) { ToolTipText = h.Nombre });
            }

            mapa.Refresh();
        }

        public Escenario EscenarioCreado { get; private set; }

        private void BtnGuardar_Click(object sender, EventArgs e)
        {
           
            if (string.IsNullOrWhiteSpace(txtNombreEscenario.Text))
            {
                MessageBox.Show("Por favor, ponle un nombre al escenario.", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

           
            List<string> elementosFaltantes = new List<string>();

        
            if (nuevoEscenario.ZonaPermitida == null || nuevoEscenario.ZonaPermitida.Count == 0)
            {
                elementosFaltantes.Add("una Zona Válida");
            }

            if (nuevoEscenario.Hosts == null || nuevoEscenario.Hosts.Count == 0)
            {
                elementosFaltantes.Add("al menos un Host");
            }

            if (elementosFaltantes.Count > 0)
            {
                string componentes = string.Join(" y ", elementosFaltantes);

                MessageBox.Show(
                    $"El escenario necesita de {componentes} para poder funcionar.",
                    "Faltan componentes obligatorios",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );
                return; 
            }

            nuevoEscenario.Nombre = txtNombreEscenario.Text;
            nuevoEscenario.BaseDron = CalcularCentroidePoligono(nuevoEscenario.ZonaPermitida);
            EscenarioCreado = nuevoEscenario;

            MessageBox.Show($"¡Escenario '{nuevoEscenario.Nombre}' guardado correctamente!", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);

            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private double CalcularDistanciaMetros(PointLatLng p1, PointLatLng p2)
        {
            double R = 6371000;
            double dLat = (p2.Lat - p1.Lat) * Math.PI / 180.0;
            double dLon = (p2.Lng - p1.Lng) * Math.PI / 180.0;
            double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) + Math.Cos(p1.Lat * Math.PI / 180.0) * Math.Cos(p2.Lat * Math.PI / 180.0) * Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
            return R * 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        }

        private bool EstaDentroDePoligono(PointLatLng p, List<PointLatLng> poligono)
        {
            bool inside = false;
            for (int i = 0, j = poligono.Count - 1; i < poligono.Count; j = i++)
            {
                if (((poligono[i].Lat > p.Lat) != (poligono[j].Lat > p.Lat)) &&
                    (p.Lng < (poligono[j].Lng - poligono[i].Lng) * (p.Lat - poligono[i].Lat) / (poligono[j].Lat - poligono[i].Lat) + poligono[i].Lng))
                {
                    inside = !inside;
                }
            }
            return inside;
        }

        private PointLatLng CalcularCentroidePoligono(List<PointLatLng> poligono)
        {
            double latSum = 0;
            double lngSum = 0;

            int puntosReales = poligono.Count;
            if (poligono.Count > 1 && poligono.First() == poligono.Last())
            {
                puntosReales--;
            }

            for (int i = 0; i < puntosReales; i++)
            {
                latSum += poligono[i].Lat;
                lngSum += poligono[i].Lng;
            }

            return new PointLatLng(latSum / puntosReales, lngSum / puntosReales);
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            this.ClientSize = new System.Drawing.Size(284, 261);
            this.Name = "CrearEscenarioForm";
            this.Load += new System.EventHandler(this.CrearEscenarioForm_Load);
            this.ResumeLayout(false);
        }
    }
}