using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Multi_Thread_Elevator.Models;

namespace Multi_Thread_Elevator.Components
{
    public class PanelDeControlUniversal : GroupBox
    {
        public event Action<int, int, int, Solicitud> SolicitudUniversalGenerada;

        public PanelDeControlUniversal(int cantidadEdificios, int ascensoresPorEdificio, int cantidadPisos)
        {
            Text = "Panel de Control Universal";
            Width = 200;
            Height = 200;

            var lblEdificio = new Label { Text = "Edificio:", Location = new Point(10, 20), Width = 80 };
            var cboEdificio = new ComboBox { Location = new Point(100, 18), Width = 80 };

            var lblAscensor = new Label { Text = "Ascensor:", Location = new Point(10, 50), Width = 80 };
            var cboAscensor = new ComboBox { Location = new Point(100, 48), Width = 80 };

            var lblHasta = new Label { Text = "Piso:", Location = new Point(10, 80), Width = 80 };
            var cboHasta = new ComboBox { Location = new Point(100, 78), Width = 80 };

            var lblTipo = new Label { Text = "Tipo:", Location = new Point(10, 110), Width = 80 };
            var cboTipo = new ComboBox { Location = new Point(100, 108), Width = 80 };

            var btnEnviar = new Button { Text = "Enviar", Location = new Point(60, 150), Width = 80 };

            for (int i = 0; i < cantidadEdificios; i++) cboEdificio.Items.Add(i);
            cboEdificio.SelectedIndex = 0;

            for (int i = 0; i < ascensoresPorEdificio; i++) cboAscensor.Items.Add(i);
            cboAscensor.SelectedIndex = 0;

            for (int i = 0; i < cantidadPisos; i++) cboHasta.Items.Add(i);
            cboHasta.SelectedIndex = 1;

            cboTipo.Items.AddRange(Enum.GetNames(typeof(TipoSolicitud)));
            cboTipo.SelectedIndex = 0;

            btnEnviar.Click += (s, e) =>
            {
                if (int.TryParse(cboHasta.SelectedItem?.ToString(), out int hasta) &&
                    Enum.TryParse(cboTipo.SelectedItem?.ToString(), out TipoSolicitud tipo))
                {
                    SolicitudUniversalGenerada?.Invoke(
                        cboEdificio.SelectedIndex,
                        cboAscensor.SelectedIndex,
                        -1,
                        new Solicitud { PisoDestino = hasta, Tipo = tipo }
                    );
                }
            };

            Controls.AddRange(new Control[]
            {
                lblEdificio, cboEdificio,
                lblAscensor, cboAscensor,
                lblHasta, cboHasta,
                lblTipo, cboTipo,
                btnEnviar
            });
        }
    }
}
