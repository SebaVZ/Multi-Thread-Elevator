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
        private const int CANTIDAD_PISOS = 8;

        private bool sistemaPausado = false;
        private Button btnPausar;
        private Button btnReanudar;
        private TableLayoutPanel layoutPrincipal;
        private FlowLayoutPanel panelSolicitudes;

        internal Dictionary<(int edificioId, int ascensorId), FlowLayoutPanel> cajasAscensor = new();

        public FormAscensores(int cantidadEdificios, int ascensoresPorEdificio)
        {
            InitializeComponent();
            this.WindowState = FormWindowState.Maximized;
            this.cantidadEdificios = cantidadEdificios;
            this.ascensoresPorEdificio = ascensoresPorEdificio;
            Ascensor.InicializarSemaforos(cantidadEdificios);

            // 2) Creo el scrollPanel y lo añado de fondo:
            var scrollPanel = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                BackColor = Color.LightGray
            };
            Controls.Add(scrollPanel);

            // 3) Dentro, el layout que crece horizontalmente:
            layoutEdificios = new TableLayoutPanel
            {
                //AutoSize = true,
                //AutoSizeMode = AutoSizeMode.GrowAndShrink,
                Dock = DockStyle.Fill,        // <-- NO Fill
                ColumnCount = cantidadEdificios,
                RowCount = 1,
                CellBorderStyle = TableLayoutPanelCellBorderStyle.Single,
                BackColor = Color.Transparent
            };
            scrollPanel.Controls.Add(layoutEdificios);

            // 4) Columnas fijas de 300px
            for (int i = 0; i < cantidadEdificios; i++)
                layoutEdificios.ColumnStyles.Add(
                    new ColumnStyle(SizeType.Absolute, 300f)
                );
            AgregarControlesGlobales();
            // 5) Finalmente cargo los edificios
            InicializarSistema();
            panelSolicitudes = new FlowLayoutPanel
            {
                Dock = DockStyle.Right,
                Width = 250,
                AutoScroll = true,
                Padding = new Padding(10),
                BackColor = Color.WhiteSmoke,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false
            };

            Controls.Add(panelSolicitudes);

        }


        /*public FormAscensores(int cantidadEdificios, int ascensoresPorEdificio)
        {
            InitializeComponent();
            this.WindowState = FormWindowState.Maximized;
            this.cantidadEdificios = cantidadEdificios;
            this.ascensoresPorEdificio = ascensoresPorEdificio;
            Ascensor.InicializarSemaforos(cantidadEdificios);

            // 1) Creamos y añadimos primero el scrollPanel (queda detrás de todo)
            var scrollPanel = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                BackColor = Color.LightGray
            };
            Controls.Add(scrollPanel);

            // 2) Dentro, la tabla de edificios
            layoutEdificios = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = cantidadEdificios,
                RowCount = 1,
                CellBorderStyle = TableLayoutPanelCellBorderStyle.Single,
                BackColor = Color.Transparent
            };
            scrollPanel.Controls.Add(layoutEdificios);

            for (int i = 0; i < cantidadEdificios; i++)
                layoutEdificios.ColumnStyles.Add(
                    new ColumnStyle(SizeType.Absolute, 300f)
                );

            // 3) Ahora sí agregamos los controles fijos **por encima** del scrollPanel:
            AgregarControlesGlobales();    // topPanel.Dock = Top
            //CrearPanelesDeControl();       // panelUniversal.Dock = Left

            // 4) Por último, llenamos los edificios en la tabla
            InicializarSistema();
        }
        */
        private void InicializarSistema()
        {
            for (int i = 0; i < cantidadEdificios; i++)
            {
                var edificio = new Edificio(i);
                var panelEdificio = new Panel
                {
                    Width = 300,
                    Dock = DockStyle.Fill,
                    Anchor = AnchorStyles.Top | AnchorStyles.Bottom,
                    BackColor = Color.LightSteelBlue,
                    Padding = new Padding(4),
                    Margin = new Padding(8)
                };


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

                var cajasAscensores = new List<Control>();

                for (int j = 0; j < ascensoresPorEdificio; j++)
                {
                    var ascensor = new Ascensor(j)
                    {
                        EdificioId = i,
                        Identificador = j.ToString()
                    };
                    edificio.Ascensores.Add(ascensor);

                    // Caja interna redimensionada
                    var caja = new FlowLayoutPanel
                    {
                        Width = 80,
                        Height = 80,
                        BackColor = Color.FromArgb(40, 40, 40),
                        Margin = new Padding(3),
                        BorderStyle = BorderStyle.FixedSingle,
                        FlowDirection = FlowDirection.TopDown,
                        WrapContents = false
                    };
                    cajasAscensor[(i, j)] = caja;
                    // Contenedor (GroupBox)
                    var contenedor = new GroupBox
                    {
                        Text = $"Asc. {j}",
                        Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                        ForeColor = Color.White,
                        BackColor = Color.FromArgb(30, 30, 30),
                        Padding = new Padding(4),
                        Margin = new Padding(4),
                        AutoSize = true,
                        Dock = DockStyle.Bottom
                    };
                    contenedor.Controls.Add(caja);

                    // Label ID y ComboBox
                    var labelId = new Label
                    {
                        Text = $"Asc. {j}",
                        ForeColor = Color.White,
                        Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                        AutoSize = true,
                        Margin = new Padding(0, 0, 0, 2)
                    };
                    var comboDestino = new ComboBox
                    {
                        Width = 80,
                        DropDownStyle = ComboBoxStyle.DropDownList,
                        Font = new Font("Segoe UI", 9f),
                        Margin = new Padding(0, 0, 0, 2),
                        FlatStyle = FlatStyle.Popup
                    };
                    for (int piso = 0; piso < CANTIDAD_PISOS; piso++)
                        comboDestino.Items.Add(piso.ToString());
                    int ultimoPisoSeleccionado = -1;
                    comboDestino.SelectedIndexChanged += (s, e) =>
                    {
                        if (comboDestino.DroppedDown) return;
                        if (comboDestino.SelectedItem != null &&
                            int.TryParse(comboDestino.SelectedItem.ToString(), out int dest) &&
                            dest != ascensor.PisoActual && dest != ultimoPisoSeleccionado)
                        {
                            ultimoPisoSeleccionado = dest;
                            ascensor.SolicitarIrAPiso(dest);
                        }
                    };

                    // Botones con tamaño reducido
                    var btnSubir = new Button
                    {
                        Text = "↑",
                        Font = new Font("Segoe UI", 12f, FontStyle.Bold),
                        Width = 30,
                        Height = 24,
                        FlatStyle = FlatStyle.Flat,
                        BackColor = Color.FromArgb(50, 50, 50),
                        ForeColor = Color.White,
                        Margin = new Padding(1),
                        Visible = true
                    };
                    btnSubir.FlatAppearance.BorderSize = 0;
                    btnSubir.Click += (s, e) => ascensor.SolicitarIrAPiso(CANTIDAD_PISOS - 1);

                    var btnBajar = new Button
                    {
                        Text = "↓",
                        Font = new Font("Segoe UI", 12f, FontStyle.Bold),
                        Width = 30,
                        Height = 24,
                        FlatStyle = FlatStyle.Flat,
                        BackColor = Color.FromArgb(50, 50, 50),
                        ForeColor = Color.White,
                        Margin = new Padding(1),
                        Visible = false
                    };
                    btnBajar.FlatAppearance.BorderSize = 0;
                    btnBajar.Click += (s, e) => ascensor.SolicitarIrAPiso(0);

                   
                    // Label de solicitudes con fuente pequeña
                    var labelSolicitudes = new Label
                    {
                        Text = "P: -",
                        ForeColor = Color.LightGray,
                        Font = new Font("Segoe UI", 8f, FontStyle.Italic),
                        AutoSize = true,
                        Margin = new Padding(0, 2, 0, 0)
                    };

                    // Ensamblar controles
                    caja.Controls.Add(labelId);
                    caja.Controls.Add(comboDestino);
                    var panelControles = new FlowLayoutPanel
                    {
                        FlowDirection = FlowDirection.LeftToRight,
                        AutoSize = true,
                        WrapContents = false,
                        Margin = new Padding(0)
                    };
                    panelControles.Controls.Add(btnSubir);
                    panelControles.Controls.Add(btnBajar);
                    caja.Controls.Add(panelControles);
                    caja.Controls.Add(labelSolicitudes);

                    cajasAscensores.Add(contenedor);

                    ascensor.ActualizarGUI = async () =>
                    {
                        await InvokeAsync(() =>
                        {
                            btnSubir.Visible = ascensor.PisoActual < CANTIDAD_PISOS - 1 && !ascensor.PuertaAbierta;
                            btnBajar.Visible = ascensor.PisoActual > 0 && !ascensor.PuertaAbierta;
                            comboDestino.Enabled = !ascensor.PuertaAbierta;

                            var pendientes = ascensor.SolicitudesPendientes.Select(s => s.PisoDestino.ToString()).ToArray();
                            labelSolicitudes.Text = pendientes.Length > 0
                                ? $"P: {string.Join(",", pendientes)}"
                                : "P: -";

                            // Reposicionar grupo
                            for (int fila = 0; fila < layoutPisos.RowCount; fila++)
                            {
                                var panelPiso = layoutPisos.GetControlFromPosition(0, fila) as Panel;
                                if (panelPiso?.Controls.Count > 0)
                                {
                                    var child = panelPiso.Controls[0];
                                    if (child.Controls.Contains(contenedor))
                                    {
                                        child.Controls.Remove(contenedor);
                                        break;
                                    }
                                }
                            }
                            int targetRow = CANTIDAD_PISOS - 1 - ascensor.PisoActual;
                            var destinoPanel = layoutPisos.GetControlFromPosition(0, targetRow) as Panel;
                            destinoPanel?.Controls[0].Controls.Add(contenedor);
                            ActualizarPanelSolicitudes();
                        });
                    };

                    ascensor.Iniciar();
                }

                for (int piso = 0; piso < CANTIDAD_PISOS; piso++)
                {

                    int currentPiso = piso;
                    // Creamos el panel contenedor del piso (pisoContainer)
                    var pisoContainer = new Panel
                    {
                        BackColor = (piso % 2 == 0) ? Color.White : Color.FromArgb(245, 245, 245),
                        Dock = DockStyle.Fill,
                        Padding = new Padding(0, 0, 0, 2),
                        Margin = new Padding(1),
                        BorderStyle = BorderStyle.Fixed3D
                    };

                    // Asociamos el evento Paint que dibuja bien centrado el número
                    pisoContainer.Paint += (s, e) =>
                    {
                        var numeroActual = CANTIDAD_PISOS - 1 - currentPiso;
                        var g = e.Graphics;
                        using var font = new Font("Segoe UI", 28, FontStyle.Bold, GraphicsUnit.Pixel);
                        var text = numeroActual.ToString();
                        var size = g.MeasureString(text, font);
                        var pos = new PointF(
                            (pisoContainer.Width - size.Width) / 2,
                            (pisoContainer.Height - size.Height) / 4   // un cuarto hacia abajo, para que no coincida con los ascensores
                        );
                        using var brush = new SolidBrush(Color.FromArgb(30, 0, 0, 0));
                        g.DrawString(text, font, brush, pos);
                    };

                    // Ahora creamos el FlowLayoutPanel de las cajas y anclamos **solo abajo**, con altura fija
                    var filaAscensores = new FlowLayoutPanel
                    {
                        FlowDirection = FlowDirection.LeftToRight,
                        BackColor = Color.Transparent,
                        Height = 90,           // altura que amortigua cajas de 80px + márgenes
                        Dock = DockStyle.Bottom,
                        Padding = new Padding(3),
                        AutoSize = false,
                        WrapContents = false,
                        Margin = new Padding(0)
                    };

                    // Añadimos aquí tus contenedores de ascensores:
                    if (piso == CANTIDAD_PISOS - 1)
                        foreach (var cont in cajasAscensores)
                            filaAscensores.Controls.Add(cont);

                    // Lo metemos en el panel de piso
                    pisoContainer.Controls.Add(filaAscensores);
                    layoutPisos.Controls.Add(pisoContainer, 0, piso);
                }

                panelEdificio.Controls.Add(layoutPisos);
                layoutEdificios.Controls.Add(panelEdificio, i, 0);
                edificios.Add(edificio);
            }

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

            // Panel de control universal ya existente
            var panelUniversal = new PanelDeControlUniversal(cantidadEdificios, ascensoresPorEdificio, CANTIDAD_PISOS);
            panelUniversal.SolicitudUniversalGenerada += (edificioIdx, ascensorIdx, pisoOrigen, solicitud) =>
            {
                var ascensor = edificios[edificioIdx].Ascensores[ascensorIdx];
                ascensor.AgregarSolicitud(solicitud);
                MessageBox.Show($"Solicitud enviada desde el piso {pisoOrigen} al {solicitud.PisoDestino}, tipo {solicitud.Tipo}");
            };
            panel.Controls.Add(panelUniversal);

            // 🔽 NUEVO: Paneles por edificio con botones de pausa/reanudar
            for (int i = 0; i < cantidadEdificios; i++)
            {
                int id = i;
                var group = new GroupBox
                {
                    Text = $"Edificio {id}",
                    Width = 200,
                    Height = 90
                };

                var btnPausar = new Button
                {
                    Text = "⏸ Detener",
                    Width = 80,
                    Height = 30,
                    Location = new Point(10, 30),
                    BackColor = Color.OrangeRed,
                    ForeColor = Color.White
                };
                var btnReanudar = new Button
                {
                    Text = "▶ Reanudar",
                    Width = 80,
                    Height = 30,
                    Location = new Point(100, 30),
                    BackColor = Color.SeaGreen,
                    ForeColor = Color.White
                };

                btnPausar.Click += (s, e) =>
                {
                    edificios[id].Pausar();
                };
                btnReanudar.Click += (s, e) =>
                {
                    edificios[id].Reanudar();
                };

                group.Controls.Add(btnPausar);
                group.Controls.Add(btnReanudar);
                panel.Controls.Add(group);
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

        public Task InvokeAsync(Action action)
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

        private void ActualizarPanelSolicitudes()
        {
            if (panelSolicitudes == null) return;

            panelSolicitudes.Controls.Clear();

            foreach (var edificio in edificios)
            {
                var labelEdificio = new Label
                {
                    Text = $"Edificio {edificio.Id}",
                    Font = new Font("Segoe UI", 10f, FontStyle.Bold),
                    AutoSize = true,
                    Margin = new Padding(0, 10, 0, 4)
                };
                panelSolicitudes.Controls.Add(labelEdificio);

                foreach (var ascensor in edificio.Ascensores)
                {
                    var labelAscensor = new Label
                    {
                        Text = $"Asc {ascensor.Id}:",
                        Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                        AutoSize = true,
                        Margin = new Padding(10, 4, 0, 2)
                    };
                    panelSolicitudes.Controls.Add(labelAscensor);

                    var solicitudes = ascensor.SolicitudesPendientes;

                    if (solicitudes.Count == 0)
                    {
                        var labelVacio = new Label
                        {
                            Text = "- (sin solicitudes)",
                            Font = new Font("Segoe UI", 9f, FontStyle.Italic),
                            AutoSize = true,
                            Margin = new Padding(20, 0, 0, 2)
                        };
                        panelSolicitudes.Controls.Add(labelVacio);
                    }
                    else
                    {
                        foreach (var s in solicitudes)
                        {
                            var tipo = s.Tipo == TipoSolicitud.Especial ? "Esp" : "Nor";
                            var labelSolicitud = new Label
                            {
                                Text = $"- {s.PisoDestino}, {tipo}",
                                Font = new Font("Segoe UI", 9f),
                                AutoSize = true,
                                Margin = new Padding(20, 0, 0, 2)
                            };
                            panelSolicitudes.Controls.Add(labelSolicitud);
                        }
                    }
                }
            }
        }

    }
}