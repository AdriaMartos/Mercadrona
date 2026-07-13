using csDronLink;
using SimpleExample.csDronLink;
using System;
using System.Linq;
using System.Windows.Forms;

namespace SimpleExample
{
    internal class EditorProductos : Form
    {
        private FuncionesPedidos funciones;
        private DataGridView dgvProductos;

        public EditorProductos(FuncionesPedidos funciones)
        {
            this.funciones = funciones;
            InitializeComponent();
            CargarProductos();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            
            this.ClientSize = new System.Drawing.Size(284, 261);
            this.Name = "EditorProductos";
            this.Load += new System.EventHandler(this.EditorProductos_Load_1);
            this.ResumeLayout(false);

        }

        private void EditorProductos_Load(object sender, EventArgs e)
        {
         
            CargarProductos();
        }

        private void CargarProductos()
        {
            dgvProductos.DataSource = null;
            dgvProductos.DataSource = funciones.ObtenerProductos().ToList(); 
        }

        private void DgvProductos_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            var row = dgvProductos.Rows[e.RowIndex];
            var producto = row.DataBoundItem as Producto;
            if (producto == null) return;

            if (e.ColumnIndex == dgvProductos.Columns["Nombre"].Index)
            {
                string nuevoNombre = row.Cells[e.ColumnIndex].Value?.ToString();
                if (string.IsNullOrWhiteSpace(nuevoNombre))
                {
                    MessageBox.Show("El nombre no puede estar vacío.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    row.Cells[e.ColumnIndex].Value = producto.Nombre;
                }
                else
                {
                    producto.Nombre = nuevoNombre;
                }
            }
            else if (e.ColumnIndex == dgvProductos.Columns["Peso"].Index)
            {
                string valor = row.Cells[e.ColumnIndex].Value?.ToString();
                if (double.TryParse(valor, out double peso) && peso > 0)
                {
                    producto.Peso = peso;
                }
                else
                {
                    MessageBox.Show("Introduzca un peso válido.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    row.Cells[e.ColumnIndex].Value = producto.Peso;
                }
            }
            else if (e.ColumnIndex == dgvProductos.Columns["Precio"].Index)
            {
                string valor = row.Cells[e.ColumnIndex].Value?.ToString();
                if (decimal.TryParse(valor, out decimal precio) && precio >= 0)
                {
                    producto.Precio = (double)precio;
                }
                else
                {
                    MessageBox.Show("Introduzca un precio válido.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    row.Cells[e.ColumnIndex].Value = producto.Precio;
                }
            }
        }

        private void EditorProductos_Load_1(object sender, EventArgs e)
        {

        }
    }
}