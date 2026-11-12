#pragma warning disable IDE0079 // Remove unnecessary suppression
#pragma warning disable IDE0290 // Use primary constructor
namespace ZumenSearch.Models.Transportation;

public sealed class RailLine
{
    public RailLine(string lineCode, string lineName)
    {
        //if (string.IsNullOrWhiteSpace(lineCode)) throw new ArgumentException("Line code cannot be empty.", nameof(lineCode));
        //if (string.IsNullOrWhiteSpace(lineName)) throw new ArgumentException("Line name cannot be empty.", nameof(lineName));
        LineCode = lineCode;
        LineName = lineName;
    }

    // 路線コード（カラム名:line_cd）
    public string LineCode {get; private set;} = string.Empty;

    // 路線名: line_name
    public string LineName {get; private set;} = string.Empty;

    /*
    // 路線区分: line_type 「0:その他　1:新幹線 2:一般 3:地下鉄 4:市電・路面電車 5:モノレール・新交通」
    [Index(7)]
    public string LineType
    {
        get; set;
    } = string.Empty;
    */
    /*
    // 経度: lon
    public string LineLon
    {
        get; set;
    } = string.Empty;

    // 緯度: lat
    public string LineLat
    {
        get; set;
    } = string.Empty;

    // Google map 倍率: zoom
    public string LineMapZoom
    {
        get; set;
    } = string.Empty;

    // ステータス: e_status 「0:運用中　1:運用前　2:廃止」
    public string LineStatus
    {
        get; set;
    } = string.Empty;

    // ソートキー: e_sort
    public string LineSort
    {
        get; set;
    } = string.Empty;
    */

    public void SetLineCode(string lineCode)
    {
        //if (string.IsNullOrWhiteSpace(lineCode)) throw new ArgumentException("Line code cannot be empty.", nameof(lineCode));
        LineCode = lineCode;
    }

    public void SetLineName(string lineName)
    {
        //if (string.IsNullOrWhiteSpace(lineName)) throw new ArgumentException("Line name cannot be empty.", nameof(lineName));
        LineName = lineName;
    }

    public void SetRainLine(string lineCode, string lineName)
    {
        //if (string.IsNullOrWhiteSpace(lineCode)) throw new ArgumentException("Line code cannot be empty.", nameof(lineCode));
        //if (string.IsNullOrWhiteSpace(lineName)) throw new ArgumentException("Line name cannot be empty.", nameof(lineName));
        LineCode = lineCode;
        LineName = lineName;
    }

    public void Clear()
    {
        LineCode = string.Empty;
        LineName = string.Empty;
    }
}
