using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace ZumenSearch.Views.Rent.Residentials.Unit;

public sealed partial class TransactionPage : Page
{
    public ViewModels.Rent.Residentials.MainViewModel? ViewModel
    {
        get;
        private set
        {
            if (value != null)
            {
                field = value;

                //_viewModel.EventBackToSummary += (sender, arg) => OnEventBackToSummary(arg);
            }
        }
    }

    public TransactionPage()
    {
        //ViewModel = new ViewModels.Rent.Residentials.Editor.Modal.TransactionViewModel();
        InitializeComponent();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        if ((e.Parameter is ViewModels.Rent.Residentials.MainViewModel) && (e.Parameter != null))
        {
            //_editorShell = e.Parameter as Views.Rent.Residentials.Editor.EditorShell;
            ViewModel = e.Parameter as ViewModels.Rent.Residentials.MainViewModel;
        }

        base.OnNavigatedTo(e);
    }
}
