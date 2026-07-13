using SimpleExample.csDronLink;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace SimpleExample
{
    public class EmpresaForm : FormBaseMercadrona
    {
        private readonly FuncionesPedidos funciones;
        private readonly Escenario _escenario;

        private Button btnCrearProducto;
        private Button btnPedidosNuevos;
        private Button btnControlEnvios;

        public EmpresaForm(FuncionesPedidos funciones, Escenario escenario)
        {
            this.funciones = funciones;
            this._escenario = escenario;

            Text = "Empresa - " + escenario.Nombre;
            ClientSize = new Size(400, 300);
            StartPosition = FormStartPosition.CenterScreen;

            InicializarControles();
        }

        private void InicializarControles()
        {

            btnCrearProducto = new Button
            {
                Text = "Crear Producto",
                Location = new Point(75, 30),
                Width = 250
            };
            btnCrearProducto.Click += (s, e) =>
            {
                new CrearProducto(funciones, _escenario).ShowDialog(this);
            };
            Controls.Add(btnCrearProducto);

            btnPedidosNuevos = new Button
            {
                Text = "Ver Pedidos Nuevos",
                Location = new Point(75, 80),
                Width = 250
            };
            btnPedidosNuevos.Click += (s, e) =>
            {
                new PedidosNuevos(funciones, _escenario).ShowDialog(this);
                
            };
            Controls.Add(btnPedidosNuevos);

            btnControlEnvios = new Button
            {
                Text = "Envío de Pedidos",
                Location = new Point(75, 130),
                Width = 250
            };
            btnControlEnvios.Click += (s, e) =>
            {
                using (var dialogo = new CantidadDrones())
                {
                    if (dialogo.ShowDialog() != DialogResult.OK) return;

                    using (var formCapacidad = new ConfiguracionCapacidadDrones(dialogo.DronesDisponibles))
                    {
                        if (formCapacidad.ShowDialog() != DialogResult.OK) return;

                        GestorDron.Instancia.InicializarDrones(dialogo.DronesDisponibles);        // 1º crear
                        GestorDron.Instancia.ConfigurarCapacidades(formCapacidad.Capacidades);    // 2º configurar

                        new EnviarPedidoForm(
                            funciones.ObtenerPedidosPendientes(),
                            funciones,
                            _escenario
                        ).Show();
                    }
                }
            };
            Controls.Add(btnControlEnvios);
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
           
            this.ClientSize = new System.Drawing.Size(284, 261);
            this.Name = "EmpresaForm";
            this.Load += new System.EventHandler(this.EmpresaForm_Load);
            this.ResumeLayout(false);

        }

        private void EmpresaForm_Load(object sender, EventArgs e)
        {

        }
    }
}