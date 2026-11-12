using Microsoft.Data.Sqlite;
using System.Collections.ObjectModel;
using System.Data;
using System.Globalization;
using ZumenSearch.Models.Transportation;
using ZumenSearch.Services.Contracts;

namespace ZumenSearch.Services;

// Repositories

public class DataAccessTransportationService : IDataAccessTransportationService
{
    private readonly SqliteConnectionStringBuilder _connectionStringBuilder;// = new("Data Source=rail_lines.db");
    private readonly string railLineDbPath;
    private readonly string railStationDbPath;


    //private readonly ReaderWriterLockSlim _readerWriterLock = new();

    public DataAccessTransportationService()
    {
        //_connectionStringBuilder = new SqliteConnectionStringBuilder("Data Source=rail_lines.db");

        //_connectionStringBuilder.DataSource = "rail_lines.db";
        //_connectionStringBuilder.DataSource = "rail_stations.db";

        railLineDbPath = Path.Combine(AppContext.BaseDirectory, "Data", "rail_lines.db");
        railStationDbPath = Path.Combine(AppContext.BaseDirectory, "Data", "rail_stations.db");
        _connectionStringBuilder = new SqliteConnectionStringBuilder($"Data Source={railLineDbPath};");
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

        _connectionStringBuilder.DataSource = railLineDbPath;//"rail_lines.db";

        using var connection = new SqliteConnection(_connectionStringBuilder.ConnectionString);

        // TODO: try catch
        connection.Open();
        using var cmd = connection.CreateCommand();
        if (string.IsNullOrEmpty(query))
        {
            cmd.CommandText = "SELECT line_cd, line_name FROM rail_lines WHERE line_name NOT LIKE '%新幹線%'";
        }
        else
        {
            cmd.CommandText = string.Format(CultureInfo.InvariantCulture, "SELECT line_cd, line_name FROM rail_lines WHERE line_name LIKE '%{0}%' AND line_name NOT LIKE '%新幹線%'", query);
        }
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            var cd = Convert.ToString(reader["line_cd"], CultureInfo.InvariantCulture) ?? "";
            var name = Convert.ToString(reader["line_name"], CultureInfo.InvariantCulture) ?? "";
            if (cd is not null)
            {
                var rline = new RailLine(cd,name);

                dataset.Add(rline);
            }
        }

        return dataset;
    }

    public ObservableCollection<RailStation> GetRailStationsBy(string railLineCode, string query)
    {
        var dataset = new ObservableCollection<RailStation>();

        if (string.IsNullOrEmpty(railLineCode))
        {
            return dataset;
        }

        query = EscapeSingleQuote(query.Trim());

        _connectionStringBuilder.DataSource = railStationDbPath;// = "rail_stations.db";

        using var connection = new SqliteConnection(_connectionStringBuilder.ConnectionString);
        connection.Open();
        using var cmd = connection.CreateCommand();

        if (string.IsNullOrEmpty(query))
        {
            cmd.CommandText = string.Format(CultureInfo.InvariantCulture, "SELECT station_cd, line_cd, station_name FROM rail_stations WHERE line_cd LIKE '{0}'", railLineCode);
        }
        else
        {
            cmd.CommandText = string.Format(CultureInfo.InvariantCulture, "SELECT station_cd, line_cd, station_name FROM rail_stations WHERE line_cd LIKE '{0}' AND station_name LIKE '%{1}%'", railLineCode, query);
        }

        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            var lcd = Convert.ToString(reader["line_cd"], CultureInfo.InvariantCulture) ?? "";
            var scd = Convert.ToString(reader["station_cd"], CultureInfo.InvariantCulture) ?? "";
            var name = Convert.ToString(reader["station_name"], CultureInfo.InvariantCulture) ?? "";
            if (!string.IsNullOrWhiteSpace(scd))
            {
                var rline = new RailStation(lcd, scd, name);

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
            if (dr.GetName(i).Equals(columnName, StringComparison.OrdinalIgnoreCase))
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
