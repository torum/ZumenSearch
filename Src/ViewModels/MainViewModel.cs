using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.Logging;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media.Animation;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Reflection;
using Windows.ApplicationModel;
using ZumenSearch.Helpers;
using ZumenSearch.Models.Base;
using ZumenSearch.Models.Common;
using ZumenSearch.Models.Messenger;
using ZumenSearch.Services.Contracts;
using ZumenSearch.Services.Extensions.AbstractFactory;
using ZumenSearch.Views;

namespace ZumenSearch.ViewModels;

public partial class MainViewModel : ObservableRecipient, IRecipient<PropertyUpdatedMessage>, IRecipient<ListingUpdatedMessage>, IRecipient<ListingWindowClosedMessage>, IRecipient<PropertyWindowClosedMessage>
{
    #region == Public Properties ==

    [ObservableProperty]
    public partial string VersionDescription { get; set; }

    #region == Window management ==

    public readonly List<Views.Rent.Residentials.Bldg.EditorWindow> BldgEditorList = [];
    public int BldgEditorWinWidth = 1366;
    public int BldgEditorWinHeight = 768;
    public int BldgEditorWinLeft = 130;
    public int BldgEditorWinTop = 130;

    public readonly List<Views.Rent.Residentials.Room.EditorWindow> RoomEditorList = [];
    public int RoomEditorWinWidth = 1366;
    public int RoomEditorWinHeight = 768;
    public int RoomEditorWinLeft = 130;
    public int RoomEditorWinTop = 130;

    #endregion

    #region == Navigation ==

    public ObservableCollection<Breadcrumb> BreadcrumbItemsResidential { get; set; } =
    [
        new() { Name = "住居用", Page = typeof(Views.Rent.Residentials.SearchPage).FullName! }
    ];
    public ObservableCollection<Breadcrumb> BreadcrumbItemsResidentialSearchResult { get; set; } =
    [
        new() { Name = "住居用", Page = typeof(Views.Rent.Residentials.SearchPage).FullName! },
        new() { Name = "検索結果", Page = typeof(Views.Rent.Residentials.SearchResultPage).FullName! },
    ];

    public ObservableCollection<Breadcrumb> BreadcrumbItemsRent { get; set; } =
    [
        new() { Name = "総合検索", Page = typeof(Views.SearchPage).FullName! }
    ];
    public ObservableCollection<Breadcrumb> BreadcrumbItemsRentSearchResult { get; set; } =
    [
        new() { Name = "総合検索", Page = typeof(Views.SearchPage).FullName! },
        new() { Name = "検索結果", Page = typeof(Views.SearchResultPage).FullName! },
    ];

    #endregion

    #region == Database ==


    #endregion

    #region == Search ==

    // Rent or BuySell
    [ObservableProperty]
    public partial int SegIndex { get; set; } = 0;

    [ObservableProperty]
    public partial string SearchQuery { get; set; } = string.Empty;

    public ObservableCollection<Models.Rent.Residentials.PropertySearchResultItem> RentResidentialBldgSearchResult
    {
        get; set
        {
            if (SetProperty(ref field, value))
            {
                //
            }
        }
    } = [];

    public ObservableCollection<Models.Rent.Residentials.ListingSearchResultItem> RentResidentialRoomSearchResult
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

    private readonly IAbstractFactory<Models.Rent.Residentials.Bldg.Property, Views.Rent.Residentials.Bldg.ShellPage> _shellRentResidentialPropertyFactory;
    private readonly IAbstractFactory<Models.Rent.Residentials.Room.Listing, Views.Rent.Residentials.Room.ShellPage> _shellRentResidentialListingFactory;
    private readonly IDataAccessService _dataAccessService;
    private readonly INavigationService _navigationService;
    private readonly IDispatcherService _dispatcherService;

    #endregion

    public MainViewModel(
        IAbstractFactory<Models.Rent.Residentials.Bldg.Property, Views.Rent.Residentials.Bldg.ShellPage> shellRentResidentialPropertyFactory, 
        IAbstractFactory<Models.Rent.Residentials.Room.Listing, Views.Rent.Residentials.Room.ShellPage> shellRentResidentialListingFactory, 
        INavigationService navigationService, 
        IDataAccessService dataAccessService, 
        IDispatcherService dispatcherService)
    {
        _shellRentResidentialPropertyFactory = shellRentResidentialPropertyFactory;
        _shellRentResidentialListingFactory = shellRentResidentialListingFactory;
        _navigationService = navigationService;
        _dataAccessService = dataAccessService;
        _dispatcherService = dispatcherService;

        VersionDescription = GetVersionDescription();

        InitializeDatabase();

        // Ready to receive messages.
        this.IsActive = true;
    }

    #region == Messages ==

    public void Receive(PropertyUpdatedMessage property)
    {
        var building = property.Value;
        if (building is not null)
        {
            foreach (var item in RentResidentialBldgSearchResult)
            {
                if (!item.Id.Equals(building.Id))
                {
                    continue;
                }

                item.Name = building.Name;
            }
        }
    }

