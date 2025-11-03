using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZumenSearch.Models.Location;


// 賃貸住居用物件の「都道府県」クラス
public class Pref(string code, string municipalityCode, string name)
{
    // 都道府県コード
    public string Code { get; private set; } = code;

    // 市区町村コード
    public string MunicipalityCode { get; private set; } = municipalityCode;

    // db:pref
    public string Name { get; private set; } = name;
};

public class CountyAndCity
{
    //machiaza_id
    public string MachiazaId
    {
        get; init;
    }

    // 郡 db:county
    public string County { get; init; }

    // 市区町村 db:city
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
    //machiaza_id
    public string MachiazaId
    {
        get; init;
    }

    public string Ward
    {
        get; init;
    }

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
    public string MachiazaId
    {
        get; init;
    }

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

