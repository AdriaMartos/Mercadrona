using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace SimpleExample
{
    public class FormHistorialVuelo : FormBaseMercadrona
    {

        private Dictionary<int, ListBox> listasDrones;

        public FormHistorialVuelo()
        {
            ConfigurarFormulario();
            InicializarComponentes();
        }

        private void ConfigurarFormulario()
        {
            this.Text = "Registro de Vuelo en Vivo";
            this.Size = new Size(1000, 500); 
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.WhiteSmoke;
        }

        private void InicializarComponentes()
        {
            var drones = GestorDron.Instancia.Drones;
            int cantidadDrones = drones.Count > 0 ? drones.Count : 1; 

            TableLayoutPanel panelPrincipal = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = cantidadDrones,
                RowCount = 2
            };

            panelPrincipal.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));
            panelPrincipal.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            listasDrones = new Dictionary<int, ListBox>();

            for (int i = 0; i < cantidadDrones; i++)
            {
                int idDron = drones[i].GetID();

                panelPrincipal.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F / cantidadDrones));

                string titulo;
                if (idDron == 1) titulo = $"DRON {idDron}";
                else if (idDron == 2) titulo = $"DRON {idDron}";
                else titulo = $"DRON {idDron}";

                Label lblDron = new Label
                {
                    Text = titulo,
                    TextAlign = ContentAlignment.MiddleCenter,
                    Dock = DockStyle.Fill,
                    Font = new Font("Segoe UI", 10, FontStyle.Bold)
                };

                ListBox lstDron = new ListBox
                {
                    Dock = DockStyle.Fill,
                    Font = new Font("Consolas", 10),
                    IntegralHeight = false,
                    Margin = new Padding(10, 5, 10, 80)
                };

                listasDrones.Add(idDron, lstDron);

                panelPrincipal.Controls.Add(lblDron, i, 0);
                panelPrincipal.Controls.Add(lstDron, i, 1);
            }

            this.Controls.Add(panelPrincipal);
        }

        public void AgregarMensaje(int idDron, string mensaje)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(() => AgregarMensaje(idDron, mensaje)));
                return;
            }

            if (listasDrones != null && listasDrones.ContainsKey(idDron))
            {
                string textoFormateado = $"{DateTime.Now:HH:mm:ss} - {mensaje}";

                var lstActual = listasDrones[idDron];
                lstActual.Items.Add(textoFormateado);

                lstActual.TopIndex = lstActual.Items.Count - 1;
            }
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
           
            this.ClientSize = new System.Drawing.Size(284, 261);
            this.Name = "FormHistorialVuelo";
            this.Load += new System.EventHandler(this.FormHistorialVuelo_Load);
            this.ResumeLayout(false);
        }

        private void FormHistorialVuelo_Load(object sender, EventArgs e)
        {
        }
    }
}