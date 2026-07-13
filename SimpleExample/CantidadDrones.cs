using System;
using System.Drawing;
using System.Windows.Forms;

namespace SimpleExample
{
    public partial class CantidadDrones : FormBaseMercadrona
    {
        public int DronesDisponibles { get; private set; }

        private NumericUpDown numDrones;
        private Button btnAceptar;
        private Button btnCancelar;

        public CantidadDrones()
        {
            Text = "Configuración de Drones";
            ClientSize = new Size(300, 200);
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;

            Label lblMensaje = new Label
            {
                Text = "Cuantos drones van a operar?",
                Location = new Point(20, 20),
                AutoSize = true
            };
            Controls.Add(lblMensaje);

            numDrones = new NumericUpDown
            {
                Location = new Point(20, 50),
                Width = 100,
                Minimum = 1,
                Maximum = 50,
                Value = 2
            };
            Controls.Add(numDrones);

            btnAceptar = new Button
            {
                Text = "Aceptar",
                DialogResult = DialogResult.OK,
                Location = new Point(110, 100)
            };
            btnAceptar.Click += (s, e) => { DronesDisponibles = (int)numDrones.Value; };
            Controls.Add(btnAceptar);

            btnCancelar = new Button
            {
                Text = "Cancelar",
                DialogResult = DialogResult.Cancel,
                Location = new Point(200, 100)
            };
            Controls.Add(btnCancelar);

            AcceptButton = btnAceptar;
            CancelButton = btnCancelar;
        }

       
        private void CantidadDrones_Load(object sender, EventArgs e)
        {
            
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
          
            this.ClientSize = new System.Drawing.Size(284, 261);
            this.Name = "CantidadDrones";
            this.Load += new System.EventHandler(this.CantidadDrones_Load_1);
            this.ResumeLayout(false);

        }

        private void CantidadDrones_Load_1(object sender, EventArgs e)
        {

        }
    }
}