// Ascensor.cs actualizado: Indicador de piso actual y lista de solicitudes pendientes
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
    public List<Solicitud> SolicitudesPendientes => new(solicitudes);
    public Label EstadoLabel { get; set; } // Label para mostrar estado visual

    private readonly List<Solicitud> solicitudes = new();
    private CancellationTokenSource cts;
    private Task tareaAscensor;
    private bool estaEnEjecucion = false;
    public static int VelocidadMovimientoMs { get; set; } = 500;
    public int EdificioId { get; set; }

    private static Dictionary<int, SemaphoreSlim> semaforosEdificioEspecial = new();
    public bool PuertaAbierta { get; private set; } = false;
    private int cantidadPisos = 8;

    public static void InicializarSemaforos(int cantidadEdificios)
    {
        semaforosEdificioEspecial.Clear();
        for (int i = 0; i < cantidadEdificios; i++)
            semaforosEdificioEspecial[i] = new SemaphoreSlim(1, 1);
    }


    public Ascensor(int id)
    {
        Id = id;
    }

    public void AgregarSolicitud(Solicitud solicitud)
    {
        lock (solicitudes)
        {
            solicitudes.Add(solicitud);
            solicitudes.Sort((a, b) =>
            {
                int prioridad = b.Tipo.CompareTo(a.Tipo);
                return prioridad != 0 ? prioridad : a.TiempoSolicitud.CompareTo(b.TiempoSolicitud);
            });
        }
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
        estaEnEjecucion = false;
    }

    private async Task Ejecutar(CancellationToken token)
    {
        try
        {
            while (!token.IsCancellationRequested)
            {
                Solicitud solicitud = null;

                lock (solicitudes)
                {
                    // Si hay solicitud especial, tomarla de inmediato
                    var especial = solicitudes.FirstOrDefault(s => s.Tipo == TipoSolicitud.Especial);
                    if (especial != null)
                    {
                        solicitud = especial;
                        solicitudes.Remove(especial);
                    }
                    else
                    {
                        // Ordenar solicitudes normales por cercanía al piso actual
                        solicitudes.Sort((a, b) =>
                        {
                            int diffA = Math.Abs(a.PisoDestino - PisoActual);
                            int diffB = Math.Abs(b.PisoDestino - PisoActual);
                            return diffA != diffB ? diffA.CompareTo(diffB) :
                                a.TiempoSolicitud.CompareTo(b.TiempoSolicitud);
                        });

                        if (solicitudes.Count > 0)
                        {
                            solicitud = solicitudes[0];
                            //solicitudes.RemoveAt(0);
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
        }
    }


    private async Task MoverAlPiso(Solicitud solicitud, CancellationToken token)
    {
        EnMovimiento = true;
        ActualizarGUI?.Invoke();

        int destino = solicitud.PisoDestino;

        // ÚNICO bucle principal
        while (PisoActual != destino && !token.IsCancellationRequested)
        {
            // 1) Si la puerta está abierta, esperamos aquí hasta que se cierre:
            if (PuertaAbierta)
            {
                await Task.Delay(200, token);
                continue;
            }

            // 2) Puerta cerrada: avanzamos un piso
            PisoActual += PisoActual < destino ? 1 : -1;
            ActualizarGUI?.Invoke();

            // 3) Pausa entre pisos
            await Task.Delay(VelocidadMovimientoMs, token);
        }

        EnMovimiento = false;
        ActualizarGUI?.Invoke();

        lock (solicitudes)
        {
            solicitudes.Remove(solicitud);
        }
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

}
