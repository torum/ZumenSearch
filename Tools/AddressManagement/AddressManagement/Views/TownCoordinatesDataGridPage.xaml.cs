using System.Collections.ObjectModel;
using System.Diagnostics;
using AddressManagement.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.Storage.Pickers;

using AddressManagement.Core.Models;
using CsvHelper.Configuration;
using CsvHelper;
using System.Globalization;
using System.Text;
using CsvHelper.Configuration.Attributes;

using Microsoft.Data.Sqlite;

namespace AddressManagement.Views;

public sealed partial class TownCoordinatesDataGridPage : Page
{
    private readonly SqliteConnectionStringBuilder connectionStringBuilder;

    private string DataBaseFilePath => System.Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory) + Path.DirectorySeparatorChar + "Address.db";

    readonly Microsoft.UI.Dispatching.DispatcherQueue _dispatcherQueue = Microsoft.UI.Dispatching.DispatcherQueue.GetForCurrentThread();

    public TownCoordinatesDataGridViewModel ViewModel
    {
        get;
    }

    public TownCoordinatesDataGridPage()
    {
        ViewModel = App.GetService<TownCoordinatesDataGridViewModel>();
        InitializeComponent();

        connectionStringBuilder = new SqliteConnectionStringBuilder("Data Source=" + DataBaseFilePath);
    }

    public void Save(object sender, RoutedEventArgs e)
    {
        if (ViewModel.TownCoordinatesDataSource.Count < 1)
            return;

        try
        {

            using var connection = new SqliteConnection(connectionStringBuilder.ConnectionString);
            try
            {
                connection.Open();

                using var tableCmd = connection.CreateCommand();
                
                tableCmd.Transaction = connection.BeginTransaction();
                try
                {
                    tableCmd.CommandText = "CREATE TABLE IF NOT EXISTS towns_coordinates (" +
                        "municipality_code TEXT NOT NULL," +
                        "town_id TEXT NOT NULL," + // PRIMARY KEY
                        "longitude TEXT NOT NULL," +
                        "latitude TEXT NOT NULL," +
                        "crs TEXT," +
                        "map_info_lovel TEXT" +
                        ")";

                    tableCmd.ExecuteNonQuery();

                    foreach (var hoge in ViewModel.TownCoordinatesDataSource)
                    {
                        var sqlInsertIntoRent = String.Format(
    "INSERT INTO towns_coordinates " +
    "(municipality_code, town_id, longitude, latitude, crs, map_info_lovel) " +
    "VALUES ('{0}', '{1}', '{2}', '{3}', '{4}', '{5}')",
    hoge.MunicipalityCode,
    hoge.TownID,
    hoge.Longitude,
    hoge.Latitude,
    hoge.CRS,
    hoge.MapInfoLovel);

                        tableCmd.CommandText = sqlInsertIntoRent;

                        var InsertIntoRentResult = tableCmd.ExecuteNonQuery();
                    }

                    tableCmd.Transaction.Commit();
                }
                catch (Exception ex)
                {
                    tableCmd.Transaction.Rollback();

                    Debug.WriteLine("DB Error: " + ex.Message);
                }
            }
            catch (System.Reflection.TargetInvocationException ex)
            {
                Debug.WriteLine("DB Error: " + ex.Message);
                if (ex.InnerException != null)
                    throw ex.InnerException;
            }
            catch (System.InvalidOperationException ex)
            {
                Debug.WriteLine("DB Error: " + ex.Message);
                if (ex.InnerException != null)
                    throw ex.InnerException;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("DB Error: " + ex.Message);
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("DB Error: " + ex.Message);
        }

        Debug.WriteLine("Insert Done");
    }

    public void Load(object sender, RoutedEventArgs e)
    {
        /*
                TownCoordinatesDataSource.Clear();
                try
                {
                    using (var connection = new SqliteConnection(connectionStringBuilder.ConnectionString))
                    {
                        connection.Open();

                        using (var cmd = connection.CreateCommand())
                        {

                            cmd.CommandText = "SELECT * FROM PostalCode";

                            using (var reader = cmd.ExecuteReader())
                            {
                                while (reader.Read())
                                {

                                    TownCoordinates hoge = new TownCoordinates();
                                    hoge.AdministrativeDivisionCode = Convert.ToString(reader["AdministrativeDivisionCode"]);
                                    hoge.PrefectureName = Convert.ToString(reader["PrefectureName"]);
                                    hoge.Code = Convert.ToString(reader["PostalCode"]);
                                    hoge.CityName = Convert.ToString(reader["CityName"]);
                                    hoge.ChouName = Convert.ToString(reader["ChouName"]);

                                    TownCoordinatesDataSource.Add(hoge);

                                }
                            }
                        }
                    }
                }
                catch (System.Reflection.TargetInvocationException ex)
                {
                    Debug.WriteLine("Opps. TargetInvocationException@DataAccess::GetSearchResultOfRentLiving");

                    if (ex.InnerException != null)
                        throw ex.InnerException;
                }
                catch (System.InvalidOperationException ex)
                {
                    Debug.WriteLine("Opps. InvalidOperationException@DataAccess::GetSearchResultOfRentLiving");

                    if (ex.InnerException != null)
                        throw ex.InnerException;
                }
                catch (Exception ex)
                {
                    if (ex.InnerException != null)
                    {
                        Debug.WriteLine(ex.InnerException.Message + " @DataAccess::GetSearchResultOfRentLiving");
                    }
                    else
                    {
                        Debug.WriteLine(ex.Message + " @DataAccess::GetSearchResultOfRentLiving");

                    }
                }

        */
    }

    private async void OpenDo(string filePath)
    {
        if (string.IsNullOrEmpty(filePath))
            return;

        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = true,
            Encoding = Encoding.UTF8,
        };
        using var reader = new StreamReader(filePath, Encoding.UTF8);
        using (var csv = new CsvReader(reader, config))
        {
            csv.Context.RegisterClassMap<TownCoordinatesClassMapper>();

            var records = csv.GetRecords<TownCoordinates>();

            foreach (var record in records)
            {
                TownCoordinates obj = new TownCoordinates();
                obj.MunicipalityCode = record.MunicipalityCode;
                obj.TownID = record.TownID;
                obj.Longitude = record.Longitude;
                obj.Latitude = record.Latitude;
                obj.CRS = record.CRS;
                obj.MapInfoLovel = record.MapInfoLovel;

                if (App.Current == null)
                    return;

                if (App.MainWindow == null)
                    return;

                _dispatcherQueue.TryEnqueue(() =>
                {
                    ViewModel.TownCoordinatesDataSource.Add(obj);
                });

                await Task.Delay(1);
            }
        }

        Debug.WriteLine("Open Done");
    }

    public async void FileOpen(object sender, RoutedEventArgs e)
    {
        ViewModel.TownCoordinatesDataSource.Clear();

        var filePicker = new FileOpenPicker();

        // Get the current window's HWND by passing in the Window object
        //var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
        // App.hwnd defined and set in App.xaml.cs
        var hwnd = App.hwnd;

        // Associate the HWND with the file picker
        WinRT.Interop.InitializeWithWindow.Initialize(filePicker, hwnd);

        // Use file picker like normal!
        filePicker.FileTypeFilter.Add("*");
        filePicker.SuggestedStartLocation = PickerLocationId.Desktop;

        var file = await filePicker.PickSingleFileAsync();

        if (file == null)
            return;

        await Task.Run(() => OpenDo(file.Path));

    }

    class TownCoordinatesMapper : CsvHelper.Configuration.ClassMap<TownCoordinates>
    {
        public TownCoordinatesMapper()
        {
            AutoMap(CultureInfo.InvariantCulture);
        }
    }

    class TownCoordinatesClassMapper : CsvHelper.Configuration.ClassMap<TownCoordinates>
    {
        public TownCoordinatesClassMapper()
        {
            Map(x => x.MunicipalityCode).Index(0);
            Map(x => x.TownID).Index(1);
            Map(x => x.Longitude).Index(3);
            Map(x => x.Latitude).Index(4);
            Map(x => x.CRS).Index(5);
            Map(x => x.MapInfoLovel).Index(6);
        }
    }
}
