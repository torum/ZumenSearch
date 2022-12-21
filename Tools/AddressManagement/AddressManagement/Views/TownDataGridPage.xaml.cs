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
using System.Data.Common;

namespace AddressManagement.Views;

public sealed partial class TownDataGridPage : Page
{
    private readonly SqliteConnectionStringBuilder connectionStringBuilder;

    private string DataBaseFilePath => System.Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory) + Path.DirectorySeparatorChar + "Address.db";

    readonly Microsoft.UI.Dispatching.DispatcherQueue _dispatcherQueue = Microsoft.UI.Dispatching.DispatcherQueue.GetForCurrentThread();

    public TownDataGridViewModel ViewModel
    {
        get;
    }

    public TownDataGridPage()
    {
        ViewModel = App.GetService<TownDataGridViewModel>();
        InitializeComponent();

        connectionStringBuilder = new SqliteConnectionStringBuilder("Data Source=" + DataBaseFilePath);
    }

    private void SaveDo()
    {
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
                    tableCmd.CommandText = "CREATE TABLE IF NOT EXISTS towns (" +
                        "municipality_code TEXT NOT NULL," +
                        "town_id TEXT NOT NULL," + // PRIMARY KEY
                        "chouaza_type TEXT," +
                        "prefecture_name TEXT," +
                        "county_name TEXT," +
                        "sikuchouson_name TEXT," +
                        "ward_name TEXT," +
                        "town_name TEXT," +
                        "choume TEXT," +
                        "koaza_name TEXT," +
                        "postal_code TEXT" +
                        ")";

                    tableCmd.ExecuteNonQuery();

                    foreach (var hoge in ViewModel.TownDataSource)
                    {
                        var sqlInsertIntoRent = String.Format(
    "INSERT INTO towns " +
    "(municipality_code, town_id, chouaza_type, prefecture_name, county_name, sikuchouson_name, ward_name, town_name, choume, koaza_name, postal_code) " +
    "VALUES ('{0}', '{1}', '{2}', '{3}', '{4}', '{5}', '{6}', '{7}', '{8}', '{9}', '{10}')",
    hoge.MunicipalityCode,
    hoge.TownID,
    hoge.ChouAzaType,
    hoge.PrefectureName,
    hoge.CountyName,
    hoge.SikuchousonName,
    hoge.WardName,
    hoge.TownName,
    hoge.Choume,
    hoge.KoazaName,
    hoge.PostalCode
    );
                    
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
    }

    public async void Save(object sender, RoutedEventArgs e)
    {
        if (ViewModel.TownDataSource.Count < 1)
            return;

        await Task.Run(() => SaveDo());

        Debug.WriteLine("Insert Done");
    }

    public void Load(object sender, RoutedEventArgs e)
    {
    
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
            csv.Context.RegisterClassMap<TownCodeClassMapper>();

            var records = csv.GetRecords<Town>();

            foreach (var record in records)
            {
                Town obj = new Town();
                obj.PrefectureName = record.PrefectureName;
                obj.TownName = record.TownName;
                obj.CountyName = record.CountyName;
                obj.Choume = record.Choume;
                obj.SikuchousonName = record.SikuchousonName;
                obj.PostalCode = record.PostalCode;
                obj.MunicipalityCode = record.MunicipalityCode;
                obj.TownID = record.TownID;
                obj.ChouAzaType = record.ChouAzaType;
                obj.WardName = record.WardName;
                obj.KoazaName = record.KoazaName;

                _dispatcherQueue.TryEnqueue(() =>
                {
                    ViewModel.TownDataSource.Add(obj);
                });

                await Task.Delay(1);
            }
        }

        Debug.WriteLine("Open Done");
    }

    public async void FileOpen(object sender, RoutedEventArgs e)
    {
        ViewModel.TownDataSource.Clear();

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

    class TownCodeMapper : CsvHelper.Configuration.ClassMap<Town>
    {
        public TownCodeMapper()
        {
            AutoMap(CultureInfo.InvariantCulture);
        }
    }

    class TownCodeClassMapper : CsvHelper.Configuration.ClassMap<Town>
    {
        public TownCodeClassMapper()
        {
            Map(x => x.MunicipalityCode).Index(0);
            Map(x => x.TownID).Index(1);
            Map(x => x.ChouAzaType).Index(2);
            Map(x => x.PrefectureName).Index(3);
            Map(x => x.CountyName).Index(6);
            Map(x => x.SikuchousonName).Index(9);
            Map(x => x.WardName).Index(12);
            Map(x => x.TownName).Index(15);
            Map(x => x.Choume).Index(18);
            Map(x => x.KoazaName).Index(21);
            Map(x => x.PostalCode).Index(35);
        }
    }
}
