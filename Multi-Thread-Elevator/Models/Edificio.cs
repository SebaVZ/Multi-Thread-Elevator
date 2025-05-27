using System.Collections.Generic;

namespace Multi_Thread_Elevator.Models
{
    public class Edificio
    {
        public int Id { get; set; }
        public List<Ascensor> Ascensores { get; set; } = new();

        public Edificio(int id)
        {
            Id = id;
        }
    }
}
