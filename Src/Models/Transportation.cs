namespace ZumenSearch.Models;


public class RailLine
{
    // 路線コード（カラム名:line_cd）
    public string LineCode
    {
        get; set;
    } = string.Empty;

    // 路線名: line_name
    public string LineName
    {
        get; set;
    } = string.Empty;

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

}


public class RailStation
{
    // 駅コード（カラム名: station_cd）
    public string StationCode
    {
        get; set;
    } = string.Empty;

    // 駅名: station_name
    public string StationName
    {
        get; set;
    } = string.Empty;

    // 路線コード（カラム名:line_cd）
    public string LineCode
    {
        get; set;
    } = string.Empty;
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
