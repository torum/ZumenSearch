using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;

namespace ZumenSearch.Views.Dialogs;

public sealed partial class RailStationSelectPage : Page
{
    public ViewModels.Dialogs.RailStationViewModel ViewModel
    {
        get;
    }

    public RailStationSelectPage(ViewModels.Dialogs.RailStationViewModel vm)
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
