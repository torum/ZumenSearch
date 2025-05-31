using Microsoft.UI.Xaml.Controls;
using ZumenSearch.ViewModels;


namespace ZumenSearch.Views.Rent.Residentials.Editor.Modal;

public sealed partial class GyousyaPage : Page
{
    public ViewModels.Rent.Residentials.Editor.Modal.GyousyaViewModel ViewModel
    {
        get;
    }

    public GyousyaPage()
    {
        ViewModel = new ViewModels.Rent.Residentials.Editor.Modal.GyousyaViewModel();
        InitializeComponent();
    }
}
