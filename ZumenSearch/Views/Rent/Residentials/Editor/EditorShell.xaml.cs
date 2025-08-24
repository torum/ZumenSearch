
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.WinUI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Markup;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.System;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using ZumenSearch.Helpers;
using ZumenSearch.Models;
using ZumenSearch.Services;
using ZumenSearch.ViewModels;
using ZumenSearch.Views;

namespace ZumenSearch.Views.Rent.Residentials.Editor;

public sealed partial class EditorShell : Page
{
    public ViewModels.Rent.Residentials.Editor.EditorViewModel ViewModel
    {
        get;
    }

    public Views.Rent.Residentials.Editor.EditorWindow EditorWin { get; private set; }

    private NavigationViewItem? navigationViewSelectedItem;

    // List of ValueTuple holding the Navigation Tag and the relative Navigation Page
    private readonly List<(string Tag, Type? Page)> _pages =
    [
        ("building", null),
        ("summary", typeof(Views.Rent.Residentials.Editor.SummaryPage)),
        //("structure", typeof(Views.Rent.Residentials.Editor.StructurePage)),
        ("location", typeof(Views.Rent.Residentials.Editor.LocationPage)),
        ("transportation", typeof(Views.Rent.Residentials.Editor.TransportationPage)),
        ("appliance", typeof(Views.Rent.Residentials.Editor.AppliancePage)),
        ("pictures", typeof(Views.Rent.Residentials.Editor.PictureListPage)),
        ("units", typeof(Views.Rent.Residentials.Editor.UnitListPage)),
        //("unitpictures", typeof(Views.Rent.Residentials.Editor.UnitPictureListPage)),
        ("zumen", typeof(Views.Rent.Residentials.Editor.ZumenListPage)),
        ("kasinusi", typeof(Views.Rent.Residentials.Editor.KasinusiPage)),
        ("gyousya", typeof(Views.Rent.Residentials.Editor.GyousyaPage)),
        ("memo", typeof(Views.Rent.Residentials.Editor.MemoPage)),
    ];

    //private readonly UISettings settings = new();

    public EditorShell(Views.Rent.Residentials.Editor.EditorWindow win, ViewModels.Rent.Residentials.Editor.EditorViewModel vm)
    {
        EditorWin = win ?? throw new ArgumentNullException(nameof(win));
        ViewModel = vm ?? throw new ArgumentNullException(nameof(vm));

        this.InitializeComponent();

        //
        EditorWin.Content = this;
        EditorWin.ExtendsContentIntoTitleBar = true;
        EditorWin.SetTitleBar(AppTitleBar);
        EditorWin.Activated += EditorWindow_Activated;
        EditorWin.Closed += EditorWindow_Closed;
        EditorWin.AppWindow.Closing += AppWindow_Closing;
        EditorWin.Title = "物件情報の編集（新規）"; 
        //
        //ViewModel.SetEditorWindow(EditorWin);

        // subscribe to ViewModel events
        ViewModel.EventBackToSummary += (sender, arg) => OnEventBackToSummary();
        ViewModel.EventEditLocation += (sender, arg) => OnEventEditLocation();
        ViewModel.EventEditTransportation += (sender, arg) => OnEventEditTransportation();
        ViewModel.EventEditAppliance += (sender, arg) => OnEventEditAppliance();
        ViewModel.EventEditMemo += (sender, arg) => OnEventEditMemo();
        //
        ViewModel.EventAddNewUnit += (sender, arg) => OnEventAddNewUnit();
        //
        ViewModel.EventAddNewBuildingPictures += (sender, arg) => OnEventAddNewBuildingPictures();
        
        //
        ViewModel.EventIsUnitOwnership += (sender, arg) => OnEventIsUnitOwnership(arg);
    }

    private void AppWindow_Closing(Microsoft.UI.Windowing.AppWindow sender, Microsoft.UI.Windowing.AppWindowClosingEventArgs args)
    {
        if (ViewModel == null)
            return;

        if (ViewModel.IsEntryDirty)
        {
            // TODO: Prompt user to save changes before closing.
            Debug.WriteLine("AppWindow_Closing: Entry is dirty, prompting save dialog(TODO).");
            args.Cancel = true; // Cancel the closing operation

            // TEMP: For now, just reset the dirty state.
            ViewModel.IsEntryDirty = false; 
        }
    }

