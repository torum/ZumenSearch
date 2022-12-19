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

namespace AddressManagement.Views;

// TODO: Change the grid as appropriate for your app. Adjust the column definitions on DataGridPage.xaml.
// For more details, see the documentation at https://docs.microsoft.com/windows/communitytoolkit/controls/datagrid.
public sealed partial class TownCodeDataGridPage : Page
{

    public ObservableCollection<TownCode> TownCodeDataSource { get; } = new ObservableCollection<TownCode>();

    public TownCodeDataGridViewModel ViewModel
    {
        get;
    }

    public TownCodeDataGridPage()
    {
        ViewModel = App.GetService<TownCodeDataGridViewModel>();
        InitializeComponent();

        //
        dg.ItemsSource = TownCodeDataSource;
    }


    public async void Test(object sender, RoutedEventArgs e)
    {
        TownCodeDataSource.Clear();

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

        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = true,
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


            var records = csv.GetRecords<TownCode>();

            foreach (var record in records)
            {
                TownCode obj = new TownCode();
                obj.PrefectureName = record.PrefectureName;
                obj.ChouName = record.ChouName;
                obj.CountyName = record.CountyName;
                obj.Choume = record.Choume;
                obj.CityName = record.CityName;
                obj.PostalCode = record.PostalCode;
                obj.AdministrativeDivisionCodeID = record.AdministrativeDivisionCodeID;
                obj.ChouAzaCodeID = record.ChouAzaCodeID;
                obj.ChouAzaCodeKindID = record.ChouAzaCodeKindID;
                obj.CityName = record.CityName;
                obj.WardName= record.WardName;
                obj.KoazaName= record.KoazaName;

                TownCodeDataSource.Add(obj);
            }
        }

        dg.ItemsSource = TownCodeDataSource;
    }

    class TownCodeMapper : CsvHelper.Configuration.ClassMap<TownCode>
    {
        public TownCodeMapper()
        {
            AutoMap(CultureInfo.InvariantCulture);
        }
    }

    class PostalCodeClassMapper : CsvHelper.Configuration.ClassMap<TownCode>
    {
        public PostalCodeClassMapper()
        {
            Map(x => x.AdministrativeDivisionCodeID).Index(0);
            Map(x => x.ChouAzaCodeID).Index(1);
            Map(x => x.ChouAzaCodeKindID).Index(2);
            Map(x => x.PrefectureName).Index(3);
            Map(x => x.CountyName).Index(6);
            Map(x => x.CityName).Index(9);
            Map(x => x.WardName).Index(12);
            Map(x => x.ChouName).Index(15);
            Map(x => x.Choume).Index(18);
            Map(x => x.KoazaName).Index(21);
            Map(x => x.PostalCode).Index(35);
        }
    }
}
