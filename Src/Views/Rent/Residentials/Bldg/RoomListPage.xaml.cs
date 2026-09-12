using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System.Diagnostics;

namespace ZumenSearch.Views.Rent.Residentials.Bldg;

public sealed partial class RoomListPage : Page
{
    public ViewModels.Rent.Residentials.Bldg.PropertyViewModel? ViewModel { get; private set; }

    public RoomListPage()
    {
        InitializeComponent();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        if ((e.Parameter is ViewModels.Rent.Residentials.Bldg.PropertyViewModel) && (e.Parameter != null))
        {
            ViewModel = e.Parameter as ViewModels.Rent.Residentials.Bldg.PropertyViewModel;
        }
        else
        {
            Debug.WriteLine("RoomListPage.OnNavigatedTo: Invalid parameter. Expected ViewModels.Rent.Residentials.Bldg.MainViewModel.");
        }

        base.OnNavigatedTo(e);
    }

    private void ItemContainer_DoubleTapped(object sender, Microsoft.UI.Xaml.Input.DoubleTappedRoutedEventArgs e)
    {
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

        if (container.DataContext is not Models.Rent.Residentials.Room.Listing room)
        {
            Debug.WriteLine($"Not Room. {container.DataContext?.GetType().FullName} @ItemContainer_DoubleTapped");
            return;
        }

        if (ViewModel is null)
        {
            return;
        }

        if (ViewModel.EditSelectedUnitCommand.CanExecute(room))
        {
            ViewModel.EditSelectedUnitCommand.Execute(room);
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
        // Stupid WinUI3 can't handle double click properly.
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

        if (container.DataContext is not Models.Rent.Residentials.Room.Listing room)
        {
            Debug.WriteLine($"Not Room. {container.DataContext?.GetType().FullName} @ItemContainer_RightTapped");
            return;
        }

        container.IsSelected = true;
    }

    private void ItemContainerKeyboardAccelerator_Invoked(Microsoft.UI.Xaml.Input.KeyboardAccelerator sender, Microsoft.UI.Xaml.Input.KeyboardAcceleratorInvokedEventArgs args)
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

        if (element.DataContext is not Models.Rent.Residentials.Room.Listing room)
        {
            Debug.WriteLine($"Not Room. {element.DataContext?.GetType().FullName} @ItemContainerKeyboardAccelerator_Invoked");
            return;
        }

        if (ViewModel is null)
        {
            return;
        }
        else
        {
            Debug.WriteLine($"ViewModel is not null. {ViewModel.GetType().FullName} @ItemContainerKeyboardAccelerator_Invoked");
        }

        if (ViewModel.EditSelectedUnitCommand.CanExecute(room))
        {
            ViewModel.EditSelectedUnitCommand.Execute(room);
        }
        else
        {
            Debug.WriteLine($"EditSelectedUnitCommand cannot execute. @ItemContainerKeyboardAccelerator_Invoked");
        }
    }

}
