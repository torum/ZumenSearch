using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using ZumenSearch.Domain.Enums;
using ZumenSearch.Domain.ValueObjects.Location;
using ZumenSearch.Infrastructure.DataAccess.Contracts;
using ZumenSearch.Presentation.Navigation;
using ZumenSearch.Presentation.Services.Contracts;
using ZumenSearch.Presentation.Shell.Models;

namespace ZumenSearch.Presentation.Features.Rent.Lessors.ViewModels;

/*
 * TODO: 
 * Add ErrorInfoBar title.
 */

public sealed partial class LessorViewModel : ObservableRecipient, IDisposable
{
    #region == Private variables and const ==

    private const string BasicPageName = NavigationKeys.RentLessorBasic;
    private readonly string _id = string.Empty;
    private readonly CancellationTokenSource _cts = new();

    // This property holds the COPY of current entity being edited.
    // Do not use it directly in the UI. Apply changes to this object in Save() to save the changes.
    private ZumenSearch.Domain.Entities.Base.PersonBase _lessor;

    #endregion

    #region == Services ==

    private readonly IDataAccessService _dataAccessService;
    private readonly IDataAccessLocationService _dataAccessLocationService;
    private readonly IDispatcherService _dispatcherService;
    private readonly IDialogGenericService _dialogService;
    private readonly INavigationGenericService _navigationService;

    #endregion

    public LessorViewModel(ZumenSearch.Domain.Entities.Base.PersonBase lessorBase, INavigationGenericService navigationService, IDialogGenericService dialogService, IDispatcherService dispatcherService, IDataAccessService dataAccessService, IDataAccessLocationService dataAccessLocationService)
    {
        _lessor = lessorBase;
        _id = lessorBase.Id;

        _navigationService = navigationService; 
        _dialogService = dialogService;
        _dispatcherService = dispatcherService;
        _dataAccessService = dataAccessService;
        _dataAccessLocationService = dataAccessLocationService;

        // Update title with dummy value.
        WindowTitle = string.Empty;

        if (_lessor.PersonKind == PersonKindType.Natural)
        {
            PersonKindIndex = 0;
        }
        else if (_lessor.PersonKind == PersonKindType.Legal)
        {
            PersonKindIndex = 1;
        }

        try
        {
            PopulateValues();

            // Reset errors
            IsNameLastHasError = false;
            //IsNameFirstHasError = false;
            IsNameCompanyHasError = false;

            // TODO: more.

            //HasErrors = false;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"LessorViewModel: {ex}");
        }
        finally
        {
            IsDirty = false;
            _lessor.SetIsModified(IsDirty);
        }

        // Ready to receive messages.
        //this.IsActive = true;
    }

    #region == Properties ==

    // TODO: Do I need this?
    //public string Id => _id;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
    public partial bool IsDirty { get; private set; }

    #region == 画面表示関連 ==

    public string WindowTitle
    {
        get
        {
            var str = $"{field}";

            if (!string.IsNullOrEmpty(Name))
            {
                str = $"{str}：{Name}";
                Debug.WriteLine(Name);
            }

            if (_lessor.Status == EntityStatus.New)
            {
                str = $"{str}：新規";
            }
            else
            {
                str = $"{str}：編集";
            }

            return str;
        }
        set
        {
            OnPropertyChanged();
        }
    } = "貸主";

    public ObservableCollection<Breadcrumb> BreadcrumbItems { get; set; } =
    [
        new() { Name = "貸主", Page = NavigationKeys.RentLessorBasic },
        new() { Name = "基本", Page = NavigationKeys.RentLessorBasic }
    ];

    #endregion

    #region == エラー関連 ==

    /*
    // TODO: update this to hold more info such as page so that it can be navigated to the page.
    [ObservableProperty]
    public partial bool HasErrors { get; private set; }
    */

    // InfoBarError is researved only for unsavable error.
    [ObservableProperty]
    public partial bool IsInfoBarErrorOpen { get; set; }

    [ObservableProperty]
    public partial string InfoBarErrorMessage { get; set; } = string.Empty;

    #endregion

    // Nameは直接編集バインドしない。あとで性と名（または法人格）をくっつける。
    public string Name
    {
        get => field ?? string.Empty; // Ensure a non-null value is returned
        set
        {
            if (SetProperty(ref field, value))
            {
                IsDirty = true;

                //ValidateName(value);

                // Update title with dummy value.
                WindowTitle = string.Empty;
            }
        }
    }

    // Natural 0 or Legal 1
    public int PersonKindIndex
    {
        get;
        set
        {
            if (SetProperty(ref field, value))
            {
                IsDirty = true;
            }
        }
    }

