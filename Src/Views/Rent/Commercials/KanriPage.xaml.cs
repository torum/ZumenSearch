using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace ZumenSearch.Views.Rent.Commercials;

public sealed partial class KanriPage : Page
{
    public ViewModels.Rent.Commercials.PropertyViewModel? ViewModel { get; private set; }

    public KanriPage()
    {
        InitializeComponent();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        if ((e.Parameter is ViewModels.Rent.Commercials.PropertyViewModel) && (e.Parameter != null))
        {
            ViewModel = e.Parameter as ViewModels.Rent.Commercials.PropertyViewModel;
            Bindings.Update();
        }

        base.OnNavigatedTo(e);
    }
}
