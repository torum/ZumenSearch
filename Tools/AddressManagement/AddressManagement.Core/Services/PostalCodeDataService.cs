using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics;
using AddressManagement.Core.Contracts.Services;
using AddressManagement.Core.Models;
using Microsoft.Data.Sqlite;

namespace AddressManagement.Core.Services;

public class PostalCodeDataService : IPostalCodeDataService
{
    private readonly SqliteConnectionStringBuilder connectionStringBuilder;

    private string DataBaseFilePath => System.Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory) + Path.DirectorySeparatorChar + "Address.db";

    private List<PostalCode> _postalCodes;

    public PostalCodeDataService()
    {
        //_postalCodes = new List<PostalCode>(PostalCodes());
        connectionStringBuilder = new SqliteConnectionStringBuilder("Data Source=" + DataBaseFilePath);
    }

    private static IEnumerable<PostalCode> PostalCodes(SqliteConnectionStringBuilder connectionStringBuilder, string postalCode)
    {
        //return new List<PostalCode>();
        var list = new List<PostalCode>();

        if (string.IsNullOrEmpty(postalCode))
            return list;
        
        postalCode = postalCode.Replace("-", string.Empty);

        try
        {
            using (var connection = new SqliteConnection(connectionStringBuilder.ConnectionString))
            {
                connection.Open();

                using (var cmd = connection.CreateCommand())
                {

                    cmd.CommandText = $"SELECT * FROM postal_codes WHERE postal_code = '{postalCode}'";

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            PostalCode hoge = new PostalCode();
                            hoge.MunicipalityCode = Convert.ToString(reader["municipality_code"]);
                            hoge.Code = Convert.ToString(reader["postal_code"]);
                            hoge.PrefectureName = Convert.ToString(reader["prefecture_name"]);
                            hoge.SikuchousonName = Convert.ToString(reader["sikuchouson_name"]);
                            hoge.ChouikiName = Convert.ToString(reader["chouiki_name"]);

                            list.Add(hoge);
                        }
                    }
                }
            }
        }
        catch (System.Reflection.TargetInvocationException ex)
        {
            Debug.WriteLine("Opps. TargetInvocationException");

            if (ex.InnerException != null)
                throw ex.InnerException;
        }
        catch (System.InvalidOperationException ex)
        {
            Debug.WriteLine("Opps. InvalidOperationException");

            if (ex.InnerException != null)
                throw ex.InnerException;
        }
        catch (Exception ex)
        {
            if (ex.InnerException != null)
            {
                Debug.WriteLine(ex.InnerException.Message);
            }
            else
            {
                Debug.WriteLine(ex.Message);
            }
        }

        return list;
    }

    public async Task<IEnumerable<PostalCode>> GetPostalCodeDataAsync(string postalCode)
    {
        _postalCodes = new List<PostalCode>(PostalCodes(connectionStringBuilder, postalCode));

        await Task.CompletedTask;
        return _postalCodes;
    }

}
