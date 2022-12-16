using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ZumenSearch.ViewModels;

public class RentLivingEditBuildingViewModel : ObservableRecipient
{
    public RentLivingEditBuildingViewModel()
    {
        Debug.WriteLine("RentLivingEditBuildingViewModel init!");
    }
}
