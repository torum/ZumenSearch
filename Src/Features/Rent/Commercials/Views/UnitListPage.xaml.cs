using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;

namespace ZumenSearch.Views.Rent.Commercials;

public sealed partial class UnitListPage : Page
{
    public ViewModels.Rent.Commercials.PropertyViewModel? ViewModel { get; private set; }

    public UnitListPage()
    {
        InitializeComponent();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        ViewModel = e.Parameter as ViewModels.Rent.Commercials.PropertyViewModel;
        Bindings.Update();

        base.OnNavigatedTo(e);
    }

    private void ItemContainer_DoubleTapped(object sender, Microsoft.UI.Xaml.Input.DoubleTappedRoutedEventArgs e)
    {
        if (e.OriginalSource is not FrameworkElement element)
        {
            return;
        }

        var container = FindParent<ItemContainer>(element);
        if (container?.DataContext is Models.Rent.Commercials.Listing.Listing unit)
        {
            EditUnit(unit);
        }
    }

    private void ItemContainer_PointerPressed(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
    {
        e.Handled = true;
    }

    private void ItemContainer_PointerReleased(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
    {
        e.Handled = true;
    }

    private void ItemContainer_RightTapped(object sender, Microsoft.UI.Xaml.Input.RightTappedRoutedEventArgs e)
    {
        if (e.OriginalSource is not FrameworkElement element)
        {
            return;
        }

        var container = FindParent<ItemContainer>(element);
        if (container is not null)
        {
            container.IsSelected = true;
        }
    }

    private void ItemContainerKeyboardAccelerator_Invoked(Microsoft.UI.Xaml.Input.KeyboardAccelerator sender, Microsoft.UI.Xaml.Input.KeyboardAcceleratorInvokedEventArgs args)
    {
        args.Handled = true;

        if (args.Element is ItemContainer { DataContext: Models.Rent.Commercials.Listing.Listing unit })
        {
            EditUnit(unit);
        }
    }

    private void EditUnit(Models.Rent.Commercials.Listing.Listing unit)
    {
        if (ViewModel?.EditSelectedUnitCommand.CanExecute(unit) == true)
        {
            ViewModel.EditSelectedUnitCommand.Execute(unit);
        }
    }

    private static T? FindParent<T>(DependencyObject child) where T : DependencyObject
    {
        var parent = VisualTreeHelper.GetParent(child);

        while (parent is not null && parent is not T)
        {
            parent = VisualTreeHelper.GetParent(parent);
        }

        return parent as T;
    }
}