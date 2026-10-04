using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml.Media.Animation;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Windows.ApplicationModel;
using ZumenSearch.Helpers;
using ZumenSearch.Models;
using ZumenSearch.Models.Messenger;
using ZumenSearch.Services.Contracts;
using ZumenSearch.Services.Extensions.AbstractFactory;

namespace ZumenSearch.ViewModels;

public partial class MainViewModel : ObservableRecipient, 
    IRecipient<PropertyUpdatedMessage>, 
    IRecipient<ListingUpdatedMessage>,
    IRecipient<LessorUpdatedMessage>,
    IRecipient<BrokerUpdatedMessage>,
    IRecipient<WindowClosedMessage>
{

    #region == Private Variables and Const ==

    private const string SearchResultPagePath = "ZumenSearch.Views.SearchResultPage";
    private const string RentResidentialSearchResultPagePath = "ZumenSearch.Views.Rent.ResidentialSearchResultPage";
    private const string RentCommercialSearchResultPagePath = "ZumenSearch.Views.Rent.CommercialSearchResultPage";
    private const string RentParkingSearchResultPagePath = "ZumenSearch.Views.Rent.ParkingSearchResultPage";
    private const string RentLessorSearchResultPagePath = "ZumenSearch.Views.Rent.LessorSearchResultPage";
    private const string SaleResidentialSearchResultPagePath = "ZumenSearch.Views.Sale.ResidentialSearchResultPage";
    private const string BrokerSearchResultPagePath = "ZumenSearch.Views.BrokerSearchResultPage";

    private readonly CancellationTokenSource _cts = new();

    #endregion

    #region == Services ==

    private readonly IAbstractFactory<Models.Rent.Residentials.Property, Views.Rent.Residentials.ShellPage> _shellRentResidentialPropertyFactory;
    private readonly IAbstractFactory<Models.Rent.Residentials.Listing, Views.Rent.Residentials.Listing.ShellPage> _shellRentResidentialListingFactory;

    private readonly IAbstractFactory<Models.Rent.Commercials.Property, Views.Rent.Commercials.ShellPage> _shellRentCommercialPropertyFactory;
    private readonly IAbstractFactory<Models.Rent.Commercials.Listing.Listing, Views.Rent.Commercials.Listing.ShellPage> _shellRentCommercialListingFactory;
    
    private readonly IAbstractFactory<Models.Base.PersonBase, Views.Rent.Lessors.ShellPage> _shellRentLessorFactory;
    private readonly IAbstractFactory<Models.Base.PersonBase, Views.Brokers.ShellPage> _shellBrokerFactory;
    private readonly IAbstractFactory<Models.Sale.Residentials.Property, Views.Sale.Residentials.ShellPage> _shellSaleResidentialPropertyFactory;

    private readonly IDataAccessService _dataAccessService;
    private readonly INavigationService _navigationService;
    private readonly IDispatcherService _dispatcherService;

    #endregion

    public MainViewModel(
        IAbstractFactory<Models.Rent.Residentials.Property, Views.Rent.Residentials.ShellPage> shellRentResidentialPropertyFactory, 
        IAbstractFactory<Models.Rent.Residentials.Listing, Views.Rent.Residentials.Listing.ShellPage> shellRentResidentialListingFactory,
        IAbstractFactory<Models.Rent.Commercials.Property, Views.Rent.Commercials.ShellPage> shellRentCommercialPropertyFactory,
        IAbstractFactory<Models.Rent.Commercials.Listing.Listing, Views.Rent.Commercials.Listing.ShellPage> shellRentCommercialListingFactory,
        IAbstractFactory<Models.Base.PersonBase, Views.Rent.Lessors.ShellPage> shellRentLessorFactory,
        IAbstractFactory<Models.Sale.Residentials.Property, Views.Sale.Residentials.ShellPage> shellSaleResidentialPropertyFactory,
        IAbstractFactory<Models.Base.PersonBase, Views.Brokers.ShellPage> shellBrokerFactory,
        INavigationService navigationService, 
        IDataAccessService dataAccessService, 
        IDispatcherService dispatcherService)
    {
        _shellRentResidentialPropertyFactory = shellRentResidentialPropertyFactory;
        _shellRentResidentialListingFactory = shellRentResidentialListingFactory;
        _shellRentCommercialPropertyFactory = shellRentCommercialPropertyFactory;
        _shellRentCommercialListingFactory = shellRentCommercialListingFactory;
        _shellRentLessorFactory = shellRentLessorFactory;
        _shellSaleResidentialPropertyFactory =shellSaleResidentialPropertyFactory;
        _shellBrokerFactory = shellBrokerFactory;
        _navigationService = navigationService;
        _dataAccessService = dataAccessService;
        _dispatcherService = dispatcherService;

        VersionDescription = GetVersionDescription();

        // Initialize the database and get recent properties.
        // No wait. 
        _ = InitializeDatabase();
        // No code after this point should be automatically executed until the database initialization is completed.

        // Ready to receive messages.
        this.IsActive = true;
    }

    #region == Properties ==

    [ObservableProperty]
    public partial string VersionDescription { get; set; }

    #region == エラー関連 ==

    // InfoBarError is researved only for unsavable error.
    [ObservableProperty]
    public partial bool IsInfoBarErrorOpen { get; set; }

    [ObservableProperty]
    public partial string InfoBarErrorMessage { get; set; } = string.Empty;

    #endregion

    #region == Window management ==

    public readonly List<Views.Rent.Residentials.EditorWindow> RentResidentialEditorList = [];
    public int RentResidentialEditorWinWidth = 1366;
    public int RentResidentialEditorWinHeight = 768;
    public int RentResidentialEditorWinLeft = 130;
    public int RentResidentialEditorWinTop = 130;

    public readonly List<Views.Rent.Residentials.Listing.EditorWindow> RentResidentialListingEditorList = [];
    public int RentResidentialListingEditorWinWidth = 1366;
    public int RentResidentialListingEditorWinHeight = 768;
    public int RentResidentialListingEditorWinLeft = 130;
    public int RentResidentialListingEditorWinTop = 130;

    public readonly List<Views.Rent.Commercials.EditorWindow> RentCommercialEditorList = [];
    public int RentCommercialEditorWinWidth = 1366;
    public int RentCommercialEditorWinHeight = 768;
    public int RentCommercialEditorWinLeft = 130;
    public int RentCommercialEditorWinTop = 130;

    public readonly List<Views.Rent.Commercials.Listing.EditorWindow> RentCommercialListingEditorList = [];
    public int RentCommercialListingEditorWinWidth = 1366;
    public int RentCommercialListingEditorWinHeight = 768;
    public int RentCommercialListingEditorWinLeft = 130;
    public int RentCommercialListingEditorWinTop = 130;

    public readonly List<Views.Rent.Lessors.EditorWindow> LessorEditorList = [];
    public int LessorEditorWinWidth = 1366;
    public int LessorEditorWinHeight = 768;
    public int LessorEditorWinLeft = 130;
    public int LessorEditorWinTop = 130;

    public readonly List<Views.Brokers.EditorWindow> BrokerEditorList = [];
    public int BrokerEditorWinWidth = 1366;
    public int BrokerEditorWinHeight = 768;
    public int BrokerEditorWinLeft = 130;
    public int BrokerEditorWinTop = 130;


    #endregion

    #region == Navigation ==

    public ObservableCollection<Breadcrumb> BreadcrumbItemsRent { get; set; } =
    [
        new() { Name = "総合検索", Page = typeof(Views.SearchPage).FullName! }
    ];
    public ObservableCollection<Breadcrumb> BreadcrumbItemsRentSearchResult { get; set; } =
    [
        new() { Name = "総合検索", Page = typeof(Views.SearchPage).FullName! },
        new() { Name = "検索結果", Page = typeof(Views.SearchResultPage).FullName! },
    ];

    public ObservableCollection<Breadcrumb> BreadcrumbItemsResidential { get; set; } =
    [
        new() { Name = "募集物件検索", Page = typeof(Views.Rent.ResidentialSearchPage).FullName! }
    ];
    public ObservableCollection<Breadcrumb> BreadcrumbItemsResidentialSearchResult { get; set; } =
    [
        new() { Name = "募集物件検索", Page = typeof(Views.Rent.ResidentialSearchPage).FullName! },
        new() { Name = "検索結果", Page = typeof(Views.Rent.ResidentialSearchResultPage).FullName! },
    ];

    public ObservableCollection<Breadcrumb> BreadcrumbItemsCommercial { get; set; } =
    [
        new() { Name = "募集物件検索", Page = typeof(Views.Rent.CommercialSearchPage).FullName! }
    ];
    public ObservableCollection<Breadcrumb> BreadcrumbItemsCommercialSearchResult { get; set; } =
    [
        new() { Name = "募集物件検索", Page = typeof(Views.Rent.CommercialSearchPage).FullName! },
        new() { Name = "検索結果", Page = typeof(Views.Rent.CommercialSearchResultPage).FullName! },
    ];

    public ObservableCollection<Breadcrumb> BreadcrumbItemsParking { get; set; } =
    [
    new() { Name = "募集物件検索", Page = typeof(Views.Rent.ParkingSearchPage).FullName! }
    ];
    public ObservableCollection<Breadcrumb> BreadcrumbItemsParkingSearchResult { get; set; } =
    [
        new() { Name = "募集物件検索", Page = typeof(Views.Rent.ParkingSearchPage).FullName! },
        new() { Name = "検索結果", Page = typeof(Views.Rent.ParkingSearchResultPage).FullName! },
    ];

    public ObservableCollection<Breadcrumb> BreadcrumbItemsLessor { get; set; } =
    [
        new() { Name = "貸主検索", Page = typeof(Views.Rent.LessorSearchPage).FullName! }
    ];
    public ObservableCollection<Breadcrumb> BreadcrumbItemsLessorSearchResult { get; set; } =
    [
        new() { Name = "貸主検索", Page = typeof(Views.Rent.LessorSearchPage).FullName! },
        new() { Name = "検索結果", Page = typeof(Views.Rent.LessorSearchResultPage).FullName! },
    ];

    public ObservableCollection<Breadcrumb> BreadcrumbItemsBroker { get; set; } =
    [
        new() { Name = "宅建業者検索", Page = typeof(Views.BrokerSearchPage).FullName! }
    ];
    public ObservableCollection<Breadcrumb> BreadcrumbItemsBrokerSearchResult { get; set; } =
    [
        new() { Name = "宅建業者検索", Page = typeof(Views.BrokerSearchPage).FullName! },
        new() { Name = "検索結果", Page = typeof(Views.BrokerSearchResultPage).FullName! },
    ];

    #endregion

    #region == RecentProperties ==

    public ObservableCollection<Models.PropertySearchResultItem> RecentProperties
    {
        get; set
        {
            if (SetProperty(ref field, value))
            {
                //
            }
        }
    } = [];

    #endregion

    #region == Search ==

    // Rent or BuySell
    [ObservableProperty]
    public partial int SegIndex { get; set; } = 0;

    [ObservableProperty]
    public partial string SearchQuery { get; set; } = string.Empty;

    public ObservableCollection<Models.PropertySearchResultItem> PropertySearchResult
    {
        get; set
        {
            if (SetProperty(ref field, value))
            {
                //
            }
        }
    } = [];

    public ObservableCollection<Models.ListingSearchResultItem> RentResidentialListingSearchResult
    {
        get; set
        {
            if (SetProperty(ref field, value))
            {
                //
            }
        }
    } = [];

    public ObservableCollection<Models.ListingSearchResultItem> RentCommercialListingSearchResult
    {
        get; set
        {
            if (SetProperty(ref field, value))
            {
                //
            }
        }
    } = [];

    public ObservableCollection<Models.PersonSearchResultItem> RentLessorSearchResult
    {
        get; set
        {
            if (SetProperty(ref field, value))
            {
                //
            }
        }
    } = [];

    public ObservableCollection<Models.PersonSearchResultItem> BrokerSearchResult
    {
        get; set
        {
            if (SetProperty(ref field, value))
            {
                //
            }
        }
    } = [];

    public ObservableCollection<Models.PropertySearchResultItem> SaleResidentialSearchResult
    {
        get;
        private set
        {
            if (SetProperty(ref field, value))
            {
            }
        }
    } = [];


    #endregion

    #region == AutoSuggest ==

    public ObservableCollection<AutoSuggestItem> AutoSuggestList
    {
        get; set
        {
            if (SetProperty(ref field, value))
            {
                //
            }
        }
    } = [];

    #endregion

    #endregion

    #region == Public Methods ==

    public void CleanUp()
    {
        _cts.Cancel();
        _cts.Dispose();
    }

    #endregion

    #region == Messages ==

    public void Receive(PropertyUpdatedMessage property)
    {
        var building = property.Value;
        if (building is null)
        {
            return;
        }

        foreach (var item in PropertySearchResult)
        {
            if (!item.Id.Equals(building.Id))
            {
                continue;
            }

            item.SetName(building.Name);
            item.ThumbnailFilename = building.ThumbnailFilename;
        }

        GetRecentPropertiesCommand.Execute(null);
    }

    public void Receive(ListingUpdatedMessage listing)
    {
        var room = listing.Value;
        if (room is null)
        {
            return;
        }

        var isFound = false;

        foreach (var item in RentResidentialListingSearchResult)
        {
            if (!item.Id.Equals(room.Id))
            {
                continue;
            }

            item.SetName(room.Name);
            isFound = true;
            break;
        }

        if (isFound)
        {
            return;
        }

        foreach (var item in RentCommercialListingSearchResult)
        {
            if (!item.Id.Equals(room.Id))
            {
                continue;
            }

            item.SetName(room.Name);
            isFound = true;
            break;
        }


        if (isFound)
        {
            return;
        }

        // more
    }

    public void Receive(LessorUpdatedMessage lessor)
    {
        var person = lessor.Value;
        if (person is null)
        {
            return;
        }

        foreach (var item in RentLessorSearchResult)
        {
            if (!item.Id.Equals(person.Id))
            {
                continue;
            }

            item.SetName(person.Name);
        }
    }

    public void Receive(BrokerUpdatedMessage broker)
    {
        var person = broker.Value;
        if (person is null)
        {
            return;
        }

        foreach (var item in BrokerSearchResult)
        {
            if (!item.Id.Equals(person.Id))
            {
                continue;
            }

            item.SetName(person.Name);
        }
    }

    public void Receive(WindowClosedMessage window)
    {
        var win = window.Value;

        if (win is null)
        {
            return;
        }

        if (win is Views.Rent.Residentials.EditorWindow rwin)
        {
            this.RentResidentialEditorList.Remove(rwin);
        }
        else if (win is Views.Rent.Residentials.Listing.EditorWindow rlwin)
        {
            this.RentResidentialListingEditorList.Remove(rlwin);
        }
        else if (win is Views.Rent.Commercials.EditorWindow cwin)
        {
            this.RentCommercialEditorList.Remove(cwin);
        }
        else if (win is Views.Rent.Commercials.Listing.EditorWindow clwin)
        {
            this.RentCommercialListingEditorList.Remove(clwin);
        }
        else if (win is Views.Rent.Lessors.EditorWindow lewin)
        {
            this.LessorEditorList.Remove(lewin);
        }
        else if (win is Views.Brokers.EditorWindow bwin)
        {
            this.BrokerEditorList.Remove(bwin);
        }

    }

    /*
    1. To be compatible with Native AOT, we need to manually register message handlers in OnActivated and unregister them in OnDeactivated. 
    This is due to the fact that the source generator for CommunityToolkit.Mvvm does not currently support Native AOT, as discussed in the following GitHub issue:
    https://github.com/CommunityToolkit/dotnet/issues/962

    2. Even though we register manually, simple IsActive = true; cause issues in Native AOT.

    // Add both attributes to perfectly match the base class signature
    [RequiresUnreferencedCode("Manually registering messages to avoid trimming issues.")]
    [RequiresDynamicCode("Manually registering messages to avoid Native AOT issues.")]
    protected override void OnActivated()
    {
        // Explicitly register each message handler
        Messenger.Register<PropertyUpdatedMessage>(this);
        Messenger.Register<ListingUpdatedMessage>(this);
        Messenger.Register<LessorUpdatedMessage>(this);
        Messenger.Register<BrokerUpdatedMessage>(this);
        Messenger.Register<WindowClosedMessage>(this);
    }

    protected override void OnDeactivated()
    {
        // Explicitly unregister to prevent memory leaks
        Messenger.Unregister<PropertyUpdatedMessage>(this);
        Messenger.Unregister<ListingUpdatedMessage>(this);
        Messenger.Unregister<LessorUpdatedMessage>(this);
        Messenger.Unregister<BrokerUpdatedMessage>(this);
        Messenger.Unregister<WindowClosedMessage>(this);
    }
    */

    #endregion

    #region == Private Methods ==

    private async Task InitializeDatabase()
    {
        try
        {
            var filePath = Path.Combine(App.AppDataFolder, "ZumenSearch.db");

            //var res = _dataAccessService.InitializeDatabase(filePath);
            var res = await Task.Run(() => _dataAccessService.InitializeDatabase(filePath), _cts.Token);
            if (res.IsError)
            {
                Debug.WriteLine(
                    res.Error.Title + Environment.NewLine +
                    res.Error.Message + Environment.NewLine +
                    res.Error.Description + Environment.NewLine + 
                    res.Error.Operation + Environment.NewLine +
                    res.Error.MethodName + Environment.NewLine +
                    res.Error.FullDump);

                InfoBarErrorMessage =
                    res.Error.Title + Environment.NewLine +
                    res.Error.Message + Environment.NewLine +
                    res.Error.Description + Environment.NewLine +
                    res.Error.Operation + Environment.NewLine +
                    res.Error.MethodName;

                IsInfoBarErrorOpen = true;

                return;
            }

            // Get recent properties after database initialization.
            GetRecentPropertiesCommand.Execute(null);
        }
        catch (Exception ex)
        {
            // TODO: Show error message to user.
            Debug.WriteLine($"InitializeDatabase: {ex}");
        }
    }

    private static string GetVersionDescription()
    {
        Version version;

        if (RuntimeHelper.IsMSIX)
        {
            var packageVersion = Package.Current.Id.Version;

            version = new(packageVersion.Major, packageVersion.Minor, packageVersion.Build, packageVersion.Revision);
        }
        else
        {
            version = Assembly.GetExecutingAssembly().GetName().Version!;
        }

        var verName = "ZumenSearch";//_resourceLoader.GetString("AppDisplayName");

        return $"{verName} - {version.Major}.{version.Minor}.{version.Build}.{version.Revision}";
    }

    #endregion

    #region == Commands ==

    #region == 総合検索 ==

    [RelayCommand]
    private async Task GetRecentProperties()
    {
        RecentProperties.Clear();

        var res = await Task.Run(() => _dataAccessService.SelectRecentProperties(), _cts.Token);

        if (res.IsError)
        {
            Debug.WriteLine(
                res.Error.Title + Environment.NewLine +
                res.Error.Message + Environment.NewLine +
                res.Error.Description + Environment.NewLine +
                res.Error.Operation + Environment.NewLine +
                res.Error.MethodName + Environment.NewLine +
                res.Error.FullDump);

            InfoBarErrorMessage =
                res.Error.Title + Environment.NewLine +
                res.Error.Message + Environment.NewLine +
                res.Error.Description + Environment.NewLine +
                res.Error.Operation + Environment.NewLine +
                res.Error.MethodName;

            IsInfoBarErrorOpen = true;
        }
        else
        {
            foreach (var item in res.PropertySearchResult)
            {
                item.BasePath = System.IO.Path.Combine(App.PropertyBlobDataFolder, item.Id);
            }

            RecentProperties = new(res.PropertySearchResult);
        }
    }

    // 物件検索
    [RelayCommand(CanExecute = nameof(SearchPropertiesCanExecute))]
    private async Task SearchProperties(string? queryText)
    {
        var query = string.Empty;

        if (string.IsNullOrWhiteSpace(queryText))
        {
            if (string.IsNullOrWhiteSpace(SearchQuery))
            {
                query = "*";
            }
            else
            {
                query = SearchQuery.Trim();
            }
        }
        else
        {
            query = queryText.Trim();
        }

        PropertySearchResult.Clear();

        var res = await Task.Run(() => _dataAccessService.SelectPropertiesByKeyword(query), _cts.Token);

        if (res.IsError)
        {
            Debug.WriteLine(
                res.Error.Title + Environment.NewLine +
                res.Error.Message + Environment.NewLine +
                res.Error.Description + Environment.NewLine +
                res.Error.Operation + Environment.NewLine +
                res.Error.MethodName + Environment.NewLine +
                res.Error.FullDump);

            InfoBarErrorMessage =
                res.Error.Title + Environment.NewLine +
                res.Error.Message + Environment.NewLine +
                res.Error.Description + Environment.NewLine +
                res.Error.Operation + Environment.NewLine +
                res.Error.MethodName;

            IsInfoBarErrorOpen = true;
        }
        else
        {
            foreach (var item in res.PropertySearchResult)
            {
                item.BasePath = System.IO.Path.Combine(App.PropertyBlobDataFolder, item.Id);
            }

            PropertySearchResult = new(res.PropertySearchResult);

            _navigationService.NavigateTo(SearchResultPagePath, SlideNavigationTransitionEffect.FromLeft);//Navigate(typeof(Views.Rent.Residentials.SearchResultPage), null, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromRight });
        }
    }
    private static bool SearchPropertiesCanExecute(string? queryText)
    {
        /*
        if (string.IsNullOrEmpty(queryText))
        {
            return false;
        }
        */
        return true;
    }

    // 物件編集
    [RelayCommand(CanExecute = nameof(EditPropertyCanExecute))]
    private async Task EditProperty(Models.PropertySearchResultItem? selected)
    {
        if (selected is null)
        {
            return;
        }

        var propertyId = selected.Id;

        if (string.IsNullOrEmpty(propertyId))
        {
            Debug.WriteLine("EditPropertiesCommand executed but no item is selected.");
            return;
        }

        if (selected.PropertyKind == Models.Base.EnumPropertyKind.RentResidential)
        {
            await EditRentResidentialFromId(propertyId);
        }
        else if (selected.PropertyKind == Models.Base.EnumPropertyKind.RentCommercial)
        {
            await EditRentCommercialFromId(propertyId);
        }
        else if (selected.PropertyKind == Models.Base.EnumPropertyKind.RentParking)
        {
            Debug.WriteLine("EditPropertiesCommand not yet implemented.");
        }
        else if (selected.PropertyKind == Models.Base.EnumPropertyKind.SaleResidential)
        {
            Debug.WriteLine("EditPropertiesCommand not yet implemented.");
        }
        else if (selected.PropertyKind == Models.Base.EnumPropertyKind.SaleCommercial)
        {
            Debug.WriteLine("EditPropertiesCommand not yet implemented.");
        }
        else if (selected.PropertyKind == Models.Base.EnumPropertyKind.SaleLand)
        {
            Debug.WriteLine("EditPropertiesCommand not yet implemented.");
        }

        // TODO lessor and broker
    }
    private static bool EditPropertyCanExecute(Models.PropertySearchResultItem? selected)
    {
        if (selected is null)
        //if (string.IsNullOrEmpty(rentId))
        {
            return false;
        }

        return true;
    }

    // 物件削除
    [RelayCommand(CanExecute = nameof(DeletePropertyCanExecute))]
    private async Task DeleteProperty(Models.PropertySearchResultItem? selected)
    {
        if (selected is null)
        {
            Debug.WriteLine("DeletePropertyCommand executed but no item is selected(null).");
            return;
        }

        var propertyId = selected.Id;

        if (string.IsNullOrEmpty(propertyId))
        {
            Debug.WriteLine("DeletePropertyCommand executed but no item is selected.");
            return;
        }

        if (selected.PropertyKind == Models.Base.EnumPropertyKind.RentResidential)
        {
            await DeleteRentResidential(selected);
        }
        else if (selected.PropertyKind == Models.Base.EnumPropertyKind.RentCommercial)
        {
            await DeleteRentCommercial(selected);
        }
        else if (selected.PropertyKind == Models.Base.EnumPropertyKind.RentParking)
        {
            Debug.WriteLine("DeletePropertyCommand not yet implemented.");
        }
        else if (selected.PropertyKind == Models.Base.EnumPropertyKind.SaleResidential)
        {
            Debug.WriteLine("DeletePropertyCommand not yet implemented.");
        }
        else if (selected.PropertyKind == Models.Base.EnumPropertyKind.SaleCommercial)
        {
            Debug.WriteLine("DeletePropertyCommand not yet implemented.");
        }
        else if (selected.PropertyKind == Models.Base.EnumPropertyKind.SaleLand)
        {
            Debug.WriteLine("DeletePropertyCommand not yet implemented.");
        }

        // TODO lessor and broker
    }
    private static bool DeletePropertyCanExecute(Models.PropertySearchResultItem? selected)
    {
        if (selected is null)
        //if (string.IsNullOrEmpty(rentId))
        {
            return false;
        }

        return true;
    }



    #endregion

    #region == 賃貸住居用 ==

    // 建物新規追加
    [RelayCommand]
    private void AddNewRentResidential()
    {
        var newId = Guid.CreateVersion7().ToString("N");
        var shell = _shellRentResidentialPropertyFactory.Create(new Models.Rent.Residentials.Property(newId, Models.Base.EnumEntityStatus.New));

        RentResidentialEditorList.Add(shell.Window);

        if (shell.Window.AppWindow.Presenter is OverlappedPresenter presenter)
        {
            presenter.IsResizable = true;
            presenter.IsModal = false;
            presenter.IsAlwaysOnTop = false;
            presenter.PreferredMinimumWidth = 1274;
            presenter.PreferredMinimumHeight = 794;
        }

        //var dpi = Windows.Win32.PInvoke.GetDpiForWindow(new Windows.Win32.Foundation.HWND(WinRT.Interop.WindowNative.GetWindowHandle(this)));
        //var scalingFactor = (float)dpi / 96;
        //AppWindow.Resize(new Windows.Graphics.SizeInt32((int)(400.0f * scalingFactor), (int)(300.0f * scalingFactor)));

        shell.Window.AppWindow.MoveAndResize(new Windows.Graphics.RectInt32(RentResidentialEditorWinLeft, RentResidentialEditorWinTop, RentResidentialEditorWinWidth, RentResidentialEditorWinHeight));

        //editorWindow.AppWindow.Show();
        shell.Window.Activate();
        shell.Window.AppWindow.MoveInZOrderAtTop();
    }

    // 建物編集（検索結果から）
    [RelayCommand(CanExecute = nameof(EditRentResidentialCanExecute))]
    private async Task EditRentResidential(Models.PropertySearchResultItem? selected) 
    {
        var propertyId = selected?.Id;

        if (string.IsNullOrEmpty(propertyId))
        {
            Debug.WriteLine("EditRentResidentialCommand executed but no item is selected.");
            return;
        }

        await EditRentResidentialFromId(propertyId);
    }
    private static bool EditRentResidentialCanExecute(Models.PropertySearchResultItem? selected)
    {
        if (selected is null)
        //if (string.IsNullOrEmpty(rentId))
        {
            return false;
        }

        return true;
    }

    // 建物編集（IDから）
    [RelayCommand(CanExecute = nameof(EditRentResidentialByIdCanExecute))]
    private async Task EditRentResidentialFromId(string propertyId)
    {
        if (string.IsNullOrEmpty(propertyId))
        {
            Debug.WriteLine("EditRentResidentialBldgFromId executed but no item is selected.");
            return;
        }

        var isFound = false;

        // Check if the selected item is already being edited in another window.
        RentResidentialEditorList.ForEach(editorWindow =>
        {
            //Debug.WriteLine($"Checking editor window with Id: {editorWindow.Id} for selected item with Id: {rentId}");
            if (editorWindow.Id == propertyId)
            {
                // If the editor window for this item is already open, activate it.
                //Debug.WriteLine($"Editor window for {rentId} is already open. Activating it.");
                isFound = true;

                editorWindow.Activate();

                // Do I need this anymore?
                //var mainWindow = App.GetService<MainWindow>();
                //mainWindow?.AppWindow.MoveInZOrderBelow(editorWindow.AppWindow.Id);
                editorWindow.AppWindow.MoveInZOrderAtTop();

                return;
            }
        });

        if (isFound)
        {
            // If the editor window for this item is already open, no need to create a new one.
            return;
        }

        // Access Database to get the full entry data.
        var res = await Task.Run(() => _dataAccessService.SelectRentResidentialById(propertyId), _cts.Token);
        if (res.IsError)
        {
            Debug.WriteLine(
                res.Error.Title + Environment.NewLine +
                res.Error.Message + Environment.NewLine +
                res.Error.Description + Environment.NewLine +
                res.Error.Operation + Environment.NewLine +
                res.Error.MethodName + Environment.NewLine +
                res.Error.FullDump);

            InfoBarErrorMessage =
                res.Error.Title + Environment.NewLine +
                res.Error.Message + Environment.NewLine +
                res.Error.Description + Environment.NewLine +
                res.Error.Operation + Environment.NewLine +
                res.Error.MethodName;

            IsInfoBarErrorOpen = true;
            return;
        }

        if (res.Building is null)
        {
            Debug.WriteLine($"Building for {propertyId} is null. Cannot open editor.");
            return;
        }

        var editorShell = _shellRentResidentialPropertyFactory.Create(res.Building);//_editorFactory.Create();

        var editorWindow = editorShell.Window;
        if (editorWindow == null)
        {
            // EditorWin should be initialized in the EditorShell constructor.
            Debug.WriteLine("EditorWin must be initialized in the EditorShell constructor");
            return;
        }

        RentResidentialEditorList.Add(editorWindow);

        editorWindow.AppWindow.MoveAndResize(new Windows.Graphics.RectInt32(RentResidentialEditorWinLeft, RentResidentialEditorWinTop, RentResidentialEditorWinWidth, RentResidentialEditorWinHeight));
        if (editorWindow.AppWindow.Presenter is OverlappedPresenter presenter)
        {
            presenter.IsResizable = true;
            presenter.IsModal = false;
            presenter.IsAlwaysOnTop = false;
            presenter.PreferredMinimumWidth = 1274;
            presenter.PreferredMinimumHeight = 794;
        }

        editorWindow.AppWindow.Show();
        editorWindow.Activate();

        editorWindow.AppWindow.MoveInZOrderAtTop();
    }
    private static bool EditRentResidentialByIdCanExecute(string propertyId)
    {
        if (string.IsNullOrEmpty(propertyId))
        {
            return false;
        }

        return true;
    }

    // クイック検索Box（建物）TODO:
    [RelayCommand(CanExecute = nameof(SearchRentForAutoSuggestCanExecute))]
    private async Task SearchRentForAutoSuggest(string? queryText)
    {
        if (string.IsNullOrWhiteSpace(queryText))
        {
            AutoSuggestList.Clear();
            return;
        }

        // TODO: Residentials only for now.
        Debug.WriteLine($"SearchRentForAutoSuggest {queryText}");

        AutoSuggestList.Clear();

        queryText = queryText.Trim();

        var res = await Task.Run(() => _dataAccessService.SelectPropertiesByKeyword(queryText), _cts.Token);
        //var res = _dataAccessService.SelectRentResidentialsByNameKeyword("*");

        if (res.IsError)
        {
            Debug.WriteLine(
                res.Error.Title + Environment.NewLine +
                res.Error.Message + Environment.NewLine +
                res.Error.Description + Environment.NewLine +
                res.Error.Operation + Environment.NewLine +
                res.Error.MethodName + Environment.NewLine +
                res.Error.FullDump);

            InfoBarErrorMessage =
                res.Error.Title + Environment.NewLine +
                res.Error.Message + Environment.NewLine +
                res.Error.Description + Environment.NewLine +
                res.Error.Operation + Environment.NewLine +
                res.Error.MethodName;

            IsInfoBarErrorOpen = true;
        }
        else
        {
            if (res.PropertySearchResult.Count > 0)
            {
                foreach (var item in res.PropertySearchResult)
                {
                    var autoSuggest = new Models.AutoSuggestItem
                    {
                        Name = item.Name,
                        Id = item.Id
                    };
                    AutoSuggestList.Add(autoSuggest);
                }
            }
            else
            {
                // TODO:
                //Debug.WriteLine("result 0");
                var autoSuggest = new Models.AutoSuggestItem
                {
                    Name = "Result 0",
                    Id = ""
                };
                AutoSuggestList.Add(autoSuggest);
            }
        }
    }
    private static bool SearchRentForAutoSuggestCanExecute(string? queryText)
    {
        if (string.IsNullOrEmpty(queryText))
        {
            return false;
        }

        return true;
    }

    // 物件削除
    [RelayCommand(CanExecute = nameof(DeleteRentResidentialCanExecute))]
    private async Task DeleteRentResidential(Models.PropertySearchResultItem? selected)
    {
        if (selected is null)
        {
            Debug.WriteLine("DeleteRentResidential executed but no item is selected(null).");
            return;
        }

        if (string.IsNullOrEmpty(selected.Id))
        {
            Debug.WriteLine("DeleteRentResidential executed but id is emptyu.");
            return;
        }

        var selectedId = selected.Id;

        var isFound = false;

        if (RentCommercialListingEditorList.Count > 0)
        {
            foreach (var unitEditor in RentCommercialListingEditorList.ToList())
            {
                if (unitEditor.PropertyId != selectedId)
                {
                    continue;
                }

                if (unitEditor.ViewModel.IsDirty)
                {
                    unitEditor.Activate();
                    isFound = true;
                    return;
                }

                unitEditor.Close();
            }
        }

        if (isFound)
        {
            // If the editor window for this item is already open, just return.
            return;
        }

        // Check if the selected item is already being edited in editor window.
        foreach (var editorWindow in RentResidentialEditorList.ToList())
        {
            //Debug.WriteLine($"Checking editor window with Id: {editorWindow.Id} for selected item with Id: {selected.Id}");
            if (editorWindow.Id == selectedId)
            {
                // If the editor window for this item is already open, activate it.
                //Debug.WriteLine($"Editor window for {selected.Id} is already open. Closing if not IsDirty otherwise activating it.");
                
                if (editorWindow.ViewModel?.IsDirty == false)
                {
                    editorWindow.Close();
                }
                else
                {
                    isFound = true;
                    editorWindow.Activate();
                }
                break;
            }
        }

        if (isFound)
        {
            // If the editor window for this item is already open, just return.
            return;
        }

        var res = await Task.Run(() => _dataAccessService.DeleteRentResidential(selectedId), _cts.Token);
        if (res.IsError)
        {
            Debug.WriteLine(
                res.Error.Title + Environment.NewLine +
                res.Error.Message + Environment.NewLine +
                res.Error.Description + Environment.NewLine +
                res.Error.Operation + Environment.NewLine +
                res.Error.MethodName + Environment.NewLine +
                res.Error.FullDump);

            InfoBarErrorMessage =
                res.Error.Title + Environment.NewLine +
                res.Error.Message + Environment.NewLine +
                res.Error.Description + Environment.NewLine +
                res.Error.Operation + Environment.NewLine +
                res.Error.MethodName;

            IsInfoBarErrorOpen = true;
        }
        else
        {
            if (PropertySearchResult.Remove(selected))
            {
                // Successfully removed the selected item from the search result.
            }
            else
            {
                Debug.WriteLine($"Selected item {selectedId} not found in the search result or could not remove.");
            }

            Debug.WriteLine($"DeleteRentResidentialCommand executed for {selectedId}");

            // clean up pics and pdfs.
            var propertyDataDirectoryPath = System.IO.Path.Combine(App.PropertyBlobDataFolder, selectedId);
            if (Directory.Exists(propertyDataDirectoryPath))
            {
                Debug.WriteLine($"Deleting folder: {propertyDataDirectoryPath}");
                Directory.Delete(propertyDataDirectoryPath, true);
            }
        }
    }
    private static bool DeleteRentResidentialCanExecute(Models.PropertySearchResultItem? selected)
    {
        if (selected is null)
        {
            return false;
        }

        return true;
    }

    // 部屋検索（TODO）
    [RelayCommand]
    private async Task SearchRentResidentialListing(string? queryText)
    {
        var query = string.IsNullOrWhiteSpace(queryText) ? "*" : queryText.Trim();

        RentResidentialListingSearchResult.Clear();

        var res = await Task.Run(() => _dataAccessService.SelectRentResidentialListings(), _cts.Token);

        if (res.IsError)
        {
            Debug.WriteLine(
                res.Error.Title + Environment.NewLine +
                res.Error.Message + Environment.NewLine +
                res.Error.Description + Environment.NewLine +
                res.Error.Operation + Environment.NewLine +
                res.Error.MethodName + Environment.NewLine +
                res.Error.FullDump);

            InfoBarErrorMessage =
                res.Error.Title + Environment.NewLine +
                res.Error.Message + Environment.NewLine +
                res.Error.Description + Environment.NewLine +
                res.Error.Operation + Environment.NewLine +
                res.Error.MethodName;

            IsInfoBarErrorOpen = true;
        }
        else
        {
            RentResidentialListingSearchResult = new(res.ListingSearchResult);

            _navigationService.NavigateTo(RentResidentialSearchResultPagePath, SlideNavigationTransitionEffect.FromLeft);//Navigate(typeof(Views.Rent.Residentials.SearchResultPage), null, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromRight });
        }
    }

    // 部屋編集
    [RelayCommand(CanExecute = nameof(EditRentResidentialRoomCanExecute))]
    private async Task EditRentResidentialRoom(Models.ListingSearchResultItem? selected)
    {
        if (selected is null)
        {
            Debug.WriteLine("EditRentResidentialRoomCommand executed but no item is selected.");
            return;
        }

        var roomId = selected.Id;
        var buildingId = selected.PropertyId;

        if (string.IsNullOrEmpty(roomId))
        {
            Debug.WriteLine("EditRentResidentialRoomCommand executed but room id is null or empty.");
            return;
        }

        //Debug.WriteLine($"EditRentResidentialRoomCommand executed for {selected.Id}");

        var isFound = false;

        // Check if the selected item is already being edited in another window.
        RentResidentialListingEditorList.ForEach(editorWindow =>
        {
            //Debug.WriteLine($"Checking editor window with Id: {editorWindow.Id} for selected item with Id: {roomId}");
            if (editorWindow.Id == roomId)
            {
                // If the editor window for this item is already open, activate it.
                //Debug.WriteLine($"Editor window for {roomId} is already open. Activating it.");
                isFound = true;

                editorWindow.Activate();

                // Do I need this anymore?
                //var mainWindow = App.GetService<MainWindow>();
                //mainWindow?.AppWindow.MoveInZOrderBelow(editorWindow.AppWindow.Id);
                editorWindow.AppWindow.MoveInZOrderAtTop();

                return;
            }
        });

        if (isFound)
        {
            // If the editor window for this item is already open, no need to create a new one.
            return;
        }

        // Access Database to get the full entry data.
        var res = await Task.Run(() => _dataAccessService.SelectRentResidentialListingById(buildingId, roomId), _cts.Token);
        if (res.IsError)
        {
            Debug.WriteLine(
                res.Error.Title + Environment.NewLine +
                res.Error.Message + Environment.NewLine +
                res.Error.Description + Environment.NewLine +
                res.Error.Operation + Environment.NewLine +
                res.Error.MethodName + Environment.NewLine +
                res.Error.FullDump);

            InfoBarErrorMessage =
                res.Error.Title + Environment.NewLine +
                res.Error.Message + Environment.NewLine +
                res.Error.Description + Environment.NewLine +
                res.Error.Operation + Environment.NewLine +
                res.Error.MethodName;

            IsInfoBarErrorOpen = true;
            return;
        }

        if (res.Room is null)
        {
            Debug.WriteLine($"Room for {roomId} is null. Cannot open editor.");
            return;
        }

        var editorShell = _shellRentResidentialListingFactory.Create(res.Room);
        var editorWindow = editorShell.Window;
        if (editorWindow == null)
        {
            // EditorWin should be initialized in the EditorShell constructor.
            Debug.WriteLine("EditorWin must be initialized in the EditorShell constructor");
            return;
        }

        RentResidentialListingEditorList.Add(editorWindow);

        editorWindow.AppWindow.MoveAndResize(new Windows.Graphics.RectInt32(RentResidentialListingEditorWinLeft, RentResidentialListingEditorWinTop, RentResidentialListingEditorWinWidth, RentResidentialListingEditorWinHeight));
        if (editorWindow.AppWindow.Presenter is OverlappedPresenter presenter)
        {
            presenter.IsResizable = true;
            presenter.IsModal = false;
            presenter.IsAlwaysOnTop = false;
            presenter.PreferredMinimumWidth = 1274;
            presenter.PreferredMinimumHeight = 794;
        }

        editorWindow.AppWindow.Show();
        editorWindow.Activate();

        // Do I need this anymore?
        //var mainWindow = App.GetService<MainWindow>();
        //mainWindow?.AppWindow.MoveInZOrderBelow(editorWindow.AppWindow.Id);

        editorWindow.AppWindow.MoveInZOrderAtTop();
    }
    private static bool EditRentResidentialRoomCanExecute(Models.ListingSearchResultItem? selected)
    {
        if (selected is null)
        {
            return false;
        }

        return true;
    }

    // 部屋削除
    [RelayCommand(CanExecute = nameof(DeleteRentResidentialRoomCanExecute))]
    private async Task DeleteRentResidentialRoom(Models.ListingSearchResultItem? selected)
    {
        if (selected is null)
        {
            Debug.WriteLine("DeleteRentResidentialRoom executed but no item is selected.");
            return;
        }

        if (string.IsNullOrEmpty(selected.Id))
        {
            Debug.WriteLine("DeleteRentResidentialRoom executed but id is empty.");
            return;
        }

        var selectedId = selected.Id;
        var selectedPropertyId = selected.PropertyId;

        //Debug.WriteLine($"TODO: DeleteRentResidentialRoom executed for {selected.Id} (PropertyId = {selected.PropertyId})");

        var isFound = false;

        // Check if the selected item is already being edited in editor window.
        foreach (var editorWindow in RentResidentialListingEditorList.ToList())
        {
            //Debug.WriteLine($"Checking editor window with Id: {editorWindow.Id} for selected item with Id: {selected.Id}");
            if (editorWindow.Id == selectedId)
            {
                // If the editor window for this item is already open, activate it.
                //Debug.WriteLine($"Editor window for {selected.Id} is already open. Closing if not IsDirty otherwise activating it.");

                if (editorWindow.ViewModel?.IsDirty == false)
                {
                    editorWindow.Close();
                }
                else
                {
                    isFound = true;
                    editorWindow.Activate();
                }
                break;
            }
        }

        if (isFound)
        {
            return;
        }

        var res = await Task.Run(() => _dataAccessService.DeleteRentResidentialListing(selectedId), _cts.Token);
        if (res.IsError)
        {
            Debug.WriteLine(
                res.Error.Title + Environment.NewLine +
                res.Error.Message + Environment.NewLine +
                res.Error.Description + Environment.NewLine +
                res.Error.Operation + Environment.NewLine +
                res.Error.MethodName + Environment.NewLine +
                res.Error.FullDump);

            InfoBarErrorMessage =
                res.Error.Title + Environment.NewLine +
                res.Error.Message + Environment.NewLine +
                res.Error.Description + Environment.NewLine +
                res.Error.Operation + Environment.NewLine +
                res.Error.MethodName;

            IsInfoBarErrorOpen = true;
        }
        else
        {
            if (RentResidentialListingSearchResult.Remove(selected))
            {
                // Successfully removed the selected item from the search result.
            }
            else
            {
                Debug.WriteLine($"Selected item {selectedId} not found in the search result or could not remove.");
            }

            Debug.WriteLine($"DeleteRentResidentialRoomCommand executed for {selectedId}");

            // Check if the selected item is already being edited in another window.
            RentResidentialEditorList.ForEach(editorWindow =>
            {
                Debug.WriteLine($"Checking editor window with Id: {editorWindow.Id} for selected item with Id: {selectedPropertyId}");
                if (editorWindow.Id == selectedPropertyId)
                {
                    // If the editor window for this item is already open, remove the room.
                    //Debug.WriteLine($"Editor window for {selected.PropertyId} is already open. Removing room.");

                    // remove room from the editor window's ViewModel if it exists.
                    WeakReferenceMessenger.Default.Send(new Models.Messenger.ListingDeletedMessage(selectedId));

                    return;
                }
            });

            // clean up pics and pdfs.
            var listingDataDirectoryPath = System.IO.Path.Combine(System.IO.Path.Combine(App.PropertyBlobDataFolder, selectedPropertyId), selectedId);
            if (Directory.Exists(listingDataDirectoryPath))
            {
                Debug.WriteLine($"Deleting folder: {listingDataDirectoryPath}");
                Directory.Delete(listingDataDirectoryPath, true);
            }
        }
    }
    private static bool DeleteRentResidentialRoomCanExecute(Models.ListingSearchResultItem? selected)
    {
        if (selected is null)
        {
            return false;
        }

        return true;
    }

    #endregion

    #region == 賃貸事業用 ==

    // 建物新規追加
    [RelayCommand]
    private void AddNewRentCommercial()
    {
        var property = new Models.Rent.Commercials.Property(
            Guid.CreateVersion7().ToString("N"),
            Models.Base.EnumEntityStatus.New);

        var shell = _shellRentCommercialPropertyFactory.Create(property);

        RentCommercialEditorList.Add(shell.Window);

        if (shell.Window.AppWindow.Presenter
            is Microsoft.UI.Windowing.OverlappedPresenter presenter)
        {
            presenter.IsResizable = true;
            presenter.IsModal = false;
            presenter.IsAlwaysOnTop = false;
            presenter.PreferredMinimumWidth = 1274;
            presenter.PreferredMinimumHeight = 794;
        }

        shell.Window.AppWindow.MoveAndResize(new Windows.Graphics.RectInt32(RentCommercialEditorWinLeft, RentCommercialEditorWinTop, RentCommercialEditorWinWidth, RentCommercialEditorWinHeight));

        //editorWindow.AppWindow.Show();
        shell.Window.Activate();
        shell.Window.AppWindow.MoveInZOrderAtTop();
    }

    // 建物編集（検索結果から）
    [RelayCommand(CanExecute = nameof(EditRentCommercialCanExecute))]
    private async Task EditRentCommercial(Models.ListingSearchResultItem? selected)
    {
        var propertyId = selected?.Id;

        if (string.IsNullOrEmpty(propertyId))
        {
            Debug.WriteLine("EditRentCommercialCommand executed but no item is selected.");
            return;
        }

        await EditRentCommercialFromId(propertyId);
    }
    private static bool EditRentCommercialCanExecute(Models.ListingSearchResultItem? selected)
    {
        return selected is not null &&
               !string.IsNullOrWhiteSpace(selected.Id);
    }

    // 建物編集（IDから）
    [RelayCommand(CanExecute = nameof(EditRentCommercialByIdCanExecute))]
    private async Task EditRentCommercialFromId(string propertyId)
    {
        if (string.IsNullOrEmpty(propertyId))
        {
            Debug.WriteLine("EditRentCommercialFromId executed but no item is selected.");
            return;
        }

        var isFound = false;

        // Check if the selected item is already being edited in another window.
        RentCommercialEditorList.ForEach(editorWindow =>
        {
            //Debug.WriteLine($"Checking editor window with Id: {editorWindow.Id} for selected item with Id: {rentId}");
            if (editorWindow.Id == propertyId)
            {
                // If the editor window for this item is already open, activate it.
                //Debug.WriteLine($"Editor window for {rentId} is already open. Activating it.");
                isFound = true;

                editorWindow.Activate();

                // Do I need this anymore?
                //var mainWindow = App.GetService<MainWindow>();
                //mainWindow?.AppWindow.MoveInZOrderBelow(editorWindow.AppWindow.Id);
                editorWindow.AppWindow.MoveInZOrderAtTop();

                return;
            }
        });

        if (isFound)
        {
            // If the editor window for this item is already open, no need to create a new one.
            return;
        }

        // Access Database to get the full entry data.
        var res = await Task.Run(() => _dataAccessService.SelectRentCommercialById(propertyId), _cts.Token);
        if (res.IsError)
        {
            Debug.WriteLine(
                res.Error.Title + Environment.NewLine +
                res.Error.Message + Environment.NewLine +
                res.Error.Description + Environment.NewLine +
                res.Error.Operation + Environment.NewLine +
                res.Error.MethodName + Environment.NewLine +
                res.Error.FullDump);

            InfoBarErrorMessage =
                res.Error.Title + Environment.NewLine +
                res.Error.Message + Environment.NewLine +
                res.Error.Description + Environment.NewLine +
                res.Error.Operation + Environment.NewLine +
                res.Error.MethodName;

            IsInfoBarErrorOpen = true;
            return;
        }

        if (res.Building is null)
        {
            Debug.WriteLine($"Building for {propertyId} is null. Cannot open editor.");
            return;
        }

        var editorShell = _shellRentCommercialPropertyFactory.Create(res.Building);//_editorFactory.Create();

        var editorWindow = editorShell.Window;
        if (editorWindow == null)
        {
            // EditorWin should be initialized in the EditorShell constructor.
            Debug.WriteLine("EditorWin must be initialized in the EditorShell constructor");
            return;
        }

        RentCommercialEditorList.Add(editorWindow);

        editorWindow.AppWindow.MoveAndResize(new Windows.Graphics.RectInt32(RentCommercialEditorWinLeft, RentCommercialEditorWinTop, RentCommercialEditorWinWidth, RentCommercialEditorWinHeight));
        if (editorWindow.AppWindow.Presenter is OverlappedPresenter presenter)
        {
            presenter.IsResizable = true;
            presenter.IsModal = false;
            presenter.IsAlwaysOnTop = false;
            presenter.PreferredMinimumWidth = 1274;
            presenter.PreferredMinimumHeight = 794;
        }

        editorWindow.AppWindow.Show();
        editorWindow.Activate();

        editorWindow.AppWindow.MoveInZOrderAtTop();
    }
    private static bool EditRentCommercialByIdCanExecute(string propertyId)
    {
        if (string.IsNullOrEmpty(propertyId))
        {
            return false;
        }

        return true;
    }

    // 建物削除
    [RelayCommand(CanExecute = nameof(DeleteRentCommercialCanExecute))]
    private async Task DeleteRentCommercial(Models.PropertySearchResultItem? selected)
    {
        if (selected is null || string.IsNullOrWhiteSpace(selected.Id))
        {
            return;
        }

        var selectedId = selected.Id;

        if (RentCommercialListingEditorList.Count > 0)
        {
            foreach (var unitEditor in RentCommercialListingEditorList.ToList())
            {
                if (unitEditor.PropertyId != selectedId)
                {
                    continue;
                }

                if (unitEditor.ViewModel.IsDirty)
                {
                    unitEditor.Activate();
                    return;
                }

                unitEditor.Close();
            }
        }

        foreach (var editorWindow in RentCommercialEditorList.ToList())
        {
            if (editorWindow.Id != selectedId)
            {
                continue;
            }

            if (editorWindow.ViewModel?.IsDirty == false)
            {
                editorWindow.Close();
            }
            else
            {
                editorWindow.Activate();
                return;
            }

            break;
        }

        var res = await Task.Run(() => _dataAccessService.DeleteRentCommercial(selected.Id), _cts.Token);

        if (res.IsError)
        {
            Debug.WriteLine(
                res.Error.Title + Environment.NewLine +
                res.Error.Message + Environment.NewLine +
                res.Error.Description + Environment.NewLine +
                res.Error.Operation + Environment.NewLine +
                res.Error.MethodName + Environment.NewLine +
                res.Error.FullDump);

            InfoBarErrorMessage =
                res.Error.Title + Environment.NewLine +
                res.Error.Message + Environment.NewLine +
                res.Error.Description + Environment.NewLine +
                res.Error.Operation + Environment.NewLine +
                res.Error.MethodName;

            IsInfoBarErrorOpen = true;

            return;
        }

        //PropertySearchResult.Remove(selected);
        var searchItem = PropertySearchResult.FirstOrDefault(item => item.Id == selectedId);
        if (searchItem is not null)
        {
            PropertySearchResult.Remove(searchItem);
        }

        var recentItem = RecentProperties.FirstOrDefault(item => item.Id == selectedId);
        if (recentItem is not null)
        {
            RecentProperties.Remove(recentItem);
        }

        var propertyDataDirectoryPath = Path.Combine(App.PropertyBlobDataFolder, selectedId);
        if (Directory.Exists(propertyDataDirectoryPath))
        {
            Directory.Delete(propertyDataDirectoryPath, true);
        }

    }
    private static bool DeleteRentCommercialCanExecute(Models.PropertySearchResultItem? selected)
    {
        return selected is not null &&
               !string.IsNullOrWhiteSpace(selected.Id);
    }

    // 区画検索（TODO）
    [RelayCommand]
    private async Task SearchRentCommercialListings(string? queryText)
    {
        var query = string.IsNullOrWhiteSpace(queryText) ? "*" : queryText.Trim();

        RentCommercialListingSearchResult.Clear();

        var res = await Task.Run(() => _dataAccessService.SelectRentCommercialListings(), _cts.Token);

        if (res.IsError)
        {
            Debug.WriteLine(
                res.Error.Title + Environment.NewLine +
                res.Error.Message + Environment.NewLine +
                res.Error.Description + Environment.NewLine +
                res.Error.Operation + Environment.NewLine +
                res.Error.MethodName + Environment.NewLine +
                res.Error.FullDump);

            InfoBarErrorMessage =
                res.Error.Title + Environment.NewLine +
                res.Error.Message + Environment.NewLine +
                res.Error.Description + Environment.NewLine +
                res.Error.Operation + Environment.NewLine +
                res.Error.MethodName;

            IsInfoBarErrorOpen = true;
            return;
        }

        RentCommercialListingSearchResult = new(res.ListingSearchResult);

        _navigationService.NavigateTo(RentCommercialSearchResultPagePath, SlideNavigationTransitionEffect.FromLeft);
    }

    // 区画編集
    [RelayCommand(CanExecute = nameof(EditRentCommercialListingCanExecute))]
    private async Task EditRentCommercialListing(Models.ListingSearchResultItem? selected)
    {
        if (selected is null)
        {
            Debug.WriteLine("EditRentCommercialUnitCommand executed but no item is selected.");
            return;
        }

        var roomId = selected.Id;
        var buildingId = selected.PropertyId;

        if (string.IsNullOrEmpty(roomId))
        {
            Debug.WriteLine("EditRentCommercialUnitCommand executed but room id is null or empty.");
            return;
        }

        //Debug.WriteLine($"EditRentCommercialUnitCommand executed for {selected.Id}");

        var isFound = false;

        // Check if the selected item is already being edited in another window.
        RentCommercialListingEditorList.ForEach(editorWindow =>
        {
            //Debug.WriteLine($"Checking editor window with Id: {editorWindow.Id} for selected item with Id: {roomId}");
            if (editorWindow.Id == roomId)
            {
                // If the editor window for this item is already open, activate it.
                //Debug.WriteLine($"Editor window for {roomId} is already open. Activating it.");
                isFound = true;

                editorWindow.Activate();

                // Do I need this anymore?
                //var mainWindow = App.GetService<MainWindow>();
                //mainWindow?.AppWindow.MoveInZOrderBelow(editorWindow.AppWindow.Id);
                editorWindow.AppWindow.MoveInZOrderAtTop();

                return;
            }
        });

        if (isFound)
        {
            // If the editor window for this item is already open, no need to create a new one.
            return;
        }

        // Access Database to get the full entry data.
        var res = await Task.Run(() => _dataAccessService.SelectRentCommercialListingById(buildingId, roomId), _cts.Token);
        if (res.IsError)
        {
            Debug.WriteLine(
                res.Error.Title + Environment.NewLine +
                res.Error.Message + Environment.NewLine +
                res.Error.Description + Environment.NewLine +
                res.Error.Operation + Environment.NewLine +
                res.Error.MethodName + Environment.NewLine +
                res.Error.FullDump);

            InfoBarErrorMessage =
                res.Error.Title + Environment.NewLine +
                res.Error.Message + Environment.NewLine +
                res.Error.Description + Environment.NewLine +
                res.Error.Operation + Environment.NewLine +
                res.Error.MethodName;

            IsInfoBarErrorOpen = true;
            return;
        }

        if (res.Unit is null)
        {
            Debug.WriteLine($"Room for {roomId} is null. Cannot open editor.");
            return;
        }

        var editorShell = _shellRentCommercialListingFactory.Create(res.Unit);
        var editorWindow = editorShell.Window;
        if (editorWindow == null)
        {
            // EditorWin should be initialized in the EditorShell constructor.
            Debug.WriteLine("EditorWin must be initialized in the EditorShell constructor");
            return;
        }

        RentCommercialListingEditorList.Add(editorWindow);

        editorWindow.AppWindow.MoveAndResize(new Windows.Graphics.RectInt32(RentCommercialListingEditorWinLeft, RentCommercialListingEditorWinTop, RentCommercialListingEditorWinWidth, RentCommercialListingEditorWinHeight));
        if (editorWindow.AppWindow.Presenter is OverlappedPresenter presenter)
        {
            presenter.IsResizable = true;
            presenter.IsModal = false;
            presenter.IsAlwaysOnTop = false;
            presenter.PreferredMinimumWidth = 1274;
            presenter.PreferredMinimumHeight = 794;
        }

        editorWindow.AppWindow.Show();
        editorWindow.Activate();

        // Do I need this anymore?
        //var mainWindow = App.GetService<MainWindow>();
        //mainWindow?.AppWindow.MoveInZOrderBelow(editorWindow.AppWindow.Id);

        editorWindow.AppWindow.MoveInZOrderAtTop();
    }
    private static bool EditRentCommercialListingCanExecute(Models.ListingSearchResultItem? selected)
    {
        if (selected is null)
        {
            return false;
        }

        return true;
    }

    // 区画削除
    [RelayCommand(CanExecute = nameof(DeleteRentCommercialListingCanExecute))]
    private async Task DeleteRentCommercialListing(Models.ListingSearchResultItem? selected)
    {
        if (selected is null || string.IsNullOrWhiteSpace(selected.Id))
        {
            return;
        }

        var selectedId = selected.Id;
        var selectedPropertyId = selected.PropertyId;
        var isFound = false;

        foreach (var editorWindow in RentCommercialListingEditorList.ToList())
        {
            if (editorWindow.Id != selectedId)
            {
                continue;
            }

            if (editorWindow.ViewModel?.IsDirty == false)
            {
                editorWindow.Close();
            }
            else
            {
                isFound = true;
                editorWindow.Activate();
            }

            break;
        }

        if (isFound)
        {
            return;
        }

        var res = await Task.Run(() => _dataAccessService.DeleteRentCommercialListing(selectedId), _cts.Token);
        if (res.IsError)
        {
            Debug.WriteLine(
                res.Error.Title + Environment.NewLine +
                res.Error.Message + Environment.NewLine +
                res.Error.Description + Environment.NewLine +
                res.Error.Operation + Environment.NewLine +
                res.Error.MethodName + Environment.NewLine +
                res.Error.FullDump);

            InfoBarErrorMessage =
                res.Error.Title + Environment.NewLine +
                res.Error.Message + Environment.NewLine +
                res.Error.Description + Environment.NewLine +
                res.Error.Operation + Environment.NewLine +
                res.Error.MethodName;

            IsInfoBarErrorOpen = true;

            return;
        }

        RentCommercialListingSearchResult.Remove(selected);

        foreach (var editorWindow in RentCommercialEditorList.ToList())
        {
            if (editorWindow.Id != selectedPropertyId)
            {
                continue;
            }

            WeakReferenceMessenger.Default.Send(
                new Models.Messenger.ListingDeletedMessage(selectedId));
            break;
        }

        // Clean up pictures and PDFs.
        var listingDataDirectoryPath = Path.Combine(
            App.PropertyBlobDataFolder,
            selectedPropertyId,
            selectedId);

        if (Directory.Exists(listingDataDirectoryPath))
        {
            Directory.Delete(listingDataDirectoryPath, true);
        }
    }

    private static bool DeleteRentCommercialListingCanExecute(Models.ListingSearchResultItem? selected)
    {
        return selected is not null &&
               !string.IsNullOrWhiteSpace(selected.Id);
    }

    #endregion

    #region == 賃貸駐車場 ==

    [RelayCommand]
    private void AddNewRentParking()
    {
        //
    }

    [RelayCommand]
    private async Task SearchRentParking(string? queryText)
    {
        /*
        if (string.IsNullOrWhiteSpace(queryText))
        {
            queryText = "*";
        }

        RentLessorSearchResult.Clear();

        var res = await Task.Run(() => _dataAccessService.SelectRentLessorsByKeyword(queryText), _cts.Token);

        if (res.IsError)
        {
            Debug.WriteLine(res.Error.ErrText + Environment.NewLine + res.Error.ErrDescription + Environment.NewLine + res.Error.ErrPlace + Environment.NewLine + res.Error.ErrPlaceParent);

            //ErrorMain = res.Error;
            //IsMainErrorInfoBarVisible = true;

            // TODO: Show error message to user
        }
        else
        {
            RentLessorSearchResult = new(res.PersonSearchResult);

            _navigationService.NavigateTo("ZumenSearch.Views.Rent.LessorSearchResultPage", SlideNavigationTransitionEffect.FromLeft);
        }
        */

        _navigationService.NavigateTo(RentParkingSearchResultPagePath, SlideNavigationTransitionEffect.FromLeft);
    }

    #endregion

    #region == 貸主 == 

    [RelayCommand]
    private async Task SearchRentLessor(string? queryText)
    {
        //Debug.WriteLine($"SearchRentLessor queryText: {queryText}");

        if (string.IsNullOrWhiteSpace(queryText))
        {
            queryText = "*";
        }

        RentLessorSearchResult.Clear();

        var res = await Task.Run(() => _dataAccessService.SelectRentLessorsByKeyword(queryText), _cts.Token);

        if (res.IsError)
        {
            Debug.WriteLine(
                res.Error.Title + Environment.NewLine +
                res.Error.Message + Environment.NewLine +
                res.Error.Description + Environment.NewLine +
                res.Error.Operation + Environment.NewLine +
                res.Error.MethodName + Environment.NewLine +
                res.Error.FullDump);

            InfoBarErrorMessage =
                res.Error.Title + Environment.NewLine +
                res.Error.Message + Environment.NewLine +
                res.Error.Description + Environment.NewLine +
                res.Error.Operation + Environment.NewLine +
                res.Error.MethodName;

            IsInfoBarErrorOpen = true;
        }
        else
        {
            RentLessorSearchResult = new(res.PersonSearchResult);

            _navigationService.NavigateTo(RentLessorSearchResultPagePath, SlideNavigationTransitionEffect.FromLeft);
        }
    }

    [RelayCommand]
    private void AddNewRentLessor()
    {
        var newId = Guid.CreateVersion7().ToString("N");
        var shell = _shellRentLessorFactory.Create(new Models.Person.NaturalPerson(newId, Models.Base.EnumEntityStatus.New));
        
        LessorEditorList.Add(shell.Window);

        if (shell.Window.AppWindow.Presenter is OverlappedPresenter presenter)
        {
            presenter.IsResizable = true;
            presenter.IsModal = false;
            presenter.IsAlwaysOnTop = false;
            presenter.PreferredMinimumWidth = 1274;
            presenter.PreferredMinimumHeight = 794;
        }

        //var dpi = Windows.Win32.PInvoke.GetDpiForWindow(new Windows.Win32.Foundation.HWND(WinRT.Interop.WindowNative.GetWindowHandle(this)));
        //var scalingFactor = (float)dpi / 96;
        //AppWindow.Resize(new Windows.Graphics.SizeInt32((int)(400.0f * scalingFactor), (int)(300.0f * scalingFactor)));

        shell.Window.AppWindow.MoveAndResize(new Windows.Graphics.RectInt32(LessorEditorWinLeft, LessorEditorWinTop, LessorEditorWinWidth, LessorEditorWinHeight));

        //editorWindow.AppWindow.Show();
        shell.Window.Activate();
        shell.Window.AppWindow.MoveInZOrderAtTop();
    }

    [RelayCommand(CanExecute = nameof(EditRentLessorCanExecute))]
    public async Task EditRentLessor(Models.Base.PersonBase selected) //Models.PersonSearchResultItem
    {
        var lessorId = selected?.Id;

        if (string.IsNullOrEmpty(lessorId))
        {
            Debug.WriteLine("EditRentLessorCommand executed but no item is selected.");
            return;
        }

        var isFound = false;

        // Check if the selected item is already being edited in another window.
        LessorEditorList.ForEach(editorWindow =>
        {
            //Debug.WriteLine($"Checking editor window with Id: {editorWindow.Id} for selected item with Id: {rentId}");
            if (editorWindow.Id == lessorId)
            {
                // If the editor window for this item is already open, activate it.
                //Debug.WriteLine($"Editor window for {rentId} is already open. Activating it.");
                isFound = true;

                editorWindow.Activate();

                // Do I need this anymore?
                //var mainWindow = App.GetService<MainWindow>();
                //mainWindow?.AppWindow.MoveInZOrderBelow(editorWindow.AppWindow.Id);
                editorWindow.AppWindow.MoveInZOrderAtTop();

                return;
            }
        });

        //Debug.WriteLine($"EditRentLessorCommand executed for {selected.Id}");

        if (isFound)
        {
            // If the editor window for this item is already open, no need to create a new one.
            return;
        }

        // 
        var res = await Task.Run(() => _dataAccessService.SelectRentLessorById(lessorId), _cts.Token);
        if (res.IsError)
        {
            Debug.WriteLine(
                res.Error.Title + Environment.NewLine +
                res.Error.Message + Environment.NewLine +
                res.Error.Description + Environment.NewLine +
                res.Error.Operation + Environment.NewLine +
                res.Error.MethodName + Environment.NewLine +
                res.Error.FullDump);

            InfoBarErrorMessage =
                res.Error.Title + Environment.NewLine +
                res.Error.Message + Environment.NewLine +
                res.Error.Description + Environment.NewLine +
                res.Error.Operation + Environment.NewLine +
                res.Error.MethodName;

            IsInfoBarErrorOpen = true;
            return;
        }

        if (res.Person is null)
        {
            Debug.WriteLine($"{lessorId} is null. Cannot open editor.");
            return;
        }

        var editorShell = _shellRentLessorFactory.Create(res.Person);//_editorFactory.Create();

        var editorWindow = editorShell.Window;
        if (editorWindow == null)
        {
            // EditorWin should be initialized in the EditorShell constructor.
            Debug.WriteLine("EditorWin must be initialized in the EditorShell constructor");
            return;
        }

        LessorEditorList.Add(editorWindow);

        editorWindow.AppWindow.MoveAndResize(new Windows.Graphics.RectInt32(LessorEditorWinLeft, LessorEditorWinTop, LessorEditorWinWidth, LessorEditorWinHeight));
        if (editorWindow.AppWindow.Presenter is OverlappedPresenter presenter)
        {
            presenter.IsResizable = true;
            presenter.IsModal = false;
            presenter.IsAlwaysOnTop = false;
            presenter.PreferredMinimumWidth = 1274;
            presenter.PreferredMinimumHeight = 794;
        }

        editorWindow.AppWindow.Show();
        editorWindow.Activate();

        editorWindow.AppWindow.MoveInZOrderAtTop();
    }
    public static bool EditRentLessorCanExecute(Models.Base.PersonBase? selected)
    {
        if (selected is null)
        //if (string.IsNullOrEmpty(rentId))
        {
            return false;
        }

        return true;
    }

    [RelayCommand(CanExecute = nameof(DeleteRentLessorCanExecute))]
    public async Task DeleteRentLessor(Models.Base.PersonBase selected) //Models.PersonSearchResultItem
    {
        if (selected is null)
        {
            return;
        }

        var lessorId = selected?.Id;

        if (string.IsNullOrEmpty(lessorId))
        {
            Debug.WriteLine("DeleteRentLessorCommand executed but no item is selected.");
            return;
        }

        var isFound = false;

        // Check if the selected item is already being edited in another window.
        LessorEditorList.ForEach(editorWindow =>
        {
            //Debug.WriteLine($"Checking editor window with Id: {editorWindow.Id} for selected item with Id: {rentId}");
            if (editorWindow.Id == lessorId)
            {
                // If the editor window for this item is already open, activate it.
                //Debug.WriteLine($"Editor window for {rentId} is already open. Activating it.");
                isFound = true;

                editorWindow.Activate();

                // Do I need this anymore?
                //var mainWindow = App.GetService<MainWindow>();
                //mainWindow?.AppWindow.MoveInZOrderBelow(editorWindow.AppWindow.Id);
                editorWindow.AppWindow.MoveInZOrderAtTop();

                return;
            }
        });

        //Debug.WriteLine($"EditRentLessorCommand executed for {selected.Id}");

        if (isFound)
        {
            // If the editor window for this item is already open, no need to create a new one.
            return;
        }

        // 
        var res = await Task.Run(() => _dataAccessService.DeleteRentLessor(lessorId), _cts.Token);
        if (res.IsError)
        {
            Debug.WriteLine(
                res.Error.Title + Environment.NewLine +
                res.Error.Message + Environment.NewLine +
                res.Error.Description + Environment.NewLine +
                res.Error.Operation + Environment.NewLine +
                res.Error.MethodName + Environment.NewLine +
                res.Error.FullDump);

            InfoBarErrorMessage =
                res.Error.Title + Environment.NewLine +
                res.Error.Message + Environment.NewLine +
                res.Error.Description + Environment.NewLine +
                res.Error.Operation + Environment.NewLine +
                res.Error.MethodName;

            IsInfoBarErrorOpen = true;
            return;
        }
        else
        {
            var match = RentLessorSearchResult.FirstOrDefault(x => x.Id.Equals(lessorId));
            if (match is not null)
            {
                if (RentLessorSearchResult.Remove(match))
                {
                    // Successfully removed the selected item from the search result.
                }
                else
                {
                    Debug.WriteLine($"Selected item {lessorId} not found in the search result or could not remove.");
                }
            }

            Debug.WriteLine($"DeleteRentLessorCommand executed for {lessorId}");

            // remove lessor from the open editor window's ViewModel if it exists.
            WeakReferenceMessenger.Default.Send(new Models.Messenger.LessorDeletedMessage(lessorId));
        }

    }
    public static bool DeleteRentLessorCanExecute(Models.Base.PersonBase? selected)
    {
        if (selected is null)
        //if (string.IsNullOrEmpty(rentId))
        {
            return false;
        }

        return true;
    }

    #endregion

    #region == 売買住居用 ==

    [RelayCommand]
    private void AddNewSaleResidentialBldg()
    {
        var property = new Models.Sale.Residentials.Property(
            Guid.CreateVersion7().ToString("N"),
            Models.Base.EnumEntityStatus.New);

        var shell =
            _shellSaleResidentialPropertyFactory.Create(property);

        if (shell.Window.AppWindow.Presenter
            is Microsoft.UI.Windowing.OverlappedPresenter presenter)
        {
            presenter.IsResizable = true;
            presenter.IsModal = false;
            presenter.IsAlwaysOnTop = false;
            presenter.PreferredMinimumWidth = 1274;
            presenter.PreferredMinimumHeight = 794;
        }

        shell.Window.AppWindow.MoveAndResize(
            new Windows.Graphics.RectInt32(
                130,
                130,
                1366,
                768));

        shell.Window.Activate();
        shell.Window.AppWindow.MoveInZOrderAtTop();
    }

    [RelayCommand]
    private async Task SearchSaleResidentialBldg(string? queryText)
    {
        var query = string.IsNullOrWhiteSpace(queryText)
            ? "*"
            : queryText.Trim();

        var res = await Task.Run(
            () => _dataAccessService
                .SelectSaleResidentialsByNameKeyword(query),
            _cts.Token);

        if (res.IsError)
        {
            Debug.WriteLine(
                res.Error.Title + Environment.NewLine +
                res.Error.Message + Environment.NewLine +
                res.Error.Description + Environment.NewLine +
                res.Error.Operation + Environment.NewLine +
                res.Error.MethodName + Environment.NewLine +
                res.Error.FullDump);

            InfoBarErrorMessage =
                res.Error.Title + Environment.NewLine +
                res.Error.Message + Environment.NewLine +
                res.Error.Description + Environment.NewLine +
                res.Error.Operation + Environment.NewLine +
                res.Error.MethodName;

            IsInfoBarErrorOpen = true;

            return;
        }

        foreach (var item in res.PropertySearchResult)
        {
            item.BasePath = Path.Combine(
                App.PropertyBlobDataFolder,
                item.Id);
        }

        SaleResidentialSearchResult =
            new(res.PropertySearchResult);

        _navigationService.NavigateTo(SaleResidentialSearchResultPagePath,
            SlideNavigationTransitionEffect.FromLeft);
    }

    [RelayCommand(CanExecute = nameof(EditSaleResidentialCanExecute))]
    private async Task EditSaleResidential(
    Models.PropertySearchResultItem? selected)
    {
        if (selected is null || string.IsNullOrWhiteSpace(selected.Id))
        {
            return;
        }

        var res = await Task.Run(
            () => _dataAccessService.SelectSaleResidentialById(selected.Id),
            _cts.Token);

        if (res.IsError || res.Building is null)
        {
            Debug.WriteLine(
                res.Error.Title + Environment.NewLine +
                res.Error.Message + Environment.NewLine +
                res.Error.Description + Environment.NewLine +
                res.Error.Operation + Environment.NewLine +
                res.Error.MethodName + Environment.NewLine +
                res.Error.FullDump);

            InfoBarErrorMessage =
                res.Error.Title + Environment.NewLine +
                res.Error.Message + Environment.NewLine +
                res.Error.Description + Environment.NewLine +
                res.Error.Operation + Environment.NewLine +
                res.Error.MethodName;

            IsInfoBarErrorOpen = true;

            return;
        }

        var shell =
            _shellSaleResidentialPropertyFactory.Create(res.Building);

        if (shell.Window.AppWindow.Presenter
            is Microsoft.UI.Windowing.OverlappedPresenter presenter)
        {
            presenter.IsResizable = true;
            presenter.IsModal = false;
            presenter.IsAlwaysOnTop = false;
            presenter.PreferredMinimumWidth = 1274;
            presenter.PreferredMinimumHeight = 794;
        }

        shell.Window.AppWindow.MoveAndResize(
            new Windows.Graphics.RectInt32(
                130,
                130,
                1366,
                768));

        shell.Window.Activate();
        shell.Window.AppWindow.MoveInZOrderAtTop();
    }
    private static bool EditSaleResidentialCanExecute(
        Models.PropertySearchResultItem? selected)
    {
        return selected is not null &&
               !string.IsNullOrWhiteSpace(selected.Id);
    }

    [RelayCommand(CanExecute = nameof(DeleteSaleResidentialCanExecute))]
    private async Task DeleteSaleResidential(
    Models.PropertySearchResultItem? selected)
    {
        if (selected is null || string.IsNullOrWhiteSpace(selected.Id))
        {
            return;
        }

        var res =
            await Task.Run(() => _dataAccessService.DeleteSaleResidential(selected.Id), _cts.Token);

        if (res.IsError)
        {
            Debug.WriteLine(
                res.Error.Title + Environment.NewLine +
                res.Error.Message + Environment.NewLine +
                res.Error.Description + Environment.NewLine +
                res.Error.Operation + Environment.NewLine +
                res.Error.MethodName + Environment.NewLine +
                res.Error.FullDump);

            InfoBarErrorMessage =
                res.Error.Title + Environment.NewLine +
                res.Error.Message + Environment.NewLine +
                res.Error.Description + Environment.NewLine +
                res.Error.Operation + Environment.NewLine +
                res.Error.MethodName;

            IsInfoBarErrorOpen = true;

            return;
        }

        SaleResidentialSearchResult.Remove(selected);
    }

    private static bool DeleteSaleResidentialCanExecute(
        Models.PropertySearchResultItem? selected)
    {
        return selected is not null &&
               !string.IsNullOrWhiteSpace(selected.Id);
    }


    #endregion

    #region == 宅建業者 == 

    [RelayCommand]
    private void AddNewBroker()
    {
        var newId = Guid.CreateVersion7().ToString("N");
        var shell = _shellBrokerFactory.Create(new Models.Person.LegalPerson(newId, Models.Base.EnumEntityStatus.New));

        BrokerEditorList.Add(shell.Window);

        if (shell.Window.AppWindow.Presenter is OverlappedPresenter presenter)
        {
            presenter.IsResizable = true;
            presenter.IsModal = false;
            presenter.IsAlwaysOnTop = false;
            presenter.PreferredMinimumWidth = 1274;
            presenter.PreferredMinimumHeight = 794;
        }

        //var dpi = Windows.Win32.PInvoke.GetDpiForWindow(new Windows.Win32.Foundation.HWND(WinRT.Interop.WindowNative.GetWindowHandle(this)));
        //var scalingFactor = (float)dpi / 96;
        //AppWindow.Resize(new Windows.Graphics.SizeInt32((int)(400.0f * scalingFactor), (int)(300.0f * scalingFactor)));

        shell.Window.AppWindow.MoveAndResize(new Windows.Graphics.RectInt32(BrokerEditorWinLeft, BrokerEditorWinTop, BrokerEditorWinWidth, BrokerEditorWinHeight));

        //editorWindow.AppWindow.Show();
        shell.Window.Activate();
        shell.Window.AppWindow.MoveInZOrderAtTop();
    }

    [RelayCommand]
    private async Task SearchBroker(string? queryText)
    {
        //Debug.WriteLine($"SearchBroker queryText: {queryText}");

        if (string.IsNullOrWhiteSpace(queryText))
        {
            queryText = "*";
        }

        BrokerSearchResult.Clear();

        var res = await Task.Run(() => _dataAccessService.SelectBrokersByKeyword(queryText), _cts.Token);

        if (res.IsError)
        {
            Debug.WriteLine(
                res.Error.Title + Environment.NewLine +
                res.Error.Message + Environment.NewLine +
                res.Error.Description + Environment.NewLine +
                res.Error.Operation + Environment.NewLine +
                res.Error.MethodName + Environment.NewLine +
                res.Error.FullDump);

            InfoBarErrorMessage =
                res.Error.Title + Environment.NewLine +
                res.Error.Message + Environment.NewLine +
                res.Error.Description + Environment.NewLine +
                res.Error.Operation + Environment.NewLine +
                res.Error.MethodName;

            IsInfoBarErrorOpen = true;
        }
        else
        {
            BrokerSearchResult = new(res.PersonSearchResult);

            _navigationService.NavigateTo(BrokerSearchResultPagePath, SlideNavigationTransitionEffect.FromLeft);
        }
    }

    [RelayCommand(CanExecute = nameof(EditBrokerCanExecute))]
    public async Task EditBroker(Models.Base.PersonBase selected) //Models.PersonSearchResultItem
    {
        var lessorId = selected?.Id;

        if (string.IsNullOrEmpty(lessorId))
        {
            Debug.WriteLine("EditBrokerCommand executed but no item is selected.");
            return;
        }

        var isFound = false;

        // Check if the selected item is already being edited in another window.
        BrokerEditorList.ForEach(editorWindow =>
        {
            //Debug.WriteLine($"Checking editor window with Id: {editorWindow.Id} for selected item with Id: {rentId}");
            if (editorWindow.Id == lessorId)
            {
                // If the editor window for this item is already open, activate it.
                //Debug.WriteLine($"Editor window for {rentId} is already open. Activating it.");
                isFound = true;

                editorWindow.Activate();

                // Do I need this anymore?
                //var mainWindow = App.GetService<MainWindow>();
                //mainWindow?.AppWindow.MoveInZOrderBelow(editorWindow.AppWindow.Id);
                editorWindow.AppWindow.MoveInZOrderAtTop();

                return;
            }
        });

        //Debug.WriteLine($"EditRentLessorCommand executed for {selected.Id}");

        if (isFound)
        {
            // If the editor window for this item is already open, no need to create a new one.
            return;
        }

        // 
        var res = await Task.Run(() => _dataAccessService.SelectBrokerById(lessorId), _cts.Token);
        if (res.IsError)
        {
            Debug.WriteLine(
                res.Error.Title + Environment.NewLine +
                res.Error.Message + Environment.NewLine +
                res.Error.Description + Environment.NewLine +
                res.Error.Operation + Environment.NewLine +
                res.Error.MethodName + Environment.NewLine +
                res.Error.FullDump);

            InfoBarErrorMessage =
                res.Error.Title + Environment.NewLine +
                res.Error.Message + Environment.NewLine +
                res.Error.Description + Environment.NewLine +
                res.Error.Operation + Environment.NewLine +
                res.Error.MethodName;

            IsInfoBarErrorOpen = true;
            return;
        }

        if (res.Person is null)
        {
            Debug.WriteLine($"{lessorId} is null. Cannot open editor.");
            return;
        }

        var editorShell = _shellBrokerFactory.Create(res.Person);//_editorFactory.Create();

        var editorWindow = editorShell.Window;
        if (editorWindow == null)
        {
            // EditorWin should be initialized in the EditorShell constructor.
            Debug.WriteLine("EditorWin must be initialized in the EditorShell constructor");
            return;
        }

        BrokerEditorList.Add(editorWindow);

        editorWindow.AppWindow.MoveAndResize(new Windows.Graphics.RectInt32(BrokerEditorWinLeft, BrokerEditorWinTop, BrokerEditorWinWidth, BrokerEditorWinHeight));
        if (editorWindow.AppWindow.Presenter is OverlappedPresenter presenter)
        {
            presenter.IsResizable = true;
            presenter.IsModal = false;
            presenter.IsAlwaysOnTop = false;
            presenter.PreferredMinimumWidth = 1274;
            presenter.PreferredMinimumHeight = 794;
        }

        editorWindow.AppWindow.Show();
        editorWindow.Activate();

        editorWindow.AppWindow.MoveInZOrderAtTop();
    }
    public static bool EditBrokerCanExecute(Models.Base.PersonBase? selected)
    {
        if (selected is null)
        //if (string.IsNullOrEmpty(rentId))
        {
            return false;
        }

        return true;
    }

    [RelayCommand(CanExecute = nameof(DeleteBrokerCanExecute))]
    public async Task DeleteBroker(Models.Base.PersonBase selected) //Models.PersonSearchResultItem
    {
        if (selected is null)
        {
            return;
        }

        var lessorId = selected?.Id;

        if (string.IsNullOrEmpty(lessorId))
        {
            Debug.WriteLine("DeleteBrokerCommand executed but no item is selected.");
            return;
        }

        var isFound = false;

        // Check if the selected item is already being edited in another window.
        BrokerEditorList.ForEach(editorWindow =>
        {
            //Debug.WriteLine($"Checking editor window with Id: {editorWindow.Id} for selected item with Id: {rentId}");
            if (editorWindow.Id == lessorId)
            {
                // If the editor window for this item is already open, activate it.
                //Debug.WriteLine($"Editor window for {rentId} is already open. Activating it.");
                isFound = true;

                editorWindow.Activate();

                // Do I need this anymore?
                //var mainWindow = App.GetService<MainWindow>();
                //mainWindow?.AppWindow.MoveInZOrderBelow(editorWindow.AppWindow.Id);
                editorWindow.AppWindow.MoveInZOrderAtTop();

                return;
            }
        });

        //Debug.WriteLine($"EditRentLessorCommand executed for {selected.Id}");

        if (isFound)
        {
            // If the editor window for this item is already open, no need to create a new one.
            return;
        }

        // 
        var res = await Task.Run(() => _dataAccessService.DeleteBroker(lessorId), _cts.Token);
        if (res.IsError)
        {
            Debug.WriteLine(
                res.Error.Title + Environment.NewLine +
                res.Error.Message + Environment.NewLine +
                res.Error.Description + Environment.NewLine +
                res.Error.Operation + Environment.NewLine +
                res.Error.MethodName + Environment.NewLine +
                res.Error.FullDump);

            InfoBarErrorMessage =
                res.Error.Title + Environment.NewLine +
                res.Error.Message + Environment.NewLine +
                res.Error.Description + Environment.NewLine +
                res.Error.Operation + Environment.NewLine +
                res.Error.MethodName;

            IsInfoBarErrorOpen = true;
            return;
        }
        else
        {
            var match = BrokerSearchResult.FirstOrDefault(x => x.Id.Equals(lessorId));
            if (match is not null)
            {
                if (BrokerSearchResult.Remove(match))
                {
                    // Successfully removed the selected item from the search result.
                }
                else
                {
                    Debug.WriteLine($"Selected item {lessorId} not found in the search result or could not remove.");
                }
            }

            Debug.WriteLine($"DeleteBrokerCommand executed for {lessorId}");

            // remove lessor from the open editor window's ViewModel if it exists.
            WeakReferenceMessenger.Default.Send(new Models.Messenger.BrokerDeletedMessage(lessorId));
        }

    }
    public static bool DeleteBrokerCanExecute(Models.Base.PersonBase? selected)
    {
        if (selected is null)
        //if (string.IsNullOrEmpty(rentId))
        {
            return false;
        }

        return true;
    }

    #endregion

    #region == ナビゲーション == 

    // GoBac
    [RelayCommand(CanExecute = nameof(GoBackCanExecute))]
    private void GoBack()
    {
        _navigationService.GoBack();
    }
    public bool GoBackCanExecute()
    {
        return _navigationService.CanGoBack();
    }

    #endregion

    #endregion

}
