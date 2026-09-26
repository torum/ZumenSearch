using CommunityToolkit.Mvvm.Messaging;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using ZumenSearch.Services.Contracts;

namespace ZumenSearch.Views.Rent.Commercials;

public sealed partial class ShellPage : Page
{
    public ViewModels.Rent.Commercials.PropertyViewModel ViewModel
    {
        get;
    }

    public EditorWindow Window
    {
        get;
    }

    private readonly INavigationGenericService _navigationService;
    private readonly IDispatcherService _dispatcherService;
    private readonly IDialogGenericService _dialogService;

    public ShellPage(
        Models.Rent.Commercials.Property building,
        INavigationGenericService navigationService,
        IDialogGenericService dialogService,
        IDispatcherService dispatcherService,
        IDataAccessService dataAccessService)
    {
        _navigationService = navigationService;
        _dispatcherService = dispatcherService;
        _dialogService = dialogService;

        ViewModel =
            new ViewModels.Rent.Commercials.PropertyViewModel(
                building,
                navigationService,
                dialogService,
                dispatcherService,
                dataAccessService);

        Window = new EditorWindow(building.Id, ViewModel)
        {
            Content = this
        };

        InitializeComponent();

        _navigationService.Initialize(
            ContentFrame,
            [
                (
                    "ZumenSearch.Views.Rent.Commercials.BasicPage",
                    "基本",
                    typeof(BasicPage)
                )
            ]);

        this.Loaded += ShellPage_Loaded;
        //this.Unloaded += ShellPage_Unloaded;
        //this.BreadcrumbBar1.ItemClicked += BreadcrumbBar_ItemClicked;

        Window.Title = "賃貸事業用：建物";
        Window.ExtendsContentIntoTitleBar = true;
        //Window.Activated += Window_Activated;
        Window.Closed += Window_Closed;
        //Window.AppWindow.Closing += AppWindow_Closing;
    }

    private void ShellPage_Loaded(object sender, RoutedEventArgs e)
    {
        if (ContentFrame.Content is null)
        {
            ContentFrame.Navigate(typeof(BasicPage),ViewModel);
        }
    }


    public async Task ShowEditorCloseConfirmationDialog()
    {
        if (ViewModel == null)
        {
            return;
        }

        if (ViewModel.IsDirty)
        {
            // show ConfirmationDialog
            var result = await _dialogService.ShowEditorCloseConfirmationDialog();

            if (result == ContentDialogResult.Primary)
            {
                if (ViewModel.IsDirty)
                {
                    ViewModel.Save();
                }

                if (ViewModel.IsDirty == false)
                {
                    Window.Close();
                }
            }
            else if (result == ContentDialogResult.Secondary)
            {
                // Discard change and close.
                ViewModel.DiscardChanges();

                Window.Close();
            }
            else if (result == ContentDialogResult.None)
            {
                // Cancel.
            }
        }
    }

    public void Window_Closed(object sender, WindowEventArgs args)
    {
        if (sender is not EditorWindow ewin)
        {
            return;
        }

        //ViewModel.CleanUp();

        //ewin.Activated -= Window_Activated;
        ewin.Closed -= Window_Closed;
        //ewin.AppWindow.Closing -= AppWindow_Closing;

        var mainVM = App.GetService<ViewModels.MainViewModel>();
        // Save window size and position.
        var appWindow = ewin.AppWindow;
        if (appWindow != null)
        {
            if (appWindow.Presenter is OverlappedPresenter)
            {
                mainVM.RentCommercEditorWinHeight = (int)appWindow.Size.Height;
                mainVM.RentCommercEditorWinWidth = (int)appWindow.Size.Width;
                mainVM.RentCommercEditorWinTop = (int)appWindow.Position.Y;
                mainVM.RentCommercEditorWinLeft = (int)appWindow.Position.X;
            }
        }

        //mainVM.CommercEditorList.Remove(ewin);
        // Update the selected search result's values such as name if it exists. Also, update building window's rooms list.
        WeakReferenceMessenger.Default.Send(new Models.Messenger.WindowClosedMessage(ewin));
    }
}