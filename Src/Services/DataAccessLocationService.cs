using Microsoft.Data.Sqlite;
using System.Data;
using ZumenSearch.Models.Common;
using ZumenSearch.Services.Contracts;

namespace ZumenSearch.Services;

public class DataAccessLocationService : IDataAccessLocationService
{
    private readonly SqliteConnectionStringBuilder connectionStringBuilder;

    //private readonly ReaderWriterLockSlim _readerWriterLock = new();

    public DataAccessLocationService()
    {
        string dbPath = Path.Combine(AppContext.BaseDirectory, "Data", "mt_town_all.db");
        connectionStringBuilder = new SqliteConnectionStringBuilder($"Data Source={dbPath}");
    }

    public List<CountyAndCity> GetCountyAndCityByPref(string pref)
    {
        var dataset = new List<CountyAndCity>();

        using var connection = new SqliteConnection(connectionStringBuilder.ConnectionString);
        // TODO; Try catch for connection.Open() and log error if fails
        connection.Open();
        using var cmd = connection.CreateCommand();
        cmd.CommandText = string.Format("SELECT machiaza_id, county, city FROM mt_town_all WHERE pref LIKE '{0}'", pref);
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            var county = Convert.ToString(reader["county"]) ?? "";
            var city = Convert.ToString(reader["city"]) ?? "";
            var id = Convert.ToString(reader["machiaza_id"]);
            if (id is not null)
            {
                var ccty = new CountyAndCity(id, county, city);

                dataset.Add(ccty);
            }
        }

        return dataset;
    }

    public List<WardAndOaza> GetWardAndOazaByPrefCountyCity(string pref, string county, string city)
    {
        var dataset = new List<WardAndOaza>();

        using var connection = new SqliteConnection(connectionStringBuilder.ConnectionString);
        connection.Open();
        using var cmd = connection.CreateCommand();
        cmd.CommandText = string.Format("SELECT machiaza_id, ward, oaza_cho FROM mt_town_all WHERE pref LIKE '{0}' AND county LIKE '{1}' AND city LIKE '{2}'", pref, county, city);
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            var ward = Convert.ToString(reader["ward"]) ?? "";
            var oaza = Convert.ToString(reader["oaza_cho"]) ?? "";
            var id = Convert.ToString(reader["machiaza_id"]);
            if (id is not null)
            {
                var ccty = new WardAndOaza(id, ward, oaza);

                dataset.Add(ccty);
            }
        }

        return dataset;
    }

    public List<Choume> GetChoumeByPrefCountyCityWardOaza(string pref, string county, string city, string ward, string oaza)
    {
        var dataset = new List<Choume>();

        using var connection = new SqliteConnection(connectionStringBuilder.ConnectionString);
        connection.Open();
        using var cmd = connection.CreateCommand();
        cmd.CommandText = string.Format("SELECT machiaza_id, chome FROM mt_town_all WHERE pref LIKE '{0}' AND county LIKE '{1}' AND city LIKE '{2}' AND ward LIKE '{3}' AND oaza_cho LIKE '{4}'", pref, county, city, ward, oaza);
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            var cho = Convert.ToString(reader["chome"]) ?? "";
            var id = Convert.ToString(reader["machiaza_id"]);
            if (id is not null)
            {
                var ccty = new Choume(id, cho);

                dataset.Add(ccty);
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
