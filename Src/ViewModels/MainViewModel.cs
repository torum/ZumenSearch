using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.Windows.ApplicationModel.Resources;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Reflection;
using Windows.ApplicationModel;
using ZumenSearch.Helpers;
using ZumenSearch.Models.Base;
using ZumenSearch.Models.Common;
using ZumenSearch.Services;
using ZumenSearch.Services.Contracts;
using ZumenSearch.Services.Extensions.AbstractFactory;
using ZumenSearch.Views;
using ZumenSearch.Views.Rent.Residentials;

namespace ZumenSearch.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private static readonly ResourceLoader _resourceLoader = new();

    private string _versionDescription;

    #region == Public Properties ==

    public string VersionDescription
    {
        get => _versionDescription;
        private set => SetProperty(ref _versionDescription, value);
    }

    // TODO:
    //private static ShellPage Shell => App.GetService<ShellPage>();

    //private static MainWindow MainWin => App.GetService<MainWindow>();

    #region == Window management ==

    // Marking EditorList as readonly to fix IDE0044
    public readonly List<Views.Rent.Residentials.EditorWindow> EditorList = [];

    // Editor window position and size
    public int EditorWinWidth = 1366;
    public int EditorWinHeight = 768;
    public int EditorWinLeft = 130;
    public int EditorWinTop = 130;

    /*
    // Modal window position and size
    public int ModalWinWidth = 1366;
    public int ModalWinHeight = 768;
    public int ModalWinLeft = 130;
    public int ModalWinTop = 130;
    */
    #endregion

    #region == Navigation ==

    // TODO: Do I need this property?
    [ObservableProperty]
    public partial bool IsBackEnabled // Implement partial property for AOT compatibility
{ get; set; } = true;

    // TODO: Do I need this property?
    [ObservableProperty]
    public partial object? SelectedNavigationViewItem { get; set; }

    public ObservableCollection<Breadcrumb> BreadcrumbItemsResidential { get; set; } =
    [
        new() { Name = "住居用", Page = typeof(Views.Rent.ResidentialSearchPage).FullName! }
    ];
    public ObservableCollection<Breadcrumb> BreadcrumbItemsResidentialSearchResult { get; set; } =
    [
        new() { Name = "住居用", Page = typeof(Views.Rent.ResidentialSearchPage).FullName! },
        new() { Name = "検索結果", Page = typeof(Views.Rent.ResidentialSearchResultPage).FullName! },
    ];

    public ObservableCollection<Breadcrumb> BreadcrumbItemsRent { get; set; } =
    [
        new() { Name = "総合検索", Page = typeof(Views.RentSearchPage).FullName! }
    ];
    public ObservableCollection<Breadcrumb> BreadcrumbItemsRentSearchResult { get; set; } =
    [
        new() { Name = "総合検索", Page = typeof(Views.RentSearchPage).FullName! },
        new() { Name = "検索結果", Page = typeof(Views.RentSearchResultPage).FullName! },
    ];

    #endregion

    #region == Database ==


    #endregion

    #region == Search ==

    public ObservableCollection<Models.Rent.Residentials.EntryResidentialSearchResult> RentResidentialEntrySearchResult
    {
        get; set
        {
            if (SetProperty(ref field, value))
            {
                //
            }
        }
    } = [];

    public ObservableCollection<Models.Rent.Residentials.UnitResidentialSearchResult> RentResidentialUnitSearchResult
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

    #region == Services ==

    //private readonly IAbstractFactory<Views.Rent.Residentials.ShellPage> _editorFactory;
    //private Func<Models.Rent.Residentials.EntryResidentialFull, Views.Rent.Residentials.ShellPage> _shellFactory;
    private IAbstractFactory<Models.Rent.Residentials.EntryResidentialFull, Views.Rent.Residentials.ShellPage> _shellFactory;

    private readonly IDataAccessService _dataAccessService;
    private readonly INavigationService _navigationService;
    private readonly IDispatcherService _dispatcherService;

    #endregion

    private readonly CancellationTokenSource _cts = new();

    public MainViewModel(IAbstractFactory<Models.Rent.Residentials.EntryResidentialFull, Views.Rent.Residentials.ShellPage> shellFactory, INavigationService navigationService, IDataAccessService dataAccessService, IDispatcherService dispatcherService)//IAbstractFactory<Views.Rent.Residentials.ShellPage> editorFactory,
    {
        _shellFactory = shellFactory;
        _dataAccessService = dataAccessService;
        _navigationService = navigationService;
        _dispatcherService = dispatcherService;

        _versionDescription = GetVersionDescription();

        InitializeDatabase();
    }

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

    // 新規物件追加（建物）
    [RelayCommand]
    private void AddNewRentResidential()
    {
        //Debug.WriteLine("AddNew command executed!");

        var shell = _shellFactory.Create(new Models.Rent.Residentials.EntryResidentialFull(Guid.CreateVersion7().ToString("N"), EnumEntryStatus.New));//_editorFactory.Create();

        EditorList.Add(shell.Win);

        if (shell.Win.AppWindow.Presenter is OverlappedPresenter presenter)
        {
            presenter.IsResizable = true;
            presenter.IsModal = false;
            presenter.IsAlwaysOnTop = false;
            presenter.PreferredMinimumWidth = 1274;
            presenter.PreferredMinimumHeight = 794;
        }
        /*
        shell.Win.Closed += (sender, e) =>
        {
            EditorList.Remove(shell.Win);

            if (EditorList.Count == 0)
            {
                var mainWindow = App.GetService<MainWindow>();

                // No more editor windows are open, activate the main window again.
                if (mainWindow?.AppWindow.Presenter is OverlappedPresenter presntr)
                {
                    presntr.Restore();
                }
                mainWindow?.Activate();
            }
        };
        */

        shell.Win.SetEntryIdToWindow(shell.ViewModel.Id);
        shell.Win.SetViewModelToWindow(shell.ViewModel);


        //var dpi = Windows.Win32.PInvoke.GetDpiForWindow(new Windows.Win32.Foundation.HWND(WinRT.Interop.WindowNative.GetWindowHandle(this)));
        //var scalingFactor = (float)dpi / 96;
        //AppWindow.Resize(new Windows.Graphics.SizeInt32((int)(400.0f * scalingFactor), (int)(300.0f * scalingFactor)));

        shell.Win.AppWindow.MoveAndResize(new Windows.Graphics.RectInt32(EditorWinLeft, EditorWinTop, EditorWinWidth, EditorWinHeight));

        //editorWindow.AppWindow.Show();
        shell.Win.Activate();

        /*
        var editorShell = _editorFactory.Create();

        var editorWindow = editorShell.EditorWin;

        if (editorWindow == null)
        {
            // EditorWin should be initialized in the EditorShell constructor.
            throw new ArgumentNullException(nameof(editorWindow));
        }

        // Add to the list of editor windows.
        EditorList.Add(editorWindow);

        if (editorWindow.AppWindow.Presenter is OverlappedPresenter presenter)
        {
            presenter.IsResizable = true;
            presenter.IsModal = false;
            presenter.IsAlwaysOnTop = false;
            presenter.PreferredMinimumWidth = 1274;
            presenter.PreferredMinimumHeight = 794;
        }

        editorWindow.Closed += (sender, e) =>
        {
            EditorList.Remove(editorWindow);

            if (EditorList.Count == 0)
            {
                var mainWindow = App.GetService<MainWindow>();

                // No more editor windows are open, activate the main window again.
                if (mainWindow?.AppWindow.Presenter is OverlappedPresenter presntr)
                {
                    presntr.Restore();
                }
                mainWindow?.Activate();
            }
        };

        
        //var dpi = Windows.Win32.PInvoke.GetDpiForWindow(new Windows.Win32.Foundation.HWND(WinRT.Interop.WindowNative.GetWindowHandle(this)));
        //var scalingFactor = (float)dpi / 96;
        //AppWindow.Resize(new Windows.Graphics.SizeInt32((int)(400.0f * scalingFactor), (int)(300.0f * scalingFactor)));
        

        editorWindow.AppWindow.MoveAndResize(new Windows.Graphics.RectInt32(EditorWinLeft, EditorWinTop, EditorWinWidth, EditorWinHeight));

        //editorWindow.AppWindow.Show();
        editorWindow.Activate();
        */
    }

    // 編集（建物）
    [RelayCommand(CanExecute = nameof(EditRentResidentialEntryCanExecute))]
    public void EditRentResidentialEntry(Models.Rent.Residentials.EntryResidentialSearchResult? selected)//Models.Rent.Residentials.EntryResidentialSearchResult? selected
    {
        var rentId = selected?.Id;

        if (string.IsNullOrEmpty(rentId))//if (selected == null)
        {
            Debug.WriteLine("EditRentResidentialCommand executed but no item is selected.");
            return;
        }

        //Debug.WriteLine($"EditRentResidentialCommand executed for {selected.Id}");

        var isFound = false;

        // Check if the selected item is already being edited in another window.
        EditorList.ForEach(async editorWindow =>
        {
            Debug.WriteLine($"Checking editor window with Id: {editorWindow.Id} for selected item with Id: {rentId}");
            if (editorWindow.Id == rentId)
            {
                // If the editor window for this item is already open, activate it.
                Debug.WriteLine($"Editor window for {rentId} is already open. Activating it.");
                isFound = true;

                // Stupid WinUI3 needs a delay here to properly activate the window.
                //await Task.Delay(30);
                //await Task.Yield();

                editorWindow.Activate();
                var mainWindow = App.GetService<MainWindow>();
                mainWindow?.AppWindow.MoveInZOrderBelow(editorWindow.AppWindow.Id);
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

        if (res.EntryFull == null)
        {
            Debug.WriteLine($"EntryResidentialFull for {rentId} is null. Cannot open editor.");
            return;
        }

        var editorShell = _shellFactory.Create(res.EntryFull);//_editorFactory.Create();

        // Sets the instance of selected Entry.
        //editorShell.SetEntryToEntryViewModel(res.EntryFull);

        //var entryViewModel = editorShell.ViewModel;
        //entryViewModel.SetEntry(res.EntryFull);

        var editorWindow = editorShell.Win;
        if (editorWindow == null)
        {
            // EditorWin should be initialized in the EditorShell constructor.
            Debug.WriteLine("EditorWin must be initialized in the EditorShell constructor");
            return;
        }
        editorWindow.SetEntryIdToWindow(editorShell.ViewModel.Id);
        editorWindow.SetViewModelToWindow(editorShell.ViewModel);

        EditorList.Add(editorWindow);

        editorWindow.AppWindow.MoveAndResize(new Windows.Graphics.RectInt32(EditorWinLeft, EditorWinTop, EditorWinWidth, EditorWinHeight));
        if (editorWindow.AppWindow.Presenter is OverlappedPresenter presenter)
        {
            presenter.IsResizable = true;
            presenter.IsModal = false;
            presenter.IsAlwaysOnTop = false;
            presenter.PreferredMinimumWidth = 1274;
            presenter.PreferredMinimumHeight = 794;
        }

        /*
        editorWindow.Closed += (sender, e) =>
        {
            // Activate the main window again.
            //App.MainWnd?.Activate();

            EditorList.Remove(editorWindow);
        };
        */

        editorWindow.AppWindow.Show();
        editorWindow.Activate();
        var mainWindow = App.GetService<MainWindow>();
        mainWindow?.AppWindow.MoveInZOrderBelow(editorWindow.AppWindow.Id);
        editorWindow.AppWindow.MoveInZOrderAtTop();
    }
    public bool EditRentResidentialEntryCanExecute(Models.Rent.Residentials.EntryResidentialSearchResult? selected)
    {
        if (selected is null)
        {
            return false;
        }

        return true;
    }

    // 編集（部屋）TODO:
    [RelayCommand(CanExecute = nameof(EditRentResidentialUnitCanExecute))]
    public void EditRentResidentialUnit(Models.Rent.Residentials.UnitResidentialSearchResult? selected)
    {
        if (selected == null)
        {
            Debug.WriteLine("EditRentResidentialUnit executed but no item is selected.");
            return;
        }

        // TODO;
        Debug.WriteLine($"TODO: EditRentResidentialUnit executed for {selected.Id} (EntryId = {selected.EntryId})");
    }
    public bool EditRentResidentialUnitCanExecute(Models.Rent.Residentials.UnitResidentialSearchResult? selected)
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
            if (res.AffectedCount > 0 && res.SelectedEntries.Count > 0)
            {
                foreach (var item in res.SelectedEntries)
                {
                    var autoSuggest = new AutoSuggestItem();
                    autoSuggest.Name = item.Name;
                    autoSuggest.Id = item.Id;
                    AutoSuggestList.Add(autoSuggest);
                }
            }
            else
            {
                // TODO:
                //Debug.WriteLine("result 0");
                var autoSuggest = new AutoSuggestItem();
                autoSuggest.Name = "Result 0";
                autoSuggest.Id = "";
                AutoSuggestList.Add(autoSuggest);
            }
        }
    }
    public bool SearchRentForAutoSuggestCanExecute(string? queryText)
    {
        if (string.IsNullOrEmpty(queryText))
        {
            return false;
        }

        return true;
    }

    // 物件検索 TODO:
    [RelayCommand(CanExecute = nameof(SearchRentResidentialEntryCanExecute))]
    private async Task SearchRentResidentialEntry(string? queryText)
    {
        var query = string.Empty;

        if (string.IsNullOrEmpty(queryText))
        {
            //RentResidentialEntrySearchResult.Clear();
            //return;
            query = "*";
        }
        else
        {
            query = queryText.Trim();
        }

        RentResidentialEntrySearchResult.Clear();

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
            RentResidentialEntrySearchResult = new(res.SelectedEntries);

            _navigationService.NavigateTo("ZumenSearch.Views.RentSearchResultPage", SlideNavigationTransitionEffect.FromLeft);//Navigate(typeof(Views.Rent.Residentials.SearchResultPage), null, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromRight });
        }
    }
    public bool SearchRentResidentialEntryCanExecute(string? queryText)
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
    private async Task SearchRentResidentialUnit()
    {
        RentResidentialUnitSearchResult.Clear();

        var res = await Task.Run(() => _dataAccessService.SelectRentResidentialUnits(), _cts.Token);

        if (res.IsError)
        {
            Debug.WriteLine(res.Error.ErrText + Environment.NewLine + res.Error.ErrDescription + Environment.NewLine + res.Error.ErrPlace + Environment.NewLine + res.Error.ErrPlaceParent);

            //ErrorMain = res.Error;
            //IsMainErrorInfoBarVisible = true;

            // TODO: Show error message to user
        }
        else
        {
            RentResidentialUnitSearchResult = new(res.SelectedUnits);

            _navigationService.NavigateTo("ZumenSearch.Views.Rent.ResidentialSearchResultPage", SlideNavigationTransitionEffect.FromLeft);//Navigate(typeof(Views.Rent.Residentials.SearchResultPage), null, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromRight });
        }
    }

    // 物件削除
    [RelayCommand(CanExecute = nameof(DeleteRentResidentialEntryCanExecute))]
    private void DeleteRentResidentialEntry(Models.Rent.Residentials.EntryResidentialSearchResult? selected)
    {
        if (selected == null)
        {
            Debug.WriteLine("DeleteRentResidential executed but no item is selected.");
            return;
        }

        var isFound = false;

        // TODO: Check if the selected item is already being edited in another window.
        EditorList.ForEach(editorWindow =>
        {
            Debug.WriteLine($"Checking editor window with Id: {editorWindow.Id} for selected item with Id: {selected.Id}");
            if (editorWindow.Id == selected.Id)
            {
                // If the editor window for this item is already open, activate it.
                Debug.WriteLine($"Editor window for {selected.Id} is already open. Activating it.");
                isFound = true;
                // TODO: show confirm close dialog?.
                editorWindow.Activate();
                return;
            }
        });

        if (isFound)
        {
            // If the editor window for this item is already open, just return.
            return;
        }

        Debug.WriteLine($"DeleteRentResidentialCommand executed for {selected.Id}");

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
            if (RentResidentialEntrySearchResult.Remove(selected))
            {
                // Successfully removed the selected item from the search result.
            }
            else
            {
                Debug.WriteLine($"Selected item {selected.Id} not found in the search result or could not remove.");
            }
        }
    }
    public bool DeleteRentResidentialEntryCanExecute(Models.Rent.Residentials.EntryResidentialSearchResult? selected)
    {
        if (selected is null)
        {
            return false;
        }

        return true;
    }

    // 部屋削除（TODO）
    [RelayCommand(CanExecute = nameof(DeleteRentResidentialUnitCanExecute))]
    private void DeleteRentResidentialUnit(Models.Rent.Residentials.UnitResidentialSearchResult? selected)
    {
        if (selected == null)
        {
            Debug.WriteLine("DeleteRentResidentialUnit executed but no item is selected.");
            return;
        }

        // TODO:
        Debug.WriteLine($"TODO: DeleteRentResidentialUnit executed for {selected.Id} (EntryId = {selected.EntryId})");
    }
    public bool DeleteRentResidentialUnitCanExecute(Models.Rent.Residentials.UnitResidentialSearchResult? selected)
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
