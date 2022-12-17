using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ZumenSearch.ViewModels;

public class RealEstateBrokerViewModel : ObservableRecipient
{
    public RealEstateBrokerViewModel()
    {
        Debug.WriteLine("RealEstateBrokerViewModel init!");
    }
}
