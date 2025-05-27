using System;

namespace Multi_Thread_Elevator.Models
{
    public enum TipoSolicitud
    {
        Normal,
        Especial
    }

    public class Solicitud
    {
        public int PisoDestino { get; set; }
        public TipoSolicitud Tipo { get; set; }
        public DateTime TiempoSolicitud { get; set; } = DateTime.Now;
    }
}
