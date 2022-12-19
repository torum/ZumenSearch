﻿using AddressManagement.ViewModels;

using Microsoft.UI.Xaml.Controls;

namespace AddressManagement.Views;

// TODO: Change the grid as appropriate for your app. Adjust the column definitions on DataGridPage.xaml.
// For more details, see the documentation at https://docs.microsoft.com/windows/communitytoolkit/controls/datagrid.
public sealed partial class DataGrid1Page : Page
{
    public DataGrid1ViewModel ViewModel
    {
        get;
    }

    public DataGrid1Page()
    {
        ViewModel = App.GetService<DataGrid1ViewModel>();
        InitializeComponent();
    }
}
