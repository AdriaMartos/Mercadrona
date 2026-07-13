using csDronLink;
using SimpleExample.csDronLink;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace SimpleExample
{
    public class ClienteForm : FormBaseMercadrona
    {
        private readonly FuncionesPedidos funciones;
        private readonly Escenario _escenario; 

        private Button btnCrearPedido;
        private Button btnRegistrarCliente;

        public ClienteForm(FuncionesPedidos funciones, Escenario escenario)
        {
            this.funciones = funciones;
            this._escenario = escenario;

            this.Text = "Cliente - " + escenario.Nombre;
            this.ClientSize = new Size(400, 250);
            this.StartPosition = FormStartPosition.CenterScreen;

            InicializarControles();
        }

        private void InicializarControles()
        {

            btnCrearPedido = new Button
            {
                Text = "Crear Pedido",
                Location = new Point(120, 40),
                Size = new Size(150, 40)
            };
            btnCrearPedido.Click += BtnCrearPedido_Click;
            Controls.Add(btnCrearPedido);

            btnRegistrarCliente = new Button
            {
                Text = "Gestion de Clientes",
                Location = new Point(120, 100),
                Size = new Size(150, 40)
            };
            btnRegistrarCliente.Click += BtnRegistrarCliente_Click;
            Controls.Add(btnRegistrarCliente);
        }

        private void BtnCrearPedido_Click(object sender, EventArgs e)
        {


            PedidosForm pedidosForm = new PedidosForm(funciones, _escenario);
            pedidosForm.StartPosition = FormStartPosition.CenterParent;
            pedidosForm.ShowDialog();
        }

        private void BtnRegistrarCliente_Click(object sender, EventArgs e)
        {
            GestionarClientes registrarForm = new GestionarClientes(_escenario); 
            registrarForm.StartPosition = FormStartPosition.CenterParent;
            registrarForm.ShowDialog();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
          
            this.ClientSize = new System.Drawing.Size(284, 261);
            this.Name = "ClienteForm";
            this.Load += new System.EventHandler(this.ClienteForm_Load);
            this.ResumeLayout(false);

        }

        private void ClienteForm_Load(object sender, EventArgs e)
        {

        }
    }

}
