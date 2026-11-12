namespace ZumenSearch.Models.Location;
#pragma warning disable IDE0079 // Remove unnecessary suppression
#pragma warning disable IDE0290 // Use primary constructor
public sealed class CountyAndCity
{
    public CountyAndCity(string machiazaId, string county, string city)
    {
        //if (string.IsNullOrWhiteSpace(machiazaId)) throw new ArgumentException("MachiazaId cannot be empty.", nameof(machiazaId));
        //if (string.IsNullOrWhiteSpace(county)) throw new ArgumentException("County cannot be empty.", nameof(county));
        //if (string.IsNullOrWhiteSpace(city)) throw new ArgumentException("City cannot be empty.", nameof(city));
        MachiazaId = machiazaId;
        County = county;
        City = city;
    }

    // db:location_machiaza_id
    public string MachiazaId { get; init; } = string.Empty;

    // 郡 db:location_county
    public string County { get; init; } = string.Empty;

    // 市区町村 db:location_city
    public string City { get; init; } = string.Empty;

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
