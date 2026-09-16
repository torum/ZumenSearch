using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using ZumenSearch.ViewModels.Rent.Lessors;

namespace ZumenSearch.Views.Dialogs;

public sealed partial class LessorSelectPage : Page
{
    public LessorSelectViewModel ViewModel
    {
        get;
    }

    public LessorSelectPage(LessorSelectViewModel vm)
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
