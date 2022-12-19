using Microsoft.UI.Xaml.Controls;
using ZumenSearch.ViewModels.RentLivingEdit;

namespace ZumenSearch.Views.RentLivingEdit;

public sealed partial class RentLivingEditGyousyaPage : Page
{
    public RentLivingEditGyousyaViewModel ViewModel
    {
        get;
    }

    public RentLivingEditGyousyaPage()
    {
        ViewModel = new RentLivingEditGyousyaViewModel();//App.GetService<RentLivingEditZumenViewModel>();
        InitializeComponent();
    }
}
