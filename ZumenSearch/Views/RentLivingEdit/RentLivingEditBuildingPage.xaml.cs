using System.Diagnostics;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using ZumenSearch.ViewModels.RentLivingEdit;

namespace ZumenSearch.Views.RentLivingEdit;

public sealed partial class RentLivingEditBuildingPage : Page
{
    public RentLivingEditBuildingViewModel ViewModel
    {
        get;
    }

    public RentLivingEditBuildingPage()
    {
        ViewModel = new RentLivingEditBuildingViewModel();//App.GetService<RentLivingEditBuildingViewModel>();
        InitializeComponent();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        if (e.Parameter is string && !string.IsNullOrWhiteSpace((string)e.Parameter))
        {
            Debug.WriteLine("------------------------" + e.Parameter.ToString());
        }
        else if (e.Parameter is Frame)
        {
            Debug.WriteLine("========================");
        }
        else
        {

            Debug.WriteLine("------------------------"+e.Content);
        }
        base.OnNavigatedTo(e);
    }

}
