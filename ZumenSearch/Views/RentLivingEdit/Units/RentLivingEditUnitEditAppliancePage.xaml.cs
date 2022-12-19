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
using ZumenSearch.ViewModels.RentLivingEdit.Units;

namespace ZumenSearch.Views.RentLivingEdit.Units;

public sealed partial class RentLivingEditUnitEditAppliancePage : Page
{
    public RentLivingEditUnitEditApplianceViewModel ViewModel
    {
        get;

    }
    public RentLivingEditUnitEditAppliancePage()
    {
        ViewModel = new RentLivingEditUnitEditApplianceViewModel();
        InitializeComponent();
    }
}