    public string NameFirst
    {
        get => field ?? string.Empty;
        set
        {
            if (SetProperty(ref field, value.Trim()))
            {
                if (!string.IsNullOrWhiteSpace(NameLast) && !string.IsNullOrWhiteSpace(NameFirst))
                {
                    Name = $"{NameLast} {NameFirst}";
                }
                else
                {
                    Name = $"{NameLast}{NameFirst}"; // prints either name without space.
                }
                IsDirty = true;
            }
        }
    }

    public string NameLast
    {
        get => field ?? string.Empty;
        set
        {
            if (SetProperty(ref field, value))
            {
                if (!string.IsNullOrWhiteSpace(NameLast) && !string.IsNullOrWhiteSpace(NameFirst))
                {
                    Name = $"{NameLast} {NameFirst}";
                }
                else
                {
                    Name = $"{NameLast}{NameFirst}"; // prints either name without space.
                }
                IsDirty = true;
            }
        }
    }

    [ObservableProperty]
    public partial bool IsNameLastHasError { get; private set; }

    public string NameCompany
    {
        get => field ?? string.Empty;
        set
        {
            if (SetProperty(ref field, value))
            {
                if (NameCompanyTypePosition == 0)
                {
                    Name = $"{NameCompanyType}{NameCompany}";
                }
                else
                {
                    Name = $"{NameCompany}{NameCompanyType}";
                }
                IsDirty = true;
            }
        }
    }

    [ObservableProperty]
    public partial bool IsNameCompanyHasError { get; private set; }

    public string NameCompanyType
    {
        get => field ?? string.Empty;
        set
        {
            if (SetProperty(ref field, value))
            {
                if (NameCompanyTypePosition == 0)
                {
                    Name = $"{NameCompanyType}{NameCompany}";
                }
                else
                {
                    Name = $"{NameCompany}{NameCompanyType}";
                }
                IsDirty = true;
            }
        }
    }

    // 0 前付け、1 後付け
    public int NameCompanyTypePosition
    {
        get;
        set
        {
            if (SetProperty(ref field, value))
            {
                if (value == 0)
                {
                    Name = $"{NameCompanyType}{NameCompany}";
                }
                else
                {
                    Name = $"{NameCompany}{NameCompanyType}";
                }
                IsDirty = true;
            }
        }
    }

    public string PhoneNumber
    {
        get;
        set
        {
            if (SetProperty(ref field, value)) IsDirty = true;
        }
    } = string.Empty;

    public string MobilePhoneNumber
    {
        get;
        set
        {
            if (SetProperty(ref field, value)) IsDirty = true;
        }
    } = string.Empty;

    public string ContactName
    {
        get;
        set
        {
            if (SetProperty(ref field, value)) IsDirty = true;
        }
    } = string.Empty;

    public string PostalCode
    {
        get;
        set
        {
            if (SetProperty(ref field, value)) IsDirty = true;
        }
    } = string.Empty;

    public string Address
    {
        get;
        set
        {
            if (SetProperty(ref field, value))
            {
                IsDirty = true;
                OnPropertyChanged(nameof(AddressPreview));
            }
        }
    } = string.Empty;

    public ObservableCollection<Prefecture> Prefectures { get; init; } = new(PrefectureMaster.Prefectures);

    public Prefecture? SelectedPrefecture
    {
        get;
        set
        {
            if (SetProperty(ref field, value))
            {
                IsDirty = true;
                SelectedCity = null;
                Cities = value is null ? [] : [.. _dataAccessLocationService.GetCountyAndCityByPref(value.Name).DistinctBy(p => p.Combined)];
                OnPropertyChanged(nameof(AddressPreview));
            }
        }
    }

    public ObservableCollection<CountyAndCity> Cities
    {
        get;
        private set { SetProperty(ref field, value); }
    } = [];

    public CountyAndCity? SelectedCity
    {
        get;
        set
        {
            if (SetProperty(ref field, value))
            {
                IsDirty = true;
                SelectedTown = null;
                Towns = value is null || SelectedPrefecture is null ? [] : [.. _dataAccessLocationService.GetWardAndOazaByPrefCountyCity(SelectedPrefecture.Name, value.County, value.City).DistinctBy(p => p.Combined)];
                OnPropertyChanged(nameof(AddressPreview));
            }
        }
    }

    public ObservableCollection<WardAndOaza> Towns
    {
        get;
        private set { SetProperty(ref field, value); }
    } = [];

