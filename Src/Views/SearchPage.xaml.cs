using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using ZumenSearch.Services.Contracts;
using ZumenSearch.ViewModels;

namespace ZumenSearch.Views;

public sealed partial class SearchPage : Page
{
    public ViewModels.MainViewModel ViewModel { get;}

    private readonly INavigationService _navigationService;

    public SearchPage()
    {
        ViewModel = App.GetService<MainViewModel>();
        _navigationService = App.GetService<INavigationService>();

        InitializeComponent();

        this.Loaded += Page_Loaded;

        //BreadcrumbBarMain.ItemsSource = new string[] { "ëççáåüçı" };
        /*
        BreadcrumbBarMain.ItemsSource = new ObservableCollection<Breadcrumb>{
        new() { Name = "ëççáåüçı", Page = typeof(IntegratedSearchPage).FullName!},
    };
        */
    }

    private void Page_Loaded(object sender, RoutedEventArgs e)
    {
        this.SearchAutoSuggestBox.Focus(Microsoft.UI.Xaml.FocusState.Programmatic);
    }

    private void AutoSuggestBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
    {
        if (ViewModel.SearchRentResidentialBldgCommand.CanExecute(args.QueryText))
        {
            ViewModel.SearchRentResidentialBldgCommand.Execute(args.QueryText);
        }
    }

    private void Image_ImageFailed(object sender, ExceptionRoutedEventArgs e)
    {
        if (sender is Microsoft.UI.Xaml.Controls.Image img)
        {
            System.Diagnostics.Debug.WriteLine($"Image failed to load: {img.Source}"); 
            /*
            img.Source = new Microsoft.UI.Xaml.Media.Imaging.BitmapImage(
                new Uri("ms-appx:///Assets/FallbackPlaceholder.png")
            );
            */
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

    private void ItemContainer_DoubleTapped(object sender, Microsoft.UI.Xaml.Input.DoubleTappedRoutedEventArgs e)
    {
        e.Handled = true;
        // WinUI3 can't handle double click properly, so we handle it manually.

        // OriginalSource is the specific element (e.g., TextBlock or Grid) that was tapped
        if (e.OriginalSource is not FrameworkElement element)
        {
            return;
        }

        var container = FindParent<Microsoft.UI.Xaml.Controls.ItemContainer>(element);
        if (container is null)
        {
            return;
        }

        if (container.DataContext is not Models.Common.PropertySearchResultItem searchresult)
        {
            Debug.WriteLine($"Not PropertySearchResult. {container.DataContext?.GetType().FullName} @ItemContainer_DoubleTapped");
            return;
        }

        if (ViewModel is null)
        {
            return;
        }

        if (ViewModel.EditRentResidentialBldgCommand.CanExecute(searchresult))
        {
            ViewModel.EditRentResidentialBldgCommand.Execute(searchresult);
        }

        // TODO: RentResidentialBldg only for now.
        if (searchresult.PropertyKind == Models.Base.EnumPropertyKind.RentResidential)
        {
            if (ViewModel.EditRentResidentialBldgCommand.CanExecute(searchresult))
            {
                ViewModel.EditRentResidentialBldgCommand.Execute(searchresult);
            }
        }
        else
        {
            Debug.WriteLine($"EnumPropertyKind is not RentResidential @ItemContainer_DoubleTapped {searchresult.PropertyKind}");
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

        if (container.DataContext is not Models.Common.PropertySearchResultItem)
        {
            Debug.WriteLine($"Not PropertySearchResult. {container.DataContext?.GetType().FullName} @ItemContainer_RightTapped");
            return;
        }

        container.IsSelected = true;
    }

    private void ItemsViewKeyboardAccelerator_Invoked(Microsoft.UI.Xaml.Input.KeyboardAccelerator sender, Microsoft.UI.Xaml.Input.KeyboardAcceleratorInvokedEventArgs args)
    {
        args.Handled = true;

        if (args.Element is not FrameworkElement)
        {
            return;
        }

        if (args.Element is not Microsoft.UI.Xaml.Controls.ItemContainer element)
        {
            return;
        }

        if (element.DataContext is not Models.Common.PropertySearchResultItem searchresult)
        {
            Debug.WriteLine($"Not PropertySearchResult. {element.DataContext?.GetType().FullName} @ItemContainerKeyboardAccelerator_Invoked");
            return;
        }

        if (ViewModel is null)
        {
            return;
        }


        Debug.WriteLine($"ItemContainerKeyboardAccelerator_Invoked");
        /*
        if (ViewModel.EditRentResidentialBldgCommand.CanExecute(searchresult))
        {
            ViewModel.EditRentResidentialBldgCommand.Execute(searchresult);
        }
        */
    }
}
