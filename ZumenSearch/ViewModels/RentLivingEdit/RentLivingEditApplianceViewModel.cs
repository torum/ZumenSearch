using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ZumenSearch.ViewModels.RentLivingEdit;

public class RentLivingEditApplianceViewModel : ObservableRecipient
{
    public RentLivingEditApplianceViewModel()
    {
        Debug.WriteLine("RentLivingEditApplianceViewModel init!");
    }
}
