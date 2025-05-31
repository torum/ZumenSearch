using System;
using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;


namespace ZumenSearch.ViewModels.Rent.Residentials.Editor;

public partial class UnitListViewModel : ObservableRecipient
{
    public UnitListViewModel()
    {
        Debug.WriteLine("RentLivingEditUnitListViewModel init!");
    }

    public event EventHandler<string>? EventAddNew;

    private RelayCommand? addNewCommand;

    public IRelayCommand AddNewCommand => addNewCommand ??= new RelayCommand(AddNew);

    private void AddNew()
    {
        Debug.WriteLine("AddNew command executed!");

        //NavigationService.NavigateTo(typeof(RentLivingEditShellViewModel).FullName!, "test");

        EventAddNew?.Invoke(this, "asdf");
    }
}
