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
public sealed partial class PostalCodeDataGridPage : Page
{
    private readonly SqliteConnectionStringBuilder connectionStringBuilder;

    //private string _dataBaseFilePath;
    public string DataBaseFilePath
    {
        get
        {
            return "C:\\Users\\torum\\Desktop\\test.db";
        }
    }

    public ObservableCollection<PostalCode> PostalCodeDataSource { get; } = new ObservableCollection<PostalCode>();

    public PostalCodeDataGridViewModel ViewModel
    {
        get;
    }

    public PostalCodeDataGridPage()
    {
        ViewModel = App.GetService<PostalCodeDataGridViewModel>();
        InitializeComponent();

        //
        dg.ItemsSource = PostalCodeDataSource;

        connectionStringBuilder = new SqliteConnectionStringBuilder("Data Source=" + DataBaseFilePath);
    }

    public async void Save(object sender, RoutedEventArgs e)
    {
        if (PostalCodeDataSource.Count < 1)
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
                    tableCmd.CommandText = "CREATE TABLE IF NOT EXISTS PostalCode (" +
                        "AdministrativeDivisionCode TEXT NOT NULL," +
                        "PostalCode TEXT NOT NULL," + // PRIMARY KEY
                        "PrefectureName TEXT NOT NULL," +
                        "CityName TEXT," +
                        "ChouName TEXT" +
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
                throw ex.InnerException;
            }
            catch (System.InvalidOperationException ex)
            {
                Debug.WriteLine("DB Create Error: " + ex.Message);
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
                    foreach (var hoge in PostalCodeDataSource)
                    {
                        var sqlInsertIntoRent = String.Format(
    "INSERT INTO PostalCode " +
    "(AdministrativeDivisionCode, PostalCode, PrefectureName, CityName, ChouName) " +
    "VALUES ('{0}', '{1}', '{2}', '{3}', '{4}')",
    hoge.AdministrativeDivisionCodeID,
    hoge.Code,
    hoge.PrefectureName,
    hoge.CityName,
    hoge.ChouName);

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

    public async void Load(object sender, RoutedEventArgs e)
    {
        PostalCodeDataSource.Clear();
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

                            PostalCode hoge = new PostalCode();
                            hoge.AdministrativeDivisionCodeID = Convert.ToString(reader["AdministrativeDivisionCode"]);
                            hoge.PrefectureName = Convert.ToString(reader["PrefectureName"]);
                            hoge.Code = Convert.ToString(reader["PostalCode"]);
                            hoge.CityName = Convert.ToString(reader["CityName"]);
                            hoge.ChouName = Convert.ToString(reader["ChouName"]);

                            PostalCodeDataSource.Add(hoge);

                        }
                    }
                }
            }
        }
        catch (System.Reflection.TargetInvocationException ex)
        {
            Debug.WriteLine("Opps. TargetInvocationException@DataAccess::GetSearchResultOfRentLiving");

            throw ex.InnerException;
        }
        catch (System.InvalidOperationException ex)
        {
            Debug.WriteLine("Opps. InvalidOperationException@DataAccess::GetSearchResultOfRentLiving");

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

    }

    public async void Open(object sender, RoutedEventArgs e)
    {
        PostalCodeDataSource.Clear();

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

        Debug.WriteLine(file.Path);

        if (file == null)
            return;

        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = false,
            Encoding = Encoding.UTF8,
        };
        using var reader = new StreamReader(file.Path, Encoding.UTF8);
        using (var csv = new CsvReader(reader, config))
        {
            csv.Context.RegisterClassMap<PostalCodeClassMapper>();
            /*
             
            //一行毎の読み込み
            csv.Read();
            //ヘッダを読み込み
            csv.ReadHeader();
            //行毎に読み込みと処理
            while (csv.Read())
            {
                var record = csv.GetRecord<Foo>();
                Console.WriteLine(record.Id);
            }
            */


            var records = csv.GetRecords<PostalCode>();

            foreach (var record in records)
            {
                PostalCode obj = new PostalCode();
                obj.PrefectureName = record.PrefectureName;
                obj.ChouName = record.ChouName;
                obj.CityName = record.CityName;
                obj.AdministrativeDivisionCodeID = record.AdministrativeDivisionCodeID;
                obj.CityName = record.CityName;
                obj.Code = record.Code;

                PostalCodeDataSource.Add(obj);
            }

        }

        dg.ItemsSource = PostalCodeDataSource;
    }

    class PostalCodeMapper : CsvHelper.Configuration.ClassMap<PostalCode>
    {
        public PostalCodeMapper()
        {
            AutoMap(CultureInfo.InvariantCulture);
        }
    }

    class PostalCodeClassMapper : CsvHelper.Configuration.ClassMap<PostalCode>
    {
        public PostalCodeClassMapper()
        {
            Map(x => x.AdministrativeDivisionCodeID).Index(0);
            Map(x => x.Code).Index(2);
            Map(x => x.PrefectureName).Index(6);
            Map(x => x.CityName).Index(7);
            Map(x => x.ChouName).Index(8);
        }
    }
}
