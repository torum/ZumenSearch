using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace ZumenSearch.Views.Rent.Commercials;

public sealed partial class UnitListPage : Page
{
    public ViewModels.Rent.Commercials.PropertyViewModel? ViewModel { get; private set; }

    public UnitListPage()
    {
        InitializeComponent();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        ViewModel = e.Parameter as ViewModels.Rent.Commercials.PropertyViewModel;
        Bindings.Update();

        base.OnNavigatedTo(e);
    }
}