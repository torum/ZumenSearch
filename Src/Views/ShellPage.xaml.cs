using Microsoft.UI.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using System.Diagnostics;
using ZumenSearch.Services.Contracts;

namespace ZumenSearch.Views;

internal sealed partial class ShellPage : Page
{
    public ViewModels.MainViewModel ViewModel { get; private set; }

    public Frame NavigationFrame => ContentFrame;

    private MainWindow? _mainWindow;
    private bool _activated;

    private readonly INavigationService _navigationService;

    public ShellPage(INavigationService navigationService, ViewModels.MainViewModel viewModel)
    {
        _navigationService = navigationService;
        ViewModel = viewModel;

        InitializeComponent();
    }

    public void CallMeWhenMainWindowIsReady(MainWindow wnd)
    {
        _mainWindow = wnd;

        wnd.SetTitleBar(AppTitleBar);

        wnd.Activated += MainWindow_Activated;
    }

    private void Page_Loaded(object sender, RoutedEventArgs e)
    {
        // Note: ContentFrame as param (instead of ViewModel) is expected by SearchPage for further navigation, such as navigating to SearchResultPage.
        //ContentFrame.Navigate(typeof(ZumenSearch.Views.Rent.ResidentialSearchPage), ContentFrame, new Microsoft.UI.Xaml.Media.Animation.EntranceNavigationTransitionInfo());//, //
        if (ContentFrame.Navigate(typeof(ZumenSearch.Views.SearchPage), ContentFrame, new Microsoft.UI.Xaml.Media.Animation.EntranceNavigationTransitionInfo()))
        {
            /*
            var selectedItem = FindNavigationViewItemWithTag("ZumenSearch.Views.Rent.ResidentialSearchPage");//"ZumenSearch.Views.Rent.ResidentialSearchPage"
            if (selectedItem != null)
            {
                NavigationViewControl.SelectedItem = selectedItem;
                //NavigationViewControl.Header = ((NavigationViewItem)NavigationViewControl.SelectedItem)?.Content?.ToString();
            }
            else
            {
                Debug.WriteLine("No menu item with tag matching the current page found in NavigationViewControl @ZumenSearch.Views.ShellPage. Current page: " + ContentFrame.SourcePageType.FullName);
            }
            */
            SetRegionsForCustomTitleBar("Page_Loaded");
        }
    }

    private void MainWindow_Activated(object sender, WindowActivatedEventArgs args)
    {
        var resource = args.WindowActivationState == WindowActivationState.Deactivated ? "WindowCaptionForegroundDisabled" : "WindowCaptionForeground";
        AppTitleBarText.Foreground = (SolidColorBrush)App.Current.Resources[resource];
        if (args.WindowActivationState == WindowActivationState.Deactivated)
        {
            AppTitleBarIcon.Opacity = 0.5;
        }
        else
        {
            AppTitleBarIcon.Opacity = 1;

            if (!_activated)
            {
                _activated = true;

                //Debug.WriteLine($"{sender}");
                if (sender is MainWindow wnd)
                {
                    _mainWindow = wnd;
                }
            }
        }
    }

    private void NavigationViewControl_DisplayModeChanged(NavigationView sender, NavigationViewDisplayModeChangedEventArgs args)
    {
        SetRegionsForCustomTitleBar("NavigationViewControl_DisplayModeChanged");
    }

    private void NavigationViewControl_Loaded(object sender, RoutedEventArgs e)
    {
        SetRegionsForCustomTitleBar("NavigationViewControl_Loaded");
    }

    private void NavigationViewControl_ItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
    {
        if (args.IsSettingsInvoked == true)
        {
            _navigationService.NavigateTo("ZumenSearch.Views.SettingsPage", SlideNavigationTransitionEffect.FromBottom);
        }
        else if (args.InvokedItemContainer != null && (args.InvokedItemContainer.Tag != null))
        {
            _navigationService.NavigateTo(args.InvokedItemContainer.Tag, SlideNavigationTransitionEffect.FromBottom);
        }
        else
        {
            Debug.WriteLine("NavigationViewControl_ItemInvoked: No valid item invoked. IsSettingsInvoked: " + args.IsSettingsInvoked + ", InvokedItemContainer: " + (args.InvokedItemContainer != null) + ", Tag: " + (args.InvokedItemContainer?.Tag != null));
        }
    }

