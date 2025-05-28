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
    private readonly SemaphoreSlim semaforoEspecial = new(1, 1);
    private CancellationTokenSource cts;
    private Task tareaAscensor;
    private bool estaEnEjecucion = false;

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
        if (tareaAscensor != null && !tareaAscensor.IsCompleted)
            return;

        cts = new CancellationTokenSource();
        tareaAscensor = Task.Run(() => Ejecutar(cts.Token));
        estaEnEjecucion = true;
    }

    public void Pausar()
    {
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
                    if (solicitudes.Count > 0)
                    {
                        solicitud = solicitudes[0];
                        solicitudes.RemoveAt(0);
                    }
                }

                if (solicitud != null)
                {
                    if (solicitud.Tipo == TipoSolicitud.Especial)
                    {
                        await semaforoEspecial.WaitAsync(token);
                        try
                        {
                            EjecutandoEspecial = true;
                            await MoverAlPiso(solicitud.PisoDestino, token);
                            EjecutandoEspecial = false;
                        }
                        finally
                        {
                            semaforoEspecial.Release();
                        }
                    }
                    else
                    {
                        await MoverAlPiso(solicitud.PisoDestino, token);
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

    private async Task MoverAlPiso(int piso, CancellationToken token)
    {
        EnMovimiento = true;
        while (PisoActual != piso && !token.IsCancellationRequested)
        {
            PisoActual += PisoActual < piso ? 1 : -1;
            ActualizarGUI?.Invoke();
            await Task.Delay(500, token);
        }
        EnMovimiento = false;
        ActualizarGUI?.Invoke();
    }

    public List<int> ObtenerPisosDisponibles()
    {
        var pisos = new List<int>();
        for (int i = 0; i < 10; i++)
        {
            if (i != PisoActual) pisos.Add(i);
        }
        return pisos;
    }

    public string ObtenerEstadoActual()
    {
        return $"Piso: {PisoActual} | Pendientes: {string.Join(", ", SolicitudesPendientes.Select(s => s.PisoDestino))}";
    }
}
