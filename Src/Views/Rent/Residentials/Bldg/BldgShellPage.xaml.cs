using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;
using System.Collections;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using ZumenSearch.Models.Common;
using ZumenSearch.Services.Contracts;

namespace ZumenSearch.Views.Rent.Residentials.Bldg;

internal sealed partial class BldgShellPage : Page
{
    #region == Properties ==

    public ViewModels.Rent.Residentials.MainViewModel? ViewModel {
        get;
        private set
        {
            if (field == value)
                return;

            field = value;

            // TODO:
            //ViewModel?.EventIsUnitOwnership += (sender, arg) => OnEventIsUnitOwnership(arg);
        }
    }

    //public Views.Rent.Residentials.EditorWindow EditorWin { get; private set; }

    public Frame NavigationFrame => ContentFrame;

    // List of ValueTuple holding the Navigation Tag and the relative Navigation Page
    private readonly List<(string Tag, string Label, Type? Page)> _pages =
    [
        ("building", "建物", null),
        ("summary", "基本", typeof(Views.Rent.Residentials.Bldg.BasicPage)),
        //("structure", "", typeof(Views.Rent.Residentials.Bldg.StructurePage)),
        ("location", "所在地", typeof(Views.Rent.Residentials.Bldg.LocationPage)),
        ("transportation", "交通", typeof(Views.Rent.Residentials.Bldg.TransportationPage)),
        ("appliance", "設備", typeof(Views.Rent.Residentials.Bldg.AppliancePage)),
        ("pictures", "写真", typeof(Views.Rent.Residentials.Bldg.PictureListPage)),
        ("units", "部屋", typeof(Views.Rent.Residentials.Bldg.UnitListPage)),
        ("zumen", "図面", typeof(Views.Rent.Residentials.Bldg.ZumenListPage)),
        ("kasinusi", "貸主", typeof(Views.Rent.Residentials.Bldg.KasinusiPage)),
        ("gyousya", "宅建業者", typeof(Views.Rent.Residentials.Bldg.GyousyaPage)),
        //("memo", "備考", typeof(Views.Rent.Residentials.Editor.MemoPage)),
    ];

    #endregion

    #region == Services ==

    private readonly IDispatcherService _dispatcherService;
    private readonly IModalDialogService _dlg;

    #endregion

    private bool _nvigated;
    private bool _initialized;

    public BldgShellPage() : this(App.GetService<IModalDialogService>(), App.GetService<IDispatcherService>())
    {
        // parameterless ctor used by XAML activator; chains to DI ctor
    }

    public BldgShellPage(IModalDialogService modalDialog, IDispatcherService dispatcherService)//Views.Rent.Residentials.Bldg.EditorWindow win, ViewModels.Rent.Residentials.ResidentialsViewModel vm, IModalDialogService modalDialog
    {
        _dlg = modalDialog;
        _dispatcherService = dispatcherService;

        InitializeComponent();

        BreadcrumbBar1.ItemClicked += BreadcrumbBar_ItemClicked;
    }

    private void Init()
    {
        if (_initialized) return;

        _initialized = true;

        ViewModel?.Bldg.EventIsUnitOwnershipChanged += (sender, arg) => OnEventIsUnitOwnershipChanged(arg);

        // needs this.
        if (ViewModel is not null)
        {
            OnEventIsUnitOwnershipChanged(ViewModel.Bldg.IsUnitOwnership);
        }

        //
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        if ((e.Parameter is ViewModels.Rent.Residentials.MainViewModel) && (e.Parameter != null))
        {
            //_editorShell = e.Parameter as Views.Rent.Residentials.EditorShell;
            ViewModel = e.Parameter as ViewModels.Rent.Residentials.MainViewModel;
            if (!_initialized)
            {
                Init();

                ViewModel?.SetBldgShell(this);
            }
        }
        else
        {
            Debug.WriteLine("BldgShellPage.OnNavigatedTo: Invalid parameter. Expected ResidentialsViewModel.");
        }

        //Debug.WriteLine("BldgShellPage.OnNavigatedTo: ViewModel is " + (ViewModel != null ? "set" : "null"));

        base.OnNavigatedTo(e);
    }

