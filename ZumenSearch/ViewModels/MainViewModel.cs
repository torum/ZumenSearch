using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.Windows.ApplicationModel.Resources;
using Windows.ApplicationModel;
using Windows.UI.ApplicationSettings;
using ZumenSearch.Helpers;
using ZumenSearch.Models;
using ZumenSearch.Models.Rent;
using ZumenSearch.Models.Rent.Residentials;
using ZumenSearch.Services;
using ZumenSearch.Services.Extensions.AbstractFactory;
using ZumenSearch.Views;

namespace ZumenSearch.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private static readonly ResourceLoader _resourceLoader = new();

    private string _versionDescription;

    public string VersionDescription
    {
        get => _versionDescription;
        private set => SetProperty(ref _versionDescription, value);
    }

    #region == Properties ==

    private static MainShell Shell => App.GetService<MainShell>();

    private static MainWindow MainWin => App.GetService<MainWindow>();

    #region == Window management ==

    // Marking EditorList as readonly to fix IDE0044
    public readonly List<Views.Rent.Residentials.Editor.EditorWindow> EditorList = [];

    // Editor window position and size
    public int EditorWinWidth = 1366;
    public int EditorWinHeight = 768;
    public int EditorWinLeft = 130;
    public int EditorWinTop = 130;

    // Modal window position and size
    public int ModalWinWidth = 1366;
    public int ModalWinHeight = 768;
    public int ModalWinLeft = 130;
    public int ModalWinTop = 130;

    #endregion

    #region == Navigation ==

    // TODO: Do I need this property?
    private bool _isBackEnabled = true;
    public bool IsBackEnabled // Implement partial property for AOT compatibility
    {
        get => _isBackEnabled;
        set => SetProperty(ref _isBackEnabled, value);
    }

    // TODO: Do I need this property?
    private object? _selectedNavigationViewItem;
    public object? SelectedNavigationViewItem
    {
        get => _selectedNavigationViewItem;
        set => SetProperty(ref _selectedNavigationViewItem, value);
    }

    #endregion

    #region == Database ==


    #endregion

    #region == Search ==

    public ObservableCollection<Models.Rent.Residentials.EntryResidentialSearchResult> RentResidentialSearchResult = [];

    #endregion

    #endregion

    #region == Services ==

    private readonly IAbstractFactory<Views.Rent.Residentials.Editor.EditorShell> _editorFactory;
    private readonly IDataAccessService _dataAccessService;

    #endregion

    #region == Constructor ==

    public MainViewModel(IAbstractFactory<Views.Rent.Residentials.Editor.EditorShell> editorFactory, IDataAccessService dataAccessService)
    {
        _editorFactory = editorFactory;
        _dataAccessService = dataAccessService;

        _versionDescription = GetVersionDescription();

        InitializeDatabaseAsync();
    }

    #endregion

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

        var verName = _resourceLoader.GetString("AppDisplayName");

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

        //MainViewModel mainShellViewModel = App.GetService<MainViewModel>();
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
            // Activate the main window again.
            //App.MainWindow?.Activate(); // Not good when multiple editor windows are opened.
        };

        // Window state and position.
        //editorWindow.AppWindow.MoveAndResize(new Windows.Graphics.RectInt32(mainShellViewModel.EditorWinLeft, mainShellViewModel.EditorWinTop, mainShellViewModel.EditorWinWidth, mainShellViewModel.EditorWinHeight));
        // TEMP:
        editorWindow.AppWindow.MoveAndResize(new Windows.Graphics.RectInt32(EditorWinLeft, EditorWinTop, EditorWinWidth, EditorWinHeight));

        //editorWindow.AppWindow.Show();
        editorWindow.Activate();

        /*
        Debug.WriteLine("AddNew command executed!");

        Views.Rent.Residentials.Editor.EditorShell editorShell = _editorFactory.Create();
        var editorWindow = editorShell.EditorWin;

        if (editorWindow == null)
        {
            // EditorWin should be initialized in the EditorShell constructor.
            throw new ArgumentNullException(nameof(editorWindow));
        }

        MainShellViewModel mainShellViewModel = App.GetService<MainShellViewModel>();
        // Add to the list of editor windows.
        mainShellViewModel.EditorList.Add(editorWindow);
        */

        /*
        // This won't work since editor window closes AFTER the main window and miss the timing for the saving settings to the config file.
        Microsoft.UI.Xaml.Window? win = App.MainWindow;
        if (win != null)
        {
            win.Closed += (s, a) =>
            {
                // TODO: when close is canceled.
                //editorEindow.CanClose

                editorWindow.Close();
            };
        }
        */

        /*
        // Window state and position.
        //editorWindow.AppWindow.MoveAndResize(new Windows.Graphics.RectInt32(mainShellViewModel.EditorWinLeft, mainShellViewModel.EditorWinTop, mainShellViewModel.EditorWinWidth, mainShellViewModel.EditorWinHeight));
        // TEMP:
        editorWindow.AppWindow.MoveAndResize(new Windows.Graphics.RectInt32(mainShellViewModel.EditorWinLeft, mainShellViewModel.EditorWinTop, 1366, 768));
        if (editorWindow.AppWindow.Presenter is OverlappedPresenter presenter)
        {
            presenter.IsResizable = false;
        }

        //editorWindow.AppWindow.Show();
        editorWindow.Activate();
        */

        /*
        //NavigationService.NavigateTo(typeof(RentLivingEditShellViewModel).FullName!, "test");

        var editor = _editorFactory.Create();

        var editorEindow = editor.Window;

        App.MainWindow.Closed += (s, a) =>
        {
            // TODO: when close is canceled.
            //editorEindow.CanClose
            editorEindow.Close();
        };

        editorEindow.Show();
        */
    }

    [RelayCommand]
    private void SearchRentResidential()
    {
        //SelectedRentResidentialItem = null;
        RentResidentialSearchResult.Clear();

        var res = _dataAccessService.SelectRentResidentialsByNameKeyword("*");
        if (res.IsError)
        {
            Debug.WriteLine(res.Error.ErrText + Environment.NewLine + res.Error.ErrDescription + Environment.NewLine + res.Error.ErrPlace + Environment.NewLine + res.Error.ErrPlaceParent);

            //ErrorMain = res.Error;
            //IsMainErrorInfoBarVisible = true;

            // TODO: Show error message to user
        }
        else
        {
            RentResidentialSearchResult = res.SelectedEntries;
        }

        Shell.NavFrame.Navigate(typeof(Views.Rent.Residentials.SearchResultPage), Shell.NavFrame, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromRight });
    }

    [RelayCommand]
    private void EditRentResidential(Models.Rent.Residentials.EntryResidentialSearchResult? selected)
    {
        var isFound = false;
        
        if (selected == null)
        {
            Debug.WriteLine("EditRentResidentialCommand executed but no item is selected.");
            return;
        }

        //Debug.WriteLine($"EditRentResidentialCommand executed for {selected.Id}");

        // Check if the selected item is already being edited in another window.
        EditorList.ForEach(editorWindow =>
        {
            Debug.WriteLine($"Checking editor window with Id: {editorWindow.Id} for selected item with Id: {selected.Id}");
            if (editorWindow.Id == selected.Id)
            {
                // If the editor window for this item is already open, activate it.
                Debug.WriteLine($"Editor window for {selected.Id} is already open. Activating it.");
                isFound = true;
                editorWindow.Activate();
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
        editorShell.SetEntryToEntryViewModel(res.EntryFull);

        var editorWindow = editorShell.EditorWin;

        if (editorWindow == null)
        {
            // EditorWin should be initialized in the EditorShell constructor.
            Debug.WriteLine("EditorWin must be initialized in the EditorShell constructor");
            return;
        }

        //MainViewModel mainShellViewModel = App.GetService<MainViewModel>();
        // Add to the list of editor windows.
        editorWindow.Id = selected.Id;
        EditorList.Add(editorWindow);

        // Window state and position.
        //editorWindow.AppWindow.MoveAndResize(new Windows.Graphics.RectInt32(mainShellViewModel.EditorWinLeft, mainShellViewModel.EditorWinTop, mainShellViewModel.EditorWinWidth, mainShellViewModel.EditorWinHeight));
        // TEMP:
        editorWindow.AppWindow.MoveAndResize(new Windows.Graphics.RectInt32(EditorWinLeft, EditorWinTop, EditorWinWidth, EditorWinHeight));
        if (editorWindow.AppWindow.Presenter is OverlappedPresenter presenter)
        {
            presenter.IsResizable = true;
            presenter.IsModal = false;
            presenter.IsAlwaysOnTop = false;
        }

        editorWindow.Closed += (sender, e) =>
        {
            // Activate the main window again.
            //App.MainWnd?.Activate();
        };

        //await Task.Delay(30).ConfigureAwait(false);

        //editorWindow.AppWindow.Show();
        editorWindow.Activate();
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

    [RelayCommand]
    private static void BackToRentResidential()
    {
        Shell.NavFrame.Navigate(typeof(Views.Rent.Residentials.SearchPage), Shell.NavFrame, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromLeft });
    }

    #endregion
}
