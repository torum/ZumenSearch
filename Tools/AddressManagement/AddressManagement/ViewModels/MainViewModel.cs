using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Xml.Linq;
using System.Xml.Schema;
using AddressManagement.Contracts.ViewModels;
using AddressManagement.Core.Contracts.Services;
using AddressManagement.Core.Models;

using CommunityToolkit.Mvvm.ComponentModel;
using Newtonsoft.Json.Linq;
using Windows.Networking;

namespace AddressManagement.ViewModels;

public partial class MainViewModel : ObservableValidator, INavigationAware
{
    private readonly IPrefectureDataService _prefectureDataService;
    private readonly IPostalCodeDataService _postalCodeDataService;

    public ObservableCollection<Prefecture> PrefectureDataSource { get; } = new ObservableCollection<Prefecture>();
    
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(FullAddress))]
    private Prefecture? selectedPrefecture;

    public ObservableCollection<string> SikuchousonDataSource { get; } = new ObservableCollection<string>();

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(FullAddress))]
    private string? selectedSikuchouson;

    public ObservableCollection<string> MultiplePostalCodeAddresses { get; } = new ObservableCollection<string>();

    private string selectedMultiplePostalCodeAddresses = string.Empty;
    public string SelectedMultiplePostalCodeAddresses
    {
        get => selectedMultiplePostalCodeAddresses;
        set
        {
            //SetProperty(ref postalCode, value, true);
            SetProperty(ref selectedMultiplePostalCodeAddresses, value, false);
            //ValidateProperty(PostalCode, nameof(PostalCode));

            // TODO:
        }
    }

    [ObservableProperty]
    public string errorMessages = string.Empty;

    public string FullAddress => $"{SelectedPrefecture?.PrefectureName} {SelectedSikuchouson}";

    [ObservableProperty]
    private bool postalCodeReturnedMultipleAddresses = false;

    private string postalCode = "498-0000";//string.Empty;

    //[Required(ErrorMessage = "郵便番号の入力は必須です。")]
    //[MinLength(7, ErrorMessage = "郵便番号は7桁で入力してください。")]
    //[MaxLength(8, ErrorMessage = "郵便番号は7桁以内で入力してください。")]
    [RegularExpression("^[0-9]{3}-?[0-9]{4}$", ErrorMessage = "郵便番号は半角英数のxxx-xxxxまたはxxxxxxxの形式で入力してください。")]
    public string PostalCode
    {
        get => postalCode;
        set
        {
            //SetProperty(ref postalCode, value, true);
            SetProperty(ref postalCode, value, false);
            //ValidateProperty(PostalCode, nameof(PostalCode));

            if (GetErrors(nameof(PostalCode)).Count() > 0)
            {
                PostalCodeErrorMessage = string.Join(", ", GetErrors(nameof(PostalCode)).Select(e => e.ErrorMessage));
                PostalCodeHasError = true;
            }
            else
            {
                PostalCodeErrorMessage = string.Empty;
                PostalCodeHasError = false;

                GetAddressesFromPostalCode(postalCode);
            }
        }
    }

    [ObservableProperty]
    private string postalCodeErrorMessage = string.Empty;

    [ObservableProperty]
    private bool postalCodeHasError = false;

    /*
    partial void OnPostalCodeChanging(string postalCode)
    {
        Debug.WriteLine($"The name is about to change to {postalCode}!");
    }

    partial void OnPostalCodeChanged(string postalCode)
    {
        Debug.WriteLine($"The name just changed to {postalCode}!");

        ValidateProperty(PostalCode);
    }
    */

    public MainViewModel(IPrefectureDataService prefectureDataService, IPostalCodeDataService postalCodeDataService)
    {
        _prefectureDataService = prefectureDataService;
        _postalCodeDataService = postalCodeDataService;

        ErrorsChanged += (sender, arg) => { this.test(arg); };


        PopulatePrefecture();

    }

    private void test(DataErrorsChangedEventArgs arg)
    {
        if (HasErrors)
            ErrorMessages = "入力項目にエラーがあります。";
        else
            ErrorMessages = string.Empty;


        //string message = string.Join(Environment.NewLine, GetErrors().Select(e => e.ErrorMessage));
        //Debug.WriteLine(message);
    }

    private async void PopulatePrefecture()
    {
        PrefectureDataSource.Clear();

        var data = await _prefectureDataService.GetPrefectureDataAsync();

        foreach (var item in data)
        {
            PrefectureDataSource.Add(item);
        }
    }

    private async void GetAddressesFromPostalCode(string value)
    {
        SelectedPrefecture = null;

        SikuchousonDataSource.Clear();

        MultiplePostalCodeAddresses.Clear();

        postalCodeReturnedMultipleAddresses = false;

        var data = await _postalCodeDataService.GetPostalCodeDataAsync(value);

        if (data.Count() <= 0)
        {
            postalCodeReturnedMultipleAddresses = false;

            // show error message?

            return;
        }
        else if (data.Count() > 1)
        {
            postalCodeReturnedMultipleAddresses = true;
        }
        else
        {
            postalCodeReturnedMultipleAddresses = false;
        }

        var i = 0;
        foreach (var item in data)
        {
            MultiplePostalCodeAddresses.Add(item.PrefectureName + item.SikuchousonName + item.ChouikiName);

            //SikuchousonDataSource.Add(item.SikuchousonName);

            if (i == 0)
            {
                //SelectedPrefecture = PrefectureDataSource.Where(x => x.PrefectureName == item.PrefectureName).FirstOrDefault();
                //SelectedSikuchouson = SikuchousonDataSource.FirstOrDefault();//item.SikuchousonName;
            }

            i++;
        }

    }

    public void OnNavigatedTo(object parameter)
    {
        /*
        PrefectureDataSource.Clear();

        var data = await _prefectureDataService.GetGridDataAsync();

        foreach (var item in data)
        {
            PrefectureDataSource.Add(item);
        }
        */
    }

    public void OnNavigatedFrom()
    {
    }


}
