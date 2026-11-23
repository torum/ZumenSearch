#pragma warning disable IDE0079 // Remove unnecessary suppression
#pragma warning disable IDE0290 // Use primary constructor

using CommunityToolkit.Mvvm.ComponentModel;

namespace ZumenSearch.Presentation.Features.Rent.Commercials.ViewModels;

// WinUI3 workaround. (to access viewmodel from inside itemrepeater's DataTemplate)
public sealed partial class ListingPersonViewModel : ObservableObject // needs to be Observable in order to update Person value.
{
    public ZumenSearch.Presentation.Features.Rent.Commercials.ViewModels.ListingViewModel ParentViewModel { get; }

    public ZumenSearch.Domain.Entities.Base.PersonBase Person
    {
        get;
        set
        {
            if (SetProperty(ref field, value))
            {
                //Debug.WriteLine("ListingPersonViewModel's Person updated");
            }
        }
    }

    public ListingPersonViewModel(ZumenSearch.Domain.Entities.Base.PersonBase person, ZumenSearch.Presentation.Features.Rent.Commercials.ViewModels.ListingViewModel parentListingViewModel)
    {
        Person = person;
        ParentViewModel = parentListingViewModel;
    }
}
