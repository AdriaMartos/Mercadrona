using System;
using System.Drawing;
using System.Windows.Forms;
using System.Linq;
using System.Threading.Tasks;
using System.Net.Http;      


namespace SimpleExample
{
    public class GestionarClientes : FormBaseMercadrona
    {
        private readonly Escenario _escenario;
        private Cliente _clienteSeleccionado;

        private ListView lvClientes;
        private Panel panelAcciones;
        private TextBox txtEditNombre;
        private Button btnGuardar;
        private Button btnBorrar;
        private Button btnAbrirMapa;

        public GestionarClientes(Escenario escenario)
        {
            _escenario = escenario;

            InitializeComponent();

            this.Size = new Size(1000, 620);

            this.Text = $"Gestión y Lista de Clientes - {_escenario.Nombre}";
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;

            InicializarComponentesLista();
            CargarClientes();
        }

        private void InicializarComponentesLista()
        {
            btnAbrirMapa = new Button
            {
                Text = "➕ Añadir Cliente (Mapa)",
                Size = new Size(200, 35),
                Location = new Point(this.ClientSize.Width - 235, 20), 
                BackColor = Color.FromArgb(40, 167, 69),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Tag = "Ignorar",
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            btnAbrirMapa.FlatAppearance.BorderSize = 0;
            btnAbrirMapa.Click += BtnAbrirMapa_Click;
            this.Controls.Add(btnAbrirMapa);

            lvClientes = new ListView
            {
                Location = new Point(35, 75), 
                Size = new Size(this.ClientSize.Width - 70, this.ClientSize.Height - 240), 
                View = View.Details,
                FullRowSelect = true,
                GridLines = true,
                Font = new Font("Segoe UI", 10),
                BorderStyle = BorderStyle.FixedSingle, 
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
            };

            lvClientes.Columns.Add("Cliente / Nombre", 200);
            lvClientes.Columns.Add("Host Asignado", 160);
            lvClientes.Columns.Add("Dirección", 900);

            lvClientes.SelectedIndexChanged += LvClientes_SelectedIndexChanged;
            this.Controls.Add(lvClientes);

            panelAcciones = new Panel
            {
                Size = new Size(this.ClientSize.Width - 250, 105),
                Location = new Point(35, this.ClientSize.Height - 140),
                BackColor = Color.FromArgb(245, 247, 250),
                BorderStyle = BorderStyle.FixedSingle, 
                Visible = false,
                Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
            };

            Label lblNom = new Label { Text = "Modificar Nombre de Cliente:", Location = new Point(15, 18), Font = new Font("Segoe UI", 9, FontStyle.Bold), AutoSize = true };
            txtEditNombre = new TextBox { Location = new Point(15, 45), Width = 350, Font = new Font("Segoe UI", 10) };

            btnGuardar = new Button
            {
                Text = "💾 Guardar Cambio",
                Size = new Size(150, 35),
                Location = new Point(390, 40),
                BackColor = Color.FromArgb(40, 167, 69),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Tag = "Ignorar",
                Cursor = Cursors.Hand
            };
            btnGuardar.FlatAppearance.BorderSize = 0;
            btnGuardar.Click += BtnGuardar_Click;

            btnBorrar = new Button
            {
                Text = "🗑️ Borrar",
                Size = new Size(110, 35),
                Location = new Point(550, 40),
                BackColor = Color.Crimson,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Tag = "Ignorar",
                Cursor = Cursors.Hand
            };
            btnBorrar.FlatAppearance.BorderSize = 0;
            btnBorrar.Click += BtnBorrar_Click;

            panelAcciones.Controls.AddRange(new Control[] { lblNom, txtEditNombre, btnGuardar, btnBorrar });
            this.Controls.Add(panelAcciones);

            lvClientes.BringToFront();
            panelAcciones.BringToFront();
        }
        private async Task<string> ObtenerDireccionDesdeCoordenadasAsync(double lat, double lon)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Add("User-Agent", "MercadronaApp/1.0");

                    string latStr = lat.ToString(System.Globalization.CultureInfo.InvariantCulture);
                    string lonStr = lon.ToString(System.Globalization.CultureInfo.InvariantCulture);

                    string url = $"https://nominatim.openstreetmap.org/reverse?format=json&lat={latStr}&lon={lonStr}";

                    HttpResponseMessage response = await client.GetAsync(url);
                    if (response.IsSuccessStatusCode)
                    {
                        string json = await response.Content.ReadAsStringAsync();

                        int index = json.IndexOf("\"display_name\":\"");
                        if (index > -1)
                        {
                            int startIndex = index + 16;
                            int endIndex = json.IndexOf("\"", startIndex);
                            return json.Substring(startIndex, endIndex - startIndex);
                        }
                    }
                }
            }
            catch
            {
            }
            return null;
        }
        public async void CargarClientes()
        {
            lvClientes.Items.Clear();
            panelAcciones.Visible = false;

            foreach (var c in _escenario.Clientes)
            {
                ListViewItem item = new ListViewItem(c.Nombre);
                item.SubItems.Add(c.ObtenerHostCercano(_escenario.Hosts));

                item.SubItems.Add("Buscando dirección...");

                item.Tag = c;
                lvClientes.Items.Add(item);
            }

            for (int i = 0; i < _escenario.Clientes.Count; i++)
            {
                var c = _escenario.Clientes[i];
                string dirAMostrar;

                if (!string.IsNullOrWhiteSpace(c.Direccion))
                {
                    dirAMostrar = c.Direccion;
                }
                else
                {
                    string calleEncontrada = await ObtenerDireccionDesdeCoordenadasAsync(c.Latitud, c.Longitud);

                    if (!string.IsNullOrWhiteSpace(calleEncontrada))
                    {
                        dirAMostrar = calleEncontrada;
                        c.Direccion = calleEncontrada; 
                    }
                    else
                    {
                        dirAMostrar = $"Dirección no disponible (Lat={c.Latitud.ToString("F4")}, Lon={c.Longitud.ToString("F4")})";
                    }
                }

                lvClientes.Items[i].SubItems[2].Text = dirAMostrar;
            }

            if (lvClientes.Items.Count > 0)
            {
                lvClientes.Columns[2].Width = -2;
            }
        }

        private void LvClientes_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lvClientes.SelectedItems.Count > 0)
            {
                _clienteSeleccionado = (Cliente)lvClientes.SelectedItems[0].Tag;
                txtEditNombre.Text = _clienteSeleccionado.Nombre;
                panelAcciones.Visible = true;
            }
            else
            {
                panelAcciones.Visible = false;
            }
        }

        private void BtnGuardar_Click(object sender, EventArgs e)
        {
            if (_clienteSeleccionado == null) return;

            if (string.IsNullOrWhiteSpace(txtEditNombre.Text))
            {
                MessageBox.Show("El nombre no puede estar vacío.");
                return;
            }

            _clienteSeleccionado.Nombre = txtEditNombre.Text.Trim();
            CargarClientes();
            MessageBox.Show("Nombre actualizado.");
        }

        private void BtnBorrar_Click(object sender, EventArgs e)
        {
            if (_clienteSeleccionado == null) return;

            var confirm = MessageBox.Show($"¿Eliminar a {_clienteSeleccionado.Nombre}?", "Eliminar", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (confirm == DialogResult.Yes)
            {
                _escenario.Clientes.Remove(_clienteSeleccionado);
                CargarClientes();
            }
        }

        private void BtnAbrirMapa_Click(object sender, EventArgs e)
        {
            MapaClientesForm mapaForm = new MapaClientesForm(_escenario, this);
            mapaForm.ShowDialog();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            this.ClientSize = new System.Drawing.Size(684, 480);
            this.Name = "GestionarClientes";
            this.Load += new System.EventHandler(this.GestionarClientes_Load);
            this.ResumeLayout(false);
        }

        private void GestionarClientes_Load(object sender, EventArgs e) { }
    }
}