    public void EditorWindow_Activated(object sender, Microsoft.UI.Xaml.WindowActivatedEventArgs args)
    {
        var resource = args.WindowActivationState == WindowActivationState.Deactivated ? "WindowCaptionForegroundDisabled" : "WindowCaptionForeground";
        AppTitleBarText.Foreground = (SolidColorBrush)App.Current.Resources[resource];

        //AppTitleBarIcon.Opacity = args.WindowActivationState == WindowActivationState.Deactivated ? 0.4 : 0.7;
        //AppMenuBar.Opacity = args.WindowActivationState == WindowActivationState.Deactivated ? 0.4 : 0.7;

        AppTitleBarIcon.Opacity = args.WindowActivationState == WindowActivationState.Deactivated ? 0.4 : 0.8;
        AppMenuBar.Opacity = args.WindowActivationState == WindowActivationState.Deactivated ? 0.4 : 0.8;
    }

    public void EditorWindow_Closed(object sender, WindowEventArgs args)
    {
        //Window.BringToFront();
        /*
        if (ViewModel.Closing())
        {
            // https://github.com/microsoft/microsoft-ui-xaml/issues/7336
            // already done this in viewmodel.
            //WebViewRichEdit.Close();
            //WebViewSourceEdit.Close();
            //WebViewPreviewBrowser.Close();
        }
        else
        {
            // Cancel
            //args.Handled = true;
        }
        */
        //settings.ColorValuesChanged -= Settings_ColorValuesChanged;
        //WeakReferenceMessenger.Default.UnregisterAll(this);
    }

    private void ContentFrame_NavigationFailed(object sender, NavigationFailedEventArgs e)
    {
        throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
    }

    private void NavView_Loaded(object sender, RoutedEventArgs e)
    {
        // Since we use ItemInvoked, we set selecteditem manually
        //NavView.SelectedItem = NavView.MenuItems.OfType<NavigationViewItem>().First();

        //NavView.SelectedItem = NavView.MenuItems.OfType<NavigationViewItem>().First().FindChildren().Where(n => n.Tag.Equals("summary"));
        var firstMenuItem = NavView.MenuItems.OfType<NavigationViewItem>().First();
        if (firstMenuItem != null)
        {
            var childItem = firstMenuItem.MenuItems.OfType<NavigationViewItem>().Where(n => n.Tag.Equals("summary"));
            if (childItem != null)
            {
                childItem.First().IsSelected = true;
                navigationViewSelectedItem = childItem.First();
            }
            else { Debug.WriteLine("No child menu item with tag 'summary' found in NavView."); }
        }
        else
        {
            Debug.WriteLine("No first menu item found in NavView.");
        }

        // Pass Frame when navigate.  //, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromLeft } //, new SuppressNavigationTransitionInfo() //new EntranceNavigationTransitionInfo()
        ContentFrame.Navigate(typeof(ZumenSearch.Views.Rent.Residentials.Editor.SummaryPage), ViewModel, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromLeft });


        // Listen to the window directly so the app responds to accelerator keys regardless of which element has focus.
        //Window.Current.CoreWindow.Dispatcher.AcceleratorKeyActivated +=  CoreDispatcher_AcceleratorKeyActivated;

        //Window.Current.CoreWindow.PointerPressed += CoreWindow_PointerPressed;

