using System;
using System.Windows.Forms;

namespace Multi_Thread_Elevator
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            var configuracion = new FormConfiguracion();
            if (configuracion.ShowDialog() == DialogResult.OK)
            {
                Application.Run(new FormAscensores(configuracion.CantidadEdificios, configuracion.AscensoresPorEdificio, configuracion.CantidadPisos));
            }
        }
    }
}
