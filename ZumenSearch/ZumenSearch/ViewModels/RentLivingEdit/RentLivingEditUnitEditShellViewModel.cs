using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ZumenSearch.Services;

namespace ZumenSearch.ViewModels.RentLivingEdit;

public class RentLivingEditUnitEditShellViewModel : ObservableRecipient
{
    private object? _selected;
    public object? Selected
    {
        get => _selected;
        set => SetProperty(ref _selected, value);
    }

    public RentLivingEditUnitEditShellViewModel()
    {
        Debug.WriteLine("RentLivingEditUnitEditShellViewModel init!");
    }

    public event EventHandler<string> eventGoBack;

    private RelayCommand? backCommand;

    public IRelayCommand BackCommand => backCommand ??= new RelayCommand(Back);

    private void Back()
    {
        Debug.WriteLine("Back command executed!");

        //NavigationService.NavigateTo(typeof(RentLivingSearchViewModel).FullName!);
        //NavigationService.GoBack();

        eventGoBack?.Invoke(this, "asdf");
    }

}