    private void NavigationViewControl_Navigated(object sender, Microsoft.UI.Xaml.Navigation.NavigationEventArgs e)
    {
        NavigationViewControl.IsBackEnabled = ContentFrame.CanGoBack;

        if (ContentFrame.SourcePageType == typeof(Views.SettingsPage))
        {
            // SettingsItem is not part of NavView.MenuItems, and doesn't have a Tag.
            NavigationViewControl.SelectedItem = (NavigationViewItem)NavigationViewControl.SettingsItem;

            // Hide SearchBox
            //SearchBox.Visibility = Visibility.Visible;
            //SetRegionsForCustomTitleBar("NavigationViewControl_Navigated");
        }
        else if (ContentFrame.SourcePageType != null)
        {
            var selectedItem = FindNavigationViewItemWithTag(ContentFrame.SourcePageType.FullName!);
            if (selectedItem != null)
            {
                NavigationViewControl.SelectedItem = selectedItem;
                //NavigationViewControl.Header = ((NavigationViewItem)NavigationViewControl.SelectedItem)?.Content?.ToString();
            }
            else
            {
                //Debug.WriteLine("No menu item with tag matching the current page found in NavView. Current page: " + ContentFrame.SourcePageType.FullName);
            }

            if (ContentFrame.SourcePageType == typeof(Views.SearchPage))
            {
                // Hide SearchBox
                //SearchBox.Visibility = Visibility.Collapsed;
                //SetRegionsForCustomTitleBar("NavigationViewControl_Navigated");
            }
            else
            {
                //SearchBox.Visibility = Visibility.Visible;
                //SetRegionsForCustomTitleBar("NavigationViewControl_Navigated");
            }
        }
    }

    private NavigationViewItem? FindNavigationViewItemWithTag(string tag)
    {
        foreach (var item in NavigationViewControl.MenuItems.OfType<NavigationViewItem>())
        {
            if (item.Tag.Equals(tag))
            {
                NavigationViewControl.SelectedItem = item;
                return item;
            }

            if (item.MenuItems.Count > 0)
            {
                foreach (var subItem in item.MenuItems.OfType<NavigationViewItem>())
                {
                    if (subItem.Tag.Equals(tag))
                    {
                        NavigationViewControl.SelectedItem = subItem;
                        return subItem;
                    }
                }
            }
        }

        return null;
    }

    private void NavigationViewControl_BackRequested(NavigationView sender, NavigationViewBackRequestedEventArgs args)
    {
        if (ContentFrame.CanGoBack) ContentFrame.GoBack();
    }

    private void BackButton_Click(object sender, RoutedEventArgs e)
    {
        if (ContentFrame.CanGoBack) ContentFrame.GoBack();
    }

    private void NavigationViewControl_PaneOpened(NavigationView sender, object args)
    {
        SetRegionsForCustomTitleBar("NavigationViewControl_PaneOpened");
    }

    private void NavigationViewControl_PaneClosed(NavigationView sender, object args)
    {
        SetRegionsForCustomTitleBar("NavigationViewControl_PaneClosed");
    }

    private void AppTitleBarGrid_Loaded(object sender, RoutedEventArgs e)
    {
        SetRegionsForCustomTitleBar("AppTitleBarGrid_Loaded");
    }

