using Microsoft.UI.Xaml.Controls;
using ZumenSearch.ViewModels.Rent.Residentials.Editor;

namespace ZumenSearch.Views.Rent.Residentials.Editor;

public sealed partial class KasinusiPage : Page
{
    public KasinusiViewModel ViewModel
    {
        get;
    }

    public KasinusiPage()
    {
        ViewModel = new KasinusiViewModel();//App.GetService<RentLivingEditZumenViewModel>();
        InitializeComponent();
    }
}
