using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace ZumenSearch.Presentation.Features.Rent.Commercials.Views;

public partial class PersonTemplateSelector : DataTemplateSelector
{
    public DataTemplate? NaturalTemplate { get; set; }
    public DataTemplate? LegalTemplate { get; set; }

    protected override DataTemplate? SelectTemplateCore(object item)
    {
        if (item is ZumenSearch.Presentation.Features.Rent.Commercials.ViewModels.PropertyPersonViewModel lessor)
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
        else if (item is ZumenSearch.Presentation.Features.Rent.Commercials.ViewModels.ListingPersonViewModel listingPerson)
        {
            if (listingPerson.Person is ZumenSearch.Domain.Entities.Person.PersonNatural)
            {
                return NaturalTemplate;
            }
            else if (listingPerson.Person is ZumenSearch.Domain.Entities.Person.PersonLegal)
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
