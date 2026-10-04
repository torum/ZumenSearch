using System;
using System.Collections.Generic;
using System.Text;

namespace ZumenSearch.Models.Location;

// Value Object

// <summary>
// Represents an address value object. Aggregate Root entities are property and person.
// </summary>
// <remarks>
// 
// </remarks>

public class AddressClass
{
    // Nested classes to represent the address components.
    public sealed class PrefectureClass(string code, string municipalityCode, string name)
    {
        // 都道府県コード
        public string Code { get; private set; } = code;

        // 市区町村コード
        // db:location_pref_id
        public string MunicipalityCode { get; private set; } = municipalityCode;

        // db:location_prefecture
        public string Name { get; private set; } = name;
    };

    public sealed class CountyAndCityClass(string machiazaId, string county, string city)
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

    public sealed class WardAndOazaClass(string machiazaId, string ward, string oaza)
    {
        // db:location_machiaza_id
        public string MachiazaId {get; init;} = machiazaId;

        // db:location_ward
        public string Ward {get; init;} = ward;

        // db:location_oaza_cho
        public string Oaza{get; init;} = oaza;

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

    public sealed class ChoumeClass(string machiazaId, string choume)
    {
        // db:location_machiaza_id
        public string MachiazaId {get; init;} = machiazaId;

        // db:location_choume
        public string Chou {get; init;} = choume;
    }

    public AddressClass()
    {
        //
        
    }

    public PrefectureClass? Prefecture { get; private set; }
    public CountyAndCityClass? CountyAndCity { get; private set; }
    public WardAndOazaClass? WardAndOaza { get; private set; }
    public ChoumeClass? Choume { get; private set; }
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

        Prefecture = new PrefectureClass(code, municipalityCode, name);
    }

    public void SetCountyAndCity(string machiazaId, string county, string city)
    {
        if (string.IsNullOrWhiteSpace(machiazaId)) throw new ArgumentException("MachiazaId cannot be empty.", nameof(machiazaId));
        //if (string.IsNullOrWhiteSpace(county)) throw new ArgumentException("County cannot be empty.", nameof(county));
        //if (string.IsNullOrWhiteSpace(city)) throw new ArgumentException("City cannot be empty.", nameof(city));
        
        CountyAndCity = new CountyAndCityClass(machiazaId, county, city);
    }

    public void SetWardAndOaza(string machiazaId, string ward, string oaza)
    {
        if (string.IsNullOrWhiteSpace(machiazaId)) throw new ArgumentException("MachiazaId cannot be empty.", nameof(machiazaId));
        //if (string.IsNullOrWhiteSpace(ward)) throw new ArgumentException("Ward cannot be empty.", nameof(ward));
        //if (string.IsNullOrWhiteSpace(oaza)) throw new ArgumentException("Oaza cannot be empty.", nameof(oaza));

        WardAndOaza = new WardAndOazaClass(machiazaId, ward, oaza);
    }

    public void SetChoume(string machiazaId, string choume)
    {
        if (string.IsNullOrWhiteSpace(machiazaId)) throw new ArgumentException("MachiazaId cannot be empty.", nameof(machiazaId));
        //if (string.IsNullOrWhiteSpace(choume)) throw new ArgumentException("Choume cannot be empty.", nameof(choume));

        Choume = new ChoumeClass(machiazaId, choume);
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