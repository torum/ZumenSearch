using Microsoft.UI.Xaml.Controls;
using ZumenSearch.ViewModels;


namespace ZumenSearch.Views.Rent.Residentials.Editor.Modal;

public sealed partial class KasinusiPage : Page
{
    public ViewModels.Rent.Residentials.Editor.Modal.KasinusiViewModel ViewModel
    {
        get;
    }

    public KasinusiPage()
    {
        ViewModel = new ViewModels.Rent.Residentials.Editor.Modal.KasinusiViewModel();
        InitializeComponent();
    }
}
