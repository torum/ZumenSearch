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
using ZumenSearch.Services;
using ZumenSearch.Services.Extensions.AbstractFactory;
using ZumenSearch.Views;

namespace ZumenSearch.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        #region == Window management ==

        private readonly IAbstractFactory<Views.Rent.Residentials.Editor.EditorShell> _editorFactory;

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
        
        private readonly IDataAccessService _dataAccessService;

        #endregion

        /*
         * Somehow this does not work with command. Decided to use RelayCommand with parameter instead.
        private Models.Rent.RentResidential? _selectedRentResidentialItem;
        public Models.Rent.RentResidential? SelectedRentResidentialItem
        {
            get => _selectedRentResidentialItem;
            //set => SetProperty(ref _selectedRentResidentialItem, value);
            set
            {
                if (SetProperty(ref _selectedRentResidentialItem, value))
                {
                    Debug.WriteLine($"SelectedRentResidentialItem changed to: {value?.Id}");
                }
            }
        }
        */

        public ObservableCollection<Models.Rent.Residentials.RentResidential> RentResidentialSearchResult = [];

        private static MainShell Shell => App.GetService<MainShell>();

        public MainViewModel(IAbstractFactory<Views.Rent.Residentials.Editor.EditorShell> editorFactory, IDataAccessService dataAccessService)
        {
            _editorFactory = editorFactory;
            _dataAccessService = dataAccessService;

            InitializeDatabase();
        }

        private async void InitializeDatabase()
        {
            var filePath = Path.Combine(App.AppDataFolder, "ZumenSearch.db");

            var res = await Task.FromResult(_dataAccessService.InitializeDatabase(filePath));
            if (res.IsError)
            {
                //ErrorMain = res.Error;
                //IsMainErrorInfoBarVisible = true;

                // TODO: Show error message to user
            }
        }

        private RelayCommand? addNewRentResidentialCommand;
        public IRelayCommand AddNewRentResidentialCommand => addNewRentResidentialCommand ??= new RelayCommand(AddNewRentResidential);
        private void AddNewRentResidential()
        {
            Debug.WriteLine("AddNew command executed!");

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
        public IRelayCommand SearchRentResidentialCommand => searchRentResidentialCommand ??= new RelayCommand(SearchRentResidential);
        private async void SearchRentResidential()
        {
            //SelectedRentResidentialItem = null;
            RentResidentialSearchResult.Clear(); 

            var res = await Task.FromResult(_dataAccessService.SelectRentResidentialsByNameKeyword("*"));
            if (res.IsError)
            {
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

        private RelayCommand<Models.Rent.Residentials.RentResidential>? editRentResidentialCommand;
        public IRelayCommand<Models.Rent.Residentials.RentResidential> EditRentResidentialCommand => editRentResidentialCommand ??= new RelayCommand<Models.Rent.Residentials.RentResidential>(EditRentResidential);
        private void EditRentResidential(Models.Rent.Residentials.RentResidential? selected)
        {
            bool isFound = false;

            if (selected != null)
            {
                Debug.WriteLine($"EditRentResidentialCommand executed for {selected.Id}");

                // TODO: Check if the selected item is already being edited in another window.
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

                Views.Rent.Residentials.Editor.EditorShell editorShell = _editorFactory.Create();

                // Sets the instance of selected Entry.
                editorShell.SetEntry(selected); 

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
            else
            {
                Debug.WriteLine("EditRentResidentialCommand executed but no item is selected.");
                return;
            }
        }

        private RelayCommand<Models.Rent.Residentials.RentResidential>? deleteRentResidentialCommand;
        public IRelayCommand<Models.Rent.Residentials.RentResidential> DeleteRentResidentialCommand => deleteRentResidentialCommand ??= new RelayCommand<Models.Rent.Residentials.RentResidential>(DeleteRentResidential);

        private async void DeleteRentResidential(Models.Rent.Residentials.RentResidential? selected)
        {
            if (selected != null)
            {
                Debug.WriteLine($"DeleteRentResidentialCommand executed for {selected.Id}");

                var res = await Task.FromResult(_dataAccessService.DeleteRentResidential(selected.Id));
                if (res.IsError)
                {
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
        }

        private RelayCommand? backToRentResidentialCommand;
        public IRelayCommand BackToRentResidentialCommand => backToRentResidentialCommand ??= new RelayCommand(BackToRentResidential);
        private void BackToRentResidential()
        {
            Shell.NavFrame.Navigate(typeof(Views.Rent.Residentials.SearchPage), Shell.NavFrame, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromLeft });
        }
    }
}
