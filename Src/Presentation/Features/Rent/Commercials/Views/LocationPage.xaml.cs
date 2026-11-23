using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace ZumenSearch.Presentation.Features.Rent.Commercials.Views;

public sealed partial class LocationPage : Page
{
    public ZumenSearch.Presentation.Features.Rent.Commercials.ViewModels.PropertyViewModel? ViewModel { get; private set; }

    public LocationPage()
    {
        //ViewModel = new LocationViewModel();//App.GetService<RentLivingEditLocationViewModel>();
        InitializeComponent();

    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        if (e.Parameter is ZumenSearch.Presentation.Features.Rent.Commercials.ViewModels.PropertyViewModel vm)
        {
            ViewModel = vm;
            Bindings.Update();
        }

        base.OnNavigatedTo(e);
    }

    private void ComboBoxPref_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (ComboBoxPref.SelectedIndex > -1)
        {
            ComboBoxCountyAndCity.IsEnabled = true;
        }
        else
        {
            ComboBoxCountyAndCity.IsEnabled = false;
        }
    }

    private void ComboBoxCountyAndCity_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (ComboBoxCountyAndCity.SelectedIndex > -1)
        {
            ComboBoxWardAndOaza.IsEnabled = true;
        }
        else
        {
            ComboBoxWardAndOaza.IsEnabled = false;
        }
    }

    private void ComboBoxWardAndOaza_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (ComboBoxWardAndOaza.SelectedIndex > -1)
        {
            ComboBoxChoume.IsEnabled = true;
        }
        else
        {
            ComboBoxChoume.IsEnabled = false;
        }
    }

}
