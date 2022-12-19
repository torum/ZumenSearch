using System.Globalization;
using System.IO;
using CsvHelper;
using CsvHelper.Configuration.Attributes;

namespace AddressManagement.Core.Models;

// 日本 町字マスター データセット
// https://registry-catalog.registries.digital.go.jp/dataset/o1-000000_g2-000003


public class TownCode
{
    //全国地方公共団体コード
    [Index(0)]
    public int AdministrativeDivisionCodeID
    {
        get; set;
    }

    //町字id
    [Index(1)]
    public int ChouAzaCodeID
    {
        get; set;
    }

    //町字区分コード
    [Index(2)]
    public int ChouAzaCodeKindID
    {
        get; set;
    }

    //都道府県名
    [Index(3)]
    public string PrefectureName
    {
        get; set;
    }

    //郡名
    [Index(6)]
    public string CountyName
    {
        get; set;
    }

    //市区町村名（eg.札幌市）
    [Index(9)]
    public string CityName
    {
        get; set;
    }

    //政令市区名（eg.中央区）
    [Index(12)]
    public string WardName
    {
        get; set;
    }

    // 大字・町名（eg.旭ヶ丘）
    [Index(15)]
    public string ChouName
    {
        get; set;
    }

    // 丁目名
    [Index(18)]
    public string Choume
    {
        get; set;
    }

    // 子字
    [Index(21)]
    public string KoazaName
    {
        get; set;
    }

    // 郵便番号
    [Index(35)]
    public string PostalCode
    {
        get; set;
    }
}
