public class Edificio
{
    public int Id { get; set; }
    public List<Ascensor> Ascensores { get; set; } = new();
    public bool EstaPausado { get; set; } = false; // NUEVO

    public Edificio(int id)
    {
        Id = id;
    }

    public void Pausar()
    {
        EstaPausado = true;
        foreach (var asc in Ascensores)
            asc.Pausar();
    }

    public void Reanudar()
    {
        EstaPausado = false;
        foreach (var asc in Ascensores)
            asc.Iniciar();
    }
}
