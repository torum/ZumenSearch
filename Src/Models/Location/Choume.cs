namespace ZumenSearch.Models.Location;
#pragma warning disable IDE0079 // Remove unnecessary suppression
#pragma warning disable IDE0290 // Use primary constructor
public sealed class Choume
{
    public Choume(string machiazaId, string choume)
    {
        MachiazaId = machiazaId;
        Chou = choume;
    }

    // db:location_machiaza_id
    public string MachiazaId { get; init; } = string.Empty;

    // db:location_choume
    public string Chou { get; init; } = string.Empty;
}
