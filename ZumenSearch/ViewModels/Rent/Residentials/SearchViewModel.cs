using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using System;
using System.Diagnostics;
using ZumenSearch.Extensions.AbstractFactory;
using ZumenSearch.Helpers;
using ZumenSearch.Views;
using ZumenSearch.ViewModels;

namespace ZumenSearch.ViewModels.Rent.Residentials;

public partial class SearchViewModel : ObservableObject
{

    //private readonly IAbstractFactory<Views.Rent.Residentials.Editor.EditorShell> _editorFactory;

    public SearchViewModel()
    {
        //
    }

    private RelayCommand? searchCommand;

    public IRelayCommand SearchCommand => searchCommand ??= new RelayCommand(Search);

    private void Search()
    {
        //Debug.WriteLine("Search command executed!");

        MainShell shell = App.GetService<MainShell>();
        shell.NavFrame.Navigate(typeof(Views.Rent.Residentials.SearchResultPage), shell.NavFrame, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromRight });
        /*
        try 
        {
            
            NavigationService.NavigateTo(typeof(ResidentialsSearchResultViewModel).FullName!);
        }
        catch (Exception ex)
        {
            Debug.WriteLine("Search() Navigate to failed: " + ex.Message);
        }
        */
    }

    private RelayCommand? addNewCommand;

    public IRelayCommand AddNewCommand => addNewCommand ??= new RelayCommand(AddNew);

    private void AddNew()
    {
        MainViewModel vm = App.GetService<MainViewModel>();
        vm.CreateNewResidentialsEditorWindow();

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
}
