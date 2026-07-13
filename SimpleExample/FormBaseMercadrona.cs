using System;
using System.Drawing;
using System.Windows.Forms;

namespace SimpleExample
{
    public class FormBaseMercadrona : Form
    {
        protected PictureBox pictureLogo;

        private Point logoSitioFijo;
        private bool logoSitioCalculado = false;

        public FormBaseMercadrona()
        {

            this.BackColor = Color.FromArgb(208, 225, 216);

            pictureLogo = new PictureBox
            {
                Size = new Size(60, 60),
                SizeMode = PictureBoxSizeMode.Zoom,
                BackColor = Color.Transparent,
            };
            try { pictureLogo.Image = Image.FromFile("iconomercadrona.png"); } catch { }
            Controls.Add(pictureLogo);

            this.Resize += (s, e) => ActualizarVisibilidadLogo();

            this.Load += (s, e) =>
            {

                logoSitioFijo = new Point(
                    this.ClientSize.Width - pictureLogo.Width - 10,
                    this.ClientSize.Height - pictureLogo.Height - 10
                );
                logoSitioCalculado = true;
                pictureLogo.Location = logoSitioFijo;   

                ActualizarVisibilidadLogo();
                AplicarEstiloBotones(this);
            };
        }

        private void ActualizarVisibilidadLogo()
        {
            if (pictureLogo == null || !logoSitioCalculado) return;

         
            bool cabeEntero =
                logoSitioFijo.X + pictureLogo.Width <= this.ClientSize.Width &&
                logoSitioFijo.Y + pictureLogo.Height <= this.ClientSize.Height;

            pictureLogo.Visible = cabeEntero;

            if (cabeEntero)
                pictureLogo.BringToFront(); 
        }

        private void AplicarEstiloBotones(Control contenedor)
        {
            foreach (Control c in contenedor.Controls)
            {
                if (c is Button btn)
                {
                    if (btn.Tag != null && btn.Tag.ToString() == "Ignorar")
                        continue;

                    btn.FlatStyle = FlatStyle.Flat;
                    btn.BackColor = Color.AliceBlue;
                }
                else if (c.HasChildren)
                {
                    AplicarEstiloBotones(c);
                }
            }
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            
            this.ClientSize = new System.Drawing.Size(284, 261);
            this.Name = "FormBaseMercadrona";
            this.Load += new System.EventHandler(this.FormBaseMercadrona_Load);
            this.ResumeLayout(false);

        }

        private void FormBaseMercadrona_Load(object sender, EventArgs e)
        {

        }
    }
}