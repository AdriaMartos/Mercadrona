using csDronLink;
using SimpleExample.csDronLink;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace SimpleExample
{
    public class Inicio : Form
    {
        private readonly FuncionesPedidos funciones;

        private Button btnEmpresa;
        private Button btnCliente;
        private Button btnCrearEscenario;
        private Button btnSeleccionarEscenario;
        private Escenario escenarioSeleccionado; 
        private PictureBox pictureLogo;

        private Escenario escenarioDronLab;
        private Escenario escenarioMercadrona;

        private List<Escenario> listaGlobalEscenarios;
        private Dictionary<string, Escenario> escenariosExtras = new Dictionary<string, Escenario>();

        private int CentroX(int ancho)
        {
            return (this.ClientSize.Width - ancho) / 2;
        }

        public Inicio(FuncionesPedidos funciones)
        {
            this.funciones = funciones;

            this.Text = "Inicio";
            this.ClientSize = new Size(600, 400);
            this.MinimumSize = new Size(600, 400);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            escenarioDronLab = EscenariosFactory.CrearDronLab();
            escenarioMercadrona = EscenariosFactory.CrearMercadrona();
            escenarioSeleccionado = null;
            listaGlobalEscenarios = new List<Escenario> { escenarioDronLab, escenarioMercadrona };

            InicializarControles();
        }

        private void InicializarControles()
        {
            this.BackColor = Color.FromArgb(208, 225, 216);
            btnSeleccionarEscenario = new Button
            {
                Text = "Escenario: Ninguno",
                Size = new Size(200, 35),
                Location = new Point(CentroX(200), 40),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.AliceBlue
            };
            btnSeleccionarEscenario.Click += BtnSeleccionarEscenario_Click;

            int ancho = 140;
            int separacion = 20;

            btnEmpresa = new Button
            {
                Text = "Empresa",
                Size = new Size(ancho, 45),
                Location = new Point(CentroX(ancho * 2 + separacion), 100),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.AliceBlue
            };
            btnEmpresa.Click += BtnEmpresa_Click;

            btnCliente = new Button
            {
                Text = "Cliente",
                Size = new Size(ancho, 45),
                Location = new Point(CentroX(ancho * 2 + separacion) + ancho + separacion, 100),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.AliceBlue
            };
            btnCliente.Click += BtnCliente_Click;

            btnCrearEscenario = new Button
            {
                Text = "Crear Escenario",
                Size = new Size(200, 45),
                Location = new Point(CentroX(200), 170),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.AliceBlue
            };
            btnCrearEscenario.Click += btnAbrirCreadorEscenarios_Click;

            Button btnInfo = new Button
            {
                Text = "i",
                Location = new Point(this.ClientSize.Width - 50, 10),
                Size = new Size(30, 30),
                Font = new Font("Arial", 12, FontStyle.Bold),
                BackColor = Color.AliceBlue,
                FlatStyle = FlatStyle.Flat,
            };
            btnInfo.Click += BtnInfo_Click;

            pictureLogo = new PictureBox
            {
                Size = new Size(80, 80),
                SizeMode = PictureBoxSizeMode.Zoom,
                BackColor = Color.Transparent,
                Image = Image.FromFile("iconomercadrona.png")
            };
            pictureLogo.Location = new Point(
                this.ClientSize.Width - pictureLogo.Width - 10,
                this.ClientSize.Height - pictureLogo.Height - 10
            );

            this.Resize += (s, e) =>
            {
                pictureLogo.Location = new Point(
                    this.ClientSize.Width - pictureLogo.Width - 10,
                    this.ClientSize.Height - pictureLogo.Height - 10
                );
            };

            Controls.Add(pictureLogo);
            Controls.Add(btnInfo);
            Controls.Add(btnSeleccionarEscenario);
            Controls.Add(btnEmpresa);
            Controls.Add(btnCliente);
            Controls.Add(btnCrearEscenario);
        }

        private void BtnInfo_Click(object sender, EventArgs e)
        {
            string mensaje = @"Buenas usuario, he aquí la guía de uso de la aplicación Mercadrona.

En este formulario inicial tiene 4 opciones principales.

Primeramente, “Crear Escenario”. Si clica, se le abrirá otra ventana donde podrá crear un escenario a su gusto, con su zona válida para drones, sus zonas prohibidas de paso y sus hosts de pedidos (puntos centrales de distribución en diferentes zonas).

También, en el formulario inicial, tiene un botón de seleccionar escenario, donde se le abrirá una ventana para previsualizar los diferentes escenarios disponibles y seleccionar o borrar el que desee.

Por otra parte, tiene el botón Cliente. Si clica en él, le saldrá una ventana con otros dos botones:

- Gestión de clientes: donde puede crear, borrar o editar los clientes que tiene.
- Crear pedido: donde, a partir de los clientes y productos existentes o packs de productos de diferentes pesos, y dependiendo del peso de los productos, puede crear pedidos. Además, en el mapa se mostrará la ruta que hará el dron hasta el lugar de entrega.

Por otra parte, en el formulario inicial, tiene el botón Empresa. Ahí se encuentra la gestión de productos y pedidos.

Si clica en él, verá 3 botones:

- Crear producto: donde puede crear un producto indicando su nombre, peso y precio.
- Pedidos nuevos: donde puede ver los pedidos realizados por los clientes y, en caso de querer hacer alguna modificación final, podrá borrar o crear nuevos productos.
- Envío de pedidos: al clicar aquí, se abrirá una ventana preguntando de cuántos drones dispone. Introduzca los drones que quiera utilizar y pulse Aceptar.

Una vez aceptado, verá una ventana con el escenario completo y todas las rutas que seguirán los drones hacia los pedidos.

Abajo encontrará varios botones:

- Conectar: conecta los drones.
- Ejecutar pedidos: los drones realizarán automáticamente los envíos siguiendo sus lógicas de entrega.
- Abortar todas las misiones: todos los drones dejarán sus tareas y regresarán a las bases.
- Ver historial: muestra una columna para cada dron donde se registra cada movimiento realizado.
- Detener dron X: aborta la misión del dron seleccionado."; 
            MessageBox.Show(mensaje, "Guía de uso - Mercadrona", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private Escenario ObtenerEscenarioSeleccionado()
        {
            return escenarioSeleccionado;
        }

        private void BtnEmpresa_Click(object sender, EventArgs e)
        {
            var escenario = ObtenerEscenarioSeleccionado();

            if (escenario == null)
            {
                MessageBox.Show("Por favor, selecciona un escenario antes de gestionar la Empresa.",
                                "Escenario requerido",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Warning);
                return;
            }

            var empresaForm = new EmpresaForm(funciones, escenario);
            empresaForm.Show();
        }

        private void BtnCliente_Click(object sender, EventArgs e)
        {
            var escenario = ObtenerEscenarioSeleccionado();

            if (escenario == null)
            {
                MessageBox.Show("Por favor, selecciona un escenario antes de gestionar los Clientes.",
                                "Escenario requerido",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Warning);
                return;
            }

            var clienteForm = new ClienteForm(funciones, escenario);
            clienteForm.Show();
        }

        private void btnAbrirCreadorEscenarios_Click(object sender, EventArgs e)
        {
            using (CrearEscenarioForm creador = new CrearEscenarioForm())
            {
                if (creador.ShowDialog() == DialogResult.OK)
                {
                    Escenario nuevo = creador.EscenarioCreado;
                    if (nuevo != null)
                    {
                        escenariosExtras[nuevo.Nombre] = nuevo;
                        listaGlobalEscenarios.Add(nuevo);

                        escenarioSeleccionado = nuevo;
                        btnSeleccionarEscenario.Text = "Escenario: " + nuevo.Nombre;
                    }
                }
            }
        }

        private void BtnSeleccionarEscenario_Click(object sender, EventArgs e)
        {
            using (SeleccionEscenarioForm form = new SeleccionEscenarioForm(listaGlobalEscenarios))
            {
                if (form.ShowDialog() == DialogResult.OK)
                {
                    escenarioSeleccionado = form.EscenarioSeleccionado;
                }

                if (escenarioSeleccionado != null && !listaGlobalEscenarios.Contains(escenarioSeleccionado))
                {
                    escenarioSeleccionado = null;
                    btnSeleccionarEscenario.Text = "Escenario: Ninguno";
                }
                else if (escenarioSeleccionado != null)
                {
                    btnSeleccionarEscenario.Text = "Escenario: " + escenarioSeleccionado.Nombre;
                }
                else
                {
                    btnSeleccionarEscenario.Text = "Escenario: Ninguno";
                }

                List<string> llavesAEliminar = new List<string>();
                foreach (var key in escenariosExtras.Keys)
                {
                    if (!listaGlobalEscenarios.Contains(escenariosExtras[key]))
                    {
                        llavesAEliminar.Add(key);
                    }
                }
                foreach (var key in llavesAEliminar)
                {
                    escenariosExtras.Remove(key);
                }
            }
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            this.ClientSize = new System.Drawing.Size(284, 261);
            this.Name = "Inicio";
            this.Load += new System.EventHandler(this.Inicio_Load);
            this.ResumeLayout(false);
        }

        private void Inicio_Load(object sender, EventArgs e)
        {
        }
    }
}