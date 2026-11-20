using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;

namespace ZumenSearch.Presentation.Dialogs.Views;

public sealed partial class LessorSelectPage : Page
{
    public ZumenSearch.Presentation.Dialogs.ViewModels.LessorSelectViewModel ViewModel
    {
        get;
    }

    public LessorSelectPage(ZumenSearch.Presentation.Dialogs.ViewModels.LessorSelectViewModel vm)
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
