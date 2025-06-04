using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Navigation;
using System.Diagnostics;
using ZumenSearch.Views.Rent.Residentials.Editor;

namespace ZumenSearch.ViewModels.Rent.Residentials.Editor;

public partial class EditorViewModel : ObservableRecipient
{
    public Views.Rent.Residentials.Editor.EditorWindow? EditorWin
    {
        get; set;
    }


    public EditorViewModel()
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
