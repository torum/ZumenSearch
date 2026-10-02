using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;

namespace ZumenSearch.Views.Dialogs;

public sealed partial class BrokerSelectPage : Page
{
    public ViewModels.Dialogs.BrokerSelectViewModel ViewModel
    {
        get;
    }

    public BrokerSelectPage(ViewModels.Dialogs.BrokerSelectViewModel vm)
    {
        ViewModel = vm;
        InitializeComponent();
    }

    private void KeyboardAccelerator_Invoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
    {
        var query = TextBoxName.Text;
        if (!string.IsNullOrEmpty(query))
        {
            if (ViewModel.SearchCommand.CanExecute(query))
            {
                ViewModel.SearchCommand.Execute(query);
            }
        }
    }
}
