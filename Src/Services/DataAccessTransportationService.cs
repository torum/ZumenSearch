using Microsoft.Data.Sqlite;
using System.Collections.ObjectModel;
using System.Data;
using ZumenSearch.Models;

namespace ZumenSearch.Services;

public class DataAccessTransportationService : IDataAccessTransportationService
{
    private readonly SqliteConnectionStringBuilder connectionStringBuilder = new("Data Source=rail_lines.db");

    //private readonly ReaderWriterLockSlim _readerWriterLock = new();

    public DataAccessTransportationService()
    {
        //connectionStringBuilder = new SqliteConnectionStringBuilder("Data Source=rail_lines.db");

        //connectionStringBuilder.DataSource = "rail_line.db";
        //connectionStringBuilder.DataSource = "rail_stations.db";
    }

    public ObservableCollection<RailLine> GetRailLinesBy(string query)
    {
        var dataset = new ObservableCollection<RailLine>();

        /*
        if (string.IsNullOrEmpty(query))
        {
            return dataset;
        }
        */

        query = EscapeSingleQuote(query.Trim());

        connectionStringBuilder.DataSource = "rail_lines.db";

        using var connection = new SqliteConnection(connectionStringBuilder.ConnectionString);
        connection.Open();
        using var cmd = connection.CreateCommand();
        if (string.IsNullOrEmpty(query))
        {
            cmd.CommandText = "SELECT line_cd, line_name FROM rail_lines WHERE line_name NOT LIKE '%新幹線%'";
        }
        else
        {
            cmd.CommandText = string.Format("SELECT line_cd, line_name FROM rail_lines WHERE line_name LIKE '%{0}%' AND line_name NOT LIKE '%新幹線%'", query);
        }
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            var cd = Convert.ToString(reader["line_cd"]) ?? "";
            var name = Convert.ToString(reader["line_name"]) ?? "";
            if (cd is not null)
            {
                var rline = new RailLine
                {
                    LineCode = cd,
                    LineName = name
                };

                dataset.Add(rline);
            }
        }

        return dataset;
    }

    public ObservableCollection<RailStation> GetRailStationsBy(string _railLineCode, string query)
    {
        var dataset = new ObservableCollection<RailStation>();

        if (string.IsNullOrEmpty(_railLineCode))
        {
            return dataset;
        }

        query = EscapeSingleQuote(query.Trim());

        connectionStringBuilder.DataSource = "rail_stations.db";

        using var connection = new SqliteConnection(connectionStringBuilder.ConnectionString);
        connection.Open();
        using var cmd = connection.CreateCommand();

        if (string.IsNullOrEmpty(query))
        {
            cmd.CommandText = string.Format("SELECT station_cd, line_cd, station_name FROM rail_stations WHERE line_cd LIKE '{0}'", _railLineCode);
        }
        else
        {
            cmd.CommandText = string.Format("SELECT station_cd, line_cd, station_name FROM rail_stations WHERE line_cd LIKE '{0}' AND station_name LIKE '%{1}%'", _railLineCode, query);
        }

        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            var scd = Convert.ToString(reader["station_cd"]) ?? "";
            var lcd = Convert.ToString(reader["line_cd"]) ?? "";
            var name = Convert.ToString(reader["station_name"]) ?? "";
            if (scd is not null)
            {
                var rline = new RailStation
                {
                    StationCode = scd,
                    LineCode = lcd,
                    StationName = name
                };

                dataset.Add(rline);
            }
        }

        return dataset;
    }

    // ColumnExists check
    private static bool ColumnExists(IDataRecord dr, string columnName)
    {
        for (var i = 0; i < dr.FieldCount; i++)
        {
            if (dr.GetName(i).Equals(columnName, StringComparison.InvariantCultureIgnoreCase))
            {
                return true;
            }
        }
        return false; ;
    }

    private static string EscapeSingleQuote(string s)
    {
        return s is null ? string.Empty : s.Replace("'", "''");
    }

    private static string RestoreSingleQuote(string s)
    {
        return s is null ? string.Empty : s.Replace("''", "'");
    }
}
