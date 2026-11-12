using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Navigation;

namespace ZumenSearch.Views.Sale.Residentials;

public sealed partial class BasicPage : Page
{
    public ViewModels.Sale.Residentials.PropertyViewModel? ViewModel
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
        if (e.Parameter is ViewModels.Sale.Residentials.PropertyViewModel vm)
        {
            ViewModel = vm;
            Bindings.Update();
        }

        base.OnNavigatedTo(e);
    }
}