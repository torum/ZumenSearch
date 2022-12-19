using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ZumenSearch.Services;

namespace ZumenSearch.ViewModels.RentLivingEdit;

public class RentLivingEditUnitListViewModel : ObservableRecipient
{
    public RentLivingEditUnitListViewModel()
    {
        Debug.WriteLine("RentLivingEditUnitListViewModel init!");
    }

    public event EventHandler<string> eventAddNew;

    private RelayCommand? addNewCommand;

    public IRelayCommand AddNewCommand => addNewCommand ??= new RelayCommand(AddNew);

    private void AddNew()
    {
        Debug.WriteLine("AddNew command executed!");

        //NavigationService.NavigateTo(typeof(RentLivingEditShellViewModel).FullName!, "test");

        eventAddNew?.Invoke(this, "asdf");
    }


}
