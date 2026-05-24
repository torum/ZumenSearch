using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using ZumenSearch.ViewModels.Transportation;

namespace ZumenSearch.Views.Dialogs;

public sealed partial class RailLineSelectPage : Page
{
    public RailLineSelectViewModel ViewModel
    {
        get;
    }

    public RailLineSelectPage(RailLineSelectViewModel vm)
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
