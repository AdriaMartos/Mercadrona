using csDronLink;
using SimpleExample.csDronLink;
using System;
using System.Windows.Forms;

namespace SimpleExample
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            FuncionesPedidos funciones = new FuncionesPedidos();
            Application.Run(new Inicio(funciones)); 
        }
    }
}