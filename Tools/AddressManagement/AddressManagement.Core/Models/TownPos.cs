using System.Globalization;
using System.IO;
using CsvHelper;
using CsvHelper.Configuration.Attributes;

namespace AddressManagement.Core.Models;

public class TownPos
{
    //全国地方公共団体コード
    [Index(0)]
    public string AdministrativeDivisionCode
    {
        get; set;
    }

    //町字ID
    [Index(1)]
    public string TownID
    {
        get; set;
    }

    //代表点_経度
    [Index(3)]
    public string Longitude
    {
        get; set;
    }

    //代表点_緯度
    [Index(4)]
    public string Latitude
    {
        get; set;
    }

    // 代表点_座標参照系 eg.EPSG:6668
    [Index(5)]
    public string CRS
    {
        get; set;
    }

    //代表点_地図情報レベル
    [Index(6)]
    public string MapInfoLovel
    {
        get; set;
    }
}