    public void Receive(ListingUpdatedMessage listing)
    {
        var room = listing.Value;
        if (room is not null)
        {
            foreach (var item in RentResidentialRoomSearchResult)
            {
                if (!item.Id.Equals(room.Id))
                {
                    continue;
                }

                item.Name = room.Name;
            }
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

    #region == Commands ==

    [RelayCommand]
    private void AddNewRentResidentialBldg()
    {
        var shell = _shellRentResidentialPropertyFactory.Create(new Models.Rent.Residentials.Bldg.Property(Guid.CreateVersion7().ToString("N"), EnumPropertyStatus.New));

        BldgEditorList.Add(shell.Window);

        if (shell.Window.AppWindow.Presenter is OverlappedPresenter presenter)
        {
            presenter.IsResizable = true;
            presenter.IsModal = false;
            presenter.IsAlwaysOnTop = false;
            presenter.PreferredMinimumWidth = 1274;
            presenter.PreferredMinimumHeight = 794;
        }

        shell.Window.SetPropertyIdToWindow(shell.ViewModel.Id);
        shell.Window.SetViewModelToWindow(shell.ViewModel);

        //var dpi = Windows.Win32.PInvoke.GetDpiForWindow(new Windows.Win32.Foundation.HWND(WinRT.Interop.WindowNative.GetWindowHandle(this)));
        //var scalingFactor = (float)dpi / 96;
        //AppWindow.Resize(new Windows.Graphics.SizeInt32((int)(400.0f * scalingFactor), (int)(300.0f * scalingFactor)));

        shell.Window.AppWindow.MoveAndResize(new Windows.Graphics.RectInt32(BldgEditorWinLeft, BldgEditorWinTop, BldgEditorWinWidth, BldgEditorWinHeight));

        //editorWindow.AppWindow.Show();
        shell.Window.Activate();
        shell.Window.AppWindow.MoveInZOrderAtTop();
    }

    [RelayCommand(CanExecute = nameof(EditRentResidentialBldgCanExecute))]
    public void EditRentResidentialBldg(Models.Rent.Residentials.PropertySearchResultItem? selected)
    {
        var rentId = selected?.Id;

        if (string.IsNullOrEmpty(rentId))
        {
            Debug.WriteLine("EditRentResidentialCommand executed but no item is selected.");
            return;
        }

        //Debug.WriteLine($"EditRentResidentialCommand executed for {selected.Id}");

        var isFound = false;

        // Check if the selected item is already being edited in another window.
        BldgEditorList.ForEach(editorWindow =>
        {
            //Debug.WriteLine($"Checking editor window with Id: {editorWindow.Id} for selected item with Id: {rentId}");
            if (editorWindow.Id == rentId)
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
        var res = _dataAccessService.SelectRentResidentialById(rentId);// Go back to UI thred. Let's not do > .ConfigureAwait(false);
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
            Debug.WriteLine($"Building for {rentId} is null. Cannot open editor.");
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

        editorWindow.SetPropertyIdToWindow(editorShell.ViewModel.Id);
        editorWindow.SetViewModelToWindow(editorShell.ViewModel);

        // TODO: Use WeakReferenceMessenger from CommunityToolkit.Mvvm (aka MVVM Toolkit) to send the selected search result from the MainWindow to this ViewModel.
        // https://learn.microsoft.com/en-us/dotnet/communitytoolkit/mvvm/messenger
        // To update the title/name and other properties in the editor window, we need to pass the selected search result to the editor's ViewModel.
        //editorShell.ViewModel.SetSearchResult(selected);

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

        // Do I need this anymore?
        //var mainWindow = App.GetService<MainWindow>();
        //mainWindow?.AppWindow.MoveInZOrderBelow(editorWindow.AppWindow.Id);

        editorWindow.AppWindow.MoveInZOrderAtTop();
    }

    public static bool EditRentResidentialBldgCanExecute(Models.Rent.Residentials.PropertySearchResultItem? selected)
    {
        if (selected is null)
        {
            return false;
        }

        return true;
    }

    [RelayCommand(CanExecute = nameof(EditRentResidentialRoomCanExecute))]
    public void EditRentResidentialRoom(Models.Rent.Residentials.ListingSearchResultItem? selected)
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

        editorWindow.SetListingIdToWindow(editorShell.ViewModel.Id);
        editorWindow.SetViewModelToWindow(editorShell.ViewModel);

        // TODO: Use WeakReferenceMessenger from CommunityToolkit.Mvvm (aka MVVM Toolkit) to send the selected search result from the MainWindow to this ViewModel.
        // https://learn.microsoft.com/en-us/dotnet/communitytoolkit/mvvm/messenger
        // To update the title/name and other properties in the editor window, we need to pass the selected search result to the editor's ViewModel.
        //editorShell.ViewModel.SetSearchResult(selected);

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
    public static bool EditRentResidentialRoomCanExecute(Models.Rent.Residentials.ListingSearchResultItem? selected)
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
            if (res.AffectedCount > 0 && res.PropertySearchResult.Count > 0)
            {
                foreach (var item in res.PropertySearchResult)
                {
                    var autoSuggest = new AutoSuggestItem
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
                var autoSuggest = new AutoSuggestItem
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

    // 物件検索 TODO:
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
            RentResidentialBldgSearchResult = new(res.PropertySearchResult);

            _navigationService.NavigateTo("ZumenSearch.Views.SearchResultPage", SlideNavigationTransitionEffect.FromLeft);//Navigate(typeof(Views.Rent.Residentials.SearchResultPage), null, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromRight });

            //_navigationService.NavigateTo("ZumenSearch.Views.Rent.Residentials.Bldg.BldgShellPage", SlideNavigationTransitionEffect.FromLeft);//Navigate(typeof(Views.Rent.Residentials.SearchResultPage), null, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromRight });
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

    // 部屋検索
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

            _navigationService.NavigateTo("ZumenSearch.Views.Rent.Residentials.SearchResultPage", SlideNavigationTransitionEffect.FromLeft);//Navigate(typeof(Views.Rent.Residentials.SearchResultPage), null, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromRight });
        }
    }

    // 物件削除
    [RelayCommand(CanExecute = nameof(DeleteRentResidentialBldgCanExecute))]
    private void DeleteRentResidentialBldg(Models.Rent.Residentials.PropertySearchResultItem? selected)
    {
        if (selected == null)
        {
            Debug.WriteLine("DeleteRentResidential executed but no item is selected.");
            return;
        }

        var isFound = false;

        // Check if the selected item is already being edited in editor window.
        foreach (var editorWindow in BldgEditorList.ToList())
        {
            //Debug.WriteLine($"Checking editor window with Id: {editorWindow.Id} for selected item with Id: {selected.Id}");
            if (editorWindow.Id == selected.Id)
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

        var res = _dataAccessService.DeleteRentResidential(selected.Id);
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
                Debug.WriteLine($"Selected item {selected.Id} not found in the search result or could not remove.");
            }

            Debug.WriteLine($"DeleteRentResidentialCommand executed for {selected.Id}");
        }
    }
    private static bool DeleteRentResidentialBldgCanExecute(Models.Rent.Residentials.PropertySearchResultItem? selected)
    {
        if (selected is null)
        {
            return false;
        }

        return true;
    }

    // 部屋削除
    [RelayCommand(CanExecute = nameof(DeleteRentResidentialRoomCanExecute))]
    private void DeleteRentResidentialRoom(Models.Rent.Residentials.ListingSearchResultItem? selected)
    {
        if (selected is null)
        {
            Debug.WriteLine("DeleteRentResidentialRoom executed but no item is selected.");
            return;
        }

        //Debug.WriteLine($"TODO: DeleteRentResidentialRoom executed for {selected.Id} (PropertyId = {selected.PropertyId})");

        var isFound = false;

        // Check if the selected item is already being edited in editor window.
        foreach (var editorWindow in RoomEditorList.ToList())
        {
            //Debug.WriteLine($"Checking editor window with Id: {editorWindow.Id} for selected item with Id: {selected.Id}");
            if (editorWindow.Id == selected.Id)
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

        var res = _dataAccessService.DeleteRentResidentialListing(selected.Id);
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
                Debug.WriteLine($"Selected item {selected.Id} not found in the search result or could not remove.");
            }

            Debug.WriteLine($"DeleteRentResidentialRoomCommand executed for {selected.Id}");
        }

        // Check if the selected item is already being edited in another window.
        BldgEditorList.ForEach(editorWindow =>
        {
            Debug.WriteLine($"Checking editor window with Id: {editorWindow.Id} for selected item with Id: {selected.PropertyId}");
            if (editorWindow.Id == selected.PropertyId)
            {
                // If the editor window for this item is already open, remove the room.
                //Debug.WriteLine($"Editor window for {selected.PropertyId} is already open. Removing room.");

                // remove room from the editor window's ViewModel if it exists.
                WeakReferenceMessenger.Default.Send(new Models.Messenger.ListingDeletedMessage(selected.Id));

                return;
            }
        });
    }
    private static bool DeleteRentResidentialRoomCanExecute(Models.Rent.Residentials.ListingSearchResultItem? selected)
    {
        if (selected is null)
        {
            return false;
        }

        return true;
    }

    // GoBack（ナビゲーション）
    [RelayCommand(CanExecute = nameof(GoBackCanExecute))]
    private void GoBack()
    {
        //_navigationService.NavigateTo("ZumenSearch.Views.Rent.Residentials.SearchPage");
        _navigationService.GoBack();
    }
    public bool GoBackCanExecute()
    {
        return _navigationService.CanGoBack();
    }

    #endregion
}
