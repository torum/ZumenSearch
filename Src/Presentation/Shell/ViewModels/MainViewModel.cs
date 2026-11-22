using System.Collections.ObjectModel;
using System.Reflection;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media.Animation;
using Windows.ApplicationModel;
using ZumenSearch.Application.SearchResults;
using ZumenSearch.Application.Services;
using ZumenSearch.Application.Services.Rent.Residentials;
using ZumenSearch.Domain.Enums;
using ZumenSearch.Infrastructure.Configuration.Contracts;
using ZumenSearch.Infrastructure.DataAccess.Contracts;
using ZumenSearch.Infrastructure.Platform;
using ZumenSearch.Presentation.Factories.AbstractFactory;
using ZumenSearch.Presentation.Messaging;
using ZumenSearch.Presentation.Search;
using ZumenSearch.Presentation.Search.ViewModels;
using ZumenSearch.Presentation.Services.Contracts;
using ZumenSearch.Presentation.Shell.Models;
using ZumenSearch.Presentation.Shell.Views;

namespace ZumenSearch.Presentation.Shell.ViewModels;

/*
 * TODO: 
 * Add Duplicate Rent Residential Property Command
 * Add Sort Rent Residential Property Command
 * Add Sort Properties Command
 * Add Inport / Export Rent Residential Property Command
 */

public partial class MainViewModel : ObservableRecipient, IRecipient<PropertyUpdatedMessage>, IRecipient<ListingUpdatedMessage>, IRecipient<LessorUpdatedMessage>, IRecipient<BrokerUpdatedMessage>, IRecipient<WindowClosedMessage<Microsoft.UI.Xaml.Window>>, IRecipient<ShowErrorInfoBarInMainWindowMessage>, IDisposable
{
    #region == Private Variables and Const ==

    private const string SearchResultPagePath = NavigationKeys.SearchResult;
    private const string RentResidentialSearchResultPagePath = NavigationKeys.RentResidentialSearchResult;
    private const string RentCommercialSearchResultPagePath = NavigationKeys.RentCommercialSearchResult;
    private const string RentParkingSearchResultPagePath = NavigationKeys.RentParkingSearchResult;
    private const string RentLessorSearchResultPagePath = NavigationKeys.RentLessorSearchResult;
    private const string SaleResidentialSearchResultPagePath = NavigationKeys.SaleResidentialSearchResult;
    private const string BrokerSearchResultPagePath = NavigationKeys.BrokerSearchResult;

    private readonly CancellationTokenSource _cts = new();

    private Task? _initializationTask;

    #endregion

    #region == Services ==

    private readonly IAbstractFactory<ZumenSearch.Domain.Entities.Rent.Residentials.Property, bool, ZumenSearch.Presentation.Features.Rent.Residentials.Views.ShellPage> _shellRentResidentialPropertyFactory;
    private readonly IAbstractFactory<ZumenSearch.Domain.Entities.Rent.Residentials.Listing, bool, ZumenSearch.Presentation.Features.Rent.Residentials.Views.Listing.ShellPage> _shellRentResidentialListingFactory;

    private readonly IAbstractFactory<ZumenSearch.Domain.Entities.Rent.Commercials.Property, ZumenSearch.Presentation.Features.Rent.Commercials.Views.ShellPage> _shellRentCommercialPropertyFactory;
    private readonly IAbstractFactory<ZumenSearch.Domain.Entities.Rent.Commercials.Listing.Listing, ZumenSearch.Presentation.Features.Rent.Commercials.Views.Listing.ShellPage> _shellRentCommercialListingFactory;
    
    private readonly IAbstractFactory<ZumenSearch.Domain.Entities.Base.PersonBase, ZumenSearch.Presentation.Features.Rent.Lessors.Views.ShellPage> _shellRentLessorFactory;
    private readonly IAbstractFactory<ZumenSearch.Domain.Entities.Base.PersonBase, ZumenSearch.Presentation.Features.Brokers.Views.ShellPage> _shellBrokerFactory;
    private readonly IAbstractFactory<ZumenSearch.Domain.Entities.Sale.Residentials.Property, ZumenSearch.Presentation.Features.Sale.Residentials.Views.ShellPage> _shellSaleResidentialPropertyFactory;

    private readonly IDataAccessService _dataAccessService;
    private readonly PropertySearchService _propertySearchService;
    private readonly RentResidentialPropertyService _rentResidentialPropertyService;
    private readonly RentResidentialListingService _rentResidentialListingService;
    private readonly RentPersonService _rentPersonService;
    private readonly INavigationService _navigationService;
    private readonly IDispatcherService _dispatcherService;
    private readonly IConfigurationService _configurationService;
    private readonly IDialogGenericService _dialogService;

    #endregion

    public MainViewModel(IAbstractFactory<ZumenSearch.Domain.Entities.Rent.Residentials.Property, bool, ZumenSearch.Presentation.Features.Rent.Residentials.Views.ShellPage> shellRentResidentialPropertyFactory, IAbstractFactory<ZumenSearch.Domain.Entities.Rent.Residentials.Listing, bool, ZumenSearch.Presentation.Features.Rent.Residentials.Views.Listing.ShellPage> shellRentResidentialListingFactory, IAbstractFactory<ZumenSearch.Domain.Entities.Rent.Commercials.Property, ZumenSearch.Presentation.Features.Rent.Commercials.Views.ShellPage> shellRentCommercialPropertyFactory, IAbstractFactory<ZumenSearch.Domain.Entities.Rent.Commercials.Listing.Listing, ZumenSearch.Presentation.Features.Rent.Commercials.Views.Listing.ShellPage> shellRentCommercialListingFactory, IAbstractFactory<ZumenSearch.Domain.Entities.Base.PersonBase, ZumenSearch.Presentation.Features.Rent.Lessors.Views.ShellPage> shellRentLessorFactory, IAbstractFactory<ZumenSearch.Domain.Entities.Sale.Residentials.Property, ZumenSearch.Presentation.Features.Sale.Residentials.Views.ShellPage> shellSaleResidentialPropertyFactory, IAbstractFactory<ZumenSearch.Domain.Entities.Base.PersonBase, ZumenSearch.Presentation.Features.Brokers.Views.ShellPage> shellBrokerFactory, INavigationService navigationService, IDataAccessService dataAccessService, PropertySearchService propertySearchService, RentResidentialPropertyService rentResidentialPropertyService, RentResidentialListingService rentResidentialListingService, RentPersonService rentPersonService, IDispatcherService dispatcherService, IConfigurationService configurationService, IDialogGenericService dialogService)
    {
        _shellRentResidentialPropertyFactory = shellRentResidentialPropertyFactory;
        _shellRentResidentialListingFactory = shellRentResidentialListingFactory;
        _shellRentCommercialPropertyFactory = shellRentCommercialPropertyFactory;
        _shellRentCommercialListingFactory = shellRentCommercialListingFactory;
        _shellRentLessorFactory = shellRentLessorFactory;
        _shellSaleResidentialPropertyFactory = shellSaleResidentialPropertyFactory;
        _shellBrokerFactory = shellBrokerFactory;
        _navigationService = navigationService;
        _dataAccessService = dataAccessService;
        _propertySearchService = propertySearchService;
        _rentResidentialPropertyService = rentResidentialPropertyService;
        _rentResidentialListingService = rentResidentialListingService;
        _rentPersonService = rentPersonService;
        _configurationService = configurationService;
        _dispatcherService = dispatcherService;
        _dialogService = dialogService;

        VersionDescription = GetVersionDescription();

        // Ready to receive messages.
        // Add matching unregistrations in Dispose().
        WeakReferenceMessenger.Default.Register<PropertyUpdatedMessage>(this);
        WeakReferenceMessenger.Default.Register<ListingUpdatedMessage>(this);
        WeakReferenceMessenger.Default.Register<LessorUpdatedMessage>(this);
        WeakReferenceMessenger.Default.Register<BrokerUpdatedMessage>(this);
        WeakReferenceMessenger.Default.Register<WindowClosedMessage<Microsoft.UI.Xaml.Window>>(this);
        WeakReferenceMessenger.Default.Register<ShowErrorInfoBarInMainWindowMessage>(this);
    }

    #region == Properties ==

    [ObservableProperty]
    public partial string VersionDescription { get; set; }

    #region == エラー関連 ==

    // Change sevirity to Error for simple error messages.
    [ObservableProperty]
    public partial bool IsInfoBarErrorOpen { get; set; }

