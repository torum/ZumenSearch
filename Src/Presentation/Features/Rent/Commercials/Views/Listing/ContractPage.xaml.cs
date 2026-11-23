using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace ZumenSearch.Presentation.Features.Rent.Commercials.Views.Listing;

public sealed partial class ContractPage : Page
{
    public ZumenSearch.Presentation.Features.Rent.Commercials.ViewModels.ListingViewModel? ViewModel { get; private set; }

    public ContractPage()
    {
        //ViewModel = new ViewZumenSearch.Presentation.Features.Rent.Commercials.Models.Editor.Modal.ContractViewModel();
        InitializeComponent();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        if (e.Parameter is ZumenSearch.Presentation.Features.Rent.Commercials.ViewModels.ListingViewModel vm)
        {
            //_editorShell = e.Parameter as ZumenSearch.Presentation.Features.Rent.Commercials.Views.Editor.EditorShell;
            ViewModel = vm;
            Bindings.Update();
        }

        base.OnNavigatedTo(e);
    }
}
