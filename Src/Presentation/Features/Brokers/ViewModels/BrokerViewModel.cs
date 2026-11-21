using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using System.Collections.ObjectModel;
using ZumenSearch.Domain.Enums;
using ZumenSearch.Application.Configuration;
using ZumenSearch.Application.Results;
using ZumenSearch.Presentation.Shell.Models;
using ZumenSearch.Domain.Entities.Base;
using ZumenSearch.Domain.Entities.Person;
using ZumenSearch.Domain.ValueObjects.Location;


using ZumenSearch.Infrastructure.Configuration.Contracts;
using ZumenSearch.Infrastructure.DataAccess.Contracts;
using ZumenSearch.Presentation.Services.Contracts;

namespace ZumenSearch.Presentation.Features.Brokers.ViewModels;

/*
 * TODO: 
 * Add ErrorInfoBar title.
 */

public sealed partial class BrokerViewModel : ObservableRecipient, IDisposable
{
    #region == Private Variables ==

    private readonly string _id = string.Empty;
    private readonly CancellationTokenSource _cts = new();

    // This property holds the COPY of current entity being edited.
    // Do not use it directly in the UI. Apply changes to this object in Save() to save the changes.
    private ZumenSearch.Domain.Entities.Base.PersonBase _broker;

    #endregion

    #region == Services ==

    private readonly IDataAccessService _dataAccessService;
    private readonly IDataAccessLocationService _dataAccessLocationService;
    private readonly IDispatcherService _dispatcherService;
    private readonly IDialogGenericService _dialogService;
    private readonly INavigationGenericService _navigationService;

    #endregion

    public BrokerViewModel(ZumenSearch.Domain.Entities.Base.PersonBase broker, INavigationGenericService navigationService, IDialogGenericService dialogService, IDispatcherService dispatcherService, IDataAccessService dataAccessService, IDataAccessLocationService dataAccessLocationService)
    {
        _broker = broker;
        _id = broker.Id;

        _navigationService = navigationService; 
        _dialogService = dialogService;
        _dispatcherService = dispatcherService;
        _dataAccessService = dataAccessService;
        _dataAccessLocationService = dataAccessLocationService;

        // Update title with dummy value.
        WindowTitle = string.Empty;

        try
        {
            PopulateValues();

            // Reset errors
            IsNameHasError = false;
            // TODO: more.

            //HasErrors = false;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"BrokerViewModel: {ex}");
        }
        finally
        {
            IsDirty = false;
            _broker.SetIsModified(IsDirty);
        }

        // Ready to receive messages.
        //this.IsActive = true;
    }