    [ObservableProperty]
    public partial string InfoBarErrorMessage { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string InfoBarErrorTitle { get; set; } = string.Empty;

    [ObservableProperty]
    public partial InfoBarSeverity InfoBarSeverity { get; set; } = InfoBarSeverity.Warning;

    #endregion

    #region == Window management ==

    public List<ZumenSearch.Presentation.Features.Rent.Residentials.Views.EditorWindow> RentResidentialEditorList { get; } = [];
    public int RentResidentialEditorWinWidth { get; set; } = 1366;
    public int RentResidentialEditorWinHeight { get; set; } = 768;
    public int RentResidentialEditorWinLeft { get; set; } = 130;
    public int RentResidentialEditorWinTop { get; set; } = 130;

    public List<ZumenSearch.Presentation.Features.Rent.Residentials.Views.Listing.EditorWindow> RentResidentialListingEditorList { get; } = [];
    public int RentResidentialListingEditorWinWidth { get; set; } = 1366;
    public int RentResidentialListingEditorWinHeight { get; set; } = 768;
    public int RentResidentialListingEditorWinLeft { get; set; } = 130;
    public int RentResidentialListingEditorWinTop { get; set; } = 130;

    public List<ZumenSearch.Presentation.Features.Rent.Commercials.Views.EditorWindow> RentCommercialEditorList { get;} = [];
    public int RentCommercialEditorWinWidth { get; set; } = 1366;
    public int RentCommercialEditorWinHeight { get; set; } = 768;
    public int RentCommercialEditorWinLeft { get; set; } = 130;
    public int RentCommercialEditorWinTop { get; set; } = 130;

    public List<ZumenSearch.Presentation.Features.Rent.Commercials.Views.Listing.EditorWindow> RentCommercialListingEditorList { get;} = [];
    public int RentCommercialListingEditorWinWidth { get; set; } = 1366;
    public int RentCommercialListingEditorWinHeight { get; set; } = 768;
    public int RentCommercialListingEditorWinLeft { get; set; } = 130;
    public int RentCommercialListingEditorWinTop { get; set; } = 130;

    public List<ZumenSearch.Presentation.Features.Rent.Lessors.Views.EditorWindow> LessorEditorList { get;} = [];
    public int LessorEditorWinWidth { get; set; } = 1366;
    public int LessorEditorWinHeight { get; set; } = 768;
    public int LessorEditorWinLeft { get; set; } = 130;
    public int LessorEditorWinTop { get; set; } = 130;

    public List<ZumenSearch.Presentation.Features.Brokers.Views.EditorWindow> BrokerEditorList { get;} = [];
    public int BrokerEditorWinWidth { get; set; } = 1366;
    public int BrokerEditorWinHeight { get; set; } = 768;
    public int BrokerEditorWinLeft { get; set; } = 130;
    public int BrokerEditorWinTop { get; set; } = 130;


    #endregion

    #region == Navigation ==

    public ObservableCollection<Breadcrumb> BreadcrumbItemsRent { get; set; } =
    [
        new() { Name = "総合物件検索", Page = NavigationKeys.Search }
    ];
    public ObservableCollection<Breadcrumb> BreadcrumbItemsRentSearchResult { get; set; } =
    [
        new() { Name = "総合物件検索", Page = NavigationKeys.Search },
        new() { Name = "検索結果", Page = NavigationKeys.SearchResult },
    ];

    public ObservableCollection<Breadcrumb> BreadcrumbItemsResidential { get; set; } =
    [
        new() { Name = "募集物件検索", Page = NavigationKeys.RentResidentialSearch }
    ];
    public ObservableCollection<Breadcrumb> BreadcrumbItemsResidentialSearchResult { get; set; } =
    [
        new() { Name = "募集物件検索", Page = NavigationKeys.RentResidentialSearch },
        new() { Name = "検索結果", Page = NavigationKeys.RentResidentialSearchResult },
    ];

    public ObservableCollection<Breadcrumb> BreadcrumbItemsCommercial { get; set; } =
    [
        new() { Name = "募集物件検索", Page = NavigationKeys.RentCommercialSearch }
    ];
    public ObservableCollection<Breadcrumb> BreadcrumbItemsCommercialSearchResult { get; set; } =
    [
        new() { Name = "募集物件検索", Page = NavigationKeys.RentCommercialSearch },
        new() { Name = "検索結果", Page = NavigationKeys.RentCommercialSearchResult },
    ];

    public ObservableCollection<Breadcrumb> BreadcrumbItemsParking { get; set; } =
    [
    new() { Name = "募集物件検索", Page = NavigationKeys.RentParkingSearch }
    ];
    public ObservableCollection<Breadcrumb> BreadcrumbItemsParkingSearchResult { get; set; } =
    [
        new() { Name = "募集物件検索", Page = NavigationKeys.RentParkingSearch },
        new() { Name = "検索結果", Page = NavigationKeys.RentParkingSearchResult },
    ];

    public ObservableCollection<Breadcrumb> BreadcrumbItemsLessor { get; set; } =
    [
        new() { Name = "貸主検索", Page = NavigationKeys.RentLessorSearch }
    ];
    public ObservableCollection<Breadcrumb> BreadcrumbItemsLessorSearchResult { get; set; } =
    [
        new() { Name = "貸主検索", Page = NavigationKeys.RentLessorSearch },
        new() { Name = "検索結果", Page = NavigationKeys.RentLessorSearchResult },
    ];

    public ObservableCollection<Breadcrumb> BreadcrumbItemsBroker { get; set; } =
    [
        new() { Name = "宅建業者検索", Page = NavigationKeys.BrokerSearch }
    ];
    public ObservableCollection<Breadcrumb> BreadcrumbItemsBrokerSearchResult { get; set; } =
    [
        new() { Name = "宅建業者検索", Page = NavigationKeys.BrokerSearch },
        new() { Name = "検索結果", Page = NavigationKeys.BrokerSearchResult },
    ];

    #endregion

    #region == RecentProperties ==

    public ObservableCollection<PropertySearchResultItemViewModel> RecentProperties
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

    // Rent or Sale
    [ObservableProperty]
    public partial int SegIndex { get; set; } = 0;

    [ObservableProperty]
    public partial string SearchQuery { get; set; } = string.Empty;

    [ObservableProperty]
    public partial PropertySearchTarget SelectedPropertySearchTarget { get; set; }

    [RelayCommand]
    private void SetPropertySearchTarget(PropertySearchTarget searchTarget)
    {
        SelectedPropertySearchTarget = searchTarget;
    }

    public ObservableCollection<PropertySearchResultItemViewModel> PropertySearchResult
    {
        get; set
        {
            if (SetProperty(ref field, value))
            {
                //
            }
        }
    } = [];

    public ObservableCollection<ListingSearchResultItemViewModel> RentResidentialListingSearchResult
    {
        get; set
        {
            if (SetProperty(ref field, value))
            {
                //
            }
        }
    } = [];

    public ObservableCollection<ListingSearchResultItemViewModel> RentCommercialListingSearchResult
    {
        get; set
        {
            if (SetProperty(ref field, value))
            {
                //
            }
        }
    } = [];

    public ObservableCollection<PersonSearchResultItemViewModel> RentLessorSearchResult
    {
        get; set
        {
            if (SetProperty(ref field, value))
            {
                //
            }
        }
    } = [];

    public ObservableCollection<PersonSearchResultItemViewModel> BrokerSearchResult
    {
        get; set
        {
            if (SetProperty(ref field, value))
            {
                //
            }
        }
    } = [];

    public ObservableCollection<PropertySearchResultItemViewModel> SaleResidentialSearchResult
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

    #region == Messages ==

    public void Receive(PropertyUpdatedMessage property)
    {
        var building = property.Value;
        if (building is null)
        {
            return;
        }

        // PropertySearchResult
        foreach (var item in PropertySearchResult)
        {
            if (!item.Id.Equals(building.Id, StringComparison.Ordinal))
            {
                continue;
            }

            Debug.WriteLine($"PropertyUpdatedMessage received for property ID: {building.Id}, updating PropertySearchResult item.");

            item.SetName(building.Name);
            item.ThumbnailFilename = building.ThumbnailFilename;
        }

        // RecentProperties
        foreach (var item in RecentProperties)
        {
            if (!item.Id.Equals(building.Id, StringComparison.Ordinal))
            {
                Debug.WriteLine($"PropertyUpdatedMessage received for property ID: {building.Id}, but no matching PropertySearchResult item found.");
                continue;
            }

            Debug.WriteLine($"PropertyUpdatedMessage received for property ID: {building.Id}, updating PropertySearchResult item.");

            item.SetName(building.Name);
            item.ThumbnailFilename = building.ThumbnailFilename;
        }

        // ?
        foreach (var item in SaleResidentialSearchResult)
        {
            if (!item.Id.Equals(building.Id, StringComparison.Ordinal)) continue;

            item.SetName(building.Name);
            item.ThumbnailFilename = building.ThumbnailFilename;
        }
    }

    public void Receive(ListingUpdatedMessage message)
    {
        var updated = message.Value;
        if (updated is null || updated.Status != EntityStatus.Saved) return;

        ObservableCollection<ListingSearchResultItemViewModel> results;
        switch (updated.PropertyContextType)
        {
            case PropertyContextType.RentResidential:
                results = RentResidentialListingSearchResult;
                break;
            case PropertyContextType.RentCommercial:
                results = RentCommercialListingSearchResult;
                break;
            default:
                return;
        }

        foreach (var item in results)
        {
            if (item.PropertyContextType != updated.PropertyContextType || !item.PropertyId.Equals(updated.PropertyId, StringComparison.Ordinal) || !item.Id.Equals(updated.Id, StringComparison.Ordinal)) continue;

            item.SetName(updated.Name);
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
            if (!item.Id.Equals(person.Id, StringComparison.Ordinal))
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
            if (!item.Id.Equals(person.Id, StringComparison.Ordinal))
            {
                continue;
            }

            item.SetName(person.Name);
        }
    }

    public void Receive(WindowClosedMessage<Microsoft.UI.Xaml.Window> window)
    {
        var win = window.Value;

        if (win is null)
        {
            return;
        }

        switch (win)
        {
            case ZumenSearch.Presentation.Features.Rent.Residentials.Views.EditorWindow rwin:
                RentResidentialEditorList.Remove(rwin);
                break;
            case ZumenSearch.Presentation.Features.Rent.Residentials.Views.Listing.EditorWindow rlwin:
                RentResidentialListingEditorList.Remove(rlwin);
                break;
            case ZumenSearch.Presentation.Features.Rent.Commercials.Views.EditorWindow cwin:
                RentCommercialEditorList.Remove(cwin);
                break;
            case ZumenSearch.Presentation.Features.Rent.Commercials.Views.Listing.EditorWindow clwin:
                RentCommercialListingEditorList.Remove(clwin);
                break;
            case ZumenSearch.Presentation.Features.Rent.Lessors.Views.EditorWindow lewin:
                LessorEditorList.Remove(lewin);
                break;
            case ZumenSearch.Presentation.Features.Brokers.Views.EditorWindow bwin:
                BrokerEditorList.Remove(bwin);
                break;
        }
    }

    public void Receive(ShowErrorInfoBarInMainWindowMessage message)
    {
        ShowSimpleErrorInfoBar(message.Title, message.Message);
    }

    #endregion

    #region == Public Methods ==

    public void Initialize(XamlRoot xamlRoot, MainWindow mainWindow)
    {
        _dialogService.Initialize(xamlRoot, mainWindow);
    }

    public Task InitializeAsync() => _initializationTask ??= InitializeDatabase();

    public void RemoveResidentialListingSearchResults(string propertyId, string listingId)
    {
        foreach (var result in RentResidentialListingSearchResult.Where(result => result.Id.Equals(listingId, StringComparison.Ordinal) && result.PropertyId.Equals(propertyId, StringComparison.Ordinal)).ToList())
        {
            RentResidentialListingSearchResult.Remove(result);
        }
    }

    public void RemoveCommercialListingSearchResults(string propertyId, string listingId)
    {
        foreach (var result in RentCommercialListingSearchResult.Where(result => result.Id.Equals(listingId, StringComparison.Ordinal) && result.PropertyId.Equals(propertyId, StringComparison.Ordinal)).ToList())
        {
            RentCommercialListingSearchResult.Remove(result);
        }
    }

    public void UpdateSettings()
    {
        // Update settings logic here
        var config = _configurationService.CurrentAppSettings;
        //config.IsNavigationViewMenuOpen = IsNavigationViewMenuOpen;
        //config.Theme = Theme;
        //config.SystemBackdrop = Material;
    }

    public void Dispose()
    {
        Debug.WriteLine("MainViewModel Dispose().");

        WeakReferenceMessenger.Default.Unregister<PropertyUpdatedMessage>(this);
        WeakReferenceMessenger.Default.Unregister<ListingUpdatedMessage>(this);
        WeakReferenceMessenger.Default.Unregister<LessorUpdatedMessage>(this);
        WeakReferenceMessenger.Default.Unregister<BrokerUpdatedMessage>(this);
        WeakReferenceMessenger.Default.Unregister<WindowClosedMessage<Microsoft.UI.Xaml.Window>>(this);
        WeakReferenceMessenger.Default.Unregister<ShowErrorInfoBarInMainWindowMessage>(this);

        _cts?.Cancel();

        _cts?.Dispose();

        GC.SuppressFinalize(this);
    }

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

                ShowErrorInfoBar(res.Error);

                return;
            }

