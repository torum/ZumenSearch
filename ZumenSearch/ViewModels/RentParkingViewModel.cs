using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ZumenSearch.ViewModels;

public class RentParkingViewModel : ObservableRecipient
{
    public RentParkingViewModel()
    {
        Debug.WriteLine("RentParkingViewModel init!");
    }
}
