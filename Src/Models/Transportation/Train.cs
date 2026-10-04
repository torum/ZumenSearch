using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;

namespace ZumenSearch.Models.Transportation;

// Value Object

// <summary>
// Represents a train value object. 
// </summary>
// <remarks>
// </remarks>
public class Train
{
    // Nested classes to represent the address components.
    public sealed class RailLineClass(string lineCode, string lineName)
    {
        // 路線コード（カラム名:line_cd）
        public string LineCode { get; private set; } = lineCode;
        // 路線名: line_name
        public string LineName { get; private set; } = lineName;

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

    public sealed class RailStationClass(string stationCode, string stationName, string lineCode)
    {
        // 駅コード（カラム名: station_cd）
        public string StationCode { get; private set; } = stationCode;
        // 駅名: station_name
        public string StationName { get; private set; } = stationName;
        // 路線コード（カラム名:line_cd）
        public string LineCode { get; private set; } = lineCode;

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

    public Train()
    {
        //
    }

    public RailLineClass? RailLine { get; private set; }

    public RailStationClass? RailStation { get; private set; }

    public void SetRailLine(string lineCode, string lineName)
    {
        if (string.IsNullOrWhiteSpace(lineCode)) throw new ArgumentException("Line code cannot be empty.", nameof(lineCode));
        if (string.IsNullOrWhiteSpace(lineName)) throw new ArgumentException("Line name cannot be empty.", nameof(lineName));
        RailLine = new RailLineClass(lineCode, lineName);
    }

    public void SetRailStation(string stationCode, string stationName, string lineCode)
    {
        if (string.IsNullOrWhiteSpace(stationCode)) throw new ArgumentException("Station code cannot be empty.", nameof(stationCode));
        if (string.IsNullOrWhiteSpace(stationName)) throw new ArgumentException("Station name cannot be empty.", nameof(stationName));
        if (string.IsNullOrWhiteSpace(lineCode)) throw new ArgumentException("Line code cannot be empty.", nameof(lineCode));
        RailStation = new RailStationClass(stationCode, stationName, lineCode);
    }
}
