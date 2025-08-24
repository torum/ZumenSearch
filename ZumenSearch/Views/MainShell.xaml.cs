using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Xml;
using Windows.Foundation;
using Windows.Foundation.Collections;
using ZumenSearch.ViewModels;
using ZumenSearch.ViewModels.Rent;
using ZumenSearch.Views.Rent;
using static System.Net.Mime.MediaTypeNames;

namespace ZumenSearch.Views
{
    public sealed partial class MainShell : Page
    {
        private MainViewModel? ViewModel { get; set; }


        private NavigationViewItem? navigationViewSelectedItem;

        // List of ValueTuple holding the Navigation Tag and the relative Navigation Page
        private readonly List<(string Tag, Type? Page)> _pages =
        [
            //("Rent", typeof(Rent.RentSearchPage)),
            ("Rent", null),
            ("RentSearch", typeof(RentSearchPage)),
            ("RentResidentials", typeof(Rent.Residentials.SearchPage)),
            ("RentCommercials", typeof(Rent.Commercials.CommercialsPage)),
            ("RentParkings", typeof(Rent.Parkings.ParkingsPage)),
            ("RentOwners", typeof(Rent.Owners.OwnersPage)),
            ("Brokers", typeof(BrokersPage)),
            //("Settings", typeof(SettingsPage)),
        ];

        // For uses of Navigation in other pages.
        public Frame NavFrame
        {
            get => NavigationFrame;
        }

        public MainShell(MainViewModel vm)
        {
            ViewModel = vm ?? throw new ArgumentNullException(nameof(vm));

            this.InitializeComponent();

            // For uses of Navigation in other pages.
            //NavFrame = NavigationFrame;

            if (App.MainWindow != null)
            {
                App.MainWindow.ExtendsContentIntoTitleBar = true;
                App.MainWindow.SetTitleBar(AppTitleBar);
                App.MainWindow.Activated += MainWindow_Activated;
                App.MainWindow.Closed += MainWindow_Closed;
            }
            else
            {
                Debug.WriteLine("MainWindow is null. Make sure to create it before MainShell in App.xaml.cs.");
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
            }
        }

        private void MainWindow_Closed(object sender, WindowEventArgs args)
        {
            //
        }

        private void NavigationViewControl_DisplayModeChanged(NavigationView sender, NavigationViewDisplayModeChangedEventArgs args)
        {
            AppTitleBar.Margin = new Thickness()
            {
                Left = sender.CompactPaneLength * (sender.DisplayMode == NavigationViewDisplayMode.Minimal ? 2 : 1),
                Top = AppTitleBar.Margin.Top,
                Right = AppTitleBar.Margin.Right,
                Bottom = AppTitleBar.Margin.Bottom
            };
        }

        private void NavigationFrame_NavigationFailed(object sender, Microsoft.UI.Xaml.Navigation.NavigationFailedEventArgs e)
        {
            e.Handled = true;
        }

        private void NavigationViewControl_Loaded(object sender, RoutedEventArgs e)
        {
            // Since we use ItemInvoked, we set selecteditem manually
            //NavigationViewControl.SelectedItem = NavigationViewControl.MenuItems.OfType<NavigationViewItem>().First();

            var firstMenuItem = NavigationViewControl.MenuItems.OfType<NavigationViewItem>().First();
            if (firstMenuItem != null)
            {
                var childItem = firstMenuItem.MenuItems.OfType<NavigationViewItem>().Where(n => n.Tag.Equals("RentResidentials"));
                if (childItem != null)
                {
                    childItem.First().IsSelected = true;
                    navigationViewSelectedItem = childItem.First();
                }
                else { Debug.WriteLine("No child menu item with tag 'RentResidentials' found in NavView."); }
            }
            else
            {
                Debug.WriteLine("No first menu item found in NavView.");
            }

            // Pass Frame when navigate.  //, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromLeft } //, new SuppressNavigationTransitionInfo() //new EntranceNavigationTransitionInfo()
            //NavigationFrame.Navigate(typeof(Rent.RentSearchPage), NavigationFrame, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromBottom });
            NavigationFrame.Navigate(typeof(Views.Rent.Residentials.SearchPage), NavigationFrame, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromBottom });//
        }

        private void NavigationViewControl_ItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
        {
            if (_pages is null)
                return;

            if (args.IsSettingsInvoked == true)
            {
                navigationViewSelectedItem = sender.SelectedItem as NavigationViewItem;

                //NavView_Navigate("settings", args.RecommendedNavigationTransitionInfo);
                NavigationFrame.Navigate(typeof(SettingsPage), NavigationFrame, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromBottom });//, args.RecommendedNavigationTransitionInfo
            }
            else if (args.InvokedItemContainer != null && (args.InvokedItemContainer.Tag != null))
            {

                if (args.InvokedItemContainer.Tag is not string tag || string.IsNullOrWhiteSpace(tag))
                {
                    Debug.WriteLine("NavigationViewControl_ItemInvoked: Invalid tag or null.");
                    //sender.SelectedItem = navigationViewSelectedItem;
                    return;
                }

                /*
                var MenuItem = sender.SelectedItem as NavigationViewItem;
                if (MenuItem != null)
                {
                    Debug.WriteLine(MenuItem.Tag + ".");
                    if (MenuItem.IsExpanded)
                    {
                        Debug.WriteLine( "IsExpanded.");
                    } 
                }
                */

                var item = _pages.FirstOrDefault(p => p.Tag.Equals(args.InvokedItemContainer.Tag.ToString()));

                if (item.Page is null)
                {
                    Debug.WriteLine("NavView_ItemInvoked: Page is null for tag " + tag);
                    // Don't. crash when complact menu.
                    //sender.SelectedItem = navigationViewSelectedItem;

                    return;
                }

                navigationViewSelectedItem = sender.SelectedItem as NavigationViewItem;

                // Pass Frame when navigate.
                NavigationFrame.Navigate(item.Page, NavigationFrame, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromLeft });//, args.RecommendedNavigationTransitionInfo
            }
        }
    }
}
