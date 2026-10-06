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

    #region ==  Address Components ==

    public string MachiazaId { get; private set; } = string.Empty;
    public Prefecture? Prefecture { get; private set; }
    public CountyAndCity? CountyAndCity { get; private set; }
    public WardAndOaza? WardAndOaza { get; private set; }
    public Choume? Choume { get; private set; }
    public string Edaban {get; private set;} = string.Empty;
    public string PostalCode {get;private set;} = string.Empty;
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
    public string LocationLatitude { get; private set; } = string.Empty;
    public string LocationLongitude { get; private set; } = string.Empty;
    /*
    public string GeoUri
    {
        get
        {
            if (string.IsNullOrEmpty(LocationLatitude) || string.IsNullOrEmpty(LocationLongitude))
            {
                return "https://maps.google.co.jp/";
            }

            return $"https://maps.google.co.jp/?q={LocationLatitude},{LocationLongitude}";
        }
    }
    */

    #endregion

    #region == Methods to Set Address Components ==

    // TODO: Add methods to set the address components, ensuring that the address remains valid and consistent.
    public void SetMachiazaId(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        { value = string.Empty; }

        MachiazaId = value;
        // TODO: ?Populate the Prefecture, CountyAndCity, WardAndOaza, and Choume based on the Machiaza ID.
    }

    public void SetPrefecture(string code, string municipalityCode, string name)
    {
        if (string.IsNullOrWhiteSpace(code)) throw new ArgumentException("Code cannot be empty.", nameof(code));
        if (string.IsNullOrWhiteSpace(municipalityCode)) throw new ArgumentException("MunicipalityCode cannot be empty.", nameof(municipalityCode));
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Name cannot be empty.", nameof(name));
        Prefecture = new Prefecture(code, municipalityCode, name);
    }

    public void SetPrefectureByMunicipalityCode(string municipalityCode, string? name = "")
    {
        //if (string.IsNullOrWhiteSpace(municipalityCode)) throw new ArgumentException("MunicipalityCode cannot be empty.", nameof(municipalityCode));
        if (string.IsNullOrWhiteSpace(municipalityCode))
        {
            Prefecture = null;
            return;
        }

        Prefecture = new Prefecture(municipalityCode, name);
    }

    public void SetPrefecture(Prefecture? value)
    {
        // TODO: Validate the prefecture value, if required.
        Prefecture = value;
    }

    public void SetCountyAndCity(string machiazaId, string county, string city)
    {
        //if (string.IsNullOrWhiteSpace(machiazaId)) throw new ArgumentException("MachiazaId cannot be empty.", nameof(machiazaId));
        //if (string.IsNullOrWhiteSpace(county)) throw new ArgumentException("County cannot be empty.", nameof(county));
        //if (string.IsNullOrWhiteSpace(city)) throw new ArgumentException("City cannot be empty.", nameof(city));
        
        CountyAndCity = new CountyAndCity(machiazaId, county, city);
    }

    public void SetCountyAndCity(CountyAndCity? value)
    {
        // TODO: Validate the city and its relationship to the selected prefecture.
        CountyAndCity = value;
    }

    public void SetWardAndOaza(string machiazaId, string ward, string oaza)
    {
        //if (string.IsNullOrWhiteSpace(machiazaId)) throw new ArgumentException("MachiazaId cannot be empty.", nameof(machiazaId));
        //if (string.IsNullOrWhiteSpace(ward)) throw new ArgumentException("Ward cannot be empty.", nameof(ward));
        //if (string.IsNullOrWhiteSpace(oaza)) throw new ArgumentException("Oaza cannot be empty.", nameof(oaza));

        WardAndOaza = new WardAndOaza(machiazaId, ward, oaza);
    }

    public void SetWardAndOaza(WardAndOaza? value)
    {
        // TODO: Validate the town and its relationship to the selected city.
        WardAndOaza = value;
    }

    public void SetChoume(string machiazaId, string choume)
    {
        //if (string.IsNullOrWhiteSpace(machiazaId)) throw new ArgumentException("MachiazaId cannot be empty.", nameof(machiazaId));
        //if (string.IsNullOrWhiteSpace(choume)) throw new ArgumentException("Choume cannot be empty.", nameof(choume));

        Choume = new Choume(machiazaId, choume);
    }
    public void SetChoume(Choume? value)
    {
        // TODO: Validate the chōme and its relationship to the selected town.
        Choume = value;
    }

    public void SetEdaban(string edaban)
    {
        //if (string.IsNullOrWhiteSpace(edaban)) throw new ArgumentException("Edaban cannot be empty.", nameof(edaban));

        Edaban = edaban;
    }

    public void SetPostalCode(string postalCode)
    {
        if (string.IsNullOrWhiteSpace(postalCode)) throw new ArgumentException("PostalCode cannot be empty.", nameof(postalCode));

        PostalCode = postalCode;
    }

    public void SetLocationLatitude(string value)
    {
        // TODO: Validate the latitude value and allowed range.
        LocationLatitude = value;
    }

    public void SetLocationLongitude(string value)
    {
        // TODO: Validate the longitude value and allowed range.
        LocationLongitude = value;
    }

    #endregion
}