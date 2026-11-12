namespace ZumenSearch.Models.Location;
#pragma warning disable IDE0079 // Remove unnecessary suppression
#pragma warning disable IDE0290 // Use primary constructor
public sealed class WardAndOaza
{
    public WardAndOaza(string machiazaId, string ward, string oaza)
    {
        MachiazaId = machiazaId;
        Ward = ward;
        Oaza = oaza;
    }
    // db:location_machiaza_id
    public string MachiazaId { get; init; } = string.Empty;

    // db:location_ward
    public string Ward { get; init; } = string.Empty;

    // db:location_oaza_cho
    public string Oaza { get; init; } = string.Empty;

    public string Combined
    {
        get
        {
            if (string.IsNullOrEmpty(Ward) && string.IsNullOrEmpty(Oaza))
            {
                return "";//該当なし
            }
            else
            {
                return Ward + Oaza;
            }
        }
    }
}
