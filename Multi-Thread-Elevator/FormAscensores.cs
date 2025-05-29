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
        private TableLayoutPanel layoutPrincipal;

        public FormAscensores(int cantidadEdificios, int ascensoresPorEdificio)
        {
            this.cantidadEdificios = cantidadEdificios;
            this.ascensoresPorEdificio = ascensoresPorEdificio;

            Ascensor.InicializarSemaforos(cantidadEdificios);

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
                    ascensor.EdificioId = i;

                    char letraAscensor = (char)('A' + j);
                    ascensor.Identificador = letraAscensor.ToString();

                    var caja = new FlowLayoutPanel
                    {
                        Width = 100,
                        Height = 120,
                        BackColor = Color.DimGray,
                        Margin = new Padding(4),
                        BorderStyle = BorderStyle.FixedSingle,
                        FlowDirection = FlowDirection.TopDown,
                        WrapContents = false
                    };

                    var labelId = new Label { Text = $"Asc. {letraAscensor}", ForeColor = Color.White, Height = 15, AutoSize = true };
                    var comboDestino = new ComboBox { Width = 90, DropDownStyle = ComboBoxStyle.DropDownList };
                    var botonAzotea = new Button { Text = "Azotea", Width = 90, Height = 22 };
                    var botonPB = new Button { Text = "Planta Baja", Width = 90, Height = 22 };
                    var botonAbrir = new Button { Text = "Abrir", Width = 90, Height = 22 };
                    var labelPiso = new Label { Text = "Piso: 0", ForeColor = Color.Yellow, AutoSize = true };

                    int ultimoPisoSeleccionado = -1;

                    comboDestino.SelectedIndexChanged += (s, e) =>
                    {
                        if (comboDestino.DroppedDown) return; // Evita ejecutar si el usuario está interactuando

                        if (comboDestino.SelectedItem != null &&
                            int.TryParse(comboDestino.SelectedItem.ToString(), out int destino) &&
                            destino != ascensor.PisoActual &&
                            destino != ultimoPisoSeleccionado)
                        {
                            ultimoPisoSeleccionado = destino;
                            ascensor.SolicitarIrAPiso(destino);
                        }
                    };

                    botonAzotea.Click += (s, e) => ascensor.SolicitarIrAPiso(CANTIDAD_PISOS - 1);
                    botonPB.Click += (s, e) => ascensor.SolicitarIrAPiso(0);

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
                    caja.Controls.Add(labelPiso);
                    caja.Controls.Add(comboDestino);
                    caja.Controls.Add(botonAzotea);
                    caja.Controls.Add(botonPB);
                    caja.Controls.Add(botonAbrir);

                    cajasAscensores.Add(caja);

                    ascensor.ActualizarGUI = async () =>
                    {
                        await InvokeAsync(() =>
                        {
                            botonAzotea.Visible = ascensor.PisoActual != CANTIDAD_PISOS - 1;
                            botonPB.Visible = ascensor.PisoActual != 0;
                            labelPiso.Text = $"Piso: {ascensor.PisoActual}";

                            var pisos = Enumerable.Range(0, CANTIDAD_PISOS).ToList();

                            comboDestino.BeginUpdate();
                            var seleccionActual = comboDestino.SelectedItem?.ToString();
                            comboDestino.Items.Clear();
                            foreach (var piso in pisos)
                                comboDestino.Items.Add(piso.ToString());

                            if (seleccionActual != null && comboDestino.Items.Contains(seleccionActual))
                                comboDestino.SelectedItem = seleccionActual;
                            else if (comboDestino.Items.Count > 0)
                                comboDestino.SelectedItem = comboDestino.Items[0];
                            comboDestino.EndUpdate();

                            if (comboDestino.Items.Count > 0)
                                comboDestino.SelectedIndex = 0;

                            // Mover visualmente el ascensor al nuevo piso
                            for (int fila = 0; fila < layoutPisos.RowCount; fila++)
                            {
                                var panelPiso = layoutPisos.GetControlFromPosition(0, fila) as Panel;
                                if (panelPiso?.Controls.Count > 0)
                                {
                                    var contenedor = panelPiso.Controls[0];
                                    if (contenedor.Controls.Contains(caja))
                                    {
                                        contenedor.Controls.Remove(caja);
                                        break;
                                    }
                                }
                            }

                            int targetRow = CANTIDAD_PISOS - 1 - ascensor.PisoActual;
                            var panelObjetivo = layoutPisos.GetControlFromPosition(0, targetRow) as Panel;
                            if (panelObjetivo?.Controls.Count > 0)
                            {
                                var contenedor = panelObjetivo.Controls[0];
                                contenedor.Controls.Add(caja);
                            }
                        });
                    };

                    ascensor.Iniciar();
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
            var panelNumerosPiso = new TableLayoutPanel
            {
                Dock = DockStyle.Left, // cambia a DockStyle.Right si los quieres al otro lado
                Width = 50,
                RowCount = CANTIDAD_PISOS,
                ColumnCount = 1,
                BackColor = Color.WhiteSmoke,
                CellBorderStyle = TableLayoutPanelCellBorderStyle.None,
                Padding = new Padding(5)
            };

            for (int r = 0; r < CANTIDAD_PISOS; r++)
            {
                panelNumerosPiso.RowStyles.Add(new RowStyle(SizeType.Percent, 100f / CANTIDAD_PISOS));
                var label = new Label
                {
                    Text = $"Piso {CANTIDAD_PISOS - 1 - r}", // para que el 9 quede arriba y el 0 abajo
                    Dock = DockStyle.Fill,
                    TextAlign = ContentAlignment.MiddleCenter,
                    Font = new Font("Segoe UI", 9, FontStyle.Bold)
                };
                panelNumerosPiso.Controls.Add(label, 0, r);
            }

            Controls.Add(panelNumerosPiso);

            CrearPanelesDeControl();
        }

        private void CrearPanelesDeControl()
        {
            var panel = new FlowLayoutPanel
            {
                Dock = DockStyle.Left,
                Width = 220,
                AutoScroll = true,
                Padding = new Padding(10),
                BackColor = Color.LightSlateGray
            };

            var panelUniversal = new PanelDeControlUniversal(cantidadEdificios, ascensoresPorEdificio, CANTIDAD_PISOS);
            panelUniversal.SolicitudUniversalGenerada += (edificioIdx, ascensorIdx, pisoOrigen, solicitud) =>
            {
                var ascensor = edificios[edificioIdx].Ascensores[ascensorIdx];
                ascensor.AgregarSolicitud(solicitud);
                MessageBox.Show($"Solicitud enviada desde el piso {pisoOrigen} al {solicitud.PisoDestino}, tipo {solicitud.Tipo}");
            };

            panel.Controls.Add(panelUniversal);
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
            var velocidadLabel = new Label
            {
                Text = "Velocidad:",
                Width = 70,
                TextAlign = ContentAlignment.MiddleLeft,
                Margin = new Padding(10, 10, 0, 0)
            };

            var velocidadSlider = new TrackBar
            {
                Minimum = 100,
                Maximum = 2000,
                TickFrequency = 100,
                Value = Ascensor.VelocidadMovimientoMs,
                Width = 200,
                SmallChange = 100,
                LargeChange = 200,
                Margin = new Padding(0, 10, 0, 0)
            };

            var valorVelocidad = new Label
            {
                Text = $"{Ascensor.VelocidadMovimientoMs} ms",
                Width = 60,
                TextAlign = ContentAlignment.MiddleLeft,
                Margin = new Padding(10, 10, 0, 0)
            };

            velocidadSlider.Scroll += (s, e) =>
            {
                Ascensor.VelocidadMovimientoMs = velocidadSlider.Value;
                valorVelocidad.Text = $"{velocidadSlider.Value} ms";
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
            topPanel.Controls.Add(velocidadLabel);
            topPanel.Controls.Add(velocidadSlider);
            topPanel.Controls.Add(valorVelocidad);

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