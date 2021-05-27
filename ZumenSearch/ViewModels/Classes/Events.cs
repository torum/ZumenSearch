using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZumenSearch.Models;
using ZumenSearch.ViewModels;
using ZumenSearch.ViewModels.Classes;
using ZumenSearch.Models.Classes;
using ZumenSearch.Views;
using ZumenSearch.Common;

namespace ZumenSearch.ViewModels.Classes
{
    public class OpenRentLivingWindowEventArgs : EventArgs
    {
        public RentLiving EditObject { get; set; }

        public string Id { get; set; }
    }

    public class OpenRentLivingSectionWindowEventArgs : EventArgs
    {
        public RentLivingSection EditObject { get; set; }

        public string Id { get; set; }
    }

}
