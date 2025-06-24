using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Xml;
using System.Xml.Linq;
using ZumenSearch.ViewModels;

namespace ZumenSearch.Models
{
    public abstract class PictureBase : ObservableObject
    {
        public string? ImageLocation { get; set; } 

        public string? Id { get; set; }

        public bool IsNew { get; set; }
        public bool IsModified { get; set; }

        protected PictureBase()
        {

        }
    };
}
