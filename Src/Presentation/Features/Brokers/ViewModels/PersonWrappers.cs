using CommunityToolkit.Mvvm.ComponentModel;

namespace ZumenSearch.Presentation.Features.Brokers.ViewModels;

#pragma warning disable IDE0079 // Remove unnecessary suppression
#pragma warning disable IDE0290 // Use primary constructor

// TODO: See if reuse this with lessor

// WinUI3 workaround. (to access viewmodel from inside itemrepeater's DataTemplate)
public sealed partial class PersonWrapperForPropertyViewModel : ObservableObject // needs to be Observable in order to update Person value.
{
    public ZumenSearch.Presentation.Features.Brokers.ViewModels.BrokerViewModel ParentViewModel { get;  }

    public ZumenSearch.Domain.Entities.Base.PersonBase Person
    {
        get;
        set
        {
            if (SetProperty(ref field, value))
            {
                //Debug.WriteLine("PersonWrapperForPropertyViewModel's Person updated");
            }
        }
    }

    public PersonWrapperForPropertyViewModel(ZumenSearch.Domain.Entities.Base.PersonBase person, ZumenSearch.Presentation.Features.Brokers.ViewModels.BrokerViewModel parentPropertyViewModel)
    {
        Person = person;
        ParentViewModel = parentPropertyViewModel;
    }
}

// WinUI3 workaround. (to access viewmodel from inside itemrepeater's DataTemplate)
public sealed partial class PersonWrapperForListingViewModel : ObservableObject // needs to be Observable in order to update Person value.
{
    public ZumenSearch.Presentation.Features.Brokers.ViewModels.BrokerViewModel ParentViewModel { get; }

    public ZumenSearch.Domain.Entities.Base.PersonBase Person
    {
        get;
        set
        {
            if (SetProperty(ref field, value))
            {
                //Debug.WriteLine("PersonWrapperForListingViewModel's Person updated");
            }
        }
    }

    public PersonWrapperForListingViewModel(ZumenSearch.Domain.Entities.Base.PersonBase person, ZumenSearch.Presentation.Features.Brokers.ViewModels.BrokerViewModel parentBrokerViewModel)
    {
        Person = person;
        ParentViewModel = parentBrokerViewModel;
    }
}