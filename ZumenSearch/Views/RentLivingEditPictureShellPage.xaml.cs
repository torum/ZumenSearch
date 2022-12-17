using Microsoft.UI.Xaml.Controls;

using ZumenSearch.ViewModels;

namespace ZumenSearch.Views;

public sealed partial class RentLivingEditPictureShellPage : Page
{
    public RentLivingEditPictureShellViewModel ViewModel
    {
        get;
    }

    public RentLivingEditPictureShellPage()
    {
        ViewModel = App.GetService<RentLivingEditPictureShellViewModel>();
        InitializeComponent();
    }
}
