using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ZumenSearch.ViewModels;

public class RentLivingEditLocationViewModel : ObservableRecipient
{
    public RentLivingEditLocationViewModel()
    {
        Debug.WriteLine("RentLivingEditLocationViewModel init!");
    }
}
