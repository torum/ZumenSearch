using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using ZumenSearch.ViewModels;

namespace ZumenSearch.Views.Rent.Residentials.Editor.Modal;

public sealed partial class ContractPage : Page
{
    public ViewModels.Rent.Residentials.Editor.Modal.ContractViewModel ViewModel
    {
        get;
    }

    public ContractPage()
    {
        ViewModel = new ViewModels.Rent.Residentials.Editor.Modal.ContractViewModel();
        InitializeComponent();
    }
}
