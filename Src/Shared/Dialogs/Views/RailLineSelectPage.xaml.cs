using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;

namespace ZumenSearch.Views.Dialogs;

public sealed partial class RailLineSelectPage : Page
{
    public ViewModels.Dialogs.RailLineSelectViewModel ViewModel
    {
        get;
    }

    public RailLineSelectPage(ViewModels.Dialogs.RailLineSelectViewModel vm)
    {
        ViewModel = vm;
        InitializeComponent();
    }

    private void KeyboardAccelerator_Invoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
    {
        var query = TextBoxRailLine.Text;
        if (!string.IsNullOrEmpty(query))
        {
            if (ViewModel.SearchRailLineCommand.CanExecute(query))
            {
                ViewModel.SearchRailLineCommand.Execute(query);
            }
        }
    }
}
