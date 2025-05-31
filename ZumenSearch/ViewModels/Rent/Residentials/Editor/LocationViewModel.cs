using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ZumenSearch.ViewModels.Rent.Residentials.Editor;

public class LocationViewModel : ObservableRecipient
{
    public LocationViewModel()
    {
        Debug.WriteLine("RentLivingEditLocationViewModel init!");
    }
}
