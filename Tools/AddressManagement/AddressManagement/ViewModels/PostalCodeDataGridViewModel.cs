﻿using System.Collections.ObjectModel;

using AddressManagement.Contracts.ViewModels;
using AddressManagement.Core.Contracts.Services;
using AddressManagement.Core.Models;

using CommunityToolkit.Mvvm.ComponentModel;

namespace AddressManagement.ViewModels;

public class PostalCodeDataGridViewModel : ObservableRecipient, INavigationAware
{

    public ObservableCollection<PostalCode> PostalCodeDataSource { get; } = new ObservableCollection<PostalCode>();

    public PostalCodeDataGridViewModel()
    {
    }

    public async void OnNavigatedTo(object parameter)
    {
    }

    public void OnNavigatedFrom()
    {
    }
}
