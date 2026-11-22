#pragma warning disable IDE0079 // Remove unnecessary suppression
#pragma warning disable IDE0290 // Use primary constructor

using CommunityToolkit.Mvvm.ComponentModel;

namespace ZumenSearch.Presentation.Features.Rent.Residentials.ViewModels;

// WinUI3 workaround. (to access viewmodel from inside itemrepeater's DataTemplate)
public sealed partial class PropertyPersonViewModel : ObservableObject // needs to be Observable in order to update Person value.
{
    public ZumenSearch.Presentation.Features.Rent.Residentials.ViewModels.PropertyViewModel ParentViewModel { get; }

    public ZumenSearch.Domain.Entities.Base.PersonBase Person
    {
        get;
        set
        {
            if (SetProperty(ref field, value))
            {
                //Debug.WriteLine("PropertyPersonViewModel's Person updated");
            }
        }
    }

    public PropertyPersonViewModel(ZumenSearch.Domain.Entities.Base.PersonBase person, ZumenSearch.Presentation.Features.Rent.Residentials.ViewModels.PropertyViewModel parentPropertyViewModel)
    {
        Person = person;
        ParentViewModel = parentPropertyViewModel;
    }
}
