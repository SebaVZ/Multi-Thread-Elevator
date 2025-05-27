// Actualización: incluye panel interno por ascensor y botones de control global (pausar/reanudar)
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Multi_Thread_Elevator.Components;
using Multi_Thread_Elevator.Models;

namespace Multi_Thread_Elevator
{
    public partial class FormAscensores : Form
    {
        private readonly int cantidadEdificios;
        private readonly int ascensoresPorEdificio;
        private readonly List<Edificio> edificios = new();
        private readonly TableLayoutPanel layoutEdificios;
        private const int CANTIDAD_PISOS = 10;

        private bool sistemaPausado = false;
        private Button btnPausar;
        private Button btnReanudar;

        public FormAscensores(int cantidadEdificios, int ascensoresPorEdificio)
        {
            this.cantidadEdificios = cantidadEdificios;
            this.ascensoresPorEdificio = ascensoresPorEdificio;

            InitializeComponent();

            layoutEdificios = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = cantidadEdificios,
                RowCount = 1,
                BackColor = Color.LightGray,
                CellBorderStyle = TableLayoutPanelCellBorderStyle.Single
            };

            for (int i = 0; i < cantidadEdificios; i++)
                layoutEdificios.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f / cantidadEdificios));

            Controls.Add(layoutEdificios);

            InicializarSistema();
            AgregarControlesGlobales();
        }

        private void InicializarSistema()
        {
            for (int i = 0; i < cantidadEdificios; i++)
            {
                var edificio = new Edificio(i);
                var panelEdificio = new Panel { Dock = DockStyle.Fill, BackColor = Color.SteelBlue, Padding = new Padding(4) };
                var layoutPisos = new TableLayoutPanel
                {
                    Dock = DockStyle.Fill,
                    RowCount = CANTIDAD_PISOS,
                    ColumnCount = 1,
                    BackColor = Color.Transparent
                };

                for (int r = 0; r < CANTIDAD_PISOS; r++)
                    layoutPisos.RowStyles.Add(new RowStyle(SizeType.Percent, 100f / CANTIDAD_PISOS));

                layoutPisos.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

                var cajasAscensores = new List<Panel>();
                for (int j = 0; j < ascensoresPorEdificio; j++)
                {
                    var ascensor = new Ascensor(j);
                    edificio.Ascensores.Add(ascensor);

                    var caja = new Panel
                    {
                        Width = 60,
                        Height = 40,
                        BackColor = Color.DarkSlateGray,
                        Margin = new Padding(2)
                    };

                    var botonAzotea = new Button { Text = "A", Width = 20, Height = 20 };
                    var botonPB = new Button { Text = "P", Width = 20, Height = 20 };
                    var botonAbrir = new Button { Text = "O", Width = 20, Height = 20 };

                    botonAzotea.Click += (s, e) => ascensor.AgregarSolicitud(new Solicitud { PisoDestino = CANTIDAD_PISOS - 1, Tipo = TipoSolicitud.Normal });
                    botonPB.Click += (s, e) => ascensor.AgregarSolicitud(new Solicitud { PisoDestino = 0, Tipo = TipoSolicitud.Normal });
                    botonAbrir.Click += (s, e) =>
                    {
                        if (!sistemaPausado && ascensor.PisoActual >= 0)
                            MessageBox.Show($"Ascensor {ascensor.Id} abre puerta en piso {ascensor.PisoActual}");
                    };

                    caja.Controls.Add(botonAzotea);
                    caja.Controls.Add(botonPB);
                    caja.Controls.Add(botonAbrir);

                    cajasAscensores.Add(caja);
                }

                for (int piso = 0; piso < CANTIDAD_PISOS; piso++)
                {
                    var pisoContainer = new Panel
                    {
                        BackColor = Color.White,
                        Dock = DockStyle.Fill,
                        Margin = new Padding(2)
                    };

                    var filaAscensores = new FlowLayoutPanel
                    {
                        Dock = DockStyle.Fill,
                        FlowDirection = FlowDirection.LeftToRight,
                        BackColor = Color.Transparent
                    };

                    for (int j = 0; j < ascensoresPorEdificio; j++)
                    {
                        if (piso == CANTIDAD_PISOS - 1)
                            filaAscensores.Controls.Add(cajasAscensores[j]);
                    }

                    pisoContainer.Controls.Add(filaAscensores);
                    layoutPisos.Controls.Add(pisoContainer, 0, piso);
                }

                for (int j = 0; j < ascensoresPorEdificio; j++)
                {
                    var ascensor = edificio.Ascensores[j];
                    var caja = cajasAscensores[j];

                    ascensor.ActualizarGUI = async () =>
                    {
                        await InvokeAsync(async () =>
                        {
                            foreach (Control piso in layoutPisos.Controls)
                                if (piso is Panel contenedor)
                                    contenedor.Controls[0].Controls.Remove(caja);

                            int targetRow = CANTIDAD_PISOS - 1 - ascensor.PisoActual;
                            var panelObjetivo = layoutPisos.GetControlFromPosition(0, targetRow) as Panel;
                            if (panelObjetivo != null && panelObjetivo.Controls.Count > 0)
                            {
                                panelObjetivo.Controls[0].Controls.Add(caja);
                                for (int step = 0; step < 5; step++)
                                {
                                    caja.Top += 1;
                                    await Task.Delay(10);
                                }
                            }
                        });
                    };
                }

                panelEdificio.Controls.Add(layoutPisos);
                layoutEdificios.Controls.Add(panelEdificio, i, 0);
                edificios.Add(edificio);
            }

            CrearPanelesDeControl();
            IniciarSistema();
        }

        private void CrearPanelesDeControl()
        {
            var panel = new FlowLayoutPanel
            {
                Dock = DockStyle.Right,
                Width = 200,
                AutoScroll = true
            };

            for (int piso = 0; piso < CANTIDAD_PISOS; piso++)
            {
                var control = new PanelDeControl(piso);
                control.SolicitudGenerada += (destino, tipo) =>
                {
                    var ascensor = edificios
                        .SelectMany(e => e.Ascensores)
                        .OrderBy(_ => Guid.NewGuid())
                        .First();

                    ascensor.AgregarSolicitud(new Solicitud
                    {
                        PisoDestino = destino,
                        Tipo = tipo
                    });
                };
                panel.Controls.Add(control);
            }

            Controls.Add(panel);
        }

        private void AgregarControlesGlobales()
        {
            btnPausar = new Button { Text = "Pausar sistema", Width = 150 };
            btnReanudar = new Button { Text = "Reanudar sistema", Width = 150 };

            btnPausar.Click += (s, e) =>
            {
                PausarSistema();
                sistemaPausado = true;
            };

            btnReanudar.Click += (s, e) =>
            {
                IniciarSistema();
                sistemaPausado = false;
            };

            var topPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 40
            };
            topPanel.Controls.Add(btnPausar);
            topPanel.Controls.Add(btnReanudar);

            Controls.Add(topPanel);
        }

        private void IniciarSistema()
        {
            foreach (var ascensor in edificios.SelectMany(e => e.Ascensores))
                ascensor.Iniciar();
        }

        private void PausarSistema()
        {
            foreach (var ascensor in edificios.SelectMany(e => e.Ascensores))
                ascensor.Pausar();
        }

        private Task InvokeAsync(Action action)
        {
            var tcs = new TaskCompletionSource<object>();
            Invoke(new MethodInvoker(() =>
            {
                try
                {
                    action();
                    tcs.SetResult(null);
                }
                catch (Exception ex)
                {
                    tcs.SetException(ex);
                }
            }));
            return tcs.Task;
        }
    }
}
