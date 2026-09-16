using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using ZumenSearch.ViewModels.Railway;

namespace ZumenSearch.Views.Dialogs;

public sealed partial class RailStationSelectPage : Page
{
    public RailStationViewModel ViewModel
    {
        get;
    }

    public RailStationSelectPage(RailStationViewModel vm)
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
