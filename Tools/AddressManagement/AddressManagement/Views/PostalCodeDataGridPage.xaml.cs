﻿using System.Collections.ObjectModel;
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

using Microsoft.Data.Sqlite;


namespace AddressManagement.Views;

public sealed partial class PostalCodeDataGridPage : Page
{
    private readonly SqliteConnectionStringBuilder connectionStringBuilder;

    private string DataBaseFilePath => System.Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory) + Path.DirectorySeparatorChar + "Address.db";

    readonly Microsoft.UI.Dispatching.DispatcherQueue _dispatcherQueue = Microsoft.UI.Dispatching.DispatcherQueue.GetForCurrentThread();

    public PostalCodeDataGridViewModel ViewModel
    {
        get;
    }

    public PostalCodeDataGridPage()
    {
        ViewModel = App.GetService<PostalCodeDataGridViewModel>();
        InitializeComponent();

        connectionStringBuilder = new SqliteConnectionStringBuilder("Data Source=" + DataBaseFilePath);
    }

    public void Save(object sender, RoutedEventArgs e)
    {
        if (ViewModel.PostalCodeDataSource.Count < 1)
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
                    // Create table if not exists.
                    tableCmd.CommandText = "CREATE TABLE IF NOT EXISTS postal_codes (" +
                        "municipality_code TEXT NOT NULL," +
                        "postal_code TEXT NOT NULL," + // PRIMARY KEY
                        "prefecture_name TEXT NOT NULL," +
                        "sikuchouson_name TEXT," +
                        "chouiki_name TEXT" +
                        ")";

                    tableCmd.ExecuteNonQuery();

                    // Insert data
                    foreach (var hoge in ViewModel.PostalCodeDataSource)
                    {
                        var sqlInsertIntoRent = String.Format(
    "INSERT INTO postal_codes " +
    "(municipality_code, postal_code, prefecture_name, sikuchouson_name, chouiki_name) " +
    "VALUES ('{0}', '{1}', '{2}', '{3}', '{4}')",
    hoge.MunicipalityCode,
    hoge.Code,
    hoge.PrefectureName,
    hoge.SikuchousonName,
    hoge.ChouikiName);

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


    }

    private async void OpenDo(string filePath)
    {
        if (string.IsNullOrEmpty(filePath))
            return;

        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = false,
            Encoding = Encoding.UTF8,
        };
        using var reader = new StreamReader(filePath, Encoding.UTF8);
        using (var csv = new CsvReader(reader, config))
        {
            csv.Context.RegisterClassMap<PostalCodeClassMapper>();

            var records = csv.GetRecords<PostalCode>();

            foreach (var record in records)
            {
                PostalCode obj = new PostalCode();
                obj.PrefectureName = record.PrefectureName;
                obj.ChouikiName = record.ChouikiName;
                obj.SikuchousonName = record.SikuchousonName;
                obj.MunicipalityCode = record.MunicipalityCode;
                obj.Code = record.Code;

                _dispatcherQueue.TryEnqueue(() =>
                {
                    ViewModel.PostalCodeDataSource.Add(obj);
                });

                await Task.Delay(1);
            }
        }

        Debug.WriteLine("Open Done");
    }

    public async void FileOpen(object sender, RoutedEventArgs e)
    {
        ViewModel.PostalCodeDataSource.Clear();

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
            Map(x => x.MunicipalityCode).Index(0);
            Map(x => x.Code).Index(2);
            Map(x => x.PrefectureName).Index(6);
            Map(x => x.SikuchousonName).Index(7);
            Map(x => x.ChouikiName).Index(8);
        }
    }
}
