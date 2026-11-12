using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Navigation;

namespace ZumenSearch.Views.Rent.Commercials;

public sealed partial class BrokerListPage : Page
{
    public ViewModels.Rent.Commercials.PropertyViewModel? ViewModel { get; private set; }

    public BrokerListPage()
    {
        //ViewModel = new GyousyaViewModel();//App.GetService<RentLivingEditZumenViewModel>();
        InitializeComponent();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        if (e.Parameter is ViewModels.Rent.Commercials.PropertyViewModel vm)
        {
            ViewModel = vm;

            // comment out when x:Bind expressions is added.
            //Bindings.Update();
        }

        base.OnNavigatedTo(e);
    }
}
