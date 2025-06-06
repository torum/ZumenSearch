
using ZumenSearch.ViewModels;
using ZumenSearch.Helpers;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Markup;
using Microsoft.UI.Xaml.Media;
using System.Diagnostics;
using Windows.UI.ViewManagement;
using CommunityToolkit.Mvvm.Messaging;
using ZumenSearch.Models;
using System.Collections.ObjectModel;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.UI.Xaml.Media.Animation;
using Windows.System;
using Windows.UI.Core;
using System.Collections.Generic;
using System;
using ZumenSearch.Views;
using System.Linq;
using CommunityToolkit.WinUI;

namespace ZumenSearch.Views.Rent.Residentials.Editor;

public sealed partial class EditorShell : Page
{
    public ViewModels.Rent.Residentials.Editor.EditorViewModel? ViewModel
    {
        get;
    }

    public Views.Rent.Residentials.Editor.EditorWindow? EditorWin { get; private set; }

    // For uses of Navigation in other pages.
    public Frame NavFrame
    {
        get => ContentFrame;
    }

    private NavigationViewItem? navigationViewSelectedItem;

    // List of ValueTuple holding the Navigation Tag and the relative Navigation Page
    private readonly List<(string Tag, Type? Page)> _pages = new()
    {
        ("building", null),
        ("summary", typeof(Views.Rent.Residentials.Editor.SummaryPage)),
        ("structure", typeof(Views.Rent.Residentials.Editor.StructurePage)),
        ("location", typeof(Views.Rent.Residentials.Editor.LocationPage)),
        ("transportation", typeof(Views.Rent.Residentials.Editor.TransportationPage)),
        ("appliance", typeof(Views.Rent.Residentials.Editor.AppliancePage)),
        ("pictures", typeof(Views.Rent.Residentials.Editor.PictureListPage)),
        ("units", typeof(Views.Rent.Residentials.Editor.UnitListPage)),
        ("unitpictures", typeof(Views.Rent.Residentials.Editor.UnitPictureListPage)),
        ("zumen", typeof(Views.Rent.Residentials.Editor.ZumenListPage)),
        ("kasinusi", typeof(Views.Rent.Residentials.Editor.KasinusiPage)),
        ("gyousya", typeof(Views.Rent.Residentials.Editor.GyousyaPage)),
    };

    //private readonly UISettings settings = new();

