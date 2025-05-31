using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Navigation;
using System.Diagnostics;
using ZumenSearch.Views;

namespace ZumenSearch.ViewModels.Rent.Residentials.Editor.Modal;

public partial class ModalViewModel : ObservableRecipient
{
    public Views.Rent.Residentials.Editor.Modal.ModalWindow? ModalWin
    {
        get; set;
    }


    private object? _selected;
    public object? Selected
    {
        get => _selected;
        set => SetProperty(ref _selected, value);
    }

    public ModalViewModel()
    {

    }

    private RelayCommand? backCommand;

    public IRelayCommand BackCommand => backCommand ??= new RelayCommand(Back);

    private void Back()
    {
        //NavigationService.NavigateTo(typeof(RentLivingSearchViewModel).FullName!);
        //NavigationService.GoBack();

    }

    private void OnNavigated(object sender, NavigationEventArgs e)
    {
        /*
        //IsBackEnabled = NavigationService.CanGoBack;
        var selectedItem = NavigationViewService.GetSelectedItem(e.SourcePageType);
        if (selectedItem != null)
        {
            Selected = selectedItem;
        }
        */
    }

    public bool Closing()
    {
        // TODO: check if dirty.

        return true;
    }
}
