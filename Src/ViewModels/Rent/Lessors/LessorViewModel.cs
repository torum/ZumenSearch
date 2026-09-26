using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using ZumenSearch.Models;
using ZumenSearch.Models.Base;
using ZumenSearch.Models.Common;
using ZumenSearch.Services.Contracts;

namespace ZumenSearch.ViewModels.Rent.Lessors;

public sealed partial class LessorViewModel : ObservableRecipient
{
    private const string BasicPageName = "ZumenSearch.Views.Rent.Lessors.BasicPage";

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
                Debug.WriteLine(Name);
            }

            if (_lessorBase.Status == EnumEntryStatus.New)
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
        new() { Name = "貸主", Page = typeof(Views.Rent.Lessors.BasicPage).FullName! },
        new() { Name = "基本", Page = typeof(Views.Rent.Lessors.BasicPage).FullName! }
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
    public int PersonKindIndex { get;
        set
        {
            if (SetProperty(ref field, value))
            {
                IsDirty = true;
            }
        }
    } = 0;

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
    private Models.Base.PersonBase _lessorBase;

    #endregion

    #region == Services ==

    private readonly IDataAccessService _dataAccessService;
    private readonly IDataAccessLocationService _dataAccessLocationService;
    private readonly IDispatcherService _dispatcherService;
    private readonly IDialogGenericService _dialogService;
    private readonly INavigationGenericService _navigationService;

    #endregion

    public LessorViewModel(Models.Base.PersonBase lessorBase,
        INavigationGenericService navigationService,
        IDialogGenericService dialogService,
        IDispatcherService dispatcherService,
        IDataAccessService dataAccessService,
        IDataAccessLocationService dataAccessLocationService)
    {
        _lessorBase = lessorBase;
        _id = lessorBase.Id;

        _navigationService = navigationService; 
        _dialogService = dialogService;
        _dispatcherService = dispatcherService;
        _dataAccessService = dataAccessService;
        _dataAccessLocationService = dataAccessLocationService;

        // Update title with dummy value.
        WindowTitle = string.Empty;

        if (_lessorBase.PersonKind == EnumPersonKind.Natural)
        {
            PersonKindIndex = 0;
        }
        else if (_lessorBase.PersonKind == EnumPersonKind.Legal)
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
        Name = _lessorBase.Name;

        if (_lessorBase is PersonNatural naturalPerson)
        {
            if (_lessorBase.PersonKind != EnumPersonKind.Natural)
            {
                // Something is wrong.
            }

            PersonKindIndex = 0;

            NameFirst = naturalPerson.NameFirst;
            NameLast = naturalPerson.NameLast;
        }
        else if (_lessorBase is PersonLegal legalPerson)
        {
            if (_lessorBase.PersonKind != EnumPersonKind.Legal)
            {
                // Something is wrong.
            }

            PersonKindIndex = 1;

            NameCompany = legalPerson.NameCompany;
            NameCompanyType = legalPerson.NameCompanyType;
            NameCompanyTypePosition = legalPerson.NameCompanyTypePosition;
        }
        else
        {
            // TODO: Raise Error
            return;
        }


        Remarks = _lessorBase.Remarks;
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

        Models.Base.PersonBase newLessor;

        //if (_personKind == EnumPersonKind.Natural)
        if (PersonKindIndex == 0)
        {
            newLessor = new Models.PersonNatural(_lessorBase.Id, _lessorBase.Status);
        }
        //else if (_personKind == EnumPersonKind.Legal)
        else if (PersonKindIndex == 1)
        {
            newLessor = new Models.PersonLegal(_lessorBase.Id, _lessorBase.Status);
        }
        else
        {
            // TODO: Raise Error
            return;
        }

        //_lessorBase.Name = Name;

        if (newLessor is Models.PersonNatural naturalPerson)
        {
             naturalPerson.NameFirst = NameFirst;
             naturalPerson.NameLast = NameLast;
        }
        else if (newLessor is Models.PersonLegal legalPerson)
        {
            legalPerson.NameCompany = NameCompany;
            legalPerson.NameCompanyType = NameCompanyType;
            legalPerson.NameCompanyTypePosition = NameCompanyTypePosition;
        }


        newLessor.Remarks = Remarks;

        // TODO: Set other properties
        // TODO: Don't forget to check if Helpers.Common.ReplaceZenkakuNumbers is needed.


        _lessorBase = newLessor;
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
            if (!_navigationService.IsCurrentPageSameAs(BasicPageName))
            {
                _navigationService.NavigateTo(BasicPageName, this);
            }
            return;
        }

        SetValues();

        var resInsert = _dataAccessService.UpsertRentLessor(_lessorBase);
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

            _lessorBase.IsModified = false;
            _lessorBase.Status = EnumEntryStatus.Saved;

            // Update title with dummy value.
            WindowTitle = string.Empty;

            // Clear error infobar.
            IsInfoBarErrorOpen = false;

            WeakReferenceMessenger.Default.Send(new Models.Messenger.LessorUpdatedMessage(_lessorBase));
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
