using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;
using Windows.Storage.Pickers;
using ZumenSearch.Models;
using ZumenSearch.Services;
using ZumenSearch.ViewModels;
using ZumenSearch.Views.Rent.Residentials.Editor.Modal;

namespace ZumenSearch.Views.Rent.Residentials.Editor;

public sealed partial class EditorShell : Page
{
    private readonly MainViewModel MainVM = App.GetService<MainViewModel>();

    public ViewModels.Rent.Residentials.ResidentialsViewModel ViewModel;

    public Views.Rent.Residentials.Editor.EditorWindow EditorWin { get; private set; }

    // List of ValueTuple holding the Navigation Tag and the relative Navigation Page
    private readonly List<(string Tag, string Label, Type? Page)> _pages =
    [
        ("building", "建物", null),
        ("summary", "基本", typeof(Views.Rent.Residentials.Editor.BasicPage)),
        //("structure", "", typeof(Views.Rent.Residentials.Editor.StructurePage)),
        ("location", "所在地", typeof(Views.Rent.Residentials.Editor.LocationPage)),
        ("transportation", "交通", typeof(Views.Rent.Residentials.Editor.TransportationPage)),
        ("appliance", "設備", typeof(Views.Rent.Residentials.Editor.AppliancePage)),
        ("pictures", "写真", typeof(Views.Rent.Residentials.Editor.PictureListPage)),
        ("units", "部屋", typeof(Views.Rent.Residentials.Editor.UnitListPage)),
        ("zumen", "図面", typeof(Views.Rent.Residentials.Editor.ZumenListPage)),
        ("kasinusi", "貸主", typeof(Views.Rent.Residentials.Editor.KasinusiPage)),
        ("gyousya", "宅建業者", typeof(Views.Rent.Residentials.Editor.GyousyaPage)),
        //("memo", "備考", typeof(Views.Rent.Residentials.Editor.MemoPage)),
    ];

    private readonly IModalDialogService _dlg;

    public EditorShell(Views.Rent.Residentials.Editor.EditorWindow win, ViewModels.Rent.Residentials.ResidentialsViewModel vm, IModalDialogService modalDialog)
    {
        EditorWin = win ?? throw new ArgumentNullException(nameof(win));
        ViewModel = vm ?? throw new ArgumentNullException(nameof(vm));
        ViewModel.SetEditorWin(win);// Must set Editor Winodw to VM.
        _dlg = modalDialog;

        InitializeComponent();

        BreadcrumbBar1.ItemClicked += BreadcrumbBar_ItemClicked;

        //
        EditorWin.Content = this;
        EditorWin.ExtendsContentIntoTitleBar = true;
        EditorWin.SetTitleBar(AppTitleBar);
        EditorWin.Activated += EditorWindow_Activated;
        EditorWin.Closed += EditorWindow_Closed;
        EditorWin.AppWindow.Closing += AppWindow_Closing;
        EditorWin.Title = "";

        ViewModel.ModalWinWidth = MainVM.ModalWinWidth;
        ViewModel.ModalWinHeight = MainVM.ModalWinHeight;
        ViewModel.ModalWinTop = MainVM.ModalWinTop;
        ViewModel.ModalWinLeft = MainVM.ModalWinLeft;

        // subscribe to ViewModel events
        ViewModel.EventBackToSummary += (sender, arg) => OnEventBackToSummary();
        ViewModel.EventEditLocation += (sender, arg) => OnEventEditLocation();
        ViewModel.EventEditTransportation += (sender, arg) => OnEventEditTransportation();
        ViewModel.EventEditAppliance += (sender, arg) => OnEventEditAppliance();
        ViewModel.EventEditPictures += (sender, arg) => OnEventEditPictures();
        ViewModel.EventEditUnits += (sender, arg) => OnEventEditUnits();
        //
        ViewModel.EventIsUnitOwnership += (sender, arg) => OnEventIsUnitOwnership(arg);
    }

