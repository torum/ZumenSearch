using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using System.Diagnostics;
using ZumenSearch.Models.Base;

namespace ZumenSearch.Models.Rent.Lessors;

#pragma warning disable IDE0079 // Remove unnecessary suppression
#pragma warning disable IDE0290 // Use primary constructor

// TODO: See if reuse this with broker

// TODO:
/*
public sealed partial class Person : PersonBase
{
    // Do not use SetProperty.
    public string Remarks
    {
        get;
        set
        {
            if (field == value) return;

            if (value is not null)
            {
                // Set this before raize PropertyChanged.
                IsModified = true;

                // Raize PropertyChanged event here.
                field = value;
            }
            else
            {
                field = string.Empty;
            }

            OnPropertyChanged();
        }
    } = string.Empty;

    public Person(string id, EnumEntryStatus status) : base(id, status, EnumPersonKind.Undetermined)
    {
        //IsModified = false;
    }

};
*/

// WinUI3 workaround. (to access viewmodel from inside itemrepeater's DataTemplate)
public sealed partial class PersonWrapperForPropertyViewModel : ObservableObject // needs to be Observable in order to update Person value.
{
    public ViewModels.Rent.Residentials.PropertyViewModel ParentViewModel { get;  }

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

    public PersonWrapperForPropertyViewModel(Models.Base.PersonBase person, ViewModels.Rent.Residentials.PropertyViewModel parentPropertyViewModel)
    {
        Person = person;
        ParentViewModel = parentPropertyViewModel;
    }
}

// WinUI3 workaround. (to access viewmodel from inside itemrepeater's DataTemplate)
public sealed partial class PersonWrapperForListingViewModel : ObservableObject // needs to be Observable in order to update Person value.
{
    public ViewModels.Rent.Residentials.Listing.ListingViewModel ParentViewModel { get; }

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

    public PersonWrapperForListingViewModel(Models.Base.PersonBase person, ViewModels.Rent.Residentials.Listing.ListingViewModel parentListingViewModel)
    {
        Person = person;
        ParentViewModel = parentListingViewModel;
    }
}