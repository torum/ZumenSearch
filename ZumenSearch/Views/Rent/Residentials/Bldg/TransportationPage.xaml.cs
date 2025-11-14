using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;
using System.Collections.ObjectModel;
using System.Diagnostics;
using ZumenSearch.Models;
using ZumenSearch.ViewModels.Rent.Residentials;

namespace ZumenSearch.Views.Rent.Residentials.Bldg;

public sealed partial class TransportationPage : Page
{
    private ViewModels.Rent.Residentials.ResidentialsViewModel? _viewModel;
    public ViewModels.Rent.Residentials.ResidentialsViewModel? ViewModel
    {
        get => _viewModel;
        private set
        {
            if (value != null)
            {
                _viewModel = value;
            }
        }
    }

    public TransportationPage()
    {
        InitializeComponent();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        if ((e.Parameter is ViewModels.Rent.Residentials.ResidentialsViewModel) && (e.Parameter != null))
        {
            ViewModel = e.Parameter as ViewModels.Rent.Residentials.ResidentialsViewModel;
        }

        base.OnNavigatedTo(e);
    }

    private void TextBoxRailLine1_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (string.IsNullOrEmpty(TextBoxRailLine1.Text))
        {
            TextBoxRailLine1.IsEnabled = false;
        }
        else
        {
            TextBoxRailLine1.IsEnabled = true;
        }
    }

    private void TextBoxRailStation1_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (string.IsNullOrEmpty(TextBoxRailStation1.Text))
        {
            TextBoxRailStation1.IsEnabled = false;
        }
        else
        {
            TextBoxRailStation1.IsEnabled = true;
        }
    }
}
