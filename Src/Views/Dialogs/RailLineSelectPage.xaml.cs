using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using ZumenSearch.ViewModels.Railway;

namespace ZumenSearch.Views.Dialogs;

public sealed partial class RailLineSelectPage : Page
{
    public RailLineViewModel ViewModel
    {
        get;
    }

    public RailLineSelectPage(RailLineViewModel vm)
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
