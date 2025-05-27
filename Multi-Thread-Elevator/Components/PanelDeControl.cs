using System;
using System.Drawing;
using System.Windows.Forms;
using Multi_Thread_Elevator.Models;

namespace Multi_Thread_Elevator.Components
{
    public class PanelDeControl : GroupBox
    {
        public event Action<int, TipoSolicitud> SolicitudGenerada;

        public PanelDeControl(int piso)
        {
            Text = $"Panel Piso {piso}";
            Width = 150;
            Height = 100;

            var btnNormal = new Button { Text = "Normal", Width = 60, Height = 25, Location = new Point(10, 20) };
            var btnEspecial = new Button { Text = "Especial", Width = 60, Height = 25, Location = new Point(80, 20) };

            btnNormal.Click += (s, e) => SolicitudGenerada?.Invoke(piso, TipoSolicitud.Normal);
            btnEspecial.Click += (s, e) => SolicitudGenerada?.Invoke(piso, TipoSolicitud.Especial);

            Controls.Add(btnNormal);
            Controls.Add(btnEspecial);
        }
    }
}