    private void BreadcrumbBar_ItemClicked(BreadcrumbBar sender, BreadcrumbBarItemClickedEventArgs args)
    {
        if (args.Index == 0)
        {
            if (ContentFrame.Navigate(typeof(Views.Rent.Residentials.Editor.BasicPage), ViewModel, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromBottom }))
            {
                BreadcrumbBar1.ItemsSource = new ObservableCollection<Breadcrumb>{
                    new() { Name = "建物", Page = typeof(Views.Rent.Residentials.Editor.BasicPage).FullName!},
                    new() { Name = "基本", Page = typeof(Views.Rent.Residentials.Editor.BasicPage).FullName!},
                };

                NavView.SelectedItem = NavView.MenuItems.OfType<NavigationViewItem>().Where(n => n.Tag.Equals("summary")).First();
            }
        }
    }

    private async void AppWindow_Closing(Microsoft.UI.Windowing.AppWindow sender, Microsoft.UI.Windowing.AppWindowClosingEventArgs args)
    {
        if (ViewModel == null)
        {
            return;
        }

        if (ViewModel.IsEntryDirty)
        {
            args.Cancel = true; // needs Cancel = true here in order to show dialog.

            // show ConfirmationDialog
            var result = await _dlg.ShowEditorCloseConfirmationDialog(EditorWin);

            if (result == ContentDialogResult.Primary)
            {
                // Save and close.

                await ViewModel.Save();
                if (ViewModel.IsEntryDirty == false)
                {
                    EditorWin.Close();
                }
            }
            else if (result == ContentDialogResult.Secondary)
            {
                // Discard change and close.

                ViewModel.IsEntryDirty = false;
                EditorWin.Close();
            }
            else if (result == ContentDialogResult.None)
            {
                // Cancel.

            }
        }
    }

    public void EditorWindow_Activated(object sender, Microsoft.UI.Xaml.WindowActivatedEventArgs args)
    {
        var resource = args.WindowActivationState == WindowActivationState.Deactivated ? "WindowCaptionForegroundDisabled" : "WindowCaptionForeground";
        AppTitleBarText.Foreground = (SolidColorBrush)App.Current.Resources[resource];

        //AppTitleBarIcon.Opacity = args.WindowActivationState == WindowActivationState.Deactivated ? 0.4 : 0.7;
        //AppMenuBar.Opacity = args.WindowActivationState == WindowActivationState.Deactivated ? 0.4 : 0.7;

        AppTitleBarIcon.Opacity = args.WindowActivationState == WindowActivationState.Deactivated ? 0.4 : 0.8;
        //AppMenuBar.Opacity = args.WindowActivationState == WindowActivationState.Deactivated ? 0.4 : 0.8;
    }

    public void EditorWindow_Closed(object sender, WindowEventArgs args)
    {
        if (sender is EditorWindow ewin)
        {
            // Save window size and position.
            var appWindow = ewin.AppWindow;
            if (appWindow != null)
            {
                if (appWindow.Presenter is OverlappedPresenter)
                {
                    MainVM.EditorWinHeight = (int)appWindow.Size.Height;
                    MainVM.EditorWinWidth = (int)appWindow.Size.Width;
                    MainVM.EditorWinTop = (int)appWindow.Position.Y;
                    MainVM.EditorWinLeft = (int)appWindow.Position.X;

                    MainVM.ModalWinHeight = ViewModel.ModalWinHeight;
                    MainVM.ModalWinWidth = ViewModel.ModalWinWidth;
                    MainVM.ModalWinTop = ViewModel.ModalWinTop;
                    MainVM.ModalWinLeft = ViewModel.ModalWinLeft;
                }
            }
        }
    }

    private void ContentFrame_NavigationFailed(object sender, NavigationFailedEventArgs e)
    {
        throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
    }

    private void NavView_Loaded(object sender, RoutedEventArgs e)
    {
        // Since we use ItemInvoked, we set selecteditem manually
        //NavView.SelectedItem = NavView.MenuItems.OfType<NavigationViewItem>().First();
        // The same with above but more precise.
        //NavView.SelectedItem = NavView.MenuItems.OfType<NavigationViewItem>().Where(n => n.Tag.Equals("summary")).First();

        /*
        // This is for hierarchical menu.
        var firstMenuItem = NavView.MenuItems.OfType<NavigationViewItem>().First();
        if (firstMenuItem != null)
        {
            var childItem = firstMenuItem.MenuItems.OfType<NavigationViewItem>().Where(n => n.Tag.Equals("summary"));
            if (childItem != null)
            {
                childItem.First().IsSelected = true;
                navigationViewSelectedItem = childItem.First();
            }
        }
        */

        // Pass Frame when navigate.  //, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromLeft } //, new SuppressNavigationTransitionInfo() //new EntranceNavigationTransitionInfo()
        if (ContentFrame.Navigate(typeof(ZumenSearch.Views.Rent.Residentials.Editor.BasicPage), ViewModel, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromBottom }))
        {
            BreadcrumbBar1.ItemsSource = new ObservableCollection<Breadcrumb>{
                new() { Name = "建物", Page = typeof(Views.Rent.Residentials.Editor.BasicPage).FullName!},
                new() { Name = "基本", Page = typeof(Views.Rent.Residentials.Editor.BasicPage).FullName!},
            };

            NavView.SelectedItem = NavView.MenuItems.OfType<NavigationViewItem>().Where(n => n.Tag.Equals("summary")).First();
        }
    }

    private void NavView_ItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
    {
        if (_pages is null)
        {
            return;
        }

        if (args.IsSettingsInvoked == true)
        {
            // Do nothing. 
        }
        else if (args.InvokedItemContainer != null && (args.InvokedItemContainer.Tag != null))
        {
            if (args.InvokedItemContainer.Tag is not string tag || string.IsNullOrWhiteSpace(tag))
            {
                Debug.WriteLine("NavView_ItemInvoked: Invalid tag or null.");
                return;
            }

            var item = _pages.FirstOrDefault(p => p.Tag.Equals(args.InvokedItemContainer.Tag.ToString()));

            if (item.Page is null)
            {
                Debug.WriteLine("NavView_ItemInvoked: Page is null for tag " + tag);
                return;
            }

            if (ContentFrame.Navigate(item.Page, ViewModel, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromBottom }))
            {
                BreadcrumbBar1.ItemsSource = new ObservableCollection<Breadcrumb>{
                    new() { Name = "建物", Page = typeof(Views.Rent.Residentials.Editor.BasicPage).FullName!},
                    new() { Name = item.Label, Page = item.Page.FullName!},
                };
            }
            //, args.RecommendedNavigationTransitionInfo
        }
    }

    // MainViewModel calls this method to set the entry.
    public void SetEntryToEntryViewModel(Models.Rent.Residentials.EntryResidentialFull entry)
    {
        if (entry is null)
        {
            return;
        }
        //ViewModel.Entry = entry ?? throw new ArgumentNullException(nameof(entry));

        ViewModel.SetEntry(entry);
        EditorWin.Id = entry.Id; // Set the EditorWin Id to the Entry Id. 

        // Just in case.
        ViewModel.SetEditorWin(EditorWin);

        // TODO: do this from VM.
        EditorWin.Title = $"物件情報の編集（{entry.Name}）";
    }

    public void OnEventIsUnitOwnership(bool arg)
    {
        if (arg)
        {
            // hide the owner and zumen menu items.
            NavigationViewItemZumen.Visibility = Visibility.Collapsed;
            NavigationViewItemKasinusi.Visibility = Visibility.Collapsed;
            NavigationViewItemGyousya.Visibility = Visibility.Collapsed;
        }
        else
        {
            // show the owner and zumen menu items.
            NavigationViewItemZumen.Visibility = Visibility.Visible;
            NavigationViewItemKasinusi.Visibility = Visibility.Visible;
            NavigationViewItemGyousya.Visibility = Visibility.Visible;
        }
    }

    public void OnEventBackToSummary()
    {
        ContentFrame.Navigate(typeof(Views.Rent.Residentials.Editor.BasicPage), ViewModel, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromLeft });
    }

    public void OnEventEditLocation()
    {
        ContentFrame.Navigate(typeof(Views.Rent.Residentials.Editor.LocationPage), ViewModel, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromRight });
    }

    public void OnEventEditTransportation()
    {
        ContentFrame.Navigate(typeof(Views.Rent.Residentials.Editor.TransportationPage), ViewModel, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromRight });
    }

    public void OnEventEditAppliance()
    {
        ContentFrame.Navigate(typeof(Views.Rent.Residentials.Editor.AppliancePage), ViewModel, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromRight });
    }

    public void OnEventEditPictures()
    {
        ContentFrame.Navigate(typeof(Views.Rent.Residentials.Editor.PictureListPage), ViewModel, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromRight });
    }

    public void OnEventEditUnits()
    {
        ContentFrame.Navigate(typeof(Views.Rent.Residentials.Editor.UnitListPage), ViewModel, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromRight });
    }

    #region == TEMP code for modal window(for setting an owner) ==

