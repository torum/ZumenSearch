using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Diagnostics;

namespace ZumenSearch.ViewModels.Rent.Residentials.Editor;

public partial class StructureViewModel : ObservableRecipient
{
    public StructureViewModel()
    {
        Debug.WriteLine("StructureViewModel init!");
    }

    public event EventHandler<string>? EventGoBack;

    private RelayCommand? goBackCommand;

    public IRelayCommand BackCommand => goBackCommand ??= new RelayCommand(GoBack);


    private void GoBack()
    {
        Debug.WriteLine("Back command executed!");
        // Navigate to the BuildingPage
        EventGoBack?.Invoke(this, "asdf");
    }
}
