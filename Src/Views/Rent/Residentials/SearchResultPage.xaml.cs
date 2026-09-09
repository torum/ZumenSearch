using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;
using System.Diagnostics;
using ZumenSearch.Models.Common;
using ZumenSearch.Models.Rent.Residentials;
using ZumenSearch.Services.Contracts;
using ZumenSearch.ViewModels;

namespace ZumenSearch.Views.Rent.Residentials;

public sealed partial class SearchResultPage : Page
{
    public MainViewModel ViewModel
    {
        get;
    }

    private readonly INavigationService _navigationService;

    public SearchResultPage()
    {
        ViewModel = App.GetService<MainViewModel>();
        _navigationService = App.GetService<INavigationService>();

        InitializeComponent();

        BreadcrumbBar1.ItemClicked += BreadcrumbBar_ItemClicked;
    }

    private void BreadcrumbBar_ItemClicked(BreadcrumbBar sender, BreadcrumbBarItemClickedEventArgs args)
    {

        if (args.Index == 0)
        {
            if (args.Item is Breadcrumb breadcrumb)
            {
                _navigationService.NavigateTo(breadcrumb.Page, SlideNavigationTransitionEffect.FromLeft);
            }
            //_navigationService.NavigateTo("ZumenSearch.Views.Rent.Residentials.SearchPage", SlideNavigationTransitionEffect.FromLeft);
        }
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);

        this.SearchResultListView.Focus(Microsoft.UI.Xaml.FocusState.Programmatic);
    }

    private void SearchResult_DoubleTapped(object sender, Microsoft.UI.Xaml.Input.DoubleTappedRoutedEventArgs e)
    {
        e.Handled = true;
        // Stupid WinUI3 can't handle double click properly.
        // Also, with this, newly created window goes behind main window because WinUI3 is stupid.

        /*
        if (e.OriginalSource is not FrameworkElement element)
        {
            return;
        }

        var container = FindParent<Microsoft.UI.Xaml.Controls.ItemContainer>(element);
        if (container is null)
        {
            return;
        }

        if (container.DataContext is not EntryResidentialSearchResult searchresult)
        {
            Debug.WriteLine($"Not EntryResidentialSearchResult. {container.DataContext?.GetType().FullName} @SearchResult_DoubleTapped");
            return;
        }

        if (ViewModel is null)
        {
            return;
        }

        if (ViewModel.EditRentResidentialEntryCommand.CanExecute(searchresult))
        {
            ViewModel.EditRentResidentialEntryCommand.Execute(searchresult);
        }

        */

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

        if (invokedItem is not Models.Rent.Residentials.ListingSearchResultItem)
        {
            return;
        }

        // Needs ItemContainer_PointerPressed Handled = true; to avoid stealing child window focus. Strupid WinUI3.
        if (ViewModel.EditRentResidentialRoomCommand.CanExecute(invokedItem))
        {
            ViewModel.EditRentResidentialRoomCommand.Execute(invokedItem);
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

    private void Page_Loaded(object sender, RoutedEventArgs e)
    {
        this.SearchResultListView.Focus(Microsoft.UI.Xaml.FocusState.Programmatic);
    }

    private void ItemsViewKeyboardAccelerator_Invoked(Microsoft.UI.Xaml.Input.KeyboardAccelerator sender, Microsoft.UI.Xaml.Input.KeyboardAcceleratorInvokedEventArgs args)
    {
        Debug.WriteLine($"sender {sender}, element{args.Element} @ItemContainerKeyboardAccelerator_Invoked");
        /*
        args.Handled = true;

        if (args.Element is not FrameworkElement)
        {
            return;
        }

        if (args.Element is not Microsoft.UI.Xaml.Controls.ItemContainer element)
        {
            return;
        }

        if (element.DataContext is not EntryResidentialSearchResult searchresult)
        {
            Debug.WriteLine($"Not EntryResidentialSearchResult. {element.DataContext?.GetType().FullName} @ItemContainerKeyboardAccelerator_Invoked");
            return;
        }

        if (ViewModel is null)
        {
            return;
        }

        if (ViewModel.EditRentResidentialEntryCommand.CanExecute(searchresult))
        {
            ViewModel.EditRentResidentialEntryCommand.Execute(searchresult);
        }
        */
    }
}