    public WardAndOaza? SelectedTown
    {
        get;
        set
        {
            if (SetProperty(ref field, value))
            {
                IsDirty = true;
                SelectedChou = null;
                Chous = value is null || SelectedPrefecture is null || SelectedCity is null ? [] : [.. _dataAccessLocationService.GetChoumeByPrefCountyCityWardOaza(SelectedPrefecture.Name, SelectedCity.County, SelectedCity.City, value.Ward, value.Oaza).DistinctBy(p => p.Chou)];
                OnPropertyChanged(nameof(AddressPreview));
            }
        }
    }

    public ObservableCollection<Choume> Chous
    {
        get;
        private set { SetProperty(ref field, value); }
    } = [];

    public Choume? SelectedChou
    {
        get;
        set
        {
            if (SetProperty(ref field, value))
            {
                IsDirty = true;
                OnPropertyChanged(nameof(AddressPreview));
            }
        }
    }

    public string Edaban
    {
        get;
        set
        {
            if (SetProperty(ref field, value))
            {
                IsDirty = true;
                OnPropertyChanged(nameof(AddressPreview));
            }
        }
    } = string.Empty;

    public string AddressPreview
    {
        get
        {
            if (SelectedPrefecture is null)
            {
                return Address;
            }

            var edaban = string.IsNullOrEmpty(Edaban) ? string.Empty : $"-{Edaban}";
            return $"{SelectedPrefecture.Name}{SelectedCity?.Combined}{SelectedTown?.Combined}{SelectedChou?.Chou}{edaban}";
        }
    }

    // 備考
    public string Remarks
    {
        get;
        set
        {
            if (SetProperty(ref field, value))
            {
                IsDirty = true;
            }
        }
    } = string.Empty;

    #endregion

    #region == Public Methods ==

    public void DiscardChanges()
    {
        //DiscardUnsavedFiles();

        IsDirty = false;
    }

    public void ResetIsDirtyState()
    {
        IsDirty = false;
    }

    public void Dispose()
    {
        _cts?.Dispose();

        GC.SuppressFinalize(this);
    }

    #endregion

    #region == Private Methods ==

    private void PopulateValues()
    {
        Name = _lessor.Name;

        if (_lessor is ZumenSearch.Domain.Entities.Person.PersonNatural naturalPerson)
        {
            if (_lessor.PersonKind != PersonKindType.Natural)
            {
                // Something is wrong.
            }

            PersonKindIndex = 0;

            NameFirst = naturalPerson.NameFirst;
            NameLast = naturalPerson.NameLast;

            MobilePhoneNumber = naturalPerson.MobilePhoneNumber;
        }
        else if (_lessor is ZumenSearch.Domain.Entities.Person.PersonLegal legalPerson)
        {
            if (_lessor.PersonKind != PersonKindType.Legal)
            {
                // Something is wrong.
            }

            PersonKindIndex = 1;

            NameCompany = legalPerson.NameCompany;
            NameCompanyType = legalPerson.NameCompanyType;
            NameCompanyTypePosition = legalPerson.NameCompanyTypePosition;

            ContactName = legalPerson.ContactName;
        }
        else
        {
            // TODO: Raise Error
            return;
        }

        PhoneNumber = _lessor.PhoneNumber;
        PostalCode = _lessor.PostalCode;
        Address = _lessor.Address;

        var location = _lessor.AddressLocation;
        SelectedPrefecture = PrefectureMaster.Prefectures.FirstOrDefault(p => p.Name == location.Prefecture?.Name);
        if (SelectedPrefecture is not null)
        {
            SelectedCity = Cities.FirstOrDefault(p => p.County == location.CountyAndCity?.County && p.City == location.CountyAndCity?.City);
            if (SelectedCity is not null)
            {
                SelectedTown = Towns.FirstOrDefault(p => p.Ward == location.WardAndOaza?.Ward && p.Oaza == location.WardAndOaza?.Oaza);
                if (SelectedTown is not null)
                {
                    SelectedChou = Chous.FirstOrDefault(p => p.Chou == location.Choume?.Chou);
                }
            }
        }
        Edaban = location.Edaban;

        Remarks = _lessor.Remarks;
    }

    private bool ValidateName()
    {
        if (PersonKindIndex == 0)
        {
            if (string.IsNullOrWhiteSpace(NameLast))
            {
                InfoBarErrorMessage = "性（必須項目）が入力されていません。保存出来ませんでした。";

                IsNameLastHasError = true;

                //HasErrors = true;

                return false;
            }
        }

        if (PersonKindIndex == 1) 
        { 
            if (string.IsNullOrWhiteSpace(NameCompany))
            {
                InfoBarErrorMessage = "会社名（必須項目）が入力されていません。保存出来ませんでした。";

                IsNameCompanyHasError = true;

                //HasErrors = true;

                return false;
            }
        }
        /*


        var realLength = new StringInfo(Name).LengthInTextElements;
        if (realLength > 100)
        {
            InfoBarErrorMessage = "物件名は100文字以内で入力してください。保存出来ませんでした。";

            IsNameHasError = true;

            //HasErrors = true;

            return false;
        }
        */
        IsNameLastHasError = false;
        IsNameCompanyHasError = false;

        return true;
    }

