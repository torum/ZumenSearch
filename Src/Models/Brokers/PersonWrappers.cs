using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using System.Diagnostics;
using ZumenSearch.Models.Base;

namespace ZumenSearch.Models.Brokers;

#pragma warning disable IDE0079 // Remove unnecessary suppression
#pragma warning disable IDE0290 // Use primary constructor


// WinUI3 workaround. (to access viewmodel from inside itemrepeater's DataTemplate)
public sealed partial class PersonWrapperForPropertyViewModel : ObservableObject // needs to be Observable in order to update Person value.
{
    public ViewModels.Brokers.BrokerViewModel ParentViewModel { get;  }

    public Models.Base.PersonBase Person
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

    public PersonWrapperForPropertyViewModel(Models.Base.PersonBase person, ViewModels.Brokers.BrokerViewModel parentPropertyViewModel)
    {
        Person = person;
        ParentViewModel = parentPropertyViewModel;
    }
}

// WinUI3 workaround. (to access viewmodel from inside itemrepeater's DataTemplate)
public sealed partial class PersonWrapperForListingViewModel : ObservableObject // needs to be Observable in order to update Person value.
{
    public ViewModels.Brokers.BrokerViewModel ParentViewModel { get; }

    public Models.Base.PersonBase Person
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

    public PersonWrapperForListingViewModel(Models.Base.PersonBase person, ViewModels.Brokers.BrokerViewModel parentBrokerViewModel)
    {
        Person = person;
        ParentViewModel = parentBrokerViewModel;
    }
}