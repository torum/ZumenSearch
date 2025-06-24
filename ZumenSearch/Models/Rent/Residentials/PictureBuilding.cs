using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZumenSearch.Views.Rent.Residentials.Editor.Modal;

namespace ZumenSearch.Models.Rent.Residentials
{
    public partial class PictureBuilding : PictureBase
    {
        // TODO: Make this combobox selection.
        private string _title = string.Empty;
        public string Title
        {
            get => _title;
            set
            {
                if (SetProperty(ref _title, value))
                {
                }
            }
        }

        private string _description = string.Empty;
        public string Description
        {
            get => _description;
            set
            {
                if (SetProperty(ref _description, value))
                {
                }
            }
        }

        private bool _isMain;
        public bool IsMain
        {
            get => _isMain;
            set
            {
                if (SetProperty(ref _isMain, value))
                {
                }
            }
        }

        public PictureBuilding(string imageLocation)
        {
            ImageLocation = imageLocation;
        }

    };
}
