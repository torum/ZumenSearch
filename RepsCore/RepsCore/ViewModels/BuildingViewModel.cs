using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RepsCore.Models;
using RepsCore.ViewModels.Classes;
using RepsCore.Common;

namespace RepsCore.ViewModels
{
    class BuildingViewModel : ViewModelBase
    {
        private string _id;
        public string Id
        {
            get
            {
                return _id;
            }
        }

        public BuildingViewModel(string id)
        {
            _id = id;
        }

    }
}
