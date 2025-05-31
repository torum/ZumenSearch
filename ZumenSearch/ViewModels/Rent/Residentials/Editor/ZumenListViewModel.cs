using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ZumenSearch.ViewModels.Rent.Residentials.Editor;

public class ZumenListViewModel : ObservableRecipient
{
    public ZumenListViewModel()
    {
        Debug.WriteLine("RentLivingEditZumenViewModel init!");
    }
}
