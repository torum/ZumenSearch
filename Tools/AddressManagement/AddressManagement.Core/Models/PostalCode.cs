using System.Globalization;
using System.IO;
using CsvHelper;
using CsvHelper.Configuration.Attributes;

namespace AddressManagement.Core.Models;

// 大元の郵便番号データ
// https://www.post.japanpost.jp/zipcode/dl/kogaki-zip.html

// 加工済み x-ken-all
// http://zipcloud.ibsnet.co.jp/


public class PostalCode
{
    //全国地方公共団体コード
    [Index(0)]
    public string AdministrativeDivisionCodeID
    {
        get; set;
    }

    //郵便番号
    [Index(2)]
    public string Code
    {
        get; set;
    }

    //都道府県名
    [Index(3)]
    public string PrefectureName
    {
        get; set;
    }

    //市区町村名（郡名含む）
    [Index(4)]
    public string CityName
    {
        get; set;
    }

    // 大字・町名
    [Index(5)]
    public string ChouName
    {
        get; set;
    }
}