    private void AppTitleBar_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        SetRegionsForCustomTitleBar("AppTitleBar_SizeChanged");
    }

    private void SetRegionsForCustomTitleBar(string str)
    {
        if (_mainWindow is null)
        {
            Debug.WriteLine($"{str} MainWindow is null. Cannot set regions for custom title bar.");
            return;
        }

        if (this.XamlRoot is null)
        {
            Debug.WriteLine($"{str} XamlRoot is null. Cannot set regions for custom title bar.");
            return;
        }

        var scaleAdjustment = this.XamlRoot.RasterizationScale;

        // Back button size
        var width = this.SearchBox.ActualWidth;//ActualWidth won't work in certain cases. e.g. when visivility changed.
        var height = this.SearchBox.ActualHeight;//ActualHeight won't work in certain cases. e.g. when visivility changed.

        if (this.SearchBox.Visibility != Visibility.Visible)
        {
            //Debug.WriteLine("SearchBox.Visibility != Visibility.Visible");
            width = 0;
            height = 0;
        }

        GeneralTransform transform1 = this.SearchBox.TransformToVisual(null);
        Windows.Foundation.Rect bounds1 = transform1.TransformBounds(new Windows.Foundation.Rect(0, 0,
                                                         width,
                                                         height));
        Windows.Graphics.RectInt32 SearchBoxRect = GetRect(bounds1, scaleAdjustment);

        // Back button size
        var width2 = this.BackButton.ActualWidth;//ActualWidth won't work in certain cases. e.g. when visivility changed.
        var height2 = this.BackButton.ActualHeight;//ActualHeight won't work in certain cases. e.g. when visivility changed.

        if (this.BackButton.Visibility != Visibility.Visible)
        {
            //Debug.WriteLine("BackButton.Visibility != Visibility.Visible");
            width2 = 0;
            height2 = 0;
        }

        GeneralTransform transform2 = this.BackButton.TransformToVisual(null);
        Windows.Foundation.Rect bounds2 = transform2.TransformBounds(new Windows.Foundation.Rect(0, 0,
                                                    width2,
                                                    height2));
        Windows.Graphics.RectInt32 BackButtonRect = GetRect(bounds2, scaleAdjustment);

        var rectArray = new Windows.Graphics.RectInt32[] { SearchBoxRect, BackButtonRect };//, SettingsButton

        InputNonClientPointerSource nonClientInputSrc = InputNonClientPointerSource.GetForWindowId(_mainWindow.AppWindow.Id);
        nonClientInputSrc.SetRegionRects(NonClientRegionKind.Passthrough, rectArray);

        //Debug.WriteLine($"{str} SetRegionsForCustomTitleBar called.");
    }

    private static Windows.Graphics.RectInt32 GetRect(Windows.Foundation.Rect bounds, double scale)
    {
        return new Windows.Graphics.RectInt32(
            _X: (int)Math.Round(bounds.X * scale),
            _Y: (int)Math.Round(bounds.Y * scale),
            _Width: (int)Math.Round(bounds.Width * scale),
            _Height: (int)Math.Round(bounds.Height * scale)
        );
    }

    private void SearchBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
    {
        if (args.ChosenSuggestion is Models.Common.AutoSuggestItem asi)
        {
            Debug.WriteLine($"QuerySubmitted ChosenSuggestion is {asi.Name}");

            if (ViewModel.EditRentResidentialEntryCommand.CanExecute(asi.Id))
            {
                ViewModel.EditRentResidentialEntryCommand.Execute(asi.Id);
            }
        }
        else
        {
            Debug.WriteLine($"QuerySubmitted No ChosenSuggestion QueryText is {args.QueryText}");
            //UpdateSuggestion(args.QueryText);
        }
    }

    private void SearchBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
    {
        if (sender is not AutoSuggestBox asb)
        {
            return;
        }

        var squery = asb.Text;
        if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
        {
            UpdateSuggestion(squery);
        }
    }

    private void UpdateSuggestion(string squery)
    {
        if (ViewModel.SearchRentForAutoSuggestCommand.CanExecute(squery))
        {
            ViewModel.SearchRentForAutoSuggestCommand.Execute(squery);
        }
    }

    private void SearchBox_SuggestionChosen(AutoSuggestBox sender, AutoSuggestBoxSuggestionChosenEventArgs args)
    {
        if (args.SelectedItem is Models.Common.AutoSuggestItem selectedItem)
        {
            if (string.IsNullOrEmpty(selectedItem.Id)) return;

            //Debug.WriteLine($"SuggestionChosen {selectedItem.Name}");

            // Set the text box content to the property you want the user to see
            sender.Text = selectedItem.Name;
        }
    }

    private void BackAccelerator_Invoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
    {
        if (this.ContentFrame != null && this.ContentFrame.CanGoBack)
        {
            this.ContentFrame.GoBack();
            args.Handled = true;
        }
    }
}
