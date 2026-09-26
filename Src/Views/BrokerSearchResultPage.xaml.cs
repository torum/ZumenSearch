using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;
using System.Diagnostics;
using ZumenSearch.Models.Common;
using ZumenSearch.Services.Contracts;
using ZumenSearch.ViewModels;

namespace ZumenSearch.Views;

public sealed partial class BrokerSearchResultPage : Page
{
    public MainViewModel ViewModel
    {
        get;
    }

    private readonly INavigationService _navigationService;

    public BrokerSearchResultPage()
    {
        ViewModel = App.GetService<MainViewModel>();
        _navigationService = App.GetService<INavigationService>();

        InitializeComponent();

        this.Loaded += Page_Loaded;
        BreadcrumbBar1.ItemClicked += BreadcrumbBar_ItemClicked;
    }

    private void Page_Loaded(object sender, RoutedEventArgs e)
    {
        this.SearchResultListView.Focus(Microsoft.UI.Xaml.FocusState.Programmatic);
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);

        this.SearchResultListView.Focus(Microsoft.UI.Xaml.FocusState.Programmatic);
    }

    private void BreadcrumbBar_ItemClicked(BreadcrumbBar sender, BreadcrumbBarItemClickedEventArgs args)
    {
        if (args.Index == 0)
        {
            if (args.Item is Breadcrumb breadcrumb)
            {
                _navigationService.NavigateTo(breadcrumb.Page, SlideNavigationTransitionEffect.FromLeft);
            }
        }
    }

    private void SearchResult_DoubleTapped(object sender, Microsoft.UI.Xaml.Input.DoubleTappedRoutedEventArgs e)
    {
        e.Handled = true;
        // WinUI3 can't handle double click properly, so we handle it manually.

        if (e.OriginalSource is not FrameworkElement element)
        {
            return;
        }

        var container = FindParent<Microsoft.UI.Xaml.Controls.ItemContainer>(element);
        if (container is null)
        {
            return;
        }

        if (container.DataContext is not Models.Common.PersonSearchResultItem searchresult)
        {
            Debug.WriteLine($"Not PersonSearchResultItem. {container.DataContext?.GetType().FullName} @SearchResult_DoubleTapped");
            return;
        }

        if (ViewModel is null)
        {
            return;
        }

        if (ViewModel.EditBrokerCommand.CanExecute(searchresult))
        {
            ViewModel.EditBrokerCommand.Execute(searchresult);
        }
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

    private void ItemContainer_PointerPressed(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
    {
        // WinUI3 can't handle double click properly.
        // This prevents newly created window goes behind the main window.
        e.Handled = true;
    }

    private void ItemContainer_PointerReleased(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
    {
        // WinUI3 can't handle double click properly.
        // This prevents newly created window goes behind the main window.
        e.Handled = true;
    }

    private void ItemContainer_RightTapped(object sender, Microsoft.UI.Xaml.Input.RightTappedRoutedEventArgs e)
    {
        if (e.OriginalSource is not FrameworkElement element)
        {
            return;
        }

        var container = FindParent<Microsoft.UI.Xaml.Controls.ItemContainer>(element);
        if (container is null)
        {
            return;
        }

        if (container.DataContext is not Models.Common.ListingSearchResultItem)
        {
            Debug.WriteLine($"Not ListingSearchResultItem. {container.DataContext?.GetType().FullName} @ItemContainer_RightTapped");
            return;
        }

        container.IsSelected = true;
    }

    private void ItemsViewKeyboardAccelerator_Invoked(Microsoft.UI.Xaml.Input.KeyboardAccelerator sender, Microsoft.UI.Xaml.Input.KeyboardAcceleratorInvokedEventArgs args)
    {
        //Debug.WriteLine($"sender {sender}, element{args.Element} @ItemContainerKeyboardAccelerator_Invoked");
        args.Handled = true;

        if (args.Element is not FrameworkElement)
        {
            return;
        }

        if (args.Element is not Microsoft.UI.Xaml.Controls.ItemContainer element)
        {
            return;
        }

        if (element.DataContext is not Models.Common.PersonSearchResultItem searchresult)
        {
            Debug.WriteLine($"Not LessorSearchResultItem. {element.DataContext?.GetType().FullName} @ItemContainerKeyboardAccelerator_Invoked");
            return;
        }

        if (ViewModel is null)
        {
            return;
        }

        if (ViewModel.EditRentLessorCommand.CanExecute(searchresult))
        {
            ViewModel.EditRentLessorCommand.Execute(searchresult);
        }
    }

}
