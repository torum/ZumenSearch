using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;
using System.Diagnostics;
using ZumenSearch.Models;
using ZumenSearch.Services.Contracts;
using ZumenSearch.Services.Extensions.AbstractFactory;
using ZumenSearch.ViewModels;

namespace ZumenSearch.Views.Rent.Residentials;

public sealed partial class ShellPage : Page
{
    public ViewModels.Rent.Residentials.MainViewModel ViewModel { get; private set; }

    public Views.Rent.Residentials.EditorWindow Win { get; private set; }

    public Frame NavigationFrame => ContentFrame;

    private readonly IModalDialogService _dlg;
    private readonly INavigationResidentialService _nav;

    public ShellPage(Views.Rent.Residentials.EditorWindow win, Models.Rent.Residentials.EntryResidentialFull entry, IAbstractFactory<Models.Rent.Residentials.EntryResidentialFull, ViewModels.Rent.Residentials.MainViewModel> vmFactory, IModalDialogService modalDialogService, INavigationResidentialService navigationResidentialService)
    {
        //Debug.WriteLine($"ShellPage {entry.Id}");

        Win = win ?? throw new ArgumentNullException(nameof(win));
        _dlg = modalDialogService;
        ViewModel = vmFactory.Create(entry);//ViewModel = vmFactory(entry);//_editorFactory.Create(new Models.Rent.Residentials.EntryResidentialFull(Guid.CreateVersion7().ToString("N"), EnumEntryStatus.New));
        ViewModel.SetEditorShell(this);

        InitializeComponent();

        _nav = navigationResidentialService;
        _nav.Initialize(ContentFrame, Win);
        ViewModel.SetEditorNavigationService(_nav);

        //BreadcrumbBar1.ItemClicked += BreadcrumbBar_ItemClicked;

        //
        Win.Content = this;
        Win.ExtendsContentIntoTitleBar = true;
        //EditorWin.SetTitleBar(AppTitleBar);
        Win.Activated += EditorWindow_Activated;
        Win.Closed += EditorWindow_Closed;
        Win.AppWindow.Closing += AppWindow_Closing;
        Win.Title = "賃貸住居用";
        
        /*
        var mainVM = App.GetService<MainViewModel>();
        
        ViewModel.ModalWinWidth = mainVM.ModalWinWidth;
        ViewModel.ModalWinHeight = mainVM.ModalWinHeight;
        ViewModel.ModalWinTop = mainVM.ModalWinTop;
        ViewModel.ModalWinLeft = mainVM.ModalWinLeft;
        */
    }

    private void Page_Loaded(object sender, RoutedEventArgs e)
    {
        ContentFrame.Navigate(typeof(ZumenSearch.Views.Rent.Residentials.Bldg.BldgShellPage), ViewModel, new EntranceNavigationTransitionInfo()); // //new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromBottom }

        //ContentFrame.Navigate(typeof(ZumenSearch.Views.Rent.Residentials.Unit.UnitShellPage), ViewModel, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromBottom });
    }

    public void EditorWindow_Activated(object sender, Microsoft.UI.Xaml.WindowActivatedEventArgs args)
    {
        var resource = args.WindowActivationState == WindowActivationState.Deactivated ? "WindowCaptionForegroundDisabled" : "WindowCaptionForeground";
        AppTitleBarText.Foreground = (SolidColorBrush)App.Current.Resources[resource];
    }

    private async void AppWindow_Closing(Microsoft.UI.Windowing.AppWindow sender, Microsoft.UI.Windowing.AppWindowClosingEventArgs args)
    {
        if (ViewModel == null)
        {
            return;
        }

        if (ViewModel.Bldg.IsDirty || ViewModel.Unit.IsDirty)
        {
            args.Cancel = true; // needs Cancel = true here in order to show dialog.

            // show ConfirmationDialog
            var result = await _dlg.ShowEditorCloseConfirmationDialog(Win);

            if (result == ContentDialogResult.Primary)
            {
                if (ViewModel.Bldg.IsDirty)
                {
                    ViewModel.Bldg.Save();
                }

                if (ViewModel.Unit.IsDirty)
                {
                    ViewModel.Unit.Save();
                }

                if ((ViewModel.Bldg.IsDirty == false) && (ViewModel.Unit.IsDirty == false))
                {
                    Win.Close();
                }
            }
            else if (result == ContentDialogResult.Secondary)
            {
                // Discard change and close.
                //ViewModel.Bldg.DiscardUnsavedFiles();
                //ViewModel.Unit.DiscardUnsavedFiles();
                ViewModel.Bldg.DiscardChanges();
                ViewModel.Unit.DiscardChanges();

                Win.Close();
            }
            else if (result == ContentDialogResult.None)
            {
                // Cancel.
            }
        }
    }

    public void EditorWindow_Closed(object sender, WindowEventArgs args)
    {
        if (sender is not EditorWindow ewin)
        {
            return;
        }

        ewin.Activated -= EditorWindow_Activated;
        ewin.Closed -= EditorWindow_Closed;
        ewin.AppWindow.Closing -= AppWindow_Closing;

        var mainVM = App.GetService<MainViewModel>();
        // Save window size and position.
        var appWindow = ewin.AppWindow;
        if (appWindow != null)
        {
            if (appWindow.Presenter is OverlappedPresenter)
            {
                mainVM.EditorWinHeight = (int)appWindow.Size.Height;
                mainVM.EditorWinWidth = (int)appWindow.Size.Width;
                mainVM.EditorWinTop = (int)appWindow.Position.Y;
                mainVM.EditorWinLeft = (int)appWindow.Position.X;
            }
        }

        if (!ewin.IsAutoClose)
        {
            mainVM.EditorList.Remove(ewin);
        }
    }

    public void OnEventTitleChanged(EventArgs args)
    {
        Win.Title = ViewModel?.WindowTitle ?? "賃貸住居用";
    }

    private void ContentFrame_NavigationFailed(object sender, NavigationFailedEventArgs e)
    {
        throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
    }

    /*
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
    */


    private void ContentFrame_Navigated(object sender, Microsoft.UI.Xaml.Navigation.NavigationEventArgs e)
    {
        /*
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
        */
    }
}
