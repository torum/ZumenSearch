using Microsoft.UI.Xaml.Controls;
using ZumenSearch.ViewModels.RentLivingEdit;

namespace ZumenSearch.Views.RentLivingEdit;

public sealed partial class RentLivingEditKasinusiPage : Page
{
    public RentLivingEditKasinusiViewModel ViewModel
    {
        get;
    }

    public RentLivingEditKasinusiPage()
    {
        ViewModel = new RentLivingEditKasinusiViewModel();//App.GetService<RentLivingEditZumenViewModel>();
        InitializeComponent();
    }
}
