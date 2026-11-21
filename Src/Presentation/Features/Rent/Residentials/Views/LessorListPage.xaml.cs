using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace ZumenSearch.Presentation.Features.Rent.Residentials.Views;

public partial class PersonTemplateSelector : DataTemplateSelector
{
    public DataTemplate? NaturalTemplate { get; set; }
    public DataTemplate? LegalTemplate { get; set; }

    protected override DataTemplate? SelectTemplateCore(object item)
    {
        if (item is ZumenSearch.Presentation.Features.Rent.Residentials.Models.PersonWrapperForPropertyViewModel lessor)
        {
            if (lessor.Person is ZumenSearch.Domain.Entities.Person.PersonNatural)
            {
                return NaturalTemplate;
            }
            else if (lessor.Person is ZumenSearch.Domain.Entities.Person.PersonLegal)
            {
                return LegalTemplate;
            }
        }
        else if (item is ZumenSearch.Domain.Entities.Person.PersonNatural)
        {
            return NaturalTemplate;
        }
        else if (item is ZumenSearch.Domain.Entities.Person.PersonLegal)
        {
            return LegalTemplate;
        }

        return base.SelectTemplateCore(item);
    }

    protected override DataTemplate? SelectTemplateCore(object item, DependencyObject container)
    {
        return SelectTemplateCore(item);
    }
}

public sealed partial class LessorListPage : Page
{
    public ZumenSearch.Presentation.Features.Rent.Residentials.ViewModels.PropertyViewModel? ViewModel { get; private set; }


    public LessorListPage()
    {
        InitializeComponent();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        if (e.Parameter is ZumenSearch.Presentation.Features.Rent.Residentials.ViewModels.PropertyViewModel vm)
        {
            ViewModel = vm;
            Bindings.Update();
        }

        base.OnNavigatedTo(e);
    }

}
