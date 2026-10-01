using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace ZumenSearch.Views.Rent.Commercials;

public sealed partial class LocationPage : Page
{
    public LocationPage()
    {
        //ViewModel = new LocationViewModel();//App.GetService<RentLivingEditLocationViewModel>();
        InitializeComponent();

    }

    public ViewModels.Rent.Commercials.PropertyViewModel? ViewModel { get; private set; }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        if (e.Parameter is ViewModels.Rent.Commercials.PropertyViewModel vm)
        {
            ViewModel = vm;
            Bindings.Update();
        }

        base.OnNavigatedTo(e);
    }

    private void ComboBoxPref_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        ComboBoxCountyAndCity.IsEnabled =
            ComboBoxPref.SelectedIndex > -1;

        if (ComboBoxPref.SelectedIndex < 0)
        {
            ComboBoxWardAndOaza.IsEnabled = false;
            ComboBoxChoume.IsEnabled = false;
        }
    }

    private void ComboBoxCountyAndCity_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        ComboBoxWardAndOaza.IsEnabled =
            ComboBoxCountyAndCity.SelectedIndex > -1;

        if (ComboBoxCountyAndCity.SelectedIndex < 0)
        {
            ComboBoxChoume.IsEnabled = false;
        }
    }

    private void ComboBoxWardAndOaza_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        ComboBoxChoume.IsEnabled =
                    ComboBoxWardAndOaza.SelectedIndex > -1;
    }

}
