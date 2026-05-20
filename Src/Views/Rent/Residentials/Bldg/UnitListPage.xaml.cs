using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
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

    /*
    private void RoomsListView_ItemInvoked(ItemsView sender, ItemsViewItemInvokedEventArgs args)
    {
        // Get the invoked item
        var invokedItem = args.InvokedItem;

        if (ViewModel is null)
        {
            return;
        }

        if (invokedItem is not UnitResidential)
        {
            ViewModel.Bldg.SelectedRoom = null;
            return;
        }

        ViewModel.Bldg.SelectedRoom = invokedItem as UnitResidential;

        ViewModel.Bldg.EditSelectedUnitCommand.Execute(invokedItem);
    }

    private void RoomsListView_SelectionChanged(ItemsView sender, ItemsViewSelectionChangedEventArgs args)
    {
        if (ViewModel is null)
        {
            return;
        }

        if (sender.SelectedItem is not UnitResidential)
        {
            ViewModel.Bldg.SelectedRoom = null;
            return;
        }

        ViewModel.Bldg.SelectedRoom = sender.SelectedItem as UnitResidential;
    }
    */

    private static T? FindParent<T>(DependencyObject child) where T : DependencyObject
    {
        DependencyObject parent = VisualTreeHelper.GetParent(child);
        while (parent != null && parent is not T)
        {
            parent = VisualTreeHelper.GetParent(parent);
        }

        if (parent is not null)
        {
            return parent as T;
        }
        else
        {
            return null;
        }
    }

    private void RoomsListView_DoubleTapped(object sender, Microsoft.UI.Xaml.Input.DoubleTappedRoutedEventArgs e)
    {
        if (sender is not ListView listView)
        {
            return;
        }

        if (ViewModel is null)
        {
            return;
        }

        // UI element that was double-clicked
        FrameworkElement element = (FrameworkElement)e.OriginalSource;

        var container = FindParent<ListViewItem>(element);

        if (container is null)
        {
            //ViewModel.Bldg.SelectedRoom = null;
            return;
        }

        if (listView.SelectedItem != container.Content)
        {
            //ViewModel.Bldg.SelectedRoom = null;
            return;
        }


        if (listView.SelectedItem is not UnitResidential room)
        {
            //ViewModel.Bldg.SelectedRoom = null;
            return;
        }

        //ViewModel.Bldg.SelectedRoom = room as UnitResidential;

        if (ViewModel.Bldg.EditSelectedUnitCommand.CanExecute(room))
        {
            ViewModel.Bldg.EditSelectedUnitCommand.Execute(room);
        }
    }
}
