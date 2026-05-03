using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.Windows.ApplicationModel.Resources;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Reflection;
using Windows.ApplicationModel;
using ZumenSearch.Helpers;
using ZumenSearch.Models;
using ZumenSearch.Services.Contracts;
using ZumenSearch.Services.Extensions.AbstractFactory;
using ZumenSearch.Views;

namespace ZumenSearch.ViewModels;

public partial class MainViewModel : ObservableObject
{
    //private MainWindow _mainWindow;
    private static readonly ResourceLoader _resourceLoader = new();

    private string _versionDescription;

    public string VersionDescription
    {
        get => _versionDescription;
        private set => SetProperty(ref _versionDescription, value);
    }

    #region == Properties ==

    // TODO:
    private static ShellPage Shell => App.GetService<ShellPage>();

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

    #endregion

    #region == Database ==


    #endregion

    #region == Search ==

    public ObservableCollection<Models.Rent.Residentials.EntryResidentialSearchResult> RentResidentialSearchResult
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

    private readonly IAbstractFactory<Views.Rent.Residentials.ShellPage> _editorFactory;
    private readonly IDataAccessService _dataAccessService;
    private readonly INavigationService _navigationService;

    #endregion

    private readonly CancellationTokenSource _cts = new();

    public MainViewModel(INavigationService navigationService, IAbstractFactory<Views.Rent.Residentials.ShellPage> editorFactory, IDataAccessService dataAccessService)
    {
        //Debug.WriteLine("MainViewModel");
        _editorFactory = editorFactory;
        _dataAccessService = dataAccessService;
        _navigationService = navigationService;

        _versionDescription = GetVersionDescription();

        InitializeDatabaseAsync();
    }

    #region == Private Methods ==

    private void InitializeDatabaseAsync()
    {
        var filePath = Path.Combine(App.AppDataFolder, "ZumenSearch.db");

        var res = _dataAccessService.InitializeDatabase(filePath);
        if (res.IsError)
        {
            Debug.WriteLine("InitializeDatabase @InitializeDatabaseAsync in MainViewModel");

            Debug.WriteLine(res.Error.ErrText + Environment.NewLine + res.Error.ErrDescription + Environment.NewLine + res.Error.ErrPlace + Environment.NewLine + res.Error.ErrPlaceParent);

            //ErrorMain = res.Error;
            //IsMainErrorInfoBarVisible = true;

            // TODO: Show error message to user
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
    private void AddNewRentResidential()
    {
        //Debug.WriteLine("AddNew command executed!");

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

        /*
        var dpi = Windows.Win32.PInvoke.GetDpiForWindow(new Windows.Win32.Foundation.HWND(WinRT.Interop.WindowNative.GetWindowHandle(this)));
        var scalingFactor = (float)dpi / 96;
        AppWindow.Resize(new Windows.Graphics.SizeInt32((int)(400.0f * scalingFactor), (int)(300.0f * scalingFactor)));
        */

        editorWindow.AppWindow.MoveAndResize(new Windows.Graphics.RectInt32(EditorWinLeft, EditorWinTop, EditorWinWidth, EditorWinHeight));

        //editorWindow.AppWindow.Show();
        editorWindow.Activate();

    }

    [RelayCommand]
    private async Task SearchRentResidential()
    {
        //SelectedRentResidentialItem = null;
        RentResidentialSearchResult.Clear();

        var res = await Task.Run(() => _dataAccessService.SelectRentResidentialsByNameKeyword("*"), _cts.Token);

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
            RentResidentialSearchResult = new(res.SelectedEntries);

            _navigationService.NavigateTo("ZumenSearch.Views.Rent.ResidentialSearchResultPage", SlideNavigationTransitionEffect.FromLeft);//Navigate(typeof(Views.Rent.Residentials.SearchResultPage), null, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromRight });
        }
    }

    [RelayCommand]
    public async Task EditRentResidential(Models.Rent.Residentials.EntryResidentialSearchResult? selected)
    {
        var isFound = false;

        if (selected == null)
        {
            Debug.WriteLine("EditRentResidentialCommand executed but no item is selected.");
            return;
        }

        Debug.WriteLine($"EditRentResidentialCommand executed for {selected.Id}");

        // Check if the selected item is already being edited in another window.
        EditorList.ForEach(async editorWindow =>
        {
            Debug.WriteLine($"Checking editor window with Id: {editorWindow.Id} for selected item with Id: {selected.Id}");
            if (editorWindow.Id == selected.Id)
            {
                // If the editor window for this item is already open, activate it.
                Debug.WriteLine($"Editor window for {selected.Id} is already open. Activating it.");
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
        var res = _dataAccessService.SelectRentResidentialById(selected.Id);// Go back to UI thred. Let's not do > .ConfigureAwait(false);
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
            Debug.WriteLine($"EntryResidentialFull for {selected.Id} is null. Cannot open editor.");
            return;
        }

        var editorShell = _editorFactory.Create();

        // Sets the instance of selected Entry.
        //editorShell.SetEntryToEntryViewModel(res.EntryFull);

        var entryViewModel = editorShell.ViewModel;
        entryViewModel.SetEntry(res.EntryFull);

        var editorWindow = editorShell.EditorWin;
        if (editorWindow == null)
        {
            // EditorWin should be initialized in the EditorShell constructor.
            Debug.WriteLine("EditorWin must be initialized in the EditorShell constructor");
            return;
        }
        editorWindow.SetEntryIdToWindow(selected.Id);

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

        editorWindow.Closed += (sender, e) =>
        {
            // Activate the main window again.
            //App.MainWnd?.Activate();

            EditorList.Remove(editorWindow);
        };

        editorWindow.AppWindow.Show();
        editorWindow.Activate();
        var mainWindow = App.GetService<MainWindow>();
        mainWindow?.AppWindow.MoveInZOrderBelow(editorWindow.AppWindow.Id);
        editorWindow.AppWindow.MoveInZOrderAtTop();
    }

    [RelayCommand]
    private void DeleteRentResidential(Models.Rent.Residentials.EntryResidentialSearchResult? selected)
    {
        if (selected == null)
        {
            Debug.WriteLine("DeleteRentResidentialAsync executed but no item is selected.");
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
            if (RentResidentialSearchResult.Remove(selected))
            {
                // Successfully removed the selected item from the search result.
            }
            else
            {
                Debug.WriteLine($"Selected item {selected.Id} not found in the search result or could not remove.");
            }
        }
    }

    [RelayCommand(CanExecute = nameof(BackToCanExecute))]
    private void BackToRentResidential()
    {
        //_navigationService.NavigateTo("ZumenSearch.Views.Rent.Residentials.SearchPage");
        _navigationService.GoBack();
    }
    public bool BackToCanExecute()
    {
        return _navigationService.CanGoBack(); 
    }
    #endregion
}
