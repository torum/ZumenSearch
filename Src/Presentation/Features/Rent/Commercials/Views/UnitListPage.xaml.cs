using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;

namespace ZumenSearch.Presentation.Features.Rent.Commercials.Views;

public sealed partial class UnitListPage : Page
{
    public ZumenSearch.Presentation.Features.Rent.Commercials.ViewModels.PropertyViewModel? ViewModel { get; private set; }

    public UnitListPage()
    {
        InitializeComponent();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        ViewModel = e.Parameter as ZumenSearch.Presentation.Features.Rent.Commercials.ViewModels.PropertyViewModel;
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
        if (container?.DataContext is ZumenSearch.Presentation.Features.Rent.Commercials.Models.ListingSummary summary)
        {
            EditUnit(summary);
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

        if (args.Element is ItemContainer { DataContext: ZumenSearch.Presentation.Features.Rent.Commercials.Models.ListingSummary summary })
        {
            EditUnit(summary);
        }
    }

    private void EditUnit(ZumenSearch.Presentation.Features.Rent.Commercials.Models.ListingSummary summary)
    {
        if (ViewModel?.EditSelectedUnitCommand.CanExecute(summary) == true)
        {
            ViewModel.EditSelectedUnitCommand.Execute(summary);
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