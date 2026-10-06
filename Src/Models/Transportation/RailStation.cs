#pragma warning disable IDE0079 // Remove unnecessary suppression
#pragma warning disable IDE0290 // Use primary constructor
namespace ZumenSearch.Models.Transportation;

public sealed class RailStation
{
    public RailStation(string stationCode, string stationName, string lineCode)
    {
        //if (string.IsNullOrWhiteSpace(stationCode)) throw new ArgumentException("Station code cannot be empty.", nameof(stationCode));
        //if (string.IsNullOrWhiteSpace(stationName)) throw new ArgumentException("Station name cannot be empty.", nameof(stationName));
        //if (string.IsNullOrWhiteSpace(lineCode)) throw new ArgumentException("Line code cannot be empty.", nameof(lineCode));
        StationCode = stationCode;
        StationName = stationName;
        LineCode = lineCode;
    }
    // 駅コード（カラム名: station_cd）
    public string StationCode{get; private set;} = string.Empty;

    // 駅名: station_name
    public string StationName {get; private set;} = string.Empty;

    // 路線コード（カラム名:line_cd）
    public string LineCode {get; private set;} = string.Empty;

    /*
    // 都道府県コード（カラム名:pref_cd） 0無しint.
    public string PrefCode
    {
        get; set;
    } = string.Empty;

    // 経度: lon
    public string StationLon
    {
        get; set;
    } = string.Empty;

    // 緯度: lat
    public string StationLat
    {
        get; set;
    } = string.Empty;

    // ステータス: e_status 「0:運用中　1:運用前　2:廃止」
    public string StationStatus
    {
        get; set;
    } = string.Empty;

    // ソートキー: e_sort
    public string StationSort
    {
        get; set;
    } = string.Empty;
    */

}
