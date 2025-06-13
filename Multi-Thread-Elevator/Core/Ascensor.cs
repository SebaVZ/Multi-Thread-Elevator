// Ascensor.cs actualizado: Indicador de piso actual y lista de solicitudes pendientes
using Multi_Thread_Elevator;
using Multi_Thread_Elevator.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

public class Ascensor
{
    public int Id { get; }
    public string Identificador { get; set; }
    public int PisoActual { get; private set; } = 0;
    public bool EnMovimiento { get; private set; } = false;
    public bool EjecutandoEspecial { get; private set; } = false;
    public Action ActualizarGUI { get; set; }
    public Action NotificarCambioSolicitudes { get; set; }
    public List<Solicitud> SolicitudesPendientes => new(solicitudes);
    public Label EstadoLabel { get; set; }

    private readonly List<Solicitud> solicitudes = new();
    private CancellationTokenSource cts;
    private Task tareaAscensor;
    private Solicitud solicitudEnCurso;
    private bool estaEnEjecucion = false;
    public static int VelocidadMovimientoMs { get; set; } = 500;
    public int EdificioId { get; set; }

    private static Dictionary<int, SemaphoreSlim> semaforosEdificioEspecial = new();
    public bool PuertaAbierta { get; private set; } = false;
    private int cantidadPisos;
    private DateTime ultimaActualizacionGui = DateTime.MinValue;

    public static void InicializarSemaforos(int cantidadEdificios)
    {
        semaforosEdificioEspecial.Clear();
        for (int i = 0; i < cantidadEdificios; i++)
            semaforosEdificioEspecial[i] = new SemaphoreSlim(1, 1);
    }

    public Ascensor(int id, int cantidadPisos)
    {
        Id = id;
        this.cantidadPisos = cantidadPisos;
    }