    #region == Public Properties ==

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
            }

            if (_broker.Status == EntityStatus.New)
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
    } = "宅建業者";

    public ObservableCollection<Breadcrumb> BreadcrumbItems { get; set; } =
    [
        new() { Name = "宅建業者", Page = NavigationKeys.BrokerBasic },
        new() { Name = "基本", Page = NavigationKeys.BrokerBasic }
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

    [ObservableProperty]
    public partial bool IsNameHasError { get; private set; }

    #endregion

    // Nameは直接編集バインドしない。あとで会社名と法人格をくっつける。
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

    public int PersonKindIndex
    {
        get;
        set
        {
            if (SetProperty(ref field, value))
            {
                if (value == 0)
                {
                    Name = string.IsNullOrWhiteSpace(NameFirst) ? NameLast : $"{NameLast} {NameFirst}".Trim();
                }
                else if (value == 1)
                {
                    Name = NameCompanyTypePosition == 0 ? $"{NameCompanyType}{NameCompany}" : $"{NameCompany}{NameCompanyType}";
                }
                IsDirty = true;
            }
        }
    } = 1;

    public string NameFirst
    {
        get => field ?? string.Empty;
        set
        {
            if (SetProperty(ref field, value.Trim()))
            {
                if (PersonKindIndex == 0) Name = string.IsNullOrWhiteSpace(NameFirst) ? NameLast : $"{NameLast} {NameFirst}".Trim();
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
                if (PersonKindIndex == 0) Name = string.IsNullOrWhiteSpace(NameFirst) ? NameLast : $"{NameLast} {NameFirst}".Trim();
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
            if (SetProperty(ref field, value.Trim()))
            {
                if (PersonKindIndex == 1)
                {
                    Name = NameCompanyTypePosition == 0 ? $"{NameCompanyType}{NameCompany}" : $"{NameCompany}{NameCompanyType}";
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
            if (SetProperty(ref field, value.Trim()))
            {
                if (PersonKindIndex == 1)
                {
                    Name = NameCompanyTypePosition == 0 ? $"{NameCompanyType}{NameCompany}" : $"{NameCompany}{NameCompanyType}";
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
                if (PersonKindIndex == 1)
                {
                    Name = value == 0 ? $"{NameCompanyType}{NameCompany}" : $"{NameCompany}{NameCompanyType}";
                }
                IsDirty = true;
            }
        }
    }


    public string PhoneNumber
    {
        get;
        set { if (SetProperty(ref field, value)) IsDirty = true; }
    } = string.Empty;

    public string MobilePhoneNumber
    {
        get;
        set { if (SetProperty(ref field, value)) IsDirty = true; }
    } = string.Empty;

    public string ContactName
    {
        get;
        set { if (SetProperty(ref field, value)) IsDirty = true; }
    } = string.Empty;

    public string PostalCode
    {
        get;
        set { if (SetProperty(ref field, value)) IsDirty = true; }
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

    public void Dispose()
    {
        _cts?.Dispose();

        GC.SuppressFinalize(this);
    }

    public void ResetIsDirtyState()
    {
        IsDirty = false;
    }

    #endregion

    #region == Private Methods ==

    private void PopulateValues()
    {
        Name = _broker.Name;

        if (_broker is PersonNatural naturalPerson)
        {
            PersonKindIndex = 0;
            NameFirst = naturalPerson.NameFirst;
            NameLast = naturalPerson.NameLast;
            MobilePhoneNumber = naturalPerson.MobilePhoneNumber;
        }
        else if (_broker is PersonLegal legalPerson)
        {
            PersonKindIndex = 1;
            NameCompany = legalPerson.NameCompany;
            NameCompanyType = legalPerson.NameCompanyType;
            NameCompanyTypePosition = legalPerson.NameCompanyTypePosition;
            ContactName = legalPerson.ContactName;
        }
        else
        {
            throw new InvalidOperationException("Unsupported broker person type.");
        }

        PhoneNumber = _broker.PhoneNumber;
        PostalCode = _broker.PostalCode;
        Address = _broker.Address;

        var location = _broker.AddressLocation;
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

        Remarks = _broker.Remarks;

        IsDirty = false;
    }

    private bool ValidateName()
    {
        IsNameLastHasError = PersonKindIndex == 0 && string.IsNullOrWhiteSpace(NameLast);
        IsNameCompanyHasError = PersonKindIndex == 1 && string.IsNullOrWhiteSpace(NameCompany);

        if (IsNameLastHasError)
        {
            InfoBarErrorMessage = "姓（必須項目）が入力されていません。保存出来ませんでした。";
            return false;
        }
        if (IsNameCompanyHasError)
        {
            InfoBarErrorMessage = "会社名（必須項目）が入力されていません。保存出来ませんでした。";
            return false;
        }
        if (PersonKindIndex != 0 && PersonKindIndex != 1)
        {
            InfoBarErrorMessage = "個人または法人を選択してください。保存出来ませんでした。";
            return false;
        }
        return true;
    }

    private void SetValues()
    {
        if (!IsDirty)
        {
            return;
        }

        PersonBase newBroker;
        if (PersonKindIndex == 0)
        {
            newBroker = new PersonNatural(_broker.Id, _broker.Status)
            {
                NameLast = NameLast,
                NameFirst = NameFirst,
                MobilePhoneNumber = MobilePhoneNumber
            };
        }
        else
        {
            newBroker = new PersonLegal(_broker.Id, _broker.Status)
            {
                NameCompany = NameCompany,
                NameCompanyType = NameCompanyType,
                NameCompanyTypePosition = NameCompanyTypePosition,
                ContactName = ContactName
            };
        }

        newBroker.PhoneNumber = PhoneNumber;
        newBroker.PostalCode = PostalCode;
        newBroker.Address = AddressPreview;
        newBroker.AddressLocation.SetMachiazaId(SelectedChou?.MachiazaId ?? SelectedTown?.MachiazaId ?? SelectedCity?.MachiazaId ?? string.Empty);
        newBroker.AddressLocation.SetPrefecture(SelectedPrefecture);
        newBroker.AddressLocation.SetCountyAndCity(SelectedCity);
        newBroker.AddressLocation.SetWardAndOaza(SelectedTown);
        newBroker.AddressLocation.SetChoume(SelectedChou);
        newBroker.AddressLocation.SetEdaban(Edaban);
        newBroker.AddressLocation.SetPostalCode(PostalCode);
        newBroker.Remarks = Remarks;
        _broker = newBroker;
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
            if (!_navigationService.IsCurrentPageSameAs(NavigationKeys.BrokerBasic))
            {
                _navigationService.NavigateTo(NavigationKeys.BrokerBasic, this);
            }
            return;
        }

        SetValues();

        var resInsert = await Task.Run(() => _dataAccessService.UpsertBroker(_broker), _cts.Token);
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

            _broker.SetIsModified(false);
            _broker.SetStatus(EntityStatus.Saved);

            // Update title with dummy value.
            WindowTitle = string.Empty;

            // Clear error infobar.
            IsInfoBarErrorOpen = false;

            WeakReferenceMessenger.Default.Send(new ZumenSearch.Presentation.Messaging.BrokerUpdatedMessage(_broker));
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
