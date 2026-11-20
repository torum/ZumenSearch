using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;

namespace ZumenSearch.Presentation.Dialogs.Views;

public sealed partial class RailLineSelectPage : Page
{
    public ZumenSearch.Presentation.Dialogs.ViewModels.RailLineSelectViewModel ViewModel
    {
        get;
    }

    public RailLineSelectPage(ZumenSearch.Presentation.Dialogs.ViewModels.RailLineSelectViewModel vm)
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