        //SystemNavigationManager.GetForCurrentView().BackRequested += System_BackRequested;

    }

    private void NavView_ItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
    {
        if (_pages is null)
            return;

        if (args.IsSettingsInvoked == true)
        {
            //NavView_Navigate("settings", args.RecommendedNavigationTransitionInfo);
        }
        else if (args.InvokedItemContainer != null && (args.InvokedItemContainer.Tag != null))
        {
            if (args.InvokedItemContainer.Tag is not string tag || string.IsNullOrWhiteSpace(tag))
            {
                Debug.WriteLine("NavView_ItemInvoked: Invalid tag or null.");
                //sender.SelectedItem = navigationViewSelectedItem; 
                return;
            }

            var item = _pages.FirstOrDefault(p => p.Tag.Equals(args.InvokedItemContainer.Tag.ToString()));

            if (item.Page is null)
            {
                //Debug.WriteLine("NavView_ItemInvoked: Page is null for tag " + tag);
                //sender.SelectedItem = navigationViewSelectedItem;

                return;
            }

            navigationViewSelectedItem = sender.SelectedItem as NavigationViewItem;

            // Pass Frame when navigate.
            ContentFrame.Navigate(item.Page, ViewModel, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromLeft });//, args.RecommendedNavigationTransitionInfo
        }
    }

    // MainViewModel calls this method to set the entry.
    public void SetEntryToEntryViewModel(Models.Rent.Residentials.EntryResidentialFull entry)
    {
        //ViewModel.Entry = entry ?? throw new ArgumentNullException(nameof(entry));
        ViewModel.SetEntry(entry ?? throw new ArgumentNullException(nameof(entry)));
        EditorWin.Id = entry.Id; // Set the EditorWin Id to the Entry Id. 
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
        ContentFrame.Navigate(typeof(Views.Rent.Residentials.Editor.SummaryPage), ViewModel, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromLeft });
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

    public void OnEventEditMemo()
    {
        ContentFrame.Navigate(typeof(Views.Rent.Residentials.Editor.MemoPage), ViewModel, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromRight });
    }

    public async void OnEventAddNewBuildingPictures()
    {
        string destDirectory = System.IO.Path.Combine(System.IO.Path.Combine(System.IO.Path.Combine(App.AppDataPictureFolder, "Rent"),"Residential_Building"), Guid.NewGuid().ToString("N"));

        if (!Directory.Exists(destDirectory))
        {
            Directory.CreateDirectory(destDirectory);
        }

        var openPicker = new Windows.Storage.Pickers.FileOpenPicker();

        var window = EditorWin;//App.MainWindow;
        var hWnd = WinRT.Interop.WindowNative.GetWindowHandle(window);
        WinRT.Interop.InitializeWithWindow.Initialize(openPicker, hWnd);

        // Set options for your file picker
        openPicker.ViewMode = PickerViewMode.List;
        openPicker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
        openPicker.FileTypeFilter.Add(".jpg");
        openPicker.FileTypeFilter.Add(".jpeg");
        openPicker.FileTypeFilter.Add(".png");
        openPicker.FileTypeFilter.Add(".gif");

        // Open the picker for the user to pick a file
        IReadOnlyList<StorageFile> files = await openPicker.PickMultipleFilesAsync();
        if (files.Count > 0)
        {
            StringBuilder output = new("");
            List<string> list = [];
            foreach (StorageFile file in files)
            {
                output.Append(file.Path + "\n");
                list.Add(file.Path);

                using FileStream sourceStream = File.Open(file.Path, FileMode.Open);
                using FileStream destinationStream = File.Create(Path.Combine(destDirectory, file.Name));
                await sourceStream.CopyToAsync(destinationStream);
            }

            Debug.WriteLine(output.ToString());
            ViewModel.SetNewBuildingPictures(list);
        }
        else
        {
            Debug.WriteLine("Operation cancelled.");
        }
    }

    public void OnEventAddNewUnit()
    {
        if (this.EditorWin == null)
        {
            // EditorWin should be initialized in the EditorShell constructor.
            Debug.WriteLine("EditorWin should be initialized in the EditorShell constructor.");
            return;
        }

        var dlgService = App.GetService<IModalDialogService>();
        dlgService.ShowUnitDialog(ViewModel, EditorWin);

        /*

        Views.Rent.Residentials.Editor.Modal.ModalWindow dialogWin = new();
        dialogWin.Content = new Views.Rent.Residentials.Editor.Modal.ModalShell(dialogWin);

        //TODO: stupid WinUI3... 
        // https://github.com/microsoft/microsoft-ui-xaml/issues/10396
        // https://github.com/microsoft/WindowsAppSDK/discussions/3680

        IntPtr hWndDialog = WinRT.Interop.WindowNative.GetWindowHandle(dialogWin);
        //Microsoft.UI.WindowId windowId1 = Microsoft.UI.Win32Interop.GetWindowIdFromWindow(hWnd1);
        //Microsoft.UI.Windowing.AppWindow appWindow = Microsoft.UI.Windowing.AppWindow.GetFromWindowId(windowId1);
        //Microsoft.UI.Windowing.OverlappedPresenter presenter = appWindow.Presenter as Microsoft.UI.Windowing.OverlappedPresenter;
        IntPtr hWndEditor = WinRT.Interop.WindowNative.GetWindowHandle(this.EditorWin);
        SetWindowLong(hWndDialog, GWL_HWNDPARENT, hWndEditor);

        Microsoft.UI.Windowing.AppWindow? appWindow = dialogWin.AppWindow;
        if (appWindow != null)
        {
            if (appWindow.Presenter is OverlappedPresenter presenter)
            {
                presenter.IsModal = true;

                // Don't use EnableWindow. This causes all sorts of problems. (as of WinAppSDK 1.7.25)
                //EnableWindow(hWndEditor, false);

                dialogWin.Closed += (sender, e) =>
                {
                    // Don't use EnableWindow. This causes all sorts of problems. (as of WinAppSDK 1.7.25)
                    //EnableWindow(hWndEditor, true);

                    // Activate the editor window again.
                    this.EditorWin.Activate();
                };

                // Close the dialog when the editor window is closed.
                this.EditorWin.Closed += (sender, e) =>
                {
                    dialogWin.Close();
                };

                //Task.Delay(30).ConfigureAwait(false);

                // Same as EnableWindow. This causes all sorts of problems. (as of WinAppSDK 1.7.25)
                //appWindow.Show(true);

                dialogWin.Activate();
                //dialogWin.Show();
            }
        }
        */
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
