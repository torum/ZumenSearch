using System;
using System.Collections.Generic;
using System.Text;

namespace ZumenSearch.Models.Transportation;

public sealed class RailStation
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
