using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace SimpleExample
{
    public partial class ConfiguracionCapacidadDrones : FormBaseMercadrona
    {

        public Dictionary<int, double> Capacidades { get; private set; }

        private readonly int cantidadDrones;
        private readonly double[] valoresKg = { 1.0, 3.0, 5.0 };
        private readonly Dictionary<int, TrackBar> trackBars = new Dictionary<int, TrackBar>();

        public ConfiguracionCapacidadDrones(int cantidadDrones)
        {
            this.cantidadDrones = cantidadDrones;
            InicializarControles();
        }

        private void InicializarControles()
        {
            Text = "Configuración de Capacidad de Drones";
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ClientSize = new Size(700, Math.Min(700, 150 + cantidadDrones * 62));

            var root = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 3,
                Padding = new Padding(12)
            };
            root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 34f));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 70f));
            Controls.Add(root);

            var lblInstrucciones = new Label
            {
                Text = "Capacidad máxima de carga de cada dron (kg):",
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };
            root.Controls.Add(lblInstrucciones, 0, 0);

            var panelScroll = new Panel { Dock = DockStyle.Fill, AutoScroll = true };
            root.Controls.Add(panelScroll, 0, 1);

            var tabla = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                ColumnCount = 3
            };
            tabla.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 70f));
            tabla.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            tabla.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 95f));
            panelScroll.Controls.Add(tabla);

            for (int i = 1; i <= cantidadDrones; i++)
            {
                var lblDron = new Label
                {
                    Text = $"Dron {i}",
                    AutoSize = true,
                    Anchor = AnchorStyles.Left,
                    Font = new Font("Segoe UI", 9, FontStyle.Bold),
                    Margin = new Padding(3, 14, 3, 3)
                };

                var trackBar = new TrackBar
                {
                    Minimum = 0,
                    Maximum = 2,
                    TickFrequency = 1,
                    LargeChange = 1,
                    SmallChange = 1,
                    Value = 0,
                    Dock = DockStyle.Fill
                };

                var indicador = new Panel
                {
                    Width = 88,
                    Height = 32,
                    BorderStyle = BorderStyle.FixedSingle,
                    Anchor = AnchorStyles.Left,
                    Margin = new Padding(3, 9, 3, 3)
                };
                var lblKg = new Label
                {
                    Dock = DockStyle.Fill,
                    TextAlign = ContentAlignment.MiddleCenter,
                    ForeColor = Color.White,
                    Font = new Font("Segoe UI", 10, FontStyle.Bold)
                };
                indicador.Controls.Add(lblKg);

                Action refrescar = () =>
                {
                    double kg = valoresKg[trackBar.Value];
                    indicador.BackColor = ColorPorPeso(kg);
                    lblKg.Text = $"{kg:0} kg";
                };
                trackBar.ValueChanged += (s, e) => refrescar();
                refrescar();

                tabla.Controls.Add(lblDron, 0, i - 1);
                tabla.Controls.Add(trackBar, 1, i - 1);
                tabla.Controls.Add(indicador, 2, i - 1);

                trackBars[i] = trackBar;
            }

            var panelBotones = new FlowLayoutPanel
            {
                Anchor = AnchorStyles.None,     
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false
            };
            root.Controls.Add(panelBotones, 0, 2);

            var btnAceptar = new Button
            {
                Text = "Aceptar",
                DialogResult = DialogResult.OK,
                Size = new Size(100, 34),
                Margin = new Padding(8, 9, 8, 9)
            };
            btnAceptar.Click += (s, e) =>
            {
                Capacidades = new Dictionary<int, double>();
                foreach (var kvp in trackBars)
                    Capacidades[kvp.Key] = valoresKg[kvp.Value.Value];
            };

            var btnCancelar = new Button
            {
                Text = "Cancelar",
                DialogResult = DialogResult.Cancel,
                Size = new Size(100, 34),
                Margin = new Padding(8, 9, 8, 9)
            };
            if (this.pictureLogo != null)   
                this.pictureLogo.Visible = false;
            panelBotones.Controls.Add(btnAceptar);
            panelBotones.Controls.Add(btnCancelar);

            AcceptButton = btnAceptar;
            CancelButton = btnCancelar;
        }

        private Color ColorPorPeso(double kg)
        {
            if (kg <= 1.0) return Color.FromArgb(76, 175, 80);  
            if (kg <= 3.0) return Color.FromArgb(255, 152, 0);  
            return Color.FromArgb(211, 47, 47);                  
        }

   
        private void InitializeComponent()
        {
            this.SuspendLayout();
            this.Name = "ConfiguracionCapacidadDrones";
            this.Load += new System.EventHandler(this.ConfiguracionCapacidadDrones_Load);
            this.ResumeLayout(false);
        }

        private void ConfiguracionCapacidadDrones_Load(object sender, EventArgs e)
        {
        }
    }
}