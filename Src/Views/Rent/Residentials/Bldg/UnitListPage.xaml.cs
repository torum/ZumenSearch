using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using System.Diagnostics;
using ZumenSearch.Models.Rent.Residentials;

namespace ZumenSearch.Views.Rent.Residentials.Bldg;

public sealed partial class UnitListPage : Page
{
    //private Views.Rent.Residentials.Editor.EditorShell? _editorShell;

    public ViewModels.Rent.Residentials.MainViewModel? ViewModel
    {
        get;
        private set
        {
            if (value != null)
            {
                field = value;
                //_viewModel.EventAddNew += (sender, arg) => OnEventAddNew(arg);
            }
        }
    }

    public UnitListPage()
    {
        //ViewModel = new ViewModels.Rent.Residentials.Editor.UnitListViewModel();//App.GetService<RentLivingEditUnitShellViewModel>();

        InitializeComponent();

    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        if ((e.Parameter is ViewModels.Rent.Residentials.MainViewModel) && (e.Parameter != null))
        {
            //_editorShell = e.Parameter as Views.Rent.Residentials.EditorShell;
            ViewModel = e.Parameter as ViewModels.Rent.Residentials.MainViewModel;
        }
        else
        {
            Debug.WriteLine("UnitListPage.OnNavigatedTo: Invalid parameter. Expected ResidentialsViewModel.");
        }

        base.OnNavigatedTo(e);
    }

    private void RoomsListView_ItemInvoked(ItemsView sender, ItemsViewItemInvokedEventArgs args)
    {
        // Get the invoked item
        var invokedItem = args.InvokedItem;

        if (ViewModel is null)
        {
            return;
        }

        if (invokedItem is not Room)
        {
            return;
        }

        ViewModel.Bldg.EditSelectedUnitCommand.Execute(invokedItem);
    }
}