    public void AgregarSolicitud(Solicitud solicitud)
    {
        // 🚫 Evitar solicitud al mismo piso actual
        if (solicitud.Tipo == TipoSolicitud.Normal && solicitud.PisoDestino == PisoActual)
            return;

        // 🚫 Evitar duplicados por piso
        lock (solicitudes)
        {
            if (solicitudes.Any(s => s.PisoDestino == solicitud.PisoDestino))
            {
                MessageBox.Show($"Ya existe una solicitud al piso {solicitud.PisoDestino} para este ascensor.", "Solicitud Duplicada", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
        }

        // 🚫 Solo se permite una solicitud especial por edificio
        if (solicitud.Tipo == TipoSolicitud.Especial)
        {
            var edificio = Application.OpenForms.OfType<FormAscensores>()
                             .FirstOrDefault()?.edificios
                             .FirstOrDefault(e => e.Id == EdificioId);

            bool especialActiva = false;

            if (edificio != null)
            {
                foreach (var asc in edificio.Ascensores)
                {
                    if (asc.EjecutandoEspecial)
                        especialActiva = true;

                    lock (asc.solicitudes)
                    {
                        if (asc.solicitudes.Any(s => s.Tipo == TipoSolicitud.Especial))
                        {
                            especialActiva = true;
                        }
                    }

                    if (especialActiva) break;
                }
            }

            if (especialActiva)
            {
                MessageBox.Show("Ya hay una solicitud especial activa o pendiente en este edificio.", "Solicitud Rechazada", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
        }

        // ✅ Si pasó todas las validaciones, agregar la solicitud
        lock (solicitudes)
        {
            solicitudes.Add(solicitud);
            solicitudes.Sort((a, b) =>
            {
                int prioridad = b.Tipo.CompareTo(a.Tipo);
                return prioridad != 0 ? prioridad : a.TiempoSolicitud.CompareTo(b.TiempoSolicitud);
            });
        }

        NotificarCambioSolicitudes?.Invoke();
    }

    public void Iniciar()
    {
        if (estaEnEjecucion)
        {
            return;
        }

        cts = new CancellationTokenSource();
        tareaAscensor = Task.Run(() => Ejecutar(cts.Token));
        estaEnEjecucion = true;
    }

    public void Pausar()
    {
        if (!estaEnEjecucion) return;

        cts?.Cancel();

        // Recuperar solicitud en curso si no se completó
        if (solicitudEnCurso != null)
        {
            lock (solicitudes)
            {
                solicitudes.Insert(0, solicitudEnCurso); // Prioridad alta
            }
            solicitudEnCurso = null;
        }

        EjecutandoEspecial = false; // Asegura que el flag no quede colgado
        estaEnEjecucion = false;
    }

    private async Task Ejecutar(CancellationToken token)
    {
        try
        {
            while (!token.IsCancellationRequested)
            {
                Solicitud solicitud = null;

                if (solicitudEnCurso != null)
                {
                    solicitud = solicitudEnCurso;
                    solicitudEnCurso = null;
                }
                else
                {
                    lock (solicitudes)
                    {
                        var especial = solicitudes.FirstOrDefault(s => s.Tipo == TipoSolicitud.Especial);
                        if (especial != null)
                        {
                            solicitud = especial;
                            solicitudes.Remove(especial);
                        }
                        else if (solicitudes.Count > 0)
                        {
                            var solicitudMasCercana = solicitudes
                                .Where(s => s.Tipo == TipoSolicitud.Normal)
                                .OrderBy(s => Math.Abs(s.PisoDestino - PisoActual))
                                .FirstOrDefault();

                            int sentido = solicitudMasCercana != null && solicitudMasCercana.PisoDestino > PisoActual ? 1 : -1;

                            var enDireccion = solicitudes
                                .Where(s => (s.PisoDestino - PisoActual) * sentido > 0)
                                .OrderBy(s => Math.Abs(s.PisoDestino - PisoActual))
                                .ThenBy(s => s.TiempoSolicitud)
                                .ToList();

                            if (enDireccion.Any())
                            {
                                solicitud = enDireccion.First();
                            }
                            else
                            {
                                solicitud = solicitudes[0];
                            }
                        }
                    }
                }

                if (solicitud != null)
                {
                    if (solicitud.Tipo == TipoSolicitud.Especial)
                    {
                        var semaforoEdificio = semaforosEdificioEspecial[EdificioId];
                        await semaforoEdificio.WaitAsync(token);
                        try
                        {
                            EjecutandoEspecial = true;
                            await MoverAlPiso(solicitud, token);
                            EjecutandoEspecial = false;
                        }
                        finally
                        {
                            semaforoEdificio.Release();
                        }
                    }
                    else
                    {
                        await MoverAlPiso(solicitud, token);
                    }
                }

                await Task.Delay(200, token);
            }
        }
        catch (OperationCanceledException) { }
        finally
        {
            estaEnEjecucion = false;
            EjecutandoEspecial = false;
            solicitudEnCurso = null;
        }
    }

    private async Task MoverAlPiso(Solicitud solicitud, CancellationToken token)
    {
        EnMovimiento = true;
        if ((DateTime.Now - ultimaActualizacionGui).TotalMilliseconds >= 100)
        {
            ultimaActualizacionGui = DateTime.Now;
            ActualizarGUI?.Invoke();
        }
        solicitudEnCurso = solicitud;
        int destino = solicitud.PisoDestino;

        while (PisoActual != destino && !token.IsCancellationRequested)
        {
            PisoActual += PisoActual < destino ? 1 : -1;
            ActualizarGUI?.Invoke();
            await Task.Delay(VelocidadMovimientoMs, token);
        }

        PuertaAbierta = true;
        ActualizarGUI?.Invoke();
        await AbrirPuertaVisual();
        PuertaAbierta = false;
        solicitudEnCurso = null;

        lock (solicitudes)
        {
            solicitudes.Remove(solicitud);
        }

        EnMovimiento = false;
        ActualizarGUI?.Invoke();
        NotificarCambioSolicitudes();
    }

    public List<int> ObtenerPisosDisponibles()
    {
        var pisos = new List<int>();
        for (int i = 0; i < cantidadPisos; i++)
        {
            if (i != PisoActual) pisos.Add(i);
        }
        return pisos;
    }

    public string ObtenerEstadoActual()
    {
        return $"Piso: {PisoActual} | Pendientes: {string.Join(", ", SolicitudesPendientes.Select(s => s.PisoDestino))}";
    }

    public void SolicitarIrAPiso(int piso)
    {
        if (piso >= 0 && piso < cantidadPisos)
        {
            if (piso == PisoActual) return;
            AgregarSolicitud(new Solicitud
            {
                PisoDestino = piso,
                Tipo = TipoSolicitud.Normal
            });
        }
    }

    public void AlternarPuerta()
    {
        if (EnMovimiento) return;

        PuertaAbierta = !PuertaAbierta;
        ActualizarGUI?.Invoke();
    }

    private async Task AbrirPuertaVisual()
    {
        await Task.Delay(50);
        var form = Application.OpenForms.OfType<FormAscensores>().FirstOrDefault();
        if (form == null) return;

        await form.InvokeAsync(() =>
        {
            if (form.cajasAscensor.TryGetValue((EdificioId, Id), out var caja))
            {
                caja.BackColor = Color.Green;
            }
        });

        await Task.Delay(500);

        await form.InvokeAsync(() =>
        {
            if (form.cajasAscensor.TryGetValue((EdificioId, Id), out var caja))
            {
                caja.BackColor = Color.FromArgb(40, 40, 40);
            }
        });
    }

    private void OrdenarSolicitudes()
    {
        if (solicitudes.Count == 0) return;

        if (solicitudes[0].PisoDestino > PisoActual)
        {
            solicitudes.Sort((a, b) => a.PisoDestino.CompareTo(b.PisoDestino));
        }
        else
        {
            solicitudes.Sort((a, b) => b.PisoDestino.CompareTo(a.PisoDestino));
        }
    }
}
