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
            Height = 220;

            var lblEdificio = new Label { Text = "Edificio:", Location = new Point(10, 20), Width = 80 };
            var cboEdificio = new ComboBox { Location = new Point(100, 18), Width = 80 };
            for (int i = 0; i < cantidadEdificios; i++) cboEdificio.Items.Add(i);
            cboEdificio.SelectedIndex = 0;

            var lblAscensor = new Label { Text = "Ascensor:", Location = new Point(10, 50), Width = 80 };
            var cboAscensor = new ComboBox { Location = new Point(100, 48), Width = 80 };
            for (int i = 0; i < ascensoresPorEdificio; i++) cboAscensor.Items.Add(i);
            cboAscensor.SelectedIndex = 0;

            var lblDesde = new Label { Text = "Desde Piso:", Location = new Point(10, 80), Width = 80 };
            var cboDesde = new ComboBox { Location = new Point(100, 78), Width = 80 };
            for (int i = 0; i < cantidadPisos; i++) cboDesde.Items.Add(i);
            cboDesde.SelectedIndex = 0;

            var lblHasta = new Label { Text = "Hasta Piso:", Location = new Point(10, 110), Width = 80 };
            var cboHasta = new ComboBox { Location = new Point(100, 108), Width = 80 };
            for (int i = 0; i < cantidadPisos; i++) cboHasta.Items.Add(i);
            cboHasta.SelectedIndex = 1;

            var lblTipo = new Label { Text = "Tipo:", Location = new Point(10, 140), Width = 80 };
            var cboTipo = new ComboBox { Location = new Point(100, 138), Width = 80 };
            cboTipo.Items.AddRange(Enum.GetNames(typeof(TipoSolicitud)));
            cboTipo.SelectedIndex = 0;

            var btnEnviar = new Button { Text = "Enviar", Location = new Point(60, 170), Width = 80 };
            btnEnviar.Click += (s, e) =>
            {
                if (int.TryParse(cboDesde.SelectedItem?.ToString(), out int desde) &&
                    int.TryParse(cboHasta.SelectedItem?.ToString(), out int hasta) &&
                    Enum.TryParse(cboTipo.SelectedItem?.ToString(), out TipoSolicitud tipo))
                {
                    if (desde == hasta) return; // Omitir solicitudes inválidas

                    SolicitudUniversalGenerada?.Invoke(
                        cboEdificio.SelectedIndex,
                        cboAscensor.SelectedIndex,
                        desde,
                        new Solicitud { PisoDestino = hasta, Tipo = tipo }
                    );
                }
            };


            Controls.AddRange(new Control[] { lblEdificio, cboEdificio, lblAscensor, cboAscensor,
                lblDesde, cboDesde, lblHasta, cboHasta, lblTipo, cboTipo, btnEnviar });
        }
    }
}
