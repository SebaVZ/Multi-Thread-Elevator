// Actualización completa: manejo de solicitudes optimizado, ComboBox para pisos, ocultamiento dinámico de botones, y asignación con identificación de ascensores
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
                var panelEdificio = new Panel { Dock = DockStyle.Fill, BackColor = Color.LightSteelBlue, Padding = new Padding(4) };
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

                    char letraAscensor = (char)('A' + j);
                    ascensor.Identificador = letraAscensor.ToString();

                    var caja = new Panel
                    {
                        Width = 100,
                        Height = 60,
                        BackColor = Color.DimGray,
                        Margin = new Padding(4),
                        BorderStyle = BorderStyle.FixedSingle
                    };

                    var estado = new Label { Dock = DockStyle.Bottom, Height = 30, ForeColor = Color.White, TextAlign = ContentAlignment.MiddleCenter };
                    caja.Controls.Add(estado);
                    ascensor.EstadoLabel = estado;

                    var labelId = new Label { Text = $"Asc. {letraAscensor}", ForeColor = Color.White, Dock = DockStyle.Top, Height = 15 };
                    var comboDestino = new ComboBox { Width = 90, DropDownStyle = ComboBoxStyle.DropDownList };
                    var botonAzotea = new Button { Text = "Azotea", Width = 90, Height = 22 };
                    var botonPB = new Button { Text = "Planta Baja", Width = 90, Height = 22 };
                    var botonAbrir = new Button { Text = "Abrir", Width = 90, Height = 22 };

                    comboDestino.SelectedIndexChanged += (s, e) =>
                    {
                        if (comboDestino.SelectedIndex >= 0 && !ascensor.EnMovimiento)
                        {
                            ascensor.AgregarSolicitud(new Solicitud
                            {
                                PisoDestino = comboDestino.SelectedIndex,
                                Tipo = TipoSolicitud.Normal
                            });
                        }
                    };

                    botonAzotea.Click += (s, e) =>
                    {
                        ascensor.AgregarSolicitud(new Solicitud { PisoDestino = CANTIDAD_PISOS - 1, Tipo = TipoSolicitud.Normal });
                    };
                    botonPB.Click += (s, e) =>
                    {
                        ascensor.AgregarSolicitud(new Solicitud { PisoDestino = 0, Tipo = TipoSolicitud.Normal });
                    };
                    botonAbrir.Click += (s, e) =>
                    {
                        if (!sistemaPausado && !ascensor.EnMovimiento)
                        {
                            botonAbrir.BackColor = Color.LightGreen;
                            MessageBox.Show($"Ascensor {ascensor.Id} abre puerta en piso {ascensor.PisoActual}");
                            Task.Delay(1000).ContinueWith(_ => Invoke(() => botonAbrir.BackColor = Color.LightGray));
                        }
                    };

                    caja.Controls.Add(labelId);
                    caja.Controls.Add(comboDestino);
                    caja.Controls.Add(botonAzotea);
                    caja.Controls.Add(botonPB);
                    caja.Controls.Add(botonAbrir);

                    cajasAscensores.Add(caja);

                    ascensor.ActualizarGUI = async () =>
                    {
                        await InvokeAsync(async () =>
                        {
                            botonAzotea.Visible = ascensor.PisoActual != CANTIDAD_PISOS - 1;
                            botonPB.Visible = ascensor.PisoActual != 0;

                            var pisos = ascensor.ObtenerPisosDisponibles();
                            comboDestino.BeginUpdate();
                            comboDestino.Items.Clear();
                            foreach (var piso in pisos)
                                comboDestino.Items.Add(piso.ToString());

                            comboDestino.EndUpdate();
                            if (comboDestino.Items.Count > 0)
                                comboDestino.SelectedIndex = 0;

                            ascensor.EstadoLabel.Text = ascensor.ObtenerEstadoActual();

                        });
                    };
                }

                for (int piso = 0; piso < CANTIDAD_PISOS; piso++)
                {
                    var pisoContainer = new Panel
                    {
                        BackColor = Color.White,
                        Dock = DockStyle.Fill,
                        Margin = new Padding(2),
                        BorderStyle = BorderStyle.FixedSingle
                    };

                    var filaAscensores = new FlowLayoutPanel
                    {
                        Dock = DockStyle.Fill,
                        FlowDirection = FlowDirection.LeftToRight,
                        BackColor = Color.Transparent,
                        Padding = new Padding(4)
                    };

                    for (int j = 0; j < ascensoresPorEdificio; j++)
                    {
                        if (piso == CANTIDAD_PISOS - 1)
                            filaAscensores.Controls.Add(cajasAscensores[j]);
                    }

                    pisoContainer.Controls.Add(filaAscensores);
                    layoutPisos.Controls.Add(pisoContainer, 0, piso);
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
                Dock = DockStyle.Left,
                Width = 200,
                AutoScroll = true,
                Padding = new Padding(10),
                BackColor = Color.LightSlateGray
            };

            for (int piso = 0; piso < CANTIDAD_PISOS; piso++)
            {
                var control = new PanelDeControl(piso)
                {
                    Width = 160,
                    Height = 60,
                    BackColor = Color.WhiteSmoke,
                    Margin = new Padding(4)
                };
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
            btnPausar = new Button
            {
                Text = "⏸ Pausar",
                Width = 120,
                Height = 30,
                Margin = new Padding(10),
                BackColor = Color.OrangeRed,
                ForeColor = Color.White
            };
            btnReanudar = new Button
            {
                Text = "▶ Reanudar",
                Width = 120,
                Height = 30,
                Margin = new Padding(10),
                BackColor = Color.SeaGreen,
                ForeColor = Color.White
            };

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
                Height = 50,
                BackColor = Color.WhiteSmoke,
                Padding = new Padding(10)
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