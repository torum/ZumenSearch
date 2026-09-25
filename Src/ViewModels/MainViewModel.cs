using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml.Media.Animation;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Reflection;
using Windows.ApplicationModel;
using ZumenSearch.Helpers;
using ZumenSearch.Models.Messenger;
using ZumenSearch.Services;
using ZumenSearch.Services.Contracts;
using ZumenSearch.Services.Extensions.AbstractFactory;

namespace ZumenSearch.ViewModels;

public partial class MainViewModel : ObservableRecipient, 
    IRecipient<PropertyUpdatedMessage>, 
    IRecipient<ListingUpdatedMessage>,
    IRecipient<LessorUpdatedMessage>,
    IRecipient<ListingWindowClosedMessage>, 
    IRecipient<PropertyWindowClosedMessage>, 
    IRecipient<LessorWindowClosedMessage>,
    IRecipient<BrokerWindowClosedMessage>
{
    #region == Public Properties ==

    [ObservableProperty]
    public partial string VersionDescription { get; set; }

    #region == Window management ==

    public readonly List<Views.Rent.Residentials.EditorWindow> BldgEditorList = [];
    public int BldgEditorWinWidth = 1366;
    public int BldgEditorWinHeight = 768;
    public int BldgEditorWinLeft = 130;
    public int BldgEditorWinTop = 130;

    public readonly List<Views.Rent.Residentials.Listing.EditorWindow> RoomEditorList = [];
    public int RoomEditorWinWidth = 1366;
    public int RoomEditorWinHeight = 768;
    public int RoomEditorWinLeft = 130;
    public int RoomEditorWinTop = 130;

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

    public ObservableCollection<Models.Common.Breadcrumb> BreadcrumbItemsRent { get; set; } =
    [
        new() { Name = "総合検索", Page = typeof(Views.SearchPage).FullName! }
    ];
    public ObservableCollection<Models.Common.Breadcrumb> BreadcrumbItemsRentSearchResult { get; set; } =
    [
        new() { Name = "総合検索", Page = typeof(Views.SearchPage).FullName! },
        new() { Name = "検索結果", Page = typeof(Views.SearchResultPage).FullName! },
    ];

    public ObservableCollection<Models.Common.Breadcrumb> BreadcrumbItemsResidential { get; set; } =
    [
        new() { Name = "賃貸住居用", Page = typeof(Views.Rent.ResidentialSearchPage).FullName! }
    ];
    public ObservableCollection<Models.Common.Breadcrumb> BreadcrumbItemsResidentialSearchResult { get; set; } =
    [
        new() { Name = "賃貸住居用", Page = typeof(Views.Rent.ResidentialSearchPage).FullName! },
        new() { Name = "検索結果", Page = typeof(Views.Rent.ResidentialSearchResultPage).FullName! },
    ];

    public ObservableCollection<Models.Common.Breadcrumb> BreadcrumbItemsCommercial { get; set; } =
[
    new() { Name = "賃貸事業用", Page = typeof(Views.Rent.CommercialSearchPage).FullName! }
];
    public ObservableCollection<Models.Common.Breadcrumb> BreadcrumbItemsCommercialSearchResult { get; set; } =
    [
        new() { Name = "賃貸事業用", Page = typeof(Views.Rent.CommercialSearchPage).FullName! },
        new() { Name = "検索結果", Page = typeof(Views.Rent.CommercialSearchResultPage).FullName! },
    ];

    public ObservableCollection<Models.Common.Breadcrumb> BreadcrumbItemsParking { get; set; } =
[
new() { Name = "賃貸駐車場", Page = typeof(Views.Rent.ParkingSearchPage).FullName! }
];
    public ObservableCollection<Models.Common.Breadcrumb> BreadcrumbItemsParkingSearchResult { get; set; } =
    [
        new() { Name = "賃貸駐車場", Page = typeof(Views.Rent.ParkingSearchPage).FullName! },
        new() { Name = "検索結果", Page = typeof(Views.Rent.ParkingSearchResultPage).FullName! },
    ];

    public ObservableCollection<Models.Common.Breadcrumb> BreadcrumbItemsLessor { get; set; } =
    [
    new() { Name = "貸主", Page = typeof(Views.Rent.LessorSearchPage).FullName! }
    ];
    public ObservableCollection<Models.Common.Breadcrumb> BreadcrumbItemsLessorSearchResult { get; set; } =
    [
        new() { Name = "貸主", Page = typeof(Views.Rent.LessorSearchPage).FullName! },
        new() { Name = "検索結果", Page = typeof(Views.Rent.LessorSearchResultPage).FullName! },
    ];

    public ObservableCollection<Models.Common.Breadcrumb> BreadcrumbItemsBroker { get; set; } =
    [
        new() { Name = "宅建業者", Page = typeof(Views.BrokerSearchPage).FullName! }
    ];
    public ObservableCollection<Models.Common.Breadcrumb> BreadcrumbItemsBrokerSearchResult { get; set; } =
    [
        new() { Name = "宅建業者", Page = typeof(Views.BrokerSearchPage).FullName! },
        new() { Name = "検索結果", Page = typeof(Views.BrokerSearchResultPage).FullName! },
    ];

    #endregion

    #region == RecentProperties ==

    public ObservableCollection<Models.Common.PropertySearchResultItem> RecentProperties
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

    public ObservableCollection<Models.Common.PropertySearchResultItem> RentResidentialBldgSearchResult
    {
        get; set
        {
            if (SetProperty(ref field, value))
            {
                //
            }
        }
    } = [];

    public ObservableCollection<Models.Common.ListingSearchResultItem> RentResidentialRoomSearchResult
    {
        get; set
        {
            if (SetProperty(ref field, value))
            {
                //
            }
        }
    } = [];

    public ObservableCollection<Models.Common.PersonSearchResultItem> RentLessorSearchResult
    {
        get; set
        {
            if (SetProperty(ref field, value))
            {
                //
            }
        }
    } = [];

    public ObservableCollection<Models.Common.PersonSearchResultItem> BrokerSearchResult
    {
        get; set
        {
            if (SetProperty(ref field, value))
            {
                //
            }
        }
    } = [];

    public ObservableCollection<Models.Common.PropertySearchResultItem> SaleResidentialBldgSearchResult
    {
        get;
        private set
        {
            if (SetProperty(ref field, value))
            {
            }
        }
    } = [];

    public ObservableCollection<
    Models.Common.PropertySearchResultItem>
    RentCommercialBldgSearchResult
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

    public ObservableCollection<Models.Common.AutoSuggestItem> AutoSuggestList
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

    #region == Private Variables ==

    private readonly CancellationTokenSource _cts = new();

    #endregion

    #region == Services ==

    private readonly IAbstractFactory<Models.Rent.Residentials.Property, Views.Rent.Residentials.ShellPage> _shellRentResidentialPropertyFactory;
    private readonly IAbstractFactory<Models.Rent.Residentials.Listing.Listing, Views.Rent.Residentials.Listing.ShellPage> _shellRentResidentialListingFactory;
    private readonly IAbstractFactory<Models.Base.PersonBase, Views.Rent.Lessors.ShellPage> _shellRentLessorFactory;
    private readonly IAbstractFactory<Models.Base.PersonBase, Views.Brokers.ShellPage> _shellBrokerFactory;
    private readonly IAbstractFactory<Models.Sale.Residentials.Property, Views.Sale.Residentials.ShellPage> _shellSaleResidentialPropertyFactory;
    private readonly IAbstractFactory<Models.Rent.Commercials.Property, Views.Rent.Commercials.ShellPage> _shellRentCommercialPropertyFactory;

    private readonly IDataAccessService _dataAccessService;
    private readonly INavigationService _navigationService;
    private readonly IDispatcherService _dispatcherService;

    #endregion

    public MainViewModel(
        IAbstractFactory<Models.Rent.Residentials.Property, Views.Rent.Residentials.ShellPage> shellRentResidentialPropertyFactory, 
        IAbstractFactory<Models.Rent.Residentials.Listing.Listing, Views.Rent.Residentials.Listing.ShellPage> shellRentResidentialListingFactory,
        IAbstractFactory<Models.Rent.Commercials.Property, Views.Rent.Commercials.ShellPage> shellRentCommercialPropertyFactory,
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
        _shellRentLessorFactory = shellRentLessorFactory;
        _shellSaleResidentialPropertyFactory =shellSaleResidentialPropertyFactory;
        _shellBrokerFactory = shellBrokerFactory;
        _navigationService = navigationService;
        _dataAccessService = dataAccessService;
        _dispatcherService = dispatcherService;

        VersionDescription = GetVersionDescription();

        InitializeDatabase();

        GetRecentPropertiesCommand.Execute(null); //GetRecentProperties();

        // Ready to receive messages.
        this.IsActive = true;
    }

    #region == Messages ==

    public void Receive(PropertyUpdatedMessage property)
    {
        var building = property.Value;
        if (building is null)
        {
            return;
        }

        foreach (var item in RentResidentialBldgSearchResult)
        {
            if (!item.Id.Equals(building.Id))
            {
                continue;
            }

            item.Name = building.Name;
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

        foreach (var item in RentResidentialRoomSearchResult)
        {
            if (!item.Id.Equals(room.Id))
            {
                continue;
            }

            item.Name = room.Name;
        }
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

            item.Name = person.Name;
        }
    }

    public void Receive(ListingWindowClosedMessage window)
    {
        var ewin = window.Value;

        if (ewin is null)
        {
            return;
        }
        
        this.RoomEditorList.Remove(ewin);
    }

    public void Receive(PropertyWindowClosedMessage window)
    {
        var ewin = window.Value;

        if (ewin is null)
        {
            return;
        }

        this.BldgEditorList.Remove(ewin);
    }

    public void Receive(LessorWindowClosedMessage window)
    {
        var ewin = window.Value;

        if (ewin is null)
        {
            return;
        }

        this.LessorEditorList.Remove(ewin);
    }

    public void Receive(BrokerWindowClosedMessage window)
    {
        var ewin = window.Value;

        if (ewin is null)
        {
            return;
        }

        this.BrokerEditorList.Remove(ewin);
    }

    #endregion

    #region == Private Methods ==

    private void InitializeDatabase()
    {
        try
        {
            var filePath = Path.Combine(App.AppDataFolder, "ZumenSearch.db");

            var res = _dataAccessService.InitializeDatabase(filePath);
            if (res.IsError)
            {
                Debug.WriteLine("InitializeDatabase @InitializeDatabase in MainViewModel");

                Debug.WriteLine(res.Error.ErrText + Environment.NewLine + res.Error.ErrDescription + Environment.NewLine + res.Error.ErrPlace + Environment.NewLine + res.Error.ErrPlaceParent);

                //ErrorMain = res.Error;
                //IsMainErrorInfoBarVisible = true;

                // TODO: Show error message to user
            }
        }
        catch (Exception ex)
        {
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

    #region == Public Methods ==

    public void CleanUp()
    {
        _cts.Cancel();
        _cts.Dispose();
    }

    #endregion

    #region == Commands ==

    #region == 総合検索 ==

    [RelayCommand]
    private async Task GetRecentProperties()
    {
        /*
        _dispatcherService.TryEnqueue(async () =>
        {

        });
        */
        RecentProperties.Clear();

        var res = await Task.Run(() => _dataAccessService.SelectRecentProperties(), _cts.Token);

        if (res.IsError)
        {
            Debug.WriteLine(res.Error.ErrText + Environment.NewLine + res.Error.ErrDescription + Environment.NewLine + res.Error.ErrPlace + Environment.NewLine + res.Error.ErrPlaceParent);

            //ErrorMain = res.Error;
            //IsMainErrorInfoBarVisible = true;

            // TODO: Show error message to user
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

    #endregion

    #region == 賃貸住居用 ==

    // 建物新規追加
    [RelayCommand]
    private void AddNewRentResidentialBldg()
    {
        var newId = Guid.CreateVersion7().ToString("N");
        var shell = _shellRentResidentialPropertyFactory.Create(new Models.Rent.Residentials.Property(newId, Models.Base.EnumEntryStatus.New));

        BldgEditorList.Add(shell.Window);

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

        shell.Window.AppWindow.MoveAndResize(new Windows.Graphics.RectInt32(BldgEditorWinLeft, BldgEditorWinTop, BldgEditorWinWidth, BldgEditorWinHeight));

        //editorWindow.AppWindow.Show();
        shell.Window.Activate();
        shell.Window.AppWindow.MoveInZOrderAtTop();
    }

    // 建物編集
    [RelayCommand(CanExecute = nameof(EditRentResidentialBldgCanExecute))]
    public async Task EditRentResidentialBldg(Models.Common.PropertySearchResultItem? selected) 
    {
        var propertyId = selected?.Id;

        if (string.IsNullOrEmpty(propertyId))
        {
            Debug.WriteLine("EditRentResidentialCommand executed but no item is selected.");
            return;
        }

        await EditRentResidentialBldgFromId(propertyId);
    }
    public static bool EditRentResidentialBldgCanExecute(Models.Common.PropertySearchResultItem? selected)
    {
        if (selected is null)
        //if (string.IsNullOrEmpty(rentId))
        {
            return false;
        }

        return true;
    }

    [RelayCommand(CanExecute = nameof(EditRentResidentialBldgByIdCanExecute))]
    public async Task EditRentResidentialBldgFromId(string propertyId)
    {
        if (string.IsNullOrEmpty(propertyId))
        {
            Debug.WriteLine("EditRentResidentialBldgFromId executed but no item is selected.");
            return;
        }

        var isFound = false;

        // Check if the selected item is already being edited in another window.
        BldgEditorList.ForEach(editorWindow =>
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
        var res = _dataAccessService.SelectRentResidentialById(propertyId);// Go back to UI thred. Let's not do > .ConfigureAwait(false);
        //var res = await Task.Run(() => _dataAccessService.SelectRentResidentialById(rentId), _cts.Token);
        if (res.IsError)
        {
            Debug.WriteLine(res.Error.ErrText + Environment.NewLine + res.Error.ErrDescription + Environment.NewLine + res.Error.ErrPlace + Environment.NewLine + res.Error.ErrPlaceParent);

            //ErrorMain = res.Error;
            //IsMainErrorInfoBarVisible = true;

            // TODO: Show error message to user
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

        BldgEditorList.Add(editorWindow);

        editorWindow.AppWindow.MoveAndResize(new Windows.Graphics.RectInt32(BldgEditorWinLeft, BldgEditorWinTop, BldgEditorWinWidth, BldgEditorWinHeight));
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
    public static bool EditRentResidentialBldgByIdCanExecute(string propertyId)
    {
        if (string.IsNullOrEmpty(propertyId))
        {
            return false;
        }

        return true;
    }

    // 部屋編集
    [RelayCommand(CanExecute = nameof(EditRentResidentialRoomCanExecute))]
    public async Task EditRentResidentialRoom(Models.Common.ListingSearchResultItem? selected)
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
        RoomEditorList.ForEach(editorWindow =>
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
        var res = _dataAccessService.SelectRentResidentialListingById(buildingId, roomId);// Go back to UI thred. Let's not do > .ConfigureAwait(false);
        //var res = await Task.Run(() => _dataAccessService.SelectRentResidentialListingById(buildingId, roomId), _cts.Token);
        if (res.IsError)
        {
            Debug.WriteLine(res.Error.ErrText + Environment.NewLine + res.Error.ErrDescription + Environment.NewLine + res.Error.ErrPlace + Environment.NewLine + res.Error.ErrPlaceParent);

            //ErrorMain = res.Error;
            //IsMainErrorInfoBarVisible = true;

            // TODO: Show error message to user
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

        RoomEditorList.Add(editorWindow);

        editorWindow.AppWindow.MoveAndResize(new Windows.Graphics.RectInt32(RoomEditorWinLeft, RoomEditorWinTop, RoomEditorWinWidth, RoomEditorWinHeight));
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
    public static bool EditRentResidentialRoomCanExecute(Models.Common.ListingSearchResultItem? selected)
    {
        if (selected is null)
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

        var res = await Task.Run(() => _dataAccessService.SelectRentResidentialsByNameKeyword(queryText), _cts.Token);
        //var res = _dataAccessService.SelectRentResidentialsByNameKeyword("*");

        if (res.IsError)
        {
            Debug.WriteLine(res.Error.ErrText + Environment.NewLine + res.Error.ErrDescription + Environment.NewLine + res.Error.ErrPlace + Environment.NewLine + res.Error.ErrPlaceParent);

            //ErrorMain = res.Error;
            //IsMainErrorInfoBarVisible = true;

            // TODO: Show error message to user
        }
        else
        {
            if (res.PropertySearchResult.Count > 0)
            {
                foreach (var item in res.PropertySearchResult)
                {
                    var autoSuggest = new Models.Common.AutoSuggestItem
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
                var autoSuggest = new Models.Common.AutoSuggestItem
                {
                    Name = "Result 0",
                    Id = ""
                };
                AutoSuggestList.Add(autoSuggest);
            }
        }
    }
    public static bool SearchRentForAutoSuggestCanExecute(string? queryText)
    {
        if (string.IsNullOrEmpty(queryText))
        {
            return false;
        }

        return true;
    }

    // 物件検索
    [RelayCommand(CanExecute = nameof(SearchRentResidentialBldgCanExecute))]
    private async Task SearchRentResidentialBldg(string? queryText)
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

        //Debug.WriteLine($"queryText is {queryText}");

        RentResidentialBldgSearchResult.Clear();

        var res = await Task.Run(() => _dataAccessService.SelectRentResidentialsByNameKeyword(query), _cts.Token);
        //var res = _dataAccessService.SelectRentResidentialsByNameKeyword("*");

        if (res.IsError)
        {
            Debug.WriteLine(res.Error.ErrText + Environment.NewLine + res.Error.ErrDescription + Environment.NewLine + res.Error.ErrPlace + Environment.NewLine + res.Error.ErrPlaceParent);

            //ErrorMain = res.Error;
            //IsMainErrorInfoBarVisible = true;

            // TODO: Show error message to user
        }
        else
        {
            foreach (var item in res.PropertySearchResult)
            {
                item.BasePath = System.IO.Path.Combine(App.PropertyBlobDataFolder, item.Id);
            }

            RentResidentialBldgSearchResult = new(res.PropertySearchResult);

            _navigationService.NavigateTo("ZumenSearch.Views.SearchResultPage", SlideNavigationTransitionEffect.FromLeft);//Navigate(typeof(Views.Rent.Residentials.SearchResultPage), null, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromRight });

            //_navigationService.NavigateTo("ZumenSearch.Views.Rent.Residentials.BldgShellPage", SlideNavigationTransitionEffect.FromLeft);//Navigate(typeof(Views.Rent.Residentials.SearchResultPage), null, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromRight });
        }
    }
    public static bool SearchRentResidentialBldgCanExecute(string? queryText)
    {
        /*
        if (string.IsNullOrEmpty(queryText))
        {
            return false;
        }
        */
        return true;
    }

    // 部屋検索（TODO）
    [RelayCommand]
    private async Task SearchRentResidentialRoom()
    {
        RentResidentialRoomSearchResult.Clear();

        var res = await Task.Run(() => _dataAccessService.SelectRentResidentialListings(), _cts.Token);

        if (res.IsError)
        {
            Debug.WriteLine(res.Error.ErrText + Environment.NewLine + res.Error.ErrDescription + Environment.NewLine + res.Error.ErrPlace + Environment.NewLine + res.Error.ErrPlaceParent);

            //ErrorMain = res.Error;
            //IsMainErrorInfoBarVisible = true;

            // TODO: Show error message to user
        }
        else
        {
            RentResidentialRoomSearchResult = new(res.ListingSearchResult);

            _navigationService.NavigateTo("ZumenSearch.Views.Rent.ResidentialSearchResultPage", SlideNavigationTransitionEffect.FromLeft);//Navigate(typeof(Views.Rent.Residentials.SearchResultPage), null, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromRight });
        }
    }

    // 物件削除
    [RelayCommand(CanExecute = nameof(DeleteRentResidentialBldgCanExecute))]
    private void DeleteRentResidentialBldg(Models.Common.PropertySearchResultItem? selected)
    {
        if (selected == null)
        {
            Debug.WriteLine("DeleteRentResidential executed but no item is selected.");
            return;
        }

        if (string.IsNullOrEmpty(selected.Id))
        {
            Debug.WriteLine("DeleteRentResidential executed but id is emptyu.");
            return;
        }

        var selectedId = selected.Id;

        var isFound = false;

        // Check if the selected item is already being edited in editor window.
        foreach (var editorWindow in BldgEditorList.ToList())
        {
            //Debug.WriteLine($"Checking editor window with Id: {editorWindow.Id} for selected item with Id: {selected.Id}");
            if (editorWindow.Id == selectedId)
            {
                // If the editor window for this item is already open, activate it.
                //Debug.WriteLine($"Editor window for {selected.Id} is already open. Closing if not IsDirty otherwise activating it.");
                
                if (editorWindow.ViewModel?.IsDirty == false)
                {
                    editorWindow.IsAutoClose = true;

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

        var res = _dataAccessService.DeleteRentResidential(selectedId);
        if (res.IsError)
        {
            Debug.WriteLine(res.Error.ErrText + Environment.NewLine + res.Error.ErrDescription + Environment.NewLine + res.Error.ErrPlace + Environment.NewLine + res.Error.ErrPlaceParent);

            //ErrorMain = res.Error;
            //IsMainErrorInfoBarVisible = true;

            // TODO: Show error message to user
        }
        else
        {
            if (RentResidentialBldgSearchResult.Remove(selected))
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
    private static bool DeleteRentResidentialBldgCanExecute(Models.Common.PropertySearchResultItem? selected)
    {
        if (selected is null)
        {
            return false;
        }

        return true;
    }

    // 部屋削除
    [RelayCommand(CanExecute = nameof(DeleteRentResidentialRoomCanExecute))]
    private void DeleteRentResidentialRoom(Models.Common.ListingSearchResultItem? selected)
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
        foreach (var editorWindow in RoomEditorList.ToList())
        {
            //Debug.WriteLine($"Checking editor window with Id: {editorWindow.Id} for selected item with Id: {selected.Id}");
            if (editorWindow.Id == selectedId)
            {
                // If the editor window for this item is already open, activate it.
                //Debug.WriteLine($"Editor window for {selected.Id} is already open. Closing if not IsDirty otherwise activating it.");
                
                if (editorWindow.ViewModel?.IsDirty == false)
                {
                    editorWindow.IsAutoClose = true;

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

        var res = _dataAccessService.DeleteRentResidentialListing(selectedId);
        if (res.IsError)
        {
            Debug.WriteLine(res.Error.ErrText + Environment.NewLine + res.Error.ErrDescription + Environment.NewLine + res.Error.ErrPlace + Environment.NewLine + res.Error.ErrPlaceParent);

            //ErrorMain = res.Error;
            //IsMainErrorInfoBarVisible = true;

            // TODO: Show error message to user
        }
        else
        {
            if (RentResidentialRoomSearchResult.Remove(selected))
            {
                // Successfully removed the selected item from the search result.
            }
            else
            {
                Debug.WriteLine($"Selected item {selectedId} not found in the search result or could not remove.");
            }

            Debug.WriteLine($"DeleteRentResidentialRoomCommand executed for {selectedId}");

            // Check if the selected item is already being edited in another window.
            BldgEditorList.ForEach(editorWindow =>
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
    private static bool DeleteRentResidentialRoomCanExecute(Models.Common.ListingSearchResultItem? selected)
    {
        if (selected is null)
        {
            return false;
        }

        return true;
    }

    #endregion

    #region == 賃貸事業用 ==

    [RelayCommand]
    private async Task SearchRentCommercial(string? queryText)
    {
        var query = string.IsNullOrWhiteSpace(queryText)
            ? "*"
            : queryText.Trim();

        var result = await Task.Run(
            () => _dataAccessService
                .SelectRentCommercialsByNameKeyword(query),
            _cts.Token);

        if (result.IsError)
        {
            Debug.WriteLine(
                $"{result.Error.ErrText}{Environment.NewLine}" +
                $"{result.Error.ErrDescription}{Environment.NewLine}" +
                $"{result.Error.ErrPlace}");

            return;
        }

        foreach (var item in result.PropertySearchResult)
        {
            item.BasePath = Path.Combine(
                App.PropertyBlobDataFolder,
                item.Id);
        }

        RentCommercialBldgSearchResult =
            new(result.PropertySearchResult);

        _navigationService.NavigateTo(
            "ZumenSearch.Views.Rent.CommercialSearchResultPage",
            SlideNavigationTransitionEffect.FromLeft);
    }

    [RelayCommand]
    private void AddNewRentCommercial()
    {
        var property = new Models.Rent.Commercials.Property(
            Guid.CreateVersion7().ToString("N"),
            Models.Base.EnumEntryStatus.New);

        var shell =
            _shellRentCommercialPropertyFactory.Create(property);

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

    [RelayCommand(CanExecute = nameof(EditRentCommercialCanExecute))]
    private async Task EditRentCommercial(Models.Common.PropertySearchResultItem? selected)
    {
        if (selected is null ||
            string.IsNullOrWhiteSpace(selected.Id))
        {
            return;
        }

        var result = await Task.Run(
            () => _dataAccessService
                .SelectRentCommercialById(selected.Id),
            _cts.Token);

        if (result.IsError || result.Building is null)
        {
            Debug.WriteLine(
                $"{result.Error.ErrText}{Environment.NewLine}" +
                $"{result.Error.ErrDescription}{Environment.NewLine}" +
                $"{result.Error.ErrPlace}");

            return;
        }

        var shell =
            _shellRentCommercialPropertyFactory.Create(
                result.Building);

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

    private static bool EditRentCommercialCanExecute(
        Models.Common.PropertySearchResultItem? selected)
    {
        return selected is not null &&
               !string.IsNullOrWhiteSpace(selected.Id);
    }

    [RelayCommand(CanExecute = nameof(DeleteRentCommercialCanExecute))]
    private void DeleteRentCommercial(Models.Common.PropertySearchResultItem? selected)
    {
        if (selected is null ||
            string.IsNullOrWhiteSpace(selected.Id))
        {
            return;
        }

        var result =
            _dataAccessService.DeleteRentCommercial(selected.Id);

        if (result.IsError)
        {
            Debug.WriteLine(
                $"{result.Error.ErrText}{Environment.NewLine}" +
                $"{result.Error.ErrDescription}{Environment.NewLine}" +
                $"{result.Error.ErrPlace}");

            return;
        }

        RentCommercialBldgSearchResult.Remove(selected);
    }

    private static bool DeleteRentCommercialCanExecute(Models.Common.PropertySearchResultItem? selected)
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

        _navigationService.NavigateTo("ZumenSearch.Views.Rent.ParkingSearchResultPage", SlideNavigationTransitionEffect.FromLeft);
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
    }

    [RelayCommand]
    private void AddNewRentLessor()
    {
        var newId = Guid.CreateVersion7().ToString("N");
        var shell = _shellRentLessorFactory.Create(new Models.PersonNatural(newId, Models.Base.EnumEntryStatus.New));
        
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
    public async Task EditRentLessor(Models.Base.PersonBase selected) //Models.Common.PersonSearchResultItem
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
        //var res = _dataAccessService.SelectRentLessorById(lessorId);// Go back to UI thred. Let's not do > .ConfigureAwait(false);
        var res = await Task.Run(() => _dataAccessService.SelectRentLessorById(lessorId), _cts.Token);
        if (res.IsError)
        {
            Debug.WriteLine(res.Error.ErrText + Environment.NewLine + res.Error.ErrDescription + Environment.NewLine + res.Error.ErrPlace + Environment.NewLine + res.Error.ErrPlaceParent);

            //ErrorMain = res.Error;
            //IsMainErrorInfoBarVisible = true;

            // TODO: Show error message to user
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
    public async Task DeleteRentLessor(Models.Base.PersonBase selected) //Models.Common.PersonSearchResultItem
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
        //var res = _dataAccessService.SelectRentLessorById(lessorId);// Go back to UI thred. Let's not do > .ConfigureAwait(false);
        var res = await Task.Run(() => _dataAccessService.DeleteRentLessor(lessorId), _cts.Token);
        if (res.IsError)
        {
            Debug.WriteLine(res.Error.ErrText + Environment.NewLine + res.Error.ErrDescription + Environment.NewLine + res.Error.ErrPlace + Environment.NewLine + res.Error.ErrPlaceParent);

            //ErrorMain = res.Error;
            //IsMainErrorInfoBarVisible = true;

            // TODO: Show error message to user
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
            Models.Base.EnumEntryStatus.New);

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

        var result = await Task.Run(
            () => _dataAccessService
                .SelectSaleResidentialsByNameKeyword(query),
            _cts.Token);

        if (result.IsError)
        {
            Debug.WriteLine(
                $"{result.Error.ErrText}{Environment.NewLine}" +
                $"{result.Error.ErrDescription}{Environment.NewLine}" +
                $"{result.Error.ErrPlace}");

            return;
        }

        foreach (var item in result.PropertySearchResult)
        {
            item.BasePath = Path.Combine(
                App.PropertyBlobDataFolder,
                item.Id);
        }

        SaleResidentialBldgSearchResult =
            new(result.PropertySearchResult);

        _navigationService.NavigateTo(
            "ZumenSearch.Views.Sale.ResidentialSearchResultPage",
            SlideNavigationTransitionEffect.FromLeft);
    }

    [RelayCommand(CanExecute = nameof(EditSaleResidentialBldgCanExecute))]
    private async Task EditSaleResidentialBldg(
    Models.Common.PropertySearchResultItem? selected)
    {
        if (selected is null || string.IsNullOrWhiteSpace(selected.Id))
        {
            return;
        }

        var result = await Task.Run(
            () => _dataAccessService.SelectSaleResidentialById(selected.Id),
            _cts.Token);

        if (result.IsError || result.Building is null)
        {
            Debug.WriteLine(
                $"{result.Error.ErrText}{Environment.NewLine}" +
                $"{result.Error.ErrDescription}{Environment.NewLine}" +
                $"{result.Error.ErrPlace}");

            return;
        }

        var shell =
            _shellSaleResidentialPropertyFactory.Create(result.Building);

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
    private static bool EditSaleResidentialBldgCanExecute(
        Models.Common.PropertySearchResultItem? selected)
    {
        return selected is not null &&
               !string.IsNullOrWhiteSpace(selected.Id);
    }

    [RelayCommand(CanExecute = nameof(DeleteSaleResidentialBldgCanExecute))]
    private void DeleteSaleResidentialBldg(
    Models.Common.PropertySearchResultItem? selected)
    {
        if (selected is null || string.IsNullOrWhiteSpace(selected.Id))
        {
            return;
        }

        var result =
            _dataAccessService.DeleteSaleResidential(selected.Id);

        if (result.IsError)
        {
            Debug.WriteLine(
                $"{result.Error.ErrText}{Environment.NewLine}" +
                $"{result.Error.ErrDescription}{Environment.NewLine}" +
                $"{result.Error.ErrPlace}");

            return;
        }

        SaleResidentialBldgSearchResult.Remove(selected);
    }

    private static bool DeleteSaleResidentialBldgCanExecute(
        Models.Common.PropertySearchResultItem? selected)
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
        var shell = _shellBrokerFactory.Create(new Models.PersonLegal(newId, Models.Base.EnumEntryStatus.New));

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
            Debug.WriteLine(res.Error.ErrText + Environment.NewLine + res.Error.ErrDescription + Environment.NewLine + res.Error.ErrPlace + Environment.NewLine + res.Error.ErrPlaceParent);

            //ErrorMain = res.Error;
            //IsMainErrorInfoBarVisible = true;

            // TODO: Show error message to user
        }
        else
        {
            BrokerSearchResult = new(res.PersonSearchResult);

            _navigationService.NavigateTo("ZumenSearch.Views.BrokerSearchResultPage", SlideNavigationTransitionEffect.FromLeft);
        }
    }

    [RelayCommand(CanExecute = nameof(EditBrokerCanExecute))]
    public async Task EditBroker(Models.Base.PersonBase selected) //Models.Common.PersonSearchResultItem
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
        //var res = _dataAccessService.SelectRentLessorById(lessorId);// Go back to UI thred. Let's not do > .ConfigureAwait(false);
        var res = await Task.Run(() => _dataAccessService.SelectBrokerById(lessorId), _cts.Token);
        if (res.IsError)
        {
            Debug.WriteLine(res.Error.ErrText + Environment.NewLine + res.Error.ErrDescription + Environment.NewLine + res.Error.ErrPlace + Environment.NewLine + res.Error.ErrPlaceParent);

            //ErrorMain = res.Error;
            //IsMainErrorInfoBarVisible = true;

            // TODO: Show error message to user
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
    public async Task DeleteBroker(Models.Base.PersonBase selected) //Models.Common.PersonSearchResultItem
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
        //var res = _dataAccessService.SelectRentLessorById(lessorId);// Go back to UI thred. Let's not do > .ConfigureAwait(false);
        var res = await Task.Run(() => _dataAccessService.DeleteBroker(lessorId), _cts.Token);
        if (res.IsError)
        {
            Debug.WriteLine(res.Error.ErrText + Environment.NewLine + res.Error.ErrDescription + Environment.NewLine + res.Error.ErrPlace + Environment.NewLine + res.Error.ErrPlaceParent);

            //ErrorMain = res.Error;
            //IsMainErrorInfoBarVisible = true;

            // TODO: Show error message to user
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
