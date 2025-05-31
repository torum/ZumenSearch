using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ZumenSearch.ViewModels.Rent.Residentials.Editor;

public class ApplianceViewModel : ObservableRecipient
{
    public ApplianceViewModel()
    {
        Debug.WriteLine("RentLivingEditApplianceViewModel init!");
    }
}