#pragma warning disable IDE0079
#pragma warning disable SYSLIB1054

    [DllImport("User32.dll", SetLastError = true, CharSet = CharSet.Unicode)]

    internal static extern bool EnableWindow(IntPtr hWnd, bool bEnable);

    internal const int GWL_HWNDPARENT = (-8);

    internal static IntPtr SetWindowLong(IntPtr hWnd, int nIndex, IntPtr dwNewLong)
    {
        if (IntPtr.Size == 4)
        {
            return SetWindowLongPtr32(hWnd, nIndex, dwNewLong);
        }
        return SetWindowLongPtr64(hWnd, nIndex, dwNewLong);
    }

    // Import the Windows API function SetWindowLong for modifying window properties on 32-bit systems.
    [DllImport("User32.dll", CharSet = CharSet.Auto, EntryPoint = "SetWindowLong")]
    internal static extern IntPtr SetWindowLongPtr32(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

    // Import the Windows API function SetWindowLongPtr for modifying window properties on 64-bit systems.
    [DllImport("User32.dll", CharSet = CharSet.Auto, EntryPoint = "SetWindowLongPtr")]
    internal static extern IntPtr SetWindowLongPtr64(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

#pragma warning restore SYSLIB1054
#pragma warning restore IDE0079

    #endregion


}
