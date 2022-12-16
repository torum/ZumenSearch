using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ZumenSearch.ViewModels;

public class RentOwnerViewModel : ObservableRecipient
{
    public RentOwnerViewModel()
    {
        Debug.WriteLine("RentOwnerViewModel init!");
    }
}