    private void SetValues()
    {
        if (!IsDirty)
        {
            return;
        }

        ZumenSearch.Domain.Entities.Base.PersonBase newLessor;

        //if (_personKind == EnumPersonKind.Natural)
        if (PersonKindIndex == 0)
        {
            newLessor = new ZumenSearch.Domain.Entities.Person.PersonNatural(_lessor.Id, _lessor.Status);
        }
        //else if (_personKind == EnumPersonKind.Legal)
        else if (PersonKindIndex == 1)
        {
            newLessor = new ZumenSearch.Domain.Entities.Person.PersonLegal(_lessor.Id, _lessor.Status);
        }
        else
        {
            // TODO: Raise Error
            return;
        }

        //_lessor.Name = Name;

        if (newLessor is ZumenSearch.Domain.Entities.Person.PersonNatural naturalPerson)
        {
            naturalPerson.NameFirst = NameFirst;
            naturalPerson.NameLast = NameLast;

            naturalPerson.MobilePhoneNumber = MobilePhoneNumber;
        }
        else if (newLessor is ZumenSearch.Domain.Entities.Person.PersonLegal legalPerson)
        {
            legalPerson.NameCompany = NameCompany;
            legalPerson.NameCompanyType = NameCompanyType;
            legalPerson.NameCompanyTypePosition = NameCompanyTypePosition;

            legalPerson.ContactName = ContactName;
        }

        newLessor.PhoneNumber = PhoneNumber;
        newLessor.PostalCode = PostalCode;
        newLessor.Address = AddressPreview;
        newLessor.AddressLocation.SetMachiazaId(SelectedChou?.MachiazaId ?? SelectedTown?.MachiazaId ?? SelectedCity?.MachiazaId ?? string.Empty);
        newLessor.AddressLocation.SetPrefecture(SelectedPrefecture);
        newLessor.AddressLocation.SetCountyAndCity(SelectedCity);
        newLessor.AddressLocation.SetWardAndOaza(SelectedTown);
        newLessor.AddressLocation.SetChoume(SelectedChou);
        newLessor.AddressLocation.SetEdaban(Edaban);
        newLessor.AddressLocation.SetPostalCode(PostalCode);
        newLessor.Remarks = Remarks;

        _lessor = newLessor;
    }

    #endregion

    #region == Commands ==

    [RelayCommand(CanExecute = nameof(CanSave))]
    public async Task Save()
    {
        if (!IsDirty)
        {
            return;
        }

        // Validate input.
        if (!ValidateName())
        {
            IsInfoBarErrorOpen = true;
            if (!_navigationService.IsCurrentPageSameAs(BasicPageName))
            {
                _navigationService.NavigateTo(BasicPageName, this);
            }
            return;
        }

        SetValues();

        var resInsert = await Task.Run(() => _dataAccessService.UpsertRentLessor(_lessor), _cts.Token);
        if (resInsert.IsError)
        {
            Debug.WriteLine(
                resInsert.Error.Title + Environment.NewLine +
                resInsert.Error.Message + Environment.NewLine +
                resInsert.Error.Description + Environment.NewLine +
                resInsert.Error.Operation + Environment.NewLine +
                resInsert.Error.MethodName + Environment.NewLine +
                resInsert.Error.FullDump);

            InfoBarErrorMessage =
                resInsert.Error.Title + Environment.NewLine +
                resInsert.Error.Message + Environment.NewLine +
                resInsert.Error.Description + Environment.NewLine +
                resInsert.Error.Operation + Environment.NewLine +
                resInsert.Error.MethodName;

            IsInfoBarErrorOpen = true;
        }
        else
        {
            Debug.WriteLine("No errors on update.");

            IsDirty = false;

            _lessor.SetStatus(EntityStatus.Saved);
            _lessor.SetIsModified(false);

            // Update title with dummy value.
            WindowTitle = string.Empty;

            // Clear error infobar.
            IsInfoBarErrorOpen = false;

            WeakReferenceMessenger.Default.Send(new ZumenSearch.Presentation.Messaging.LessorUpdatedMessage(_lessor));
        }

    }
    private bool CanSave()
    {
        if (IsDirty)
        {
            return true;
        }
        return false;
    }

    #endregion

}
