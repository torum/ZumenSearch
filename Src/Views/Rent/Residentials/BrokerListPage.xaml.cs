using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Navigation;

namespace ZumenSearch.Views.Rent.Residentials;

public sealed partial class BrokerListPage : Page
{
    public ViewModels.Rent.Residentials.PropertyViewModel? ViewModel { get; private set; }

    public BrokerListPage()
    {
        //ViewModel = new GyousyaViewModel();//App.GetService<RentLivingEditZumenViewModel>();
        InitializeComponent();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        if ((e.Parameter is ViewModels.Rent.Residentials.PropertyViewModel) && (e.Parameter != null))
        {
            ViewModel = e.Parameter as ViewModels.Rent.Residentials.PropertyViewModel;

            // comment out when x:Bind expressions is added.
            //Bindings.Update();
        }

        base.OnNavigatedTo(e);
    }
}
