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
    public Train()
    {
        //
    }

    public bool IsModified { get; private set; }

    public RailLine RailLine { get; private set; } = new RailLine(string.Empty, string.Empty);

    public RailStation RailStation { get; private set; } = new RailStation(string.Empty, string.Empty, string.Empty);

    public int EkiToho { get; private set; }

    public void SetIsModified(bool isModified)
    {
        IsModified = isModified;
    }

    public void SetRailLine(string lineCode, string lineName)
    {
        //if (string.IsNullOrWhiteSpace(lineCode)) throw new ArgumentException("Line code cannot be empty.", nameof(lineCode));
        //if (string.IsNullOrWhiteSpace(lineName)) throw new ArgumentException("Line name cannot be empty.", nameof(lineName));
        RailLine = new RailLine(lineCode, lineName);
        IsModified = true;
    }

    public void SetRailStation(string lineCode, string stationCode, string stationName)
    {
        //if (string.IsNullOrWhiteSpace(stationCode)) throw new ArgumentException("Station code cannot be empty.", nameof(stationCode));
        //if (string.IsNullOrWhiteSpace(stationName)) throw new ArgumentException("Station name cannot be empty.", nameof(stationName));
        //if (string.IsNullOrWhiteSpace(lineCode)) throw new ArgumentException("Line code cannot be empty.", nameof(lineCode));
        RailStation = new RailStation(lineCode, stationCode, stationName);
        IsModified = true;
    }

    public void SetEkiToho(int ekiToho)
    {
        if (ekiToho < 0) throw new ArgumentException("EkiToho cannot be negative.", nameof(ekiToho));
        EkiToho = ekiToho;
        IsModified = true;
    }

}