    private void BreadcrumbBar_ItemClicked(BreadcrumbBar sender, BreadcrumbBarItemClickedEventArgs args)
    {
        if (args.Index == 0)
        {
            //var hoge = args.Item as Breadcrumb;
            //Debug.WriteLine("BreadcrumbBar_ItemClicked: " + hoge?.Name + ", Page: " + hoge?.Page);
            ViewModel?.GoToBldgShellPageCommand.Execute(null);
        }
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

        if (_nvigated)
        {
            return;
        }

        //Debug.WriteLine("NavView_Loaded: Navigating to BasicPage with ViewModel. ViewModel is " + (ViewModel != null ? "set" : "null"));

        // Pass Frame when navigate.  //, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromLeft } //, new SuppressNavigationTransitionInfo() //new EntranceNavigationTransitionInfo()
        if (ContentFrame.Navigate(typeof(ZumenSearch.Views.Rent.Residentials.Bldg.BasicPage), ViewModel, new EntranceNavigationTransitionInfo()))//new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromBottom }
        {
            _nvigated = true;

            if (BreadcrumbBar1.ItemsSource is ObservableCollection<Breadcrumb> crumbs)
            {
                if (crumbs.Count > 1)
                {
                    var item = _pages.FirstOrDefault(p => p.Tag.Equals("summary"));
                    if (item.Page is not null)
                    {
                        crumbs.RemoveAt(crumbs.Count - 1); // Remove the last breadcrumb if exists to avoid duplication.
                        crumbs.Add(new Breadcrumb { Name = item.Label, Page = item.Page.FullName! });
                    }
                }
            }

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
                Debug.WriteLine("BldgShellPage: NavView_ItemInvoked: Invalid tag or null.");
                return;
            }

            var item = _pages.FirstOrDefault(p => p.Tag.Equals(args.InvokedItemContainer.Tag.ToString()));

            if (item.Page is null)
            {
                Debug.WriteLine("BldgShellPage: NavView_ItemInvoked: Page is null for tag " + tag);
                return;
            }

            if (ContentFrame.Navigate(item.Page, ViewModel, new DrillInNavigationTransitionInfo())) //new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromBottom })SuppressNavigationTransitionInfo
            {
                /*
                BreadcrumbBar1.ItemsSource = new ObservableCollection<Breadcrumb>{
                    new() { Name = "建物", Page = typeof(Views.Rent.Residentials.Bldg.BasicPage).FullName!},
                    new() { Name = item.Label, Page = item.Page.FullName!},
                };
                */
                if (BreadcrumbBar1.ItemsSource is ObservableCollection<Breadcrumb> crumbs)
                {
                    if (crumbs.Count > 1)
                    {
                        crumbs.RemoveAt(crumbs.Count - 1); // Remove the last breadcrumb if exists to avoid duplication.
                        crumbs.Add(new Breadcrumb { Name = item.Label, Page = item.Page.FullName! });
                    }
                }
            }
            //, args.RecommendedNavigationTransitionInfo
        }
    }

    private void ContentFrame_NavigationFailed(object sender, NavigationFailedEventArgs e)
    {
        throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
    }

    public void OnEventIsUnitOwnershipChanged(bool arg)
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
        ContentFrame.Navigate(typeof(Views.Rent.Residentials.Bldg.BasicPage), ViewModel, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromLeft });
    }

    public void OnEventEditLocation()
    {
        ContentFrame.Navigate(typeof(Views.Rent.Residentials.Bldg.LocationPage), ViewModel, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromRight });
    }

    public void OnEventEditTransportation()
    {
        ContentFrame.Navigate(typeof(Views.Rent.Residentials.Bldg.TransportationPage), ViewModel, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromRight });
    }

    public void OnEventEditAppliance()
    {
        ContentFrame.Navigate(typeof(Views.Rent.Residentials.Bldg.AppliancePage), ViewModel, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromRight });
    }

    public void OnEventEditPictures()
    {
        ContentFrame.Navigate(typeof(Views.Rent.Residentials.Bldg.PictureListPage), ViewModel, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromRight });
    }

    public void OnEventEditUnits()
    {
        ContentFrame.Navigate(typeof(Views.Rent.Residentials.Bldg.UnitListPage), ViewModel, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromRight });
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

    private void Page_Unloaded(object sender, RoutedEventArgs e)
    {
        // TODO:
        //ViewModel?.EventIsUnitOwnership -= (sender, arg) => OnEventIsUnitOwnership(arg);
    }
}
