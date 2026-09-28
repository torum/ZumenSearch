using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using ZumenSearch.Services.Contracts;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.UI.Windowing;

namespace ZumenSearch.Views.Rent.Commercials.Listing;

public sealed partial class ShellPage : Page
{
    public ViewModels.Rent.Commercials.Listing.ListingViewModel ViewModel { get; }
    public EditorWindow Window { get; }

    private readonly INavigationGenericService _navigationService;
    private readonly IDialogGenericService _dialogService;

    private readonly List<(string Tag, string Label, Type? Page)> _pages =
    [
        (
            "ZumenSearch.Views.Rent.Commercials.Listing.BasicPage",
            "基本",
            typeof(BasicPage)
        )
    ];

    public ShellPage(
        Models.Rent.Commercials.Listing.Listing unit,
        INavigationGenericService navigationService,
        IDialogGenericService dialogService,
        IDataAccessService dataAccessService)
    {
        _navigationService = navigationService;
        _dialogService = dialogService;

        ViewModel = new ViewModels.Rent.Commercials.Listing.ListingViewModel(
            unit,
            navigationService,
            dataAccessService);

        Window = new EditorWindow(unit.Id, ViewModel)
        {
            Content = this
        };

        InitializeComponent();
        _navigationService.Initialize(ContentFrame, _pages);

        Loaded += ShellPage_Loaded;
        Window.Activated += Window_Activated;
        Window.Closed += Window_Closed;
        Window.AppWindow.Closing += AppWindow_Closing;
    }

    public async Task ShowEditorCloseConfirmationDialog()
    {
        if (ViewModel == null)
        {
            return;
        }

        if (!ViewModel.IsDirty)
        {
            return;
        }

        // show ConfirmationDialog
        var result = await _dialogService.ShowEditorCloseConfirmationDialog();

        if (result == ContentDialogResult.Primary)
        {
            if (ViewModel.IsDirty)
            {
                //ViewModel.Save();
                if (ViewModel.SaveCommand.CanExecute(null))
                {
                    ViewModel.SaveCommand.Execute(null);
                }
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

    private void ShellPage_Loaded(object sender, RoutedEventArgs e)
    {
        _dialogService.Initialize(XamlRoot, Window);

        if (ContentFrame.Content is null)
        {
            ContentFrame.Navigate(typeof(BasicPage), ViewModel);
        }
    }

    private void NavView_Loaded(object sender, RoutedEventArgs e)
    {
        if (ContentFrame.Content is null)
        {
            ContentFrame.Navigate(typeof(BasicPage), ViewModel);
        }
    }

    private void NavView_ItemInvoked(
        NavigationView sender,
        NavigationViewItemInvokedEventArgs args)
    {
        if (args.InvokedItemContainer?.Tag is string tag)
        {
            _navigationService.NavigateTo(tag, ViewModel);
        }
    }

    private void ContentFrame_NavigationFailed(
        object sender,
        NavigationFailedEventArgs e)
    {
        throw new InvalidOperationException(
            $"Failed to load page '{e.SourcePageType.FullName}'.");
    }

    private void Window_Activated(object sender, Microsoft.UI.Xaml.WindowActivatedEventArgs args)
    {
        /*
        var resource = args.WindowActivationState == WindowActivationState.Deactivated ? "WindowCaptionForegroundDisabled" : "WindowCaptionForeground";
        AppTitleBarText.Foreground = (SolidColorBrush)App.Current.Resources[resource];

        BreadcrumbBar1.Opacity = args.WindowActivationState == WindowActivationState.Deactivated ? 0.5 : 1;
        NavView.Opacity = args.WindowActivationState == WindowActivationState.Deactivated ? 0.7 : 1;
        //ContentFrame.Opacity = args.WindowActivationState == WindowActivationState.Deactivated ? 0.7 : 1;
        */
    }

    private void Window_Closed(object sender, WindowEventArgs args)
    {
        if (sender is not EditorWindow ewin)
        {
            return;
        }

        ewin.Activated -= Window_Activated;
        ewin.Closed -= Window_Closed;
        ewin.AppWindow.Closing -= AppWindow_Closing;

        ViewModel.CleanUp();

        var mainVM = App.GetService<ViewModels.MainViewModel>();
        // Save window size and position.
        var appWindow = ewin.AppWindow;
        if (appWindow != null)
        {
            if (appWindow.Presenter is OverlappedPresenter)
            {
                mainVM.RentCommercialListingEditorWinHeight = (int)appWindow.Size.Height;
                mainVM.RentCommercialListingEditorWinWidth = (int)appWindow.Size.Width;
                mainVM.RentCommercialListingEditorWinTop = (int)appWindow.Position.Y;
                mainVM.RentCommercialListingEditorWinLeft = (int)appWindow.Position.X;
            }
        }

        //mainVM.RoomEditorList.Remove(ewin);

        // Update the selected search result's values such as name if it exists. Also, update building window's rooms list.
        WeakReferenceMessenger.Default.Send(new Models.Messenger.WindowClosedMessage(ewin));
    }

    //private bool _allowClose;

    private async void AppWindow_Closing(Microsoft.UI.Windowing.AppWindow sender, Microsoft.UI.Windowing.AppWindowClosingEventArgs args)
    {
        /*
        if (_allowClose || !ViewModel.IsDirty)
        {
            return;
        }

        args.Cancel = true;

        var result = await _dialogService.ShowEditorCloseConfirmationDialog();
        if (result == ContentDialogResult.Primary)
        {
            ViewModel.Save();
            if (!ViewModel.IsDirty)
            {
                _allowClose = true;
                Window.Close();
            }
        }
        else if (result == ContentDialogResult.Secondary)
        {
            ViewModel.DiscardChanges();
            _allowClose = true;
            Window.Close();
        }
        */

        if (ViewModel == null)
        {
            return;
        }

        if (ViewModel.IsDirty)
        {
            args.Cancel = true; // needs Cancel = true here in order to show dialog.

            await ShowEditorCloseConfirmationDialog();
        }
    }


}