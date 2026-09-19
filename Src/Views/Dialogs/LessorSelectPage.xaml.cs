using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;

namespace ZumenSearch.Views.Dialogs;

public sealed partial class LessorSelectPage : Page
{
    public ViewModels.Dialogs.LessorSelectViewModel ViewModel
    {
        get;
    }

    public LessorSelectPage(ViewModels.Dialogs.LessorSelectViewModel vm)
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
