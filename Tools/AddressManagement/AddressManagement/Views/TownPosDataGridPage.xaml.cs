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

// TODO: Change the grid as appropriate for your app. Adjust the column definitions on DataGridPage.xaml.
// For more details, see the documentation at https://docs.microsoft.com/windows/communitytoolkit/controls/datagrid.
public sealed partial class TownPosDataGridPage : Page
{
    private readonly SqliteConnectionStringBuilder connectionStringBuilder;

    private string DataBaseFilePath => System.Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory) + Path.DirectorySeparatorChar + "test.db";

    Microsoft.UI.Dispatching.DispatcherQueue _dispatcherQueue = Microsoft.UI.Dispatching.DispatcherQueue.GetForCurrentThread();

    public ObservableCollection<TownPos> TownPosDataSource { get; } = new ObservableCollection<TownPos>();

    public TownPosDataGridViewModel ViewModel
    {
        get;
    }

    public TownPosDataGridPage()
    {
        ViewModel = App.GetService<TownPosDataGridViewModel>();
        InitializeComponent();

        //
        dg.ItemsSource = TownPosDataSource;

        connectionStringBuilder = new SqliteConnectionStringBuilder("Data Source=" + DataBaseFilePath);
    }

    public void Save(object sender, RoutedEventArgs e)
    {
        if (TownPosDataSource.Count < 1)
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
                    tableCmd.CommandText = "CREATE TABLE IF NOT EXISTS TownPos (" + // towns_coordinates
                        "AdministrativeDivisionCode TEXT NOT NULL," +
                        "TownCode TEXT NOT NULL," + // PRIMARY KEY
                        "Longitude TEXT NOT NULL," +
                        "Latitude TEXT NOT NULL," +
                        "CRS TEXT," +
                        "MapInfoLovel TEXT" +
                        ")";
                    tableCmd.ExecuteNonQuery();

                    // トランザクションコミット
                    tableCmd.Transaction.Commit();
                }
                catch (Exception ex)
                {
                    // トランザクションのロールバック
                    tableCmd.Transaction.Rollback();

                    Debug.WriteLine("DB Create Error: " + ex.Message);
                }
            }
            catch (System.Reflection.TargetInvocationException ex)
            {
                Debug.WriteLine("DB Create Error: " + ex.Message);
                if (ex.InnerException != null)
                    throw ex.InnerException;
            }
            catch (System.InvalidOperationException ex)
            {
                Debug.WriteLine("DB Create Error: " + ex.Message);
                if (ex.InnerException != null)
                    throw ex.InnerException;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("DB Create Error: " + ex.Message);
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("DB Create Error: " + ex.Message);
        }


        using (var connection = new SqliteConnection(connectionStringBuilder.ConnectionString))
        {
            connection.Open();

            using (var cmd = connection.CreateCommand())
            {
                cmd.Transaction = connection.BeginTransaction();
                try
                {
                    foreach (var hoge in TownPosDataSource)
                    {
                        var sqlInsertIntoRent = String.Format(
    "INSERT INTO TownPos " +
    "(AdministrativeDivisionCode, TownCode, Longitude, Latitude, CRS, MapInfoLovel) " +
    "VALUES ('{0}', '{1}', '{2}', '{3}', '{4}', '{5}')",
    hoge.AdministrativeDivisionCode,
    hoge.TownID,
    hoge.Longitude,
    hoge.Latitude,
    hoge.CRS,
    hoge.MapInfoLovel);

                        cmd.CommandText = sqlInsertIntoRent;

                        var InsertIntoRentResult = cmd.ExecuteNonQuery();
                        if (InsertIntoRentResult != 1)
                        {
                            // これいる?
                        }
                    }

                    //　コミット
                    cmd.Transaction.Commit();
                }
                catch (Exception ex)
                {
                    cmd.Transaction.Rollback();

                    Debug.WriteLine(ex.Message + " Add::Transaction.Commit");
                }
            }
        }

        Debug.WriteLine("Done");
    }

    public void Load(object sender, RoutedEventArgs e)
    {
        /*
                TownPosDataSource.Clear();
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

                                    TownPos hoge = new TownPos();
                                    hoge.AdministrativeDivisionCode = Convert.ToString(reader["AdministrativeDivisionCode"]);
                                    hoge.PrefectureName = Convert.ToString(reader["PrefectureName"]);
                                    hoge.Code = Convert.ToString(reader["PostalCode"]);
                                    hoge.CityName = Convert.ToString(reader["CityName"]);
                                    hoge.ChouName = Convert.ToString(reader["ChouName"]);

                                    TownPosDataSource.Add(hoge);

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
            csv.Context.RegisterClassMap<TownPosClassMapper>();

            var records = csv.GetRecords<TownPos>();

            foreach (var record in records)
            {
                TownPos obj = new TownPos();
                obj.AdministrativeDivisionCode = record.AdministrativeDivisionCode;
                obj.TownID = record.TownID;
                obj.Longitude = record.Longitude;
                obj.Latitude = record.Latitude;
                obj.CRS = record.CRS;
                obj.MapInfoLovel = record.MapInfoLovel;

                _dispatcherQueue.TryEnqueue(() =>
                {
                    TownPosDataSource.Add(obj);
                });

                await Task.Delay(1);
            }

        }

        Debug.WriteLine("Open Done");
    }

    public async void Open(object sender, RoutedEventArgs e)
    {
        TownPosDataSource.Clear();

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

    class TownPosMapper : CsvHelper.Configuration.ClassMap<TownPos>
    {
        public TownPosMapper()
        {
            AutoMap(CultureInfo.InvariantCulture);
        }
    }

    class TownPosClassMapper : CsvHelper.Configuration.ClassMap<TownPos>
    {
        public TownPosClassMapper()
        {
            Map(x => x.AdministrativeDivisionCode).Index(0);
            Map(x => x.TownID).Index(1);
            Map(x => x.Longitude).Index(3);
            Map(x => x.Latitude).Index(4);
            Map(x => x.CRS).Index(5);
            Map(x => x.MapInfoLovel).Index(6);
        }
    }
}
