using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace ZumenSearch.Views.Rent.Residentials;

public partial class PersonTemplateSelector : DataTemplateSelector
{
    public DataTemplate? NaturalTemplate { get; set; }
    public DataTemplate? LegalTemplate { get; set; }

    protected override DataTemplate? SelectTemplateCore(object item)
    {
        if (item is Models.Rent.Residentials.PersonWrapperForPropertyViewModel lessor)
        {
            if (lessor.Person is Models.Person.NaturalPerson)
            {
                return NaturalTemplate;
            }
            else if (lessor.Person is Models.Person.LegalPerson)
            {
                return LegalTemplate;
            }
        }
        else if (item is Models.Person.NaturalPerson)
        {
            return NaturalTemplate;
        }
        else if (item is Models.Person.LegalPerson)
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
    public ViewModels.Rent.Residentials.PropertyViewModel? ViewModel { get; private set; }


    public LessorListPage()
    {
        InitializeComponent();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        if (e.Parameter is ViewModels.Rent.Residentials.PropertyViewModel vm)
        {
            ViewModel = vm;
            Bindings.Update();
        }

        base.OnNavigatedTo(e);
    }

}