    public EditorShell(Views.Rent.Residentials.Editor.EditorWindow win, ViewModels.Rent.Residentials.Editor.EditorViewModel vm)
    {
        EditorWin = win ?? throw new ArgumentNullException(nameof(win));
        ViewModel = vm ?? throw new ArgumentNullException(nameof(vm));

        this.InitializeComponent();

        EditorWin.Content = this;
        EditorWin.ExtendsContentIntoTitleBar = true;
        EditorWin.SetTitleBar(AppTitleBar);
        EditorWin.Activated += EditorWindow_Activated;
        EditorWin.Closed += EditorWindow_Closed;
        EditorWin.AppWindow.Closing += AppWindow_Closing;

        ViewModel.EditorWin = EditorWin;


        //_entry = new Models.Rent.RentResidential(Guid.NewGuid().ToString());

        /*
        Window = new ResidentialsEditorWindow();

        ViewModel = App.GetService<ResidentialsEditorShellViewModel>();

        Window.ExtendsContentIntoTitleBar = true;

        Window.Content = this;
        */
        //InitializeComponent();

        /*
        BreadcrumbBar1.ItemsSource = new ObservableCollection<Folder>{
            new Folder { Name = "物件情報の編集", Page = typeof(ResidentialsSearchResultViewModel).FullName! },
        };

        // AppTitleBar needs InitializeComponent() beforehand.
        //Window.SetTitleBar(AppTitleBar);

        Window.Activated += EditorWindow_Activated;
        Window.Closed += EditorWindow_Closed;

        if (App.MainWindow.Content is FrameworkElement rootElement)
        {
            if (RequestedTheme != rootElement.RequestedTheme)
            {
                OnThemeChanged(rootElement.RequestedTheme);
            }
        }
        else
        {
            // not sure about this.
            if (App.Current.RequestedTheme == ApplicationTheme.Dark)
            {
                if (RequestedTheme != ElementTheme.Dark)
                {
                    OnThemeChanged(ElementTheme.Dark);
                }
            }
            else if (App.Current.RequestedTheme == ApplicationTheme.Light)
            {
                if (RequestedTheme != ElementTheme.Light) { OnThemeChanged(ElementTheme.Light); }
            }
            else
            {
                if (RequestedTheme != ElementTheme.Default) { OnThemeChanged(ElementTheme.Default); }
            }
        }

        //
        settings.ColorValuesChanged += Settings_ColorValuesChanged; // cannot use FrameworkElement.ActualThemeChanged event

        // Catch message from Settings page.
        WeakReferenceMessenger.Default.Register<ThemeChangedMessage>(this, (r, m) =>
        {
            var thm = m.Value;
            if (!string.IsNullOrEmpty(thm))
            {
                ElementTheme _theme = ElementTheme.Default;
                if (thm == "dark")
                {
                    _theme = ElementTheme.Dark;
                }
                else if (thm == "light")
                {
                    _theme = ElementTheme.Light;
                }
                else if (thm == "default")
                {
                    if (App.Current.RequestedTheme == ApplicationTheme.Dark)
                    {
                        _theme = ElementTheme.Dark;
                    }
                    else if (App.Current.RequestedTheme == ApplicationTheme.Light)
                    {
                        _theme = ElementTheme.Light;
                    }
                }

                OnThemeChanged(_theme);
            }
        });
        */
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

        //AppTitleBarText.Foreground = (SolidColorBrush)App.Current.Resources[resource];
        //AppTitleBarIcon.Opacity = args.WindowActivationState == WindowActivationState.Deactivated ? 0.4 : 0.7;
        //AppMenuBar.Opacity = args.WindowActivationState == WindowActivationState.Deactivated ? 0.4 : 0.7;
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
        ContentFrame.Navigate(typeof(ZumenSearch.Views.Rent.Residentials.Editor.SummaryPage), this, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromLeft });


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
                sender.SelectedItem = navigationViewSelectedItem; 
                return;
            }

            var item = _pages.FirstOrDefault(p => p.Tag.Equals(args.InvokedItemContainer.Tag.ToString()));

            if (item.Page is null)
            {
                //Debug.WriteLine("NavView_ItemInvoked: Page is null for tag " + tag);
                sender.SelectedItem = navigationViewSelectedItem;

                return;
            }

            navigationViewSelectedItem = sender.SelectedItem as NavigationViewItem;

            // Pass Frame when navigate.
            ContentFrame.Navigate(item.Page, this, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromLeft });//, args.RecommendedNavigationTransitionInfo
        }
    }

    private void NavView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
    {
        /*
        if (args.IsSettingsSelected == true)
        {
            //NavView_Navigate("settings", args.RecommendedNavigationTransitionInfo);
        }
        else if (args.SelectedItemContainer != null && args.SelectedItemContainer.Tag != null)
        {
            var navItemTag = args.SelectedItemContainer.Tag.ToString();

            if (navItemTag != null)
            {
                //NavView_Navigate(navItemTag, args.RecommendedNavigationTransitionInfo);

                Debug.WriteLine("■■■SelectionChanged: "+ args.SelectedItemContainer.Tag.ToString());

                var item = _pages.First(p => p.Tag.Equals(args.SelectedItemContainer.Tag.ToString()));

                var _page = item.Page;
                if (_page is null)
                    return;

                var preNavPageType = ContentFrame.CurrentSourcePageType;
                if (preNavPageType is null)
                    return;
                if (Type.Equals(preNavPageType, _page))
                    return;

                ContentFrame.Navigate(_page, null, args.RecommendedNavigationTransitionInfo);
            }
        }
        */
    }

    private void NavView_Navigate(string navItemTag, NavigationTransitionInfo transitionInfo)
    {
        /*
        Type? _page = null;
        if (navItemTag == "settings")
        {
            //_page = typeof(SettingsPage);
        }
        else
        {
            var item = _pages.FirstOrDefault(p => p.Tag.Equals(navItemTag));
            _page = item.Page;
        }

        // Get the page type before navigation so you can prevent duplicate
        // entries in the backstack.
        var preNavPageType = ContentFrame.CurrentSourcePageType;

        // Only navigate if the selected page isn't currently loaded.
        if (!(_page is null) && !Type.Equals(preNavPageType, _page))
        {
            ContentFrame.Navigate(_page, null, transitionInfo);
            Debug.WriteLine("NavView_Navigate: " + _page.FullName);
        }
        */
    }

    private void NavView_BackRequested(NavigationView sender, NavigationViewBackRequestedEventArgs args)
    {
        TryGoBack();
    }

    private void CoreDispatcher_AcceleratorKeyActivated(CoreDispatcher sender, AcceleratorKeyEventArgs e)
    {
        // When Alt+Left are pressed navigate back
        if (e.EventType == CoreAcceleratorKeyEventType.SystemKeyDown
            && e.VirtualKey == VirtualKey.Left
            && e.KeyStatus.IsMenuKeyDown == true
            && !e.Handled)
        {
            e.Handled = TryGoBack();
        }
    }

    private void System_BackRequested(object sender, BackRequestedEventArgs e)
    {
        if (!e.Handled)
        {
            e.Handled = TryGoBack();
        }
    }

    private void CoreWindow_PointerPressed(CoreWindow sender, PointerEventArgs e)
    {
        // Handle mouse back button.
        if (e.CurrentPoint.Properties.IsXButton1Pressed)
        {
            e.Handled = TryGoBack();
        }
    }

    private bool TryGoBack()
    {
        if (!ContentFrame.CanGoBack)
            return false;

        // Don't go back if the nav pane is overlayed.
        if (NavView.IsPaneOpen &&
            (NavView.DisplayMode == NavigationViewDisplayMode.Compact ||
             NavView.DisplayMode == NavigationViewDisplayMode.Minimal))
            return false;

        ContentFrame.GoBack();
        return true;
    }

    private void ContentFrame_Navigated(object sender, NavigationEventArgs e)
    {
        //NavView.IsBackEnabled = ContentFrame.CanGoBack;

        if (ContentFrame.SourcePageType != null)
        {
            /*
            var item = _pages.FirstOrDefault(p => p.Page == e.SourcePageType);
            // This only works for flat NavigationView
            NavView.SelectedItem = NavView.MenuItems
                .OfType<NavigationViewItem>()
                .First(n => n.Tag.Equals(item.Tag));
            */
            //NavView.Header = ((NavigationViewItem)NavView.SelectedItem)?.Content?.ToString();

        }
    }

    private void OnThemeChanged(ElementTheme arg)
    {
        RequestedTheme = arg;

        // not good...
        //TitleBarHelper.UpdateTitleBar(RequestedTheme, EditorWindow);

        App.CurrentDispatcherQueue?.TryEnqueue(() =>
        {
            //TODO: make this work for editor window.
            //TitleBarHelper.ApplySystemThemeToCaptionButtons();
        });
    }

    private void Settings_ColorValuesChanged(UISettings sender, object args)
    {
        Debug.WriteLine("Settings_ColorValuesChanged");

        // This calls comes off-thread, hence we will need to dispatch it to current app's thread
        App.CurrentDispatcherQueue?.TryEnqueue(() =>
        {
            //TODO: make this work for editor window.
            //TitleBarHelper.ApplySystemThemeToCaptionButtons();
        });
    }

    public void SetEntry(Models.Rent.RentResidential entry)
    {
        if (ViewModel is null)
        {
            throw new InvalidOperationException("ViewModel is not initialized.");
        }
        ViewModel.Entry = entry ?? throw new ArgumentNullException(nameof(entry));
    }

}
