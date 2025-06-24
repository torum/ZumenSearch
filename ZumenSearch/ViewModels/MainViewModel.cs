using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
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
    #region == Properties ==
    private static MainShell Shell => App.GetService<MainShell>();

    #region == Window management ==

    // Marking EditorList as readonly to fix IDE0044
    public readonly List<Views.Rent.Residentials.Editor.EditorWindow> EditorList = [];

    // Editor window position and size
    public int EditorWinWidth = 1366;
    public int EditorWinHeight = 768;
    public int EditorWinLeft = 130;
    public int EditorWinTop = 130;

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

        InitializeDatabaseAsync();
    }

    #endregion

    #region == Methods ==

    private async void InitializeDatabaseAsync()
    {
        var filePath = Path.Combine(App.AppDataFolder, "ZumenSearch.db");

        var res = await Task.FromResult(_dataAccessService.InitializeDatabase(filePath)).ConfigureAwait(false);
        if (res.IsError)
        {

            Debug.WriteLine(res.Error.ErrText + Environment.NewLine + res.Error.ErrDescription + Environment.NewLine + res.Error.ErrPlace + Environment.NewLine + res.Error.ErrPlaceParent);

            //ErrorMain = res.Error;
            //IsMainErrorInfoBarVisible = true;

            // TODO: Show error message to user
        }
    }

    #endregion

    #region == Commands ==

    private RelayCommand? addNewRentResidentialCommand;
    public IRelayCommand AddNewRentResidentialCommand => addNewRentResidentialCommand ??= new RelayCommand(AddNewRentResidential);
    private void AddNewRentResidential()
    {
        //Debug.WriteLine("AddNew command executed!");

        Views.Rent.Residentials.Editor.EditorShell editorShell = _editorFactory.Create();

        var editorWindow = editorShell.EditorWin;

        if (editorWindow == null)
        {
            // EditorWin should be initialized in the EditorShell constructor.
            throw new ArgumentNullException(nameof(editorWindow));
        }

        //MainViewModel mainShellViewModel = App.GetService<MainViewModel>();
        // Add to the list of editor windows.
        this.EditorList.Add(editorWindow);

        // Window state and position.
        //editorWindow.AppWindow.MoveAndResize(new Windows.Graphics.RectInt32(mainShellViewModel.EditorWinLeft, mainShellViewModel.EditorWinTop, mainShellViewModel.EditorWinWidth, mainShellViewModel.EditorWinHeight));
        // TEMP:
        editorWindow.AppWindow.MoveAndResize(new Windows.Graphics.RectInt32(this.EditorWinLeft, this.EditorWinTop, 1366, 768));
        if (editorWindow.AppWindow.Presenter is OverlappedPresenter presenter)
        {
            presenter.IsResizable = false;
        }

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

    private RelayCommand? searchRentResidentialCommand;
    public IRelayCommand SearchRentResidentialCommand => searchRentResidentialCommand ??= new RelayCommand(SearchRentResidentialAsync);
    private async void SearchRentResidentialAsync()
    {
        //SelectedRentResidentialItem = null;
        RentResidentialSearchResult.Clear();

        var res = await Task.FromResult(_dataAccessService.SelectRentResidentialsByNameKeyword("*")).ConfigureAwait(ConfigureAwaitOptions.ContinueOnCapturedContext);// Go back to UI thred. Let's not do > .ConfigureAwait(false);
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

    private RelayCommand<Models.Rent.Residentials.EntryResidentialSearchResult>? editRentResidentialCommand;
    public IRelayCommand<Models.Rent.Residentials.EntryResidentialSearchResult> EditRentResidentialCommand => editRentResidentialCommand ??= new RelayCommand<Models.Rent.Residentials.EntryResidentialSearchResult>(EditRentResidential);
    private async void EditRentResidential(Models.Rent.Residentials.EntryResidentialSearchResult? selected)
    {
        bool isFound = false;
        
        if (selected == null)
        {
            Debug.WriteLine("EditRentResidentialCommand executed but no item is selected.");
            return;
        }

        //Debug.WriteLine($"EditRentResidentialCommand executed for {selected.Id}");

        // Check if the selected item is already being edited in another window.
        this.EditorList.ForEach(editorWindow =>
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
        var res = await Task.FromResult(_dataAccessService.SelectRentResidentialById(selected.Id)).ConfigureAwait(ConfigureAwaitOptions.ContinueOnCapturedContext);// Go back to UI thred. Let's not do > .ConfigureAwait(false);
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

        Views.Rent.Residentials.Editor.EditorShell editorShell = _editorFactory.Create();

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
        this.EditorList.Add(editorWindow);

        // Window state and position.
        //editorWindow.AppWindow.MoveAndResize(new Windows.Graphics.RectInt32(mainShellViewModel.EditorWinLeft, mainShellViewModel.EditorWinTop, mainShellViewModel.EditorWinWidth, mainShellViewModel.EditorWinHeight));
        // TEMP:
        editorWindow.AppWindow.MoveAndResize(new Windows.Graphics.RectInt32(this.EditorWinLeft, this.EditorWinTop, 1366, 768));
        if (editorWindow.AppWindow.Presenter is OverlappedPresenter presenter)
        {
            presenter.IsResizable = false;
        }

        //editorWindow.AppWindow.Show();
        editorWindow.Activate();
    }

    private RelayCommand<Models.Rent.Residentials.EntryResidentialSearchResult>? deleteRentResidentialCommand;
    public IRelayCommand<Models.Rent.Residentials.EntryResidentialSearchResult> DeleteRentResidentialCommand => deleteRentResidentialCommand ??= new RelayCommand<Models.Rent.Residentials.EntryResidentialSearchResult>(DeleteRentResidentialAsync);
    private async void DeleteRentResidentialAsync(Models.Rent.Residentials.EntryResidentialSearchResult? selected)
    {
        if (selected == null)
        {
            Debug.WriteLine("DeleteRentResidentialAsync executed but no item is selected.");
            return;
        }

        bool isFound = false;

        // TODO: Check if the selected item is already being edited in another window.
        this.EditorList.ForEach(editorWindow =>
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

        var res = await Task.FromResult(_dataAccessService.DeleteRentResidential(selected.Id)).ConfigureAwait(false);
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

    private RelayCommand? backToRentResidentialCommand;
    public IRelayCommand BackToRentResidentialCommand => backToRentResidentialCommand ??= new RelayCommand(BackToRentResidential);
    private void BackToRentResidential()
    {
        Shell.NavFrame.Navigate(typeof(Views.Rent.Residentials.SearchPage), Shell.NavFrame, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromLeft });
    }

    #endregion
}
