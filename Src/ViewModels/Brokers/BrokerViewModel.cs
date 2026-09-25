using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using ZumenSearch.Models.Base;
using ZumenSearch.Models.Common;
using ZumenSearch.Services.Contracts;

namespace ZumenSearch.ViewModels.Brokers;

public sealed partial class BrokerViewModel : ObservableRecipient
{
    #region == Public Properties ==

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
            }

            if (_broker.Status == EnumEntryStatus.New)
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
        new() { Name = "宅建業者", Page = typeof(Views.Brokers.BasicPage).FullName! },
        new() { Name = "基本", Page = typeof(Views.Brokers.BasicPage).FullName! }
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

    #region == Private Variables ==

    private readonly string _id = string.Empty;

    // The Entry property holds the COPY of current RentResidential entry being edited.
    // Do not use it directly in the UI. Apply changes to this object in SaveAsync() to save the changes.
    // MainViewModel creates a new instance of this class and call EditorShell.SetEntry(EntryResidentialFull) and sets this property.
    private readonly Models.Base.PersonBase _broker;


    #endregion

    #region == Services ==

    private readonly IDataAccessService _dataAccessService;
    private readonly IDataAccessLocationService _dataAccessLocationService;
    private readonly IDispatcherService _dispatcherService;
    private readonly IDialogGenericService _dialogService;
    private readonly INavigationGenericService _navigationService;

    #endregion

    public BrokerViewModel(Models.Base.PersonBase broker,
        INavigationGenericService navigationService,
        IDialogGenericService dialogService,
        IDispatcherService dispatcherService,
        IDataAccessService dataAccessService,
        IDataAccessLocationService dataAccessLocationService)
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
        }

        // Ready to receive messages.
        this.IsActive = true;
    }

    #region == Public Methods ==

    public void DiscardChanges()
    {
        //DiscardUnsavedFiles();

        IsDirty = false;
    }

    #endregion

    #region == Private Methods ==

    private void PopulateValues()
    {
        Name = _broker.Name;

        //Remarks = _broker.Remarks;
    }

    private bool ValidateName()
    {
        if (string.IsNullOrWhiteSpace(Name))
        {
            InfoBarErrorMessage = "会社名（必須項目）が入力されていません。保存出来ませんでした。";

            IsNameHasError = true;

            //HasErrors = true;

            return false;
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
        IsNameHasError = false;

        return true;
    }

    private void SetValues()
    {
        if (!IsDirty)
        {
            return;
        }

        // TODO: Create PersonLegal and set it.

        _broker.Name = Name;

        _broker.PersonKind = EnumPersonKind.Legal;


        //_broker.Remarks = Remarks;

        // TODO: Set other properties
        // TODO: Don't forget to check if Helpers.Common.ReplaceZenkakuNumbers is needed.

    }

    #endregion

    #region == Commands ==

    [RelayCommand(CanExecute = nameof(CanSave))]
    public void Save()
    {
        if (!IsDirty)
        {
            return;
        }

        // Validate input.
        if (!ValidateName())
        {
            IsInfoBarErrorOpen = true;
            if (!_navigationService.IsCurrentPageSameAs("ZumenSearch.Views.Brokers.BasicPage"))
            {
                _navigationService.NavigateTo("ZumenSearch.Views.Brokers.BasicPage", this);
            }
            return;
        }

        SetValues();

        var resInsert = _dataAccessService.UpsertBroker(_broker);
        if (resInsert.IsError)
        {
            Debug.WriteLine("Error on update. @Save in BrokerViewModel");
            Debug.WriteLine(resInsert.Error.ErrText + Environment.NewLine + resInsert.Error.ErrDescription + Environment.NewLine + resInsert.Error.ErrPlace + Environment.NewLine + resInsert.Error.ErrPlaceParent);

            // TODO: fix format.
            var errText = resInsert.Error.ErrText + Environment.NewLine + resInsert.Error.ErrDescription + Environment.NewLine + resInsert.Error.ErrPlace + Environment.NewLine + resInsert.Error.ErrPlaceParent;
            InfoBarErrorMessage = errText;
            IsInfoBarErrorOpen = true;

        }
        else
        {
            Debug.WriteLine("No errors on update.");

            IsDirty = false;

            _broker.IsModified = false;
            _broker.Status = EnumEntryStatus.Saved;

            // Update title with dummy value.
            WindowTitle = string.Empty;

            // Clear error infobar.
            IsInfoBarErrorOpen = false;

            WeakReferenceMessenger.Default.Send(new Models.Messenger.BrokerUpdatedMessage(_broker));
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
