namespace ZumenSearch.Models.Location;

// Value Object

// <summary>
// Represents an address value object. Aggregate Root entities are property and person.
// </summary>
// <remarks>
// 
// </remarks>

public class Address
{
    public Address()
    {
        //
    }

    public Prefecture? Prefecture { get; private set; }
    public CountyAndCity? CountyAndCity { get; private set; }
    public WardAndOaza? WardAndOaza { get; private set; }
    public Choume? Choume { get; private set; }
    public string Edaban {get; private set;} = string.Empty;
    // TODO:
    public string PostalCode {get;set;} = string.Empty;
    public string AddressFull
    {
        get
        {
            // If the Prefecture is not set, return an empty string.
            if (Prefecture == null)
            {
                return string.Empty;
            }

            var s = string.Empty;
            if (!string.IsNullOrEmpty(Edaban))
            {
                s = "-" + Edaban;
            }

            // TODO: Chcek
            return $"{Prefecture.Name}{CountyAndCity?.Combined}{WardAndOaza?.Combined}{Choume?.Chou}{s}";
        }
    }

    public void SetPrefecture(string code, string municipalityCode, string name)
    {
        if (string.IsNullOrWhiteSpace(code)) throw new ArgumentException("Code cannot be empty.", nameof(code));
        if (string.IsNullOrWhiteSpace(municipalityCode)) throw new ArgumentException("MunicipalityCode cannot be empty.", nameof(municipalityCode));
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Name cannot be empty.", nameof(name));

        Prefecture = new Prefecture(code, municipalityCode, name);
    }

    public void SetCountyAndCity(string machiazaId, string county, string city)
    {
        if (string.IsNullOrWhiteSpace(machiazaId)) throw new ArgumentException("MachiazaId cannot be empty.", nameof(machiazaId));
        //if (string.IsNullOrWhiteSpace(county)) throw new ArgumentException("County cannot be empty.", nameof(county));
        //if (string.IsNullOrWhiteSpace(city)) throw new ArgumentException("City cannot be empty.", nameof(city));
        
        CountyAndCity = new CountyAndCity(machiazaId, county, city);
    }

    public void SetWardAndOaza(string machiazaId, string ward, string oaza)
    {
        if (string.IsNullOrWhiteSpace(machiazaId)) throw new ArgumentException("MachiazaId cannot be empty.", nameof(machiazaId));
        //if (string.IsNullOrWhiteSpace(ward)) throw new ArgumentException("Ward cannot be empty.", nameof(ward));
        //if (string.IsNullOrWhiteSpace(oaza)) throw new ArgumentException("Oaza cannot be empty.", nameof(oaza));

        WardAndOaza = new WardAndOaza(machiazaId, ward, oaza);
    }

    public void SetChoume(string machiazaId, string choume)
    {
        if (string.IsNullOrWhiteSpace(machiazaId)) throw new ArgumentException("MachiazaId cannot be empty.", nameof(machiazaId));
        //if (string.IsNullOrWhiteSpace(choume)) throw new ArgumentException("Choume cannot be empty.", nameof(choume));

        Choume = new Choume(machiazaId, choume);
    }

    public void SetEdaban(string edaban)
    {
        if (string.IsNullOrWhiteSpace(edaban)) throw new ArgumentException("Edaban cannot be empty.", nameof(edaban));

        Edaban = edaban;
    }

    public void SetPostalCode(string postalCode)
    {
        if (string.IsNullOrWhiteSpace(postalCode)) throw new ArgumentException("PostalCode cannot be empty.", nameof(postalCode));

        PostalCode = postalCode;
    }
}