            // Get recent properties after database initialization.
            GetRecentPropertiesCommand.Execute(null);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"InitializeDatabase: {ex}");

            ShowSimpleErrorInfoBar("データベース初期化エラー", $"データベースの初期化中に予期しないエラーが発生しました。{Environment.NewLine}{ex.Message}", InfoBarSeverity.Error);
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

    private void ShowSimpleErrorInfoBar(string title, string message, InfoBarSeverity severity = InfoBarSeverity.Warning)
    {
        InfoBarErrorTitle = title;
        InfoBarErrorMessage = message;
        InfoBarSeverity = severity;
        IsInfoBarErrorOpen = true;
    }

    private void ShowErrorInfoBar(ZumenSearch.Application.Results.ErrorInfo err)
    {
        InfoBarErrorTitle = err.Title;
        InfoBarErrorMessage =
            err.Message + Environment.NewLine +
            err.Description + Environment.NewLine +
            err.Operation + Environment.NewLine +
            err.MethodName;
        InfoBarSeverity = InfoBarSeverity.Error;
        IsInfoBarErrorOpen = true;
    }

    #endregion

    #region == Commands ==

    #region == 総合検索 ==

    [RelayCommand]
    private async Task GetRecentProperties()
    {
        RecentProperties.Clear();

        var res = await Task.Run(() => _propertySearchService.GetRecent(), _cts.Token);

        if (res.IsError)
        {
            Debug.WriteLine(
                res.Error.Title + Environment.NewLine +
                res.Error.Message + Environment.NewLine +
                res.Error.Description + Environment.NewLine +
                res.Error.Operation + Environment.NewLine +
                res.Error.MethodName + Environment.NewLine +
                res.Error.FullDump);

            ShowErrorInfoBar(res.Error);
        }
        else
        {
            // Set the BasePath for each property in the search result to show thumbnails.
            foreach (var item in res.PropertySearchResult)
            {
                item.BasePath = System.IO.Path.Combine(App.PropertyBlobDataFolder, item.Id);
            }

            RecentProperties = new(res.PropertySearchResult.Select(item => new PropertySearchResultItemViewModel(item)));
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

        var res = await Task.Run(() => _propertySearchService.Search(query, SelectedPropertySearchTarget), _cts.Token);

        if (res.IsError)
        {
            Debug.WriteLine(
                res.Error.Title + Environment.NewLine +
                res.Error.Message + Environment.NewLine +
                res.Error.Description + Environment.NewLine +
                res.Error.Operation + Environment.NewLine +
                res.Error.MethodName + Environment.NewLine +
                res.Error.FullDump);

            ShowErrorInfoBar(res.Error);
        }
        else
        {
            foreach (var item in res.PropertySearchResult)
            {
                item.BasePath = System.IO.Path.Combine(App.PropertyBlobDataFolder, item.Id);

                Debug.WriteLine($"SearchProperties: Name: {item.Name}, BasePath: {item.BasePath}, Thumbnail: {item.ThumbImagePath}, ThumbnailFilename: {item.ThumbnailFilename}");
            }

            PropertySearchResult = new(res.PropertySearchResult.Select(item => new PropertySearchResultItemViewModel(item)));

            _navigationService.NavigateTo(SearchResultPagePath, SlideNavigationTransitionEffect.FromLeft);
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
    private async Task EditProperty(PropertySearchResultItemViewModel? selected)
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

        if (selected.PropertyContextType == PropertyContextType.RentResidential)
        {
            await EditRentResidentialFromId(propertyId);
        }
        else if (selected.PropertyContextType == PropertyContextType.RentCommercial)
        {
            await EditRentCommercialFromId(propertyId);
        }
        else if (selected.PropertyContextType == PropertyContextType.RentParking)
        {
            Debug.WriteLine("EditPropertiesCommand not yet implemented.");
        }
        else if (selected.PropertyContextType == PropertyContextType.SaleResidential)
        {
            Debug.WriteLine("EditPropertiesCommand not yet implemented.");
        }
        else if (selected.PropertyContextType == PropertyContextType.SaleCommercial)
        {
            Debug.WriteLine("EditPropertiesCommand not yet implemented.");
        }
        else if (selected.PropertyContextType == PropertyContextType.SaleLand)
        {
            Debug.WriteLine("EditPropertiesCommand not yet implemented.");
        }

        // TODO lessor and broker
    }
    private static bool EditPropertyCanExecute(PropertySearchResultItemViewModel? selected)
    {
        if (selected is null)
        //if (string.IsNullOrEmpty(rentId))
        {
            return false;
        }

        return true;
    }

    // 物件複製
    [RelayCommand(CanExecute = nameof(DuplicatePropertyCanExecute))]
    private async Task DuplicateProperty(PropertySearchResultItemViewModel? selected)
    {
        if (selected is null)
        {
            return;
        }

        var propertyId = selected.Id;

        if (string.IsNullOrEmpty(propertyId))
        {
            Debug.WriteLine("DuplicatePropertyCommand executed but no item is selected.");
            return;
        }

        if (selected.PropertyContextType == PropertyContextType.RentResidential)
        {
            await DuplicateRentResidentialFromId(propertyId);
        }
        else if (selected.PropertyContextType == PropertyContextType.RentCommercial)
        {
            Debug.WriteLine("DuplicatePropertyCommand not yet implemented.");
        }
        else if (selected.PropertyContextType == PropertyContextType.RentParking)
        {
            Debug.WriteLine("DuplicatePropertyCommand not yet implemented.");
        }
        else if (selected.PropertyContextType == PropertyContextType.SaleResidential)
        {
            Debug.WriteLine("DuplicatePropertyCommand not yet implemented.");
        }
        else if (selected.PropertyContextType == PropertyContextType.SaleCommercial)
        {
            Debug.WriteLine("DuplicatePropertyCommand not yet implemented.");
        }
        else if (selected.PropertyContextType == PropertyContextType.SaleLand)
        {
            Debug.WriteLine("DuplicatePropertyCommand not yet implemented.");
        }

        // TODO lessor and broker
    }
    private static bool DuplicatePropertyCanExecute(PropertySearchResultItemViewModel? selected)
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
    private async Task DeleteProperty(PropertySearchResultItemViewModel? selected)
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

        if (selected.PropertyContextType == PropertyContextType.RentResidential)
        {
            await DeleteRentResidential(selected);
        }
        else if (selected.PropertyContextType == PropertyContextType.RentCommercial)
        {
            await DeleteRentCommercial(selected);
        }
        else if (selected.PropertyContextType == PropertyContextType.RentParking)
        {
            Debug.WriteLine("DeletePropertyCommand not yet implemented.");
        }
        else if (selected.PropertyContextType == PropertyContextType.SaleResidential)
        {
            Debug.WriteLine("DeletePropertyCommand not yet implemented.");
        }
        else if (selected.PropertyContextType == PropertyContextType.SaleCommercial)
        {
            Debug.WriteLine("DeletePropertyCommand not yet implemented.");
        }
        else if (selected.PropertyContextType == PropertyContextType.SaleLand)
        {
            Debug.WriteLine("DeletePropertyCommand not yet implemented.");
        }

        // TODO lessor and broker
    }
    private static bool DeletePropertyCanExecute(PropertySearchResultItemViewModel? selected)
    {
        if (selected is null)
        //if (string.IsNullOrEmpty(rentId))
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

        var res = await Task.Run(() => _propertySearchService.Search(queryText), _cts.Token);
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

            ShowErrorInfoBar(res.Error);
        }
        else
        {
            if (res.PropertySearchResult.Count > 0)
            {
                foreach (var item in res.PropertySearchResult)
                {
                    var autoSuggest = new ZumenSearch.Presentation.Search.AutoSuggestItem
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
                var autoSuggest = new ZumenSearch.Presentation.Search.AutoSuggestItem
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



    #endregion

    #region == 賃貸住居用 ==

    // 建物新規追加
    [RelayCommand]
    private void AddNewRentResidential()
    {
        var newId = Guid.CreateVersion7().ToString("N");
        var shell = _shellRentResidentialPropertyFactory.Create(new ZumenSearch.Domain.Entities.Rent.Residentials.Property(newId, EntityStatus.New), false);

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
    private async Task EditRentResidential(PropertySearchResultItemViewModel? selected)
    {
        var propertyId = selected?.Id;

        if (string.IsNullOrEmpty(propertyId))
        {
            Debug.WriteLine("EditRentResidentialCommand executed but no item is selected.");
            return;
        }

        await EditRentResidentialFromId(propertyId);
    }
    private static bool EditRentResidentialCanExecute(PropertySearchResultItemViewModel? selected)
    {
        if (selected is null)
        //if (string.IsNullOrEmpty(rentId))
        {
            return false;
        }

        return true;
    }

    // 建物編集（IDから）
    [RelayCommand(CanExecute = nameof(EditRentResidentialFromIdCanExecute))]
    private async Task EditRentResidentialFromId(string propertyId)
    {
        if (string.IsNullOrEmpty(propertyId))
        {
            Debug.WriteLine("EditRentResidentialBldgFromId executed but no item is selected.");
            return;
        }

        await EditRentResidentialFromIdCore(propertyId);
    }

    private async Task EditRentResidentialFromIdCore(string propertyId)
    {
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
        var res = await Task.Run(() => _rentResidentialPropertyService.GetById(propertyId), _cts.Token);
        if (res.IsError)
        {
            Debug.WriteLine(
                res.Error.Title + Environment.NewLine +
                res.Error.Message + Environment.NewLine +
                res.Error.Description + Environment.NewLine +
                res.Error.Operation + Environment.NewLine +
                res.Error.MethodName + Environment.NewLine +
                res.Error.FullDump);

            ShowErrorInfoBar(res.Error);

            return;
        }

        if (res.Property is null)
        {
            // TODO: Show error message to user.
            Debug.WriteLine($"Property for {propertyId} is null. Cannot open editor.");
            return;
        }

        var editorShell = _shellRentResidentialPropertyFactory.Create(res.Property, false);//_editorFactory.Create();
        //editorShell.ViewModel.SetListingSummaries(res.ListingSummaries);

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
    private static bool EditRentResidentialFromIdCanExecute(string propertyId)
    {
        if (string.IsNullOrEmpty(propertyId))
        {
            return false;
        }

        return true;
    }

    // 建物複製（IDから）
    [RelayCommand(CanExecute = nameof(DuplicateRentResidentialFromIdCanExecute))]
    private async Task DuplicateRentResidentialFromId(string propertyId)
    {
        if (string.IsNullOrEmpty(propertyId))
        {
            Debug.WriteLine("DuplicateRentResidential executed but no item is selected.");
            return;
        }

        await DuplicateRentResidentialCore(propertyId);
    }

    private async Task DuplicateRentResidentialCore(string propertyId)
    {
        // Access Database to get the full entry data.
        var res = await Task.Run(() => _rentResidentialPropertyService.GetById(propertyId, true), _cts.Token);
        if (res.IsError)
        {
            Debug.WriteLine(
                res.Error.Title + Environment.NewLine +
                res.Error.Message + Environment.NewLine +
                res.Error.Description + Environment.NewLine +
                res.Error.Operation + Environment.NewLine +
                res.Error.MethodName + Environment.NewLine +
                res.Error.FullDump);

            ShowErrorInfoBar(res.Error);

            return;
        }

        if (res.Property is null)
        {
            // TODO: Show error message to user.
            Debug.WriteLine($"Property for {propertyId} is null. Cannot open editor.");
            return;
        }

        // All the ids in the duplicated property and its attachments are already new,
        // we just need to set the BasePath for the original pictures and PDFs to the source directory.

        // Set the BasePath for ORIGINAL pictures and PDFs to the source directory here.
        // This is necessary because the duplicated property has a new ID, and we want to keep the original files in their original location
        // so that we can copy them to the new location and also keep track of them as unsaved in the PropertyViewModel when opens the duplicated property in the editor.
        var sourceDirectory = Path.Combine(App.PropertyBlobDataFolder, propertyId); // use the original propertyId to get the source directory for the original files.
        foreach (var picture in res.Property.Pictures)
        {
            picture.BasePath = sourceDirectory;
        }
        foreach (var pdf in res.Property.Pdfs)
        {
            pdf.BasePath = sourceDirectory;
        }

        var editorShell = _shellRentResidentialPropertyFactory.Create(res.Property, true);
        //editorShell.ViewModel.SetListingSummaries(res.ListingSummaries);

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
    private static bool DuplicateRentResidentialFromIdCanExecute(string propertyId)
    {
        if (string.IsNullOrEmpty(propertyId))
        {
            return false;
        }

        return true;
    }

    // 物件削除
    [RelayCommand(CanExecute = nameof(DeleteRentResidentialCanExecute))]
    private async Task DeleteRentResidential(PropertySearchResultItemViewModel? selected)
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

        await DeleteRentResidentialCore(selected);
    }

    private async Task DeleteRentResidentialCore(PropertySearchResultItemViewModel selected)
    {
        var selectedId = selected.Id;
        var listingEditor = RentResidentialListingEditorList.FirstOrDefault(editor => editor.PropertyId == selectedId);
        if (listingEditor is not null)
        {
            ShowSimpleErrorInfoBar("編集画面が開いています", "物件の編集画面を閉じてから削除してください。");
            listingEditor.Activate();
            return;
        }

        var propertyEditor = RentResidentialEditorList.FirstOrDefault(editor => editor.Id == selectedId);
        if (propertyEditor is not null)
        {
            ShowSimpleErrorInfoBar("編集画面が開いています", "物件の編集画面を閉じてから削除してください。");
            propertyEditor.Activate();
            return;
        }

        if (await _dialogService.ShowDeleteConfirmationDialog("この物件を削除しますか？") != Microsoft.UI.Xaml.Controls.ContentDialogResult.Primary) return;

        var res = await Task.Run(() => _rentResidentialPropertyService.Delete(selectedId), _cts.Token);
        if (res.IsError)
        {
            Debug.WriteLine(
                res.Error.Title + Environment.NewLine +
                res.Error.Message + Environment.NewLine +
                res.Error.Description + Environment.NewLine +
                res.Error.Operation + Environment.NewLine +
                res.Error.MethodName + Environment.NewLine +
                res.Error.FullDump);

            ShowErrorInfoBar(res.Error);
        }
        else
        {
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

            Debug.WriteLine($"DeleteRentResidentialCommand executed for {selectedId}");

            // clean up pics and pdfs.
            var propertyDataDirectoryPath = System.IO.Path.Combine(App.PropertyBlobDataFolder, selectedId);
            if (Directory.Exists(propertyDataDirectoryPath))
            {
                try
                {
                    Directory.Delete(propertyDataDirectoryPath, true);
                }
                catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
                {
                    Debug.WriteLine($"Failed to delete property attachments '{propertyDataDirectoryPath}': {ex}");
                    ShowSimpleErrorInfoBar("添付ファイルの削除に失敗しました", $"物件は削除されましたが、添付ファイルを削除できませんでした: {propertyDataDirectoryPath}。{ex.Message}");
                }
            }
        }
    }
    private static bool DeleteRentResidentialCanExecute(PropertySearchResultItemViewModel? selected)
    {
        if (selected is null)
        {
            return false;
        }

        return true;
    }

    // 部屋検索
    [RelayCommand]
    private async Task SearchRentResidentialListings(ZumenSearch.Presentation.Features.Rent.Residentials.Models.ListingSearchCriteria? criteria)
    {
        RentResidentialListingSearchResult.Clear();

        var searchCriteria = (criteria ?? new ZumenSearch.Presentation.Features.Rent.Residentials.Models.ListingSearchCriteria()).Clone().ToApplicationQuery();

                var res = await Task.Run(() => _rentResidentialListingService.Search(searchCriteria), _cts.Token);

        if (res.IsError)
        {
            Debug.WriteLine(
                res.Error.Title + Environment.NewLine +
                res.Error.Message + Environment.NewLine +
                res.Error.Description + Environment.NewLine +
                res.Error.Operation + Environment.NewLine +
                res.Error.MethodName + Environment.NewLine +
                res.Error.FullDump);

            ShowErrorInfoBar(res.Error);
        }
        else
        {
            RentResidentialListingSearchResult = new(res.ListingSearchResult.Select(item => new ListingSearchResultItemViewModel(item)));

            _navigationService.NavigateTo(RentResidentialSearchResultPagePath, SlideNavigationTransitionEffect.FromLeft);//Navigate(typeof(ZumenSearch.Presentation.Search.Views.SearchResultPage), null, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromRight });
        }
    }

    // 部屋編集
    [RelayCommand(CanExecute = nameof(EditRentResidentialListingCanExecute))]
    private async Task EditRentResidentialListing(ListingSearchResultItemViewModel? selected)
    {
        if (selected is null)
        {
            Debug.WriteLine("EditRentResidentialRoomCommand executed but no item is selected.");
            return;
        }

        var listingId = selected.Id;
        var buildingId = selected.PropertyId;

        if (string.IsNullOrEmpty(listingId) || string.IsNullOrEmpty(buildingId))
        {
            Debug.WriteLine("EditRentResidentialListingCommand executed without a listing or property id.");
            return;
        }

        await EditRentResidentialListingCore(selected);
    }

    private async Task EditRentResidentialListingCore(ListingSearchResultItemViewModel selected)
    {
        var listingId = selected.Id;
        var buildingId = selected.PropertyId;

        //Debug.WriteLine($"EditRentResidentialRoomCommand executed for {selected.Id}");

        // Access Database to get the full entry data.
        var res = await Task.Run(() => _rentResidentialListingService.GetById(buildingId, listingId), _cts.Token);
        if (res.IsError)
        {
            Debug.WriteLine(
                res.Error.Title + Environment.NewLine +
                res.Error.Message + Environment.NewLine +
                res.Error.Description + Environment.NewLine +
                res.Error.Operation + Environment.NewLine +
                res.Error.MethodName + Environment.NewLine +
                res.Error.FullDump);

            ShowErrorInfoBar(res.Error);

            return;
        }

        if (res.Listing is null)
        {
            Debug.WriteLine($"Listing for {listingId} is null. Cannot open editor.");
            return;
        }

        var editorShell = _shellRentResidentialListingFactory.Create(res.Listing, false);
        var propertyEditor = RentResidentialEditorList.FirstOrDefault(editor => editor.Id == buildingId && editor.ViewModel is { IsActive: true });
        if (propertyEditor?.ViewModel is { } propertyViewModel) editorShell.ViewModel.SetOwnershipVisibility(propertyViewModel.IsUnitLevelOwnership);
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
        editorWindow.AppWindow.MoveInZOrderAtTop();
    }

    private static bool EditRentResidentialListingCanExecute(ListingSearchResultItemViewModel? selected)
    {
        if (selected is null)
        {
            return false;
        }

        return true;
    }

    // 部屋複製
    [RelayCommand(CanExecute = nameof(DuplicateRentResidentialListingCanExecute))]
    private async Task DuplicateRentResidentialListing(ListingSearchResultItemViewModel? selected)
    {
        if (selected is null)
        {
            Debug.WriteLine("DuplicateRentResidentialRoom executed but no item is selected.");
            return;
        }

        var roomId = selected.Id;
        var buildingId = selected.PropertyId;

        if (string.IsNullOrEmpty(roomId) || string.IsNullOrEmpty(buildingId))
        {
            Debug.WriteLine("DuplicateRentResidentialRoom executed without a listing or property id.");
            return;
        }

        await DuplicateRentResidentialListingCore(selected);
    }

    private async Task DuplicateRentResidentialListingCore(ListingSearchResultItemViewModel selected)
    {
        var listingId = selected.Id;
        var buildingId = selected.PropertyId;

        //Debug.WriteLine($"DuplicateRentResidentialListingCore executed for {selected.Id}");

        // Access Database to get the full entry data.
        var res = await Task.Run(() => _rentResidentialListingService.GetById(buildingId, listingId, true), _cts.Token);
        if (res.IsError)
        {
            Debug.WriteLine(
                res.Error.Title + Environment.NewLine +
                res.Error.Message + Environment.NewLine +
                res.Error.Description + Environment.NewLine +
                res.Error.Operation + Environment.NewLine +
                res.Error.MethodName + Environment.NewLine +
                res.Error.FullDump);

            ShowErrorInfoBar(res.Error);

            return;
        }

        if (res.Listing is null)
        {
            Debug.WriteLine($"Listing for {listingId} is null. Cannot open editor.");
            return;
        }

        // All the ids in the duplicated listing and its attachments are already new,
        // we just need to set the BasePath for the original pictures and PDFs to the source directory.

        // Set the BasePath for ORIGINAL pictures and PDFs to the source directory here.
        // This is necessary because the duplicated listing has a new ID, and we want to keep the original files in their original location
        // so that we can copy them to the new location and also keep track of them as unsaved in the ListingViewModel when opens the duplicated listing in the editor.
        var sourceDirectory = Path.Combine(App.PropertyBlobDataFolder, buildingId, listingId); // use the original propertyId and listingId to get the source directory for the original files.
        foreach (var picture in res.Listing.Pictures)
        {
            picture.BasePath = sourceDirectory;
        }
        foreach (var pdf in res.Listing.Pdfs)
        {
            pdf.BasePath = sourceDirectory;
        }

        // use second parameter true to indicate this is a duplicated listing, so that the editor can handle it accordingly.
        var editorShell = _shellRentResidentialListingFactory.Create(res.Listing, true);
        var propertyEditor = RentResidentialEditorList.FirstOrDefault(editor => editor.Id == buildingId && editor.ViewModel is { IsActive: true });
        if (propertyEditor?.ViewModel is { } propertyViewModel) editorShell.ViewModel.SetOwnershipVisibility(propertyViewModel.IsUnitLevelOwnership);
        var editorWindow = editorShell.Window;
        if (editorWindow is null)
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
        editorWindow.AppWindow.MoveInZOrderAtTop();
    }
    private static bool DuplicateRentResidentialListingCanExecute(ListingSearchResultItemViewModel? selected)
    {
        if (selected is null)
        {
            return false;
        }

        return true;
    }

    // 部屋削除
    [RelayCommand(CanExecute = nameof(DeleteRentResidentialListingCanExecute))]
    private async Task DeleteRentResidentialListing(ListingSearchResultItemViewModel? selected)
    {
        if (selected is null)
        {
            Debug.WriteLine("DeleteRentResidentialListing executed but no item is selected.");
            return;
        }

        if (string.IsNullOrEmpty(selected.Id))
        {
            Debug.WriteLine("DeleteRentResidentialListing executed but id is empty.");
            return;
        }

        var selectedPropertyId = selected.PropertyId;
        if (string.IsNullOrWhiteSpace(selectedPropertyId))
        {
            ShowSimpleErrorInfoBar("エラー", "募集物件の建物IDがないため、削除できません。");
            return;
        }

        await DeleteRentResidentialListingCore(selected);
    }

    private async Task DeleteRentResidentialListingCore(ListingSearchResultItemViewModel selected)
    {
        var selectedId = selected.Id;
        var selectedPropertyId = selected.PropertyId;
        var listingEditor = RentResidentialListingEditorList.FirstOrDefault(editor => editor.Id == selectedId);
        if (listingEditor is not null)
        {
            ShowSimpleErrorInfoBar("編集画面が開いています", "募集物件の編集画面を閉じてから削除してください。");
            listingEditor.Activate();
            return;
        }

        var res = await Task.Run(() => _rentResidentialListingService.Delete(selectedId), _cts.Token);
        if (res.IsError)
        {
            Debug.WriteLine(
                res.Error.Title + Environment.NewLine +
                res.Error.Message + Environment.NewLine +
                res.Error.Description + Environment.NewLine +
                res.Error.Operation + Environment.NewLine +
                res.Error.MethodName + Environment.NewLine +
                res.Error.FullDump);

            ShowErrorInfoBar(res.Error);
        }
        else
        {
            RemoveResidentialListingSearchResults(selectedPropertyId, selectedId);

            Debug.WriteLine($"DeleteRentResidentialRoomCommand executed for {selectedId}");

            // Check if the selected item is already being edited in another window.
            RentResidentialEditorList.ForEach(editorWindow =>
            {
                Debug.WriteLine($"Checking editor window with Id: {editorWindow.Id} for selected item with Id: {selectedPropertyId}");
                if (editorWindow.Id == selectedPropertyId)
                {
                    // If the editor window for this item is already open, remove the listing.
                    //Debug.WriteLine($"Editor window for {selected.PropertyId} is already open. Removing listing.");

                    // remove listing from the editor window's ViewModel if it exists.
                    WeakReferenceMessenger.Default.Send(new ZumenSearch.Presentation.Messaging.ListingDeletedMessage(selectedId));

                    return;
                }
            });

            // clean up pics and pdfs.
            var listingDataDirectoryPath = System.IO.Path.Combine(App.PropertyBlobDataFolder, selectedPropertyId, selectedId);
            if (Directory.Exists(listingDataDirectoryPath))
            {
                try
                {
                    Directory.Delete(listingDataDirectoryPath, true);
                }
                catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
                {
                    Debug.WriteLine($"Failed to delete listing attachments '{listingDataDirectoryPath}': {ex}");
                    ShowSimpleErrorInfoBar("添付ファイルの削除に失敗しました", $"募集物件は削除されましたが、添付ファイルを削除できませんでした: {listingDataDirectoryPath}。{ex.Message}");
                }
            }
        }
    }
    private static bool DeleteRentResidentialListingCanExecute(ListingSearchResultItemViewModel? selected)
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
        var property = new ZumenSearch.Domain.Entities.Rent.Commercials.Property(
            Guid.CreateVersion7().ToString("N"),
            EntityStatus.New);

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
    private async Task EditRentCommercial(ListingSearchResultItemViewModel? selected)
    {
        var propertyId = selected?.Id;

        if (string.IsNullOrEmpty(propertyId))
        {
            Debug.WriteLine("EditRentCommercialCommand executed but no item is selected.");
            return;
        }

        await EditRentCommercialFromId(propertyId);
    }
    private static bool EditRentCommercialCanExecute(ListingSearchResultItemViewModel? selected)
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

        await EditRentCommercialFromIdCore(propertyId);
    }

    private async Task EditRentCommercialFromIdCore(string propertyId)
    {
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

            ShowErrorInfoBar(res.Error);

            return;
        }

        if (res.Property is null)
        {
            Debug.WriteLine($"Property for {propertyId} is null. Cannot open editor.");
            return;
        }

        var editorShell = _shellRentCommercialPropertyFactory.Create(res.Property);
        editorShell.ViewModel.SetListingSummaries(res.ListingSummaries);

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
    private async Task DeleteRentCommercial(PropertySearchResultItemViewModel? selected)
    {
        if (selected is null || string.IsNullOrWhiteSpace(selected.Id))
        {
            return;
        }

        await DeleteRentCommercialCore(selected);
    }

    private async Task DeleteRentCommercialCore(PropertySearchResultItemViewModel selected)
    {
        var selectedId = selected.Id;
        if (RentCommercialListingEditorList.Count > 0)
        {
            foreach (var unitEditor in RentCommercialListingEditorList.ToList())
            {
                if (unitEditor.PropertyId != selectedId)
                {
                    continue;
                }

                if (unitEditor.ViewModel.IsDirty || unitEditor.ViewModel.SaveCommand.IsRunning || unitEditor.ViewModel.HasPendingEditorOperations || unitEditor.ViewModel.IsDiscardPending || (unitEditor.Content is ZumenSearch.Presentation.Features.Rent.Commercials.Views.Listing.ShellPage listingShell && listingShell.IsClosePending))
                {
                    unitEditor.Activate();
                    return;
                }

                unitEditor.Close();
                if (RentCommercialListingEditorList.Contains(unitEditor)) return;
            }
        }

        foreach (var editorWindow in RentCommercialEditorList.ToList())
        {
            if (editorWindow.Id != selectedId)
            {
                continue;
            }

            var editorViewModel = editorWindow.ViewModel;
            if (editorViewModel is null || editorViewModel.IsDirty || editorViewModel.IsSaving || editorViewModel.IsClosing || editorViewModel.HasPendingEditorOperations || editorViewModel.UnsavedChildEditorList.Count > 0)
            {
                editorWindow.Activate();
                return;
            }

            editorWindow.Close();
            if (RentCommercialEditorList.Contains(editorWindow)) return;
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

            ShowErrorInfoBar(res.Error);

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
    private static bool DeleteRentCommercialCanExecute(PropertySearchResultItemViewModel? selected)
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

        var res = await Task.Run(() => _dataAccessService.SelectRentCommercialListings(query), _cts.Token);

        if (res.IsError)
        {
            Debug.WriteLine(
                res.Error.Title + Environment.NewLine +
                res.Error.Message + Environment.NewLine +
                res.Error.Description + Environment.NewLine +
                res.Error.Operation + Environment.NewLine +
                res.Error.MethodName + Environment.NewLine +
                res.Error.FullDump);

            ShowErrorInfoBar(res.Error);

            return;
        }

        RentCommercialListingSearchResult = new(res.ListingSearchResult.Select(item => new ListingSearchResultItemViewModel(item)));

        _navigationService.NavigateTo(RentCommercialSearchResultPagePath, SlideNavigationTransitionEffect.FromLeft);
    }

    // 区画編集
    [RelayCommand(CanExecute = nameof(EditRentCommercialListingCanExecute))]
    private async Task EditRentCommercialListing(ListingSearchResultItemViewModel? selected)
    {
        if (selected is null)
        {
            Debug.WriteLine("EditRentCommercialUnitCommand executed but no item is selected.");
            return;
        }

        var roomId = selected.Id;
        var buildingId = selected.PropertyId;

        if (string.IsNullOrEmpty(roomId) || string.IsNullOrEmpty(buildingId))
        {
            Debug.WriteLine("EditRentCommercialUnitCommand executed without a listing or property id.");
            return;
        }

        await EditRentCommercialListingCore(selected);
    }

    private async Task EditRentCommercialListingCore(ListingSearchResultItemViewModel selected)
    {
        var listingId = selected.Id;
        var buildingId = selected.PropertyId;
        if (RentCommercialEditorList.Any(editor => editor.Id == buildingId && editor.ViewModel?.IsListingPendingDeletion(listingId) == true))
        {
            ShowSimpleErrorInfoBar("編集画面が開いています", "建物の編集画面で削除予定の募集物件は開けません。");
            return;
        }

        //Debug.WriteLine($"EditRentCommercialUnitCommand executed for {selected.Id}");

        var isFound = false;

        // Check if the selected item is already being edited in another window.
        RentCommercialListingEditorList.ForEach(editorWindow =>
        {
            //Debug.WriteLine($"Checking editor window with Id: {editorWindow.Id} for selected item with Id: {listingId}");
            if (editorWindow.Id == buildingId)
            {
                // If the editor window for this item is already open, activate it.
                //Debug.WriteLine($"Editor window for {listingId} is already open. Activating it.");
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
        var res = await Task.Run(() => _dataAccessService.SelectRentCommercialListingById(buildingId, listingId), _cts.Token);
        if (res.IsError)
        {
            Debug.WriteLine(
                res.Error.Title + Environment.NewLine +
                res.Error.Message + Environment.NewLine +
                res.Error.Description + Environment.NewLine +
                res.Error.Operation + Environment.NewLine +
                res.Error.MethodName + Environment.NewLine +
                res.Error.FullDump);

            ShowErrorInfoBar(res.Error);

            return;
        }

        if (res.Unit is null)
        {
            Debug.WriteLine($"Listing for {listingId} is null. Cannot open editor.");
            return;
        }

        var editorShell = _shellRentCommercialListingFactory.Create(res.Unit);
        
        //var propertyEditor = RentCommercialEditorList.FirstOrDefault(editor => editor.Id == buildingId && editor.ViewModel is { IsActive: true });
        //if (propertyEditor?.ViewModel is { } propertyViewModel) editorShell.ViewModel.SetOwnershipVisibility(propertyViewModel.IsUnitOwnership);

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
    private static bool EditRentCommercialListingCanExecute(ListingSearchResultItemViewModel? selected)
    {
        if (selected is null)
        {
            return false;
        }

        return true;
    }

    // 区画削除
    [RelayCommand(CanExecute = nameof(DeleteRentCommercialListingCanExecute))]
    private async Task DeleteRentCommercialListing(ListingSearchResultItemViewModel? selected)
    {
        if (selected is null || string.IsNullOrWhiteSpace(selected.Id))
        {
            return;
        }

        await DeleteRentCommercialListingCore(selected);
    }

    private async Task DeleteRentCommercialListingCore(ListingSearchResultItemViewModel selected)
    {
        var selectedId = selected.Id;
        var selectedPropertyId = selected.PropertyId;
        var propertyEditor = RentCommercialEditorList.FirstOrDefault(editor => editor.Id == selectedPropertyId);
        if (propertyEditor is not null)
        {
            ShowSimpleErrorInfoBar("削除できません", "建物の編集画面を閉じてから、募集物件を削除してください。");
            propertyEditor.Activate();
            return;
        }

        var mainWindow = App.GetService<ZumenSearch.Presentation.Shell.Views.MainWindow>();
        if (mainWindow.Content is not Microsoft.UI.Xaml.FrameworkElement { XamlRoot: { } xamlRoot })
        {
            ShowSimpleErrorInfoBar("削除できません", "募集物件の削除を確認できませんでした。画面を開き直して再試行してください。");
            return;
        }

        try
        {
            var dialogService = App.GetService<IDialogGenericService>();
            dialogService.Initialize(xamlRoot, mainWindow);
            if (await dialogService.ShowDeleteConfirmationDialog("この募集物件を削除しますか？") != Microsoft.UI.Xaml.Controls.ContentDialogResult.Primary) return;
        }
        catch (Exception ex) when (ex is System.Runtime.InteropServices.COMException or InvalidOperationException)
        {
            Debug.WriteLine($"Failed to confirm commercial listing deletion: {ex}");
            ShowSimpleErrorInfoBar("削除できません", $"募集物件の削除を確認できませんでした: {ex.Message}");
            return;
        }

        propertyEditor = RentCommercialEditorList.FirstOrDefault(editor => editor.Id == selectedPropertyId);
        if (propertyEditor is not null)
        {
            ShowSimpleErrorInfoBar("削除できません", "建物の編集画面を閉じてから、募集物件を削除してください。");
            propertyEditor.Activate();
            return;
        }

        var isFound = false;
        foreach (var editorWindow in RentCommercialListingEditorList.ToList())
        {
            if (editorWindow.Id != selectedId)
            {
                continue;
            }

            if (editorWindow.ViewModel?.IsDirty == false && !editorWindow.ViewModel.SaveCommand.IsRunning && !editorWindow.ViewModel.HasPendingEditorOperations && !editorWindow.ViewModel.IsDiscardPending && !(editorWindow.Content is ZumenSearch.Presentation.Features.Rent.Commercials.Views.Listing.ShellPage listingShell && listingShell.IsClosePending))
            {
                editorWindow.Close();
                isFound = RentCommercialListingEditorList.Contains(editorWindow);
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
            ShowSimpleErrorInfoBar("削除できません", "募集物件の編集画面を閉じてから削除してください。");
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

            ShowErrorInfoBar(res.Error);

            return;
        }

        RemoveCommercialListingSearchResults(selectedPropertyId, selectedId);

        foreach (var editorWindow in RentCommercialEditorList.ToList())
        {
            if (editorWindow.Id != selectedPropertyId)
            {
                continue;
            }

            WeakReferenceMessenger.Default.Send(
                new ZumenSearch.Presentation.Messaging.ListingDeletedMessage(selectedId));
            break;
        }

        // Clean up pictures and PDFs.
        var listingDataDirectoryPath = Path.Combine(
            App.PropertyBlobDataFolder,
            selectedPropertyId,
            selectedId);

        try
        {
            Directory.Delete(listingDataDirectoryPath, true);
        }
        catch (DirectoryNotFoundException)
        {
            // Listings without attachments have no directory to remove.
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            Debug.WriteLine($"Failed to delete commercial listing attachments '{listingDataDirectoryPath}': {ex}");
            ShowSimpleErrorInfoBar("削除できません", $"募集物件は削除されましたが、添付ファイルを削除できませんでした: {listingDataDirectoryPath}。{ex.Message}");
        }
    }

    private static bool DeleteRentCommercialListingCanExecute(ListingSearchResultItemViewModel? selected)
    {
        return selected is not null &&
               !string.IsNullOrWhiteSpace(selected.Id);
    }

    #endregion

    #region == 賃貸駐車場 ==

    [RelayCommand]
#pragma warning disable CA1822 // Mark members as static
    private void AddNewRentParking()
#pragma warning restore CA1822 // Mark members as static
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

            _navigationService.NavigateTo(NavigationKeys.RentLessorSearchResult, SlideNavigationTransitionEffect.FromLeft);
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

        var res = await Task.Run(() => _rentPersonService.SearchLessors(queryText), _cts.Token);

        if (res.IsError)
        {
            Debug.WriteLine(
                res.Error.Title + Environment.NewLine +
                res.Error.Message + Environment.NewLine +
                res.Error.Description + Environment.NewLine +
                res.Error.Operation + Environment.NewLine +
                res.Error.MethodName + Environment.NewLine +
                res.Error.FullDump);

            ShowErrorInfoBar(res.Error);

        }
        else
        {
            RentLessorSearchResult = new(res.PersonSearchResult.Select(item => new PersonSearchResultItemViewModel(item)));

            _navigationService.NavigateTo(RentLessorSearchResultPagePath, SlideNavigationTransitionEffect.FromLeft);
        }
    }

    [RelayCommand]
    private void AddNewRentLessor()
    {
        var newId = Guid.CreateVersion7().ToString("N");
        var shell = _shellRentLessorFactory.Create(new ZumenSearch.Domain.Entities.Person.PersonNatural(newId, EntityStatus.New));
        
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

    [RelayCommand(CanExecute = nameof(CanExecutePersonSearchResult))]
    private async Task EditRentLessorSearchResult(PersonSearchResultItemViewModel? selected)
    {
        if (selected is not null) await EditRentLessorCommand.ExecuteAsync(selected.Model);
    }

    [RelayCommand(CanExecute = nameof(CanExecutePersonSearchResult))]
    private async Task DeleteRentLessorSearchResult(PersonSearchResultItemViewModel? selected)
    {
        if (selected is not null) await DeleteRentLessorCommand.ExecuteAsync(selected.Model);
    }

    private static bool CanExecutePersonSearchResult(PersonSearchResultItemViewModel? selected) => selected is not null;

    [RelayCommand(CanExecute = nameof(EditRentLessorCanExecute))]
    public async Task EditRentLessor(ZumenSearch.Domain.Entities.Base.PersonBase selected) //ZumenSearch.Application.SearchResults.PersonSearchResultItem
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
        var res = await Task.Run(() => _rentPersonService.GetLessorById(lessorId), _cts.Token);
        if (res.IsError)
        {
            Debug.WriteLine(
                res.Error.Title + Environment.NewLine +
                res.Error.Message + Environment.NewLine +
                res.Error.Description + Environment.NewLine +
                res.Error.Operation + Environment.NewLine +
                res.Error.MethodName + Environment.NewLine +
                res.Error.FullDump);

            ShowErrorInfoBar(res.Error);

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
    public static bool EditRentLessorCanExecute(ZumenSearch.Domain.Entities.Base.PersonBase? selected)
    {
        if (selected is null)
        //if (string.IsNullOrEmpty(rentId))
        {
            return false;
        }

        return true;
    }

    [RelayCommand(CanExecute = nameof(DeleteRentLessorCanExecute))]
    public async Task DeleteRentLessor(ZumenSearch.Domain.Entities.Base.PersonBase selected) //Models.PersonSearchResultItem
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
        var res = await Task.Run(() => _rentPersonService.DeleteLessor(lessorId), _cts.Token);
        if (res.IsError)
        {
            Debug.WriteLine(
                res.Error.Title + Environment.NewLine +
                res.Error.Message + Environment.NewLine +
                res.Error.Description + Environment.NewLine +
                res.Error.Operation + Environment.NewLine +
                res.Error.MethodName + Environment.NewLine +
                res.Error.FullDump);

            ShowErrorInfoBar(res.Error);

            return;
        }
        else
        {
            var match = RentLessorSearchResult.FirstOrDefault(x => x.Id.Equals(lessorId, StringComparison.Ordinal));
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
            WeakReferenceMessenger.Default.Send(new ZumenSearch.Presentation.Messaging.LessorDeletedMessage(lessorId));
        }

    }
    public static bool DeleteRentLessorCanExecute(ZumenSearch.Domain.Entities.Base.PersonBase? selected)
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
        var property = new ZumenSearch.Domain.Entities.Sale.Residentials.Property(
            Guid.CreateVersion7().ToString("N"),
            EntityStatus.New);

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

            ShowErrorInfoBar(res.Error);

            return;
        }

        foreach (var item in res.PropertySearchResult)
        {
            item.BasePath = Path.Combine(
                App.PropertyBlobDataFolder,
                item.Id);
        }

        SaleResidentialSearchResult =
            new(res.PropertySearchResult.Select(item => new PropertySearchResultItemViewModel(item)));

        _navigationService.NavigateTo(SaleResidentialSearchResultPagePath,
            SlideNavigationTransitionEffect.FromLeft);
    }

    [RelayCommand(CanExecute = nameof(EditSaleResidentialCanExecute))]
    private async Task EditSaleResidential(PropertySearchResultItemViewModel? selected)
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

            ShowErrorInfoBar(res.Error);

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
    private static bool EditSaleResidentialCanExecute(PropertySearchResultItemViewModel? selected)
    {
        return selected is not null &&
               !string.IsNullOrWhiteSpace(selected.Id);
    }

    [RelayCommand(CanExecute = nameof(DeleteSaleResidentialCanExecute))]
    private async Task DeleteSaleResidential(PropertySearchResultItemViewModel? selected)
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

            ShowErrorInfoBar(res.Error);

            return;
        }

        SaleResidentialSearchResult.Remove(selected);

        var searchItem = PropertySearchResult.FirstOrDefault(item => item.Id == selected.Id);
        if (searchItem is not null)
        {
            PropertySearchResult.Remove(searchItem);
        }

        var recentItem = RecentProperties.FirstOrDefault(item => item.Id == selected.Id);
        if (recentItem is not null)
        {
            RecentProperties.Remove(recentItem);
        }
    }

    private static bool DeleteSaleResidentialCanExecute(PropertySearchResultItemViewModel? selected)
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
        var shell = _shellBrokerFactory.Create(new ZumenSearch.Domain.Entities.Person.PersonLegal(newId, EntityStatus.New));

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

        var res = await Task.Run(() => _rentPersonService.SearchBrokers(queryText), _cts.Token);

        if (res.IsError)
        {
            Debug.WriteLine(
                res.Error.Title + Environment.NewLine +
                res.Error.Message + Environment.NewLine +
                res.Error.Description + Environment.NewLine +
                res.Error.Operation + Environment.NewLine +
                res.Error.MethodName + Environment.NewLine +
                res.Error.FullDump);

            ShowErrorInfoBar(res.Error);

        }
        else
        {
            BrokerSearchResult = new(res.PersonSearchResult.Select(item => new PersonSearchResultItemViewModel(item)));

            _navigationService.NavigateTo(BrokerSearchResultPagePath, SlideNavigationTransitionEffect.FromLeft);
        }
    }

    [RelayCommand(CanExecute = nameof(CanExecutePersonSearchResult))]
    private async Task EditBrokerSearchResult(PersonSearchResultItemViewModel? selected)
    {
        if (selected is not null) await EditBrokerCommand.ExecuteAsync(selected.Model);
    }

    [RelayCommand(CanExecute = nameof(CanExecutePersonSearchResult))]
    private async Task DeleteBrokerSearchResult(PersonSearchResultItemViewModel? selected)
    {
        if (selected is not null) await DeleteBrokerCommand.ExecuteAsync(selected.Model);
    }

    [RelayCommand(CanExecute = nameof(EditBrokerCanExecute))]
    public async Task EditBroker(ZumenSearch.Domain.Entities.Base.PersonBase selected) //Models.PersonSearchResultItem
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
        var res = await Task.Run(() => _rentPersonService.GetBrokerById(lessorId), _cts.Token);
        if (res.IsError)
        {
            Debug.WriteLine(
                res.Error.Title + Environment.NewLine +
                res.Error.Message + Environment.NewLine +
                res.Error.Description + Environment.NewLine +
                res.Error.Operation + Environment.NewLine +
                res.Error.MethodName + Environment.NewLine +
                res.Error.FullDump);

            ShowErrorInfoBar(res.Error);

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
    public static bool EditBrokerCanExecute(ZumenSearch.Domain.Entities.Base.PersonBase? selected)
    {
        if (selected is null)
        //if (string.IsNullOrEmpty(rentId))
        {
            return false;
        }

        return true;
    }

    [RelayCommand(CanExecute = nameof(DeleteBrokerCanExecute))]
    public async Task DeleteBroker(ZumenSearch.Domain.Entities.Base.PersonBase selected) //Models.PersonSearchResultItem
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
        var res = await Task.Run(() => _rentPersonService.DeleteBroker(lessorId), _cts.Token);
        if (res.IsError)
        {
            Debug.WriteLine(
                res.Error.Title + Environment.NewLine +
                res.Error.Message + Environment.NewLine +
                res.Error.Description + Environment.NewLine +
                res.Error.Operation + Environment.NewLine +
                res.Error.MethodName + Environment.NewLine +
                res.Error.FullDump);

            ShowErrorInfoBar(res.Error);

            return;
        }
        else
        {
            var match = BrokerSearchResult.FirstOrDefault(x => x.Id.Equals(lessorId, StringComparison.Ordinal));
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
            WeakReferenceMessenger.Default.Send(new ZumenSearch.Presentation.Messaging.BrokerDeletedMessage(lessorId));
        }

    }
    public static bool DeleteBrokerCanExecute(ZumenSearch.Domain.Entities.Base.PersonBase? selected)
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
