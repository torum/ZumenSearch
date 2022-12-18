using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ZumenSearch.ViewModels;

public class RentBussinessViewModel : ObservableRecipient
{
    public RentBussinessViewModel()
    {
        Debug.WriteLine("RentBussinessViewModel init!");
    }
}
