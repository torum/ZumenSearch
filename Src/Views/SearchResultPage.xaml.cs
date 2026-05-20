using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;
using System.Collections.ObjectModel;
using ZumenSearch.Models;
using ZumenSearch.Models.Rent.Residentials;
using ZumenSearch.ViewModels;

namespace ZumenSearch.Views;

public sealed partial class SearchResultPage : Page
{
    public MainViewModel ViewModel
    {
        get;
    }

    //private MainShell? Shell => App.GetService<MainShell>();

    private Frame? ContentFrame;

    public SearchResultPage()
    {
        ViewModel = App.GetService<MainViewModel>();

        InitializeComponent();

        BreadcrumbBar1.ItemClicked += BreadcrumbBar_ItemClicked;
    }

    private void BreadcrumbBar_ItemClicked(BreadcrumbBar sender, BreadcrumbBarItemClickedEventArgs args)
    {
        //ViewModel.NavigationService.NavigateTo(items[args.Index].Page!);
        
        //MainShell shell = App.GetService<MainShell>();

        if (ContentFrame is null) return;

        if (args.Index == 0)
        {
            ContentFrame.Navigate(typeof(Views.SearchPage), ContentFrame, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromLeft });
        }
        else if ( args.Index == 1)
        {
            //shell.NavFrame.Navigate(typeof(Views.Rent.Residentials.SearchPage), shell.NavFrame, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromLeft });
        }
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {

        if ((e.Parameter is Frame) && (e.Parameter != null))
        {
            ContentFrame = e.Parameter as Frame;
        }

        base.OnNavigatedTo(e);
    }

    private void SearchResult_DoubleTapped(object sender, Microsoft.UI.Xaml.Input.DoubleTappedRoutedEventArgs e)
    {
        // Stupid WinUI3 can't handle double click properly.
        // Also, with this, newly created window goes behind main window because WinUI3 is stupid.

        /*
        if (sender is not ListView listView)
        {
            return;
        }

        // UI element that was double-clicked
        FrameworkElement element = (FrameworkElement)e.OriginalSource;

        var container = FindParent<ListViewItem>(element);

        if (container is null)
        {
            Debug.WriteLine("container is null @SearchResult_DoubleTapped()");
            return;
        }

        if (listView.SelectedItem != container.Content)
        {
            Debug.WriteLine("(listView.SelectedItem != container.Content) @SearchResult_DoubleTapped()");
            return;
        }

        if (ViewModel is null)
        {
            return;
        }

        if (listView.SelectedItem is not EntryResidentialSearchResult searchresult)
        {
            return;
        }

        ViewModel.EditRentResidential(searchresult);
        */
    }

    private static T? FindParent<T>(DependencyObject child) where T : DependencyObject
    {
        DependencyObject parent = VisualTreeHelper.GetParent(child);
        while (parent != null && parent is not T)
        {
            parent = VisualTreeHelper.GetParent(parent);
        }

        if (parent is not null)
        {
            return parent as T;
        }
        else
        {
            return null;
        }
    }

    private void SearchResultListView_ItemInvoked(ItemsView sender, ItemsViewItemInvokedEventArgs args)
    {
        // Get the invoked item
        var invokedItem = args.InvokedItem;

        if (ViewModel is null)
        {
            return;
        }

        if (invokedItem is not EntryResidentialSearchResult)
        {
            return;
        }

        // Needs ItemContainer_PointerPressed Handled = true; to avoid stealing child window focus. Strupid WinUI3.
        if (ViewModel.EditRentResidentialEntryCommand.CanExecute(invokedItem))
        {
            ViewModel.EditRentResidentialEntryCommand.Execute(invokedItem);
        }

    }

    private void ItemContainer_PointerPressed(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
    {
        // Stupid WinUI3 can't handle double click properly.
        // This prevents newly created window goes behind the main window.
        // WinUI3 is so stupid.
        e.Handled = true;
    }

    private void ItemContainer_PointerReleased(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
    {
        // Stupid WinUI3 can't handle double click properly.
        // This prevents newly created window goes behind the main window.
        // WinUI3 is so stupid.
        e.Handled = true;
    }
}
