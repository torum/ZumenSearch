using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using ZumenSearch.Models;
using ZumenSearch.ViewModels;
using ZumenSearch.ViewModels.Rent;
using ZumenSearch.ViewModels.Rent.Residentials;

namespace ZumenSearch.Views.Rent
{
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

    public sealed partial class RentSearchPage : Page
    {
        private MainViewModel? ViewModel { get; set; }

        public RentSearchPage()
        {
            ViewModel = App.GetService<MainViewModel>();

            InitializeComponent();

            //BreadcrumbBarMain.ItemsSource = new string[] { "ëççáåüçı" };
            BreadcrumbBarMain.ItemsSource = new ObservableCollection<Breadcrumbs>{
            new() { Name = "í¿ë›", Page = typeof(RentSearchPage).FullName!},
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
}
