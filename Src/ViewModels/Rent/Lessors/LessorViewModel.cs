using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using ZumenSearch.Models.Base;
using ZumenSearch.Models.Common;
using ZumenSearch.Services.Contracts;

namespace ZumenSearch.ViewModels.Rent.Lessors;

public sealed partial class LessorViewModel : ObservableRecipient
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

            if (_lessor.Status == EnumEntryStatus.New)
            {
                str = $"{str}：(新規)";
            }
            else
            {
                str = $"{str}：(編集)";
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

    [ObservableProperty]
    public partial bool IsNameLastHasError { get; private set; }
    [ObservableProperty]
    public partial bool IsNameFirstHasError { get; private set; }

    #endregion

    // Nameは直接編集バインドしない。あとで性と名をくっつける。
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

    public string NameFirst
    {
        get => field ?? string.Empty;
        set
        {
            if (SetProperty(ref field, value))
            {
                Name = $"{NameLast} {NameFirst}";
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
                Name = $"{NameLast} {NameFirst}";
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
    private readonly Models.Rent.Lessors.Person _lessor;


    #endregion

    #region == Services ==

    private readonly IDataAccessService _dataAccessService;
    private readonly IDataAccessLocationService _dataAccessLocationService;
    private readonly IDispatcherService _dispatcherService;
    private readonly IDialogGenericService _dialogService;
    private readonly INavigationGenericService _navigationService;

    #endregion

    public LessorViewModel(Models.Rent.Lessors.Person lessor,
        INavigationGenericService navigationService,
        IDialogGenericService dialogService,
        IDispatcherService dispatcherService,
        IDataAccessService dataAccessService,
        IDataAccessLocationService dataAccessLocationService)
    {
        _lessor = lessor;
        _id = lessor.Id;

        _navigationService = navigationService; 
        _dialogService = dialogService;
        _dispatcherService = dispatcherService;
        _dataAccessService = dataAccessService;
        _dataAccessLocationService = dataAccessLocationService;

        // Update title with dummy value.
        WindowTitle = string.Empty;

        /*


        */

        // TODO:
        //_propertyDataDirectoryPath = System.IO.Path.Combine(System.IO.Path.Combine(System.IO.Path.Combine(App.PropertyBlobDataFolder, "Rent"), "Residential"), _id);

        try
        {
            PopulateValues();

            // Reset errors
            IsNameLastHasError = false;
            IsNameFirstHasError = false;
            // TODO: more.

            //HasErrors = false;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"BldgViewModel: {ex}");
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
        Name = _lessor.Name;
        NameFirst = _lessor.NameFirst;
        NameLast = _lessor.NameLast;

        Remarks = _lessor.Remarks;
    }

    private bool ValidateName()
    {
        if (string.IsNullOrWhiteSpace(NameLast))
        {
            InfoBarErrorMessage = "性（必須項目）が入力されていません。保存出来ませんでした。";

            IsNameLastHasError = true;

            //HasErrors = true;

            return false;
        }

        if (string.IsNullOrWhiteSpace(NameFirst))
        {
            InfoBarErrorMessage = "名（必須項目）が入力されていません。保存出来ませんでした。";

            IsNameFirstHasError = true;

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
        IsNameLastHasError = false;
        IsNameFirstHasError = false;

        return true;
    }

    private void SetValues()
    {
        if (!IsDirty)
        {
            return;
        }

        _lessor.Name = Name;
        _lessor.NameFirst = NameFirst;
        _lessor.NameLast = NameLast;

        _lessor.Remarks = Remarks;

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
            if (!_navigationService.IsCurrentPageSameAs("ZumenSearch.Views.Rent.Lessors.BasicPage"))
            {
                _navigationService.NavigateTo("ZumenSearch.Views.Rent.Lessors.BasicPage", this);
            }
            return;
        }

        SetValues();

        var resInsert = _dataAccessService.UpsertRentLessor(_lessor);
        if (resInsert.IsError)
        {
            Debug.WriteLine("Error on update. @Save in LessorViewModel");
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

            _lessor.IsModified = false;
            _lessor.Status = EnumEntryStatus.Saved;

            // Update title with dummy value.
            WindowTitle = string.Empty;

            // Clear error infobar.
            IsInfoBarErrorOpen = false;

            //WeakReferenceMessenger.Default.Send(new Models.Messenger.PropertyUpdatedMessage(_building as Models.Base.PropertyBase));
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
