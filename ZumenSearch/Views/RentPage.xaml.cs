using System.Collections.ObjectModel;
using Microsoft.UI.Xaml.Controls;
using ZumenSearch.Models;
using ZumenSearch.ViewModels;

namespace ZumenSearch.Views.Rent;

public class CustomDataObject
{
    public string? Title
    {
        get; set;
    }
    public string? ImageLocation
    {
        get; set;
    }
    public string? Views
    {
        get; set;
    }
    public string? Likes
    {
        get; set;
    }
    public string? Description
    {
        get; set;
    }

    public CustomDataObject()
    {
    }


    // ... Methods ...
}

public sealed partial class RentPage : Page
{
    private MainViewModel? ViewModel { get; set; }

    public RentPage()
    {
        ViewModel = App.GetService<MainViewModel>();

        InitializeComponent();

        //BreadcrumbBarMain.ItemsSource = new string[] { "ëççáåüçı" };
        BreadcrumbBarMain.ItemsSource = new ObservableCollection<Breadcrumb>{
        new() { Name = "í¿ë›", Page = typeof(RentPage).FullName!},
    };

        var Items = new ObservableCollection<CustomDataObject>();

        var temp = new CustomDataObject
        {
            Title = "test"
        };

        Items.Add(temp);



        Items.Add(temp);
        Items.Add(temp);
        Items.Add(temp);
        Items.Add(temp);
        Items.Add(temp);
        Items.Add(temp);
        Items.Add(temp);
        Items.Add(temp);
        Items.Add(temp);
        Items.Add(temp);
        Items.Add(temp);
        Items.Add(temp);
        BasicGridView.ItemsSource = Items;
    }
}
