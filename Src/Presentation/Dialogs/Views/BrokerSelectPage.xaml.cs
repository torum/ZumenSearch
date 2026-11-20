using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;

namespace ZumenSearch.Presentation.Dialogs.Views;

public sealed partial class BrokerSelectPage : Page
{
    public ZumenSearch.Presentation.Dialogs.ViewModels.BrokerSelectViewModel ViewModel
    {
        get;
    }

    public BrokerSelectPage(ZumenSearch.Presentation.Dialogs.ViewModels.BrokerSelectViewModel vm)
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
