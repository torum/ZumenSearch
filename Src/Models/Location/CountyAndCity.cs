namespace ZumenSearch.Models.Location;

public sealed class CountyAndCity(string machiazaId, string county, string city)
{
    // db:location_machiaza_id
    public string MachiazaId { get; init; } = machiazaId;

    // 郡 db:location_county
    public string County { get; init; } = county;

    // 市区町村 db:location_city
    public string City { get; init; } = city;

    public string Combined
    {
        get
        {
            if (string.IsNullOrEmpty(County) && string.IsNullOrEmpty(City))
            {
                return "";//該当なし
            }
            else
            {
                return County + City;
            }
        }
    }
}
