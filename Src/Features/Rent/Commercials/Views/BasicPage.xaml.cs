using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Navigation;

namespace ZumenSearch.Views.Rent.Commercials;

public sealed partial class BasicPage : Page
{
    public ViewModels.Rent.Commercials.PropertyViewModel? ViewModel
    {
        get;
        private set;
    }

    public BasicPage()
    {
        InitializeComponent();
    }

    protected override void OnNavigatedTo(
        NavigationEventArgs e)
    {
        if (e.Parameter
            is ViewModels.Rent.Commercials.PropertyViewModel vm)
        {
            ViewModel = vm;
            Bindings.Update();
        }

        base.OnNavigatedTo(e);
    }
}