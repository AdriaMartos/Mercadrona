using SimpleExample.csDronLink;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace SimpleExample
{
    public class PedidosNuevos : FormBaseMercadrona
    {
        private readonly FuncionesPedidos funciones;
        private readonly Escenario escenario;
        private ListBox listPedidos;
        private Button btnBorrar, btnAñadir;
        private List<Pedido> listaPedidos;
        private Pedido pedidoSeleccionado;

        public PedidosNuevos(FuncionesPedidos funciones, Escenario escenario)
        {
            this.funciones = funciones;
            this.escenario = escenario;
            Text = $"Pedidos Nuevos - {escenario.Nombre}";
            Size = new Size(600, 600);
            StartPosition = FormStartPosition.CenterScreen;

            InicializarControles();
            CargarPedidos();
        }

        private void InicializarControles()
        {
            listPedidos = new ListBox
            {
                Location = new Point(20, 20),
                Size = new Size(540, 350),
                Font = new Font("Segoe UI", 11)
            };
            listPedidos.SelectedIndexChanged += (s, e) =>
            {
                pedidoSeleccionado = listPedidos.SelectedIndex == -1 ? null : listaPedidos[listPedidos.SelectedIndex];
                btnBorrar.Enabled = pedidoSeleccionado != null;
            };
            Controls.Add(listPedidos);
            btnBorrar = new Button
            {
                Text = "Borrar Pedido",
                Location = new Point(20, 390),
                Width = 150,
                Enabled = false
            };
            btnBorrar.Click += BtnBorrar_Click;
            Controls.Add(btnBorrar);
            btnAñadir = new Button
            {
                Text = "Añadir Pedido",
                Location = new Point(200, 390),
                Width = 150
            };
            btnAñadir.Click += BtnAñadir_Click;
            Controls.Add(btnAñadir);
        }

        private void CargarPedidos()
        {
            listaPedidos = funciones.ObtenerPedidos();
            listPedidos.Items.Clear();
            if (listaPedidos.Count == 0)
            {
                listPedidos.Items.Add("No hay pedidos pendientes");
                return;
            }

            foreach (var p in listaPedidos)
                listPedidos.Items.Add($"{p.GetDestinatario()} - {p.GetPesoTotal():F2}kg - {p.GetPrecioTotal():F2}€");
        }

        private void BtnBorrar_Click(object sender, EventArgs e)
        {
            if (pedidoSeleccionado == null) return;

            var resultado = MessageBox.Show(
                $"¿Desea borrar el pedido de {pedidoSeleccionado.GetDestinatario()}?",
                "Confirmar borrado",
                MessageBoxButtons.YesNo
            );

            if (resultado == DialogResult.Yes)
            {
                listaPedidos.Remove(pedidoSeleccionado);
                funciones.SetPedidos(listaPedidos);
                CargarPedidos();
            }
        }
        private void BtnAñadir_Click(object sender, EventArgs e)
        {
            using (var formCrear = new PedidosForm(funciones, escenario))
            {
                formCrear.ShowDialog();
            }

            CargarPedidos();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            this.ClientSize = new System.Drawing.Size(284, 261);
            this.Name = "PedidosNuevos";
            this.Load += new System.EventHandler(this.PedidosNuevos_Load);
            this.ResumeLayout(false);
        }

        private void PedidosNuevos_Load(object sender, EventArgs e) { }
    }
}