using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace ZumenSearch.Views.Rent.Residentials.Room;

public sealed partial class TransactionPage : Page
{
    public ViewModels.Rent.Residentials.Room.ListingViewModel? ViewModel { get; private set; }

    public TransactionPage()
    {
        //ViewModel = new ViewModels.Rent.Residentials.Editor.Modal.TransactionViewModel();
        InitializeComponent();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        if ((e.Parameter is ViewModels.Rent.Residentials.Room.ListingViewModel) && (e.Parameter != null))
        {
            //_editorShell = e.Parameter as Views.Rent.Residentials.Editor.EditorShell;
            ViewModel = e.Parameter as ViewModels.Rent.Residentials.Room.ListingViewModel;
        }

        base.OnNavigatedTo(e);
    }
}
