using Microsoft.UI.Xaml.Controls;
using ZumenSearch.ViewModels.Rent.Parkings;

namespace ZumenSearch.Views.Rent.Parkings;

public sealed partial class ParkingsPage : Page
{
    public ParkingsViewModel ViewModel
    {
        get;
    }

    public ParkingsPage()
    {
        ViewModel = App.GetService<ParkingsViewModel>();
        InitializeComponent();
    }
}
