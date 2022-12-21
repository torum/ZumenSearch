using System.Globalization;
using System.IO;
using CsvHelper;
using CsvHelper.Configuration.Attributes;

namespace AddressManagement.Core.Models;

// 大元の郵便番号データ
// https://www.post.japanpost.jp/zipcode/dl/kogaki-zip.html

// 加工済み x-ken-all
// http://zipcloud.ibsnet.co.jp/





// 郵便番号（データベーステーブル名：postal_codes）
public class PostalCode
{
    //全国地方公共団体コード（カラム名:municipality_code）
    [Index(0)]
    public string MunicipalityCode
    {
        get; set;
    }

    //郵便番号（カラム名:postal_code）
    [Index(2)]
    public string Code
    {
        get; set;
    }

    //都道府県名（カラム名:prefecture_name）
    [Index(3)]
    public string PrefectureName
    {
        get; set;
    }

    //市区町村名（郡名・政令都市名含む）（カラム名:sikuchouson_name）
    [Index(4)]
    public string SikuchousonName
    {
        get; set;
    }

    // 町域名（カラム名:chouiki_name）
    [Index(5)]
    public string ChouikiName
    {
        get; set;
    }
}
