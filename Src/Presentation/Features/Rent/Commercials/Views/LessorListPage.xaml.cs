using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace ZumenSearch.Presentation.Features.Rent.Commercials.Views;

public sealed partial class LessorListPage : Page
{
    public ZumenSearch.Presentation.Features.Rent.Commercials.ViewModels.PropertyViewModel? ViewModel { get; private set; }


    public LessorListPage()
    {
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

}
