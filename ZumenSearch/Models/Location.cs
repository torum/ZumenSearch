using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZumenSearch.Models;


// 賃貸住居用物件の「都道府県」クラス
public class Pref(string code, string municipalityCode, string name)
{
    // 都道府県コード
    public string Code { get; private set; } = code;

    // 市区町村コード
    // db:loc_pref_id
    public string MunicipalityCode { get; private set; } = municipalityCode;

    // db:loc_prefecture
    public string Name { get; private set; } = name;
};

public class CountyAndCity
{
    // db:loc_machiaza_id
    public string MachiazaId
    {
        get; init;
    }

    // 郡 db:loc_county
    public string County { get; init; }

    // 市区町村 db:loc_city
    public string City { get; init; }

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

    public CountyAndCity(string machiazaId, string county, string city)
    {
        MachiazaId = machiazaId;
        County = county;
        City = city;
    }
}

public class WardAndOaza
{
    // db:loc_machiaza_id
    public string MachiazaId
    {
        get; init;
    }

    // db:loc_ward
    public string Ward
    {
        get; init;
    }

    // db:loc_oaza_cho
    public string Oaza
    {
        get; init;
    }

    public WardAndOaza(string machiazaId, string ward, string oaza)
    {
        MachiazaId = machiazaId;
        Ward = ward;
        Oaza = oaza;
    }

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

public class Choume
{
    // db:loc_machiaza_id
    public string MachiazaId
    {
        get; init;
    }

    // db:loc_choume
    public string Chou
    {
        get; init;
    }

    public Choume(string machiazaId, string choume)
    {
        MachiazaId = machiazaId;
        Chou = choume;
    }
}

