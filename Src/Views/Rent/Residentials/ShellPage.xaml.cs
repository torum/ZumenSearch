using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;
using System.Diagnostics;
using ZumenSearch.Services;
using ZumenSearch.ViewModels;

namespace ZumenSearch.Views.Rent.Residentials;

public sealed partial class ShellPage : Page
{
    public ViewModels.Rent.ResidentialsViewModel ViewModel {get; private set;}

    public Views.Rent.Residentials.EditorWindow EditorWin { get; private set; }

    public Frame NavigationFrame => ContentFrame;


    private readonly IModalDialogService _dlg;

    public ShellPage(Views.Rent.Residentials.EditorWindow win, ViewModels.Rent.ResidentialsViewModel vm, IModalDialogService modalDialog)
    {
        EditorWin = win ?? throw new ArgumentNullException(nameof(win));
        ViewModel = vm ?? throw new ArgumentNullException(nameof(vm));
        ViewModel.SetEditorWin(win);// Must set Editor Winodw to VM.
        ViewModel.SetEditorShell(this);
        _dlg = modalDialog;

        InitializeComponent();

        //BreadcrumbBar1.ItemClicked += BreadcrumbBar_ItemClicked;

        //
        EditorWin.Content = this;
        EditorWin.ExtendsContentIntoTitleBar = true;
        //EditorWin.SetTitleBar(AppTitleBar);
        EditorWin.Activated += EditorWindow_Activated;
        EditorWin.Closed += EditorWindow_Closed;
        EditorWin.AppWindow.Closing += AppWindow_Closing;
        EditorWin.Title = "";

        var mainVM = App.GetService<MainViewModel>();
        ViewModel.ModalWinWidth = mainVM.ModalWinWidth;
        ViewModel.ModalWinHeight = mainVM.ModalWinHeight;
        ViewModel.ModalWinTop = mainVM.ModalWinTop;
        ViewModel.ModalWinLeft = mainVM.ModalWinLeft;

    }

    private void Page_Loaded(object sender, RoutedEventArgs e)
    {
        ContentFrame.Navigate(typeof(ZumenSearch.Views.Rent.Residentials.Bldg.BldgShellPage), ViewModel, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromBottom });

        //ContentFrame.Navigate(typeof(ZumenSearch.Views.Rent.Residentials.Unit.UnitShellPage), ViewModel, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromBottom });
    }

    private async void AppWindow_Closing(Microsoft.UI.Windowing.AppWindow sender, Microsoft.UI.Windowing.AppWindowClosingEventArgs args)
    {

    }

    public void EditorWindow_Activated(object sender, Microsoft.UI.Xaml.WindowActivatedEventArgs args)
    {
        var resource = args.WindowActivationState == WindowActivationState.Deactivated ? "WindowCaptionForegroundDisabled" : "WindowCaptionForeground";
        AppTitleBarText.Foreground = (SolidColorBrush)App.Current.Resources[resource];
    }

    public void EditorWindow_Closed(object sender, WindowEventArgs args)
    {
        EditorWin.Activated -= EditorWindow_Activated;
        EditorWin.Closed -= EditorWindow_Closed;
        EditorWin.AppWindow.Closing -= AppWindow_Closing;
    }

    private void ContentFrame_NavigationFailed(object sender, NavigationFailedEventArgs e)
    {
        throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
    }

    private void NavigationViewControl_ItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
    {
        if (args.IsSettingsInvoked == true)
        {
            
        }
        else if (args.InvokedItemContainer != null && (args.InvokedItemContainer.Tag != null))
        {
            if (args.InvokedItemContainer.Tag is string tag)
            {
                if (tag.Equals("ZumenSearch.Views.Rent.Residentials.Bldg.BldgShellPage"))
                {
                    ContentFrame.Navigate(typeof(ZumenSearch.Views.Rent.Residentials.Bldg.BldgShellPage), ViewModel, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromBottom });

                }
                else if (tag.Equals("ZumenSearch.Views.Rent.Residentials.Unit.UnitShellPage"))
                {
                    ContentFrame.Navigate(typeof(ZumenSearch.Views.Rent.Residentials.Unit.UnitShellPage), ViewModel, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromBottom });
                }
            }

        }
        else
        {
            Debug.WriteLine("NavigationViewControl_ItemInvoked: No valid item invoked. IsSettingsInvoked: " + args.IsSettingsInvoked + ", InvokedItemContainer: " + (args.InvokedItemContainer != null) + ", Tag: " + (args.InvokedItemContainer?.Tag != null));
        }
    }


    private void NavigationViewControl_Navigated(object sender, Microsoft.UI.Xaml.Navigation.NavigationEventArgs e)
    {
        if (ContentFrame.SourcePageType == typeof(Views.SettingsPage))
        {
            // SettingsItem is not part of NavView.MenuItems, and doesn't have a Tag.
            NavigationViewControl.SelectedItem = (NavigationViewItem)NavigationViewControl.SettingsItem;
        }
        else if (ContentFrame.SourcePageType != null)
        {
            var selectedItem = FindNavigationViewItemWithTag(ContentFrame.SourcePageType.FullName!);
            if (selectedItem != null)
            {
                NavigationViewControl.SelectedItem = selectedItem;
            }
            else
            {
                Debug.WriteLine("No menu item with tag matching the current page found in NavigationViewControl @Views.Rent.Residentials.ShellPage. Current page: " + ContentFrame.SourcePageType.FullName);
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

    private void NavigationViewControl_Loaded(object sender, RoutedEventArgs e)
    {

    }
}
