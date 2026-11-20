using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;

namespace ZumenSearch.Presentation.Dialogs.Views;

public sealed partial class RailStationSelectPage : Page
{
    public ZumenSearch.Presentation.Dialogs.ViewModels.RailStationSelectViewModel ViewModel
    {
        get;
    }

    public RailStationSelectPage(ZumenSearch.Presentation.Dialogs.ViewModels.RailStationSelectViewModel vm)
    {
        ViewModel = vm;
        InitializeComponent();
    }

    private void KeyboardAccelerator_Invoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
    {
        var query = TextBoxStation.Text;
        if (!string.IsNullOrEmpty(query))
        {
            if (ViewModel.SearchRailStationCommand.CanExecute(query))
            {
                ViewModel.SearchRailStationCommand.Execute(query);
            }
        }
    }
}
