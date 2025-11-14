using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Linq;
using Microsoft.Data.Sqlite;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;
using ZumenSearch.Models;
using static Microsoft.Extensions.Logging.EventSource.LoggingEventSource;

namespace ZumenSearch.Views.Rent.Residentials.Bldg;

public sealed partial class LocationPage : Page
{
    public ViewModels.Rent.Residentials.ResidentialsViewModel? ViewModel;

    public LocationPage()
    {
        //ViewModel = new LocationViewModel();//App.GetService<RentLivingEditLocationViewModel>();
        InitializeComponent();

    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        if (e.Parameter is ViewModels.Rent.Residentials.ResidentialsViewModel vm)
        {
            ViewModel = vm;
        }

        base.OnNavigatedTo(e);
    }

    private void ComboBoxPref_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (ComboBoxPref.SelectedIndex > -1)
        {
            ComboBoxCountyAndCity.IsEnabled = true;
        }
        else
        {
            ComboBoxCountyAndCity.IsEnabled = false;
        }
    }

    private void ComboBoxCountyAndCity_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (ComboBoxCountyAndCity.SelectedIndex > -1)
        {
            ComboBoxWardAndOaza.IsEnabled = true;
        }
        else
        {
            ComboBoxWardAndOaza.IsEnabled = false;
        }
    }

    private void ComboBoxWardAndOaza_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (ComboBoxWardAndOaza.SelectedIndex > -1)
        {
            ComboBoxChoume.IsEnabled = true;
        }
        else
        {
            ComboBoxChoume.IsEnabled = false;
        }
    }

}
