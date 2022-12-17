using System.Collections.ObjectModel;
using Microsoft.UI.Xaml.Controls;

using ZumenSearch.ViewModels;

namespace ZumenSearch.Views;

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

public sealed partial class RentMainPage : Page
{



    public RentMainViewModel ViewModel
    {
        get;
    }

    public RentMainPage()
    {
        ViewModel = App.GetService<RentMainViewModel>();
        InitializeComponent();

        BreadcrumbBarMain.ItemsSource = new string[] { "総合検索" };

        ObservableCollection<CustomDataObject> Items = new ObservableCollection<CustomDataObject>();

        var temp = new CustomDataObject();
        temp.Title = "test";

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
