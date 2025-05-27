using Multi_Thread_Elevator.Models;
using System.Collections.Concurrent;

public class Ascensor
{
    public int Id { get; }
    public int PisoActual { get; private set; } = 0;
    public bool EnMovimiento { get; private set; } = false;
    public bool EjecutandoEspecial { get; private set; } = false;

    public Action ActualizarGUI { get; set; }

    private readonly ConcurrentQueue<Solicitud> solicitudes = new();
    private readonly object lockObj = new();
    private CancellationTokenSource cts;

    public Ascensor(int id, Action? dummy = null)
    {
        Id = id;
        // Se puede ignorar dummy en esta versión si no se usa constructor alterno
    }

    public void AgregarSolicitud(Solicitud solicitud) => solicitudes.Enqueue(solicitud);

    public void Iniciar()
    {
        cts = new CancellationTokenSource();
        Task.Run(() => Ejecutar(cts.Token));
    }

    public void Pausar() => cts?.Cancel();

    private void Ejecutar(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            if (solicitudes.TryDequeue(out var solicitud))
            {
                if (solicitud.Tipo == TipoSolicitud.Especial)
                {
                    lock (lockObj)
                    {
                        EjecutandoEspecial = true;
                        MoverAlPiso(solicitud.PisoDestino, token);
                        EjecutandoEspecial = false;
                    }
                }
                else
                {
                    MoverAlPiso(solicitud.PisoDestino, token);
                }
            }

            Thread.Sleep(200);
        }
    }

    private void MoverAlPiso(int piso, CancellationToken token)
    {
        EnMovimiento = true;
        while (PisoActual != piso && !token.IsCancellationRequested)
        {
            PisoActual += PisoActual < piso ? 1 : -1;
            ActualizarGUI?.Invoke();
            Thread.Sleep(500);
        }
        EnMovimiento = false;
    }
}
