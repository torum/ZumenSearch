using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RepsCore.Common;

namespace RepsCore.Models.Classes
{
    /// <summary>
    /// オーナークラス
    /// </summary>
    public class Owner : ViewModelBase
    {
        // GUID and Primary Key
        protected string _owner_id;
        public string Owner_ID
        {
            get
            {
                return _owner_id;
            }
        }

        private string _name;
        public string Name
        {
            get
            {
                return _name;
            }
            set
            {
                if (_name == value) return;

                _name = value;
                this.NotifyPropertyChanged("Name");
            }
        }

        private string _telNumber;
        public string TelNumber
        {
            get
            {
                return _telNumber;
            }
            set
            {
                if (_telNumber == value) return;

                _telNumber = value;
                this.NotifyPropertyChanged("TelNumber");
            }
        }

        private string _faxNumber;
        public string FaxNumber
        {
            get
            {
                return _faxNumber;
            }
            set
            {
                if (_faxNumber == value) return;

                _faxNumber = value;
                this.NotifyPropertyChanged("FaxNumber");
            }
        }

        private string _postalCode;
        public string PostalCode
        {
            get
            {
                return _postalCode;
            }
            set
            {
                if (_postalCode == value) return;

                _postalCode = value;
                this.NotifyPropertyChanged("PostalCode");
            }
        }

        private string _address;
        public string Address
        {
            get
            {
                return _address;
            }
            set
            {
                if (_address == value) return;

                _address = value;
                this.NotifyPropertyChanged("Address");
            }
        }

        private string _memo;
        public string Memo
        {
            get
            {
                return _memo;
            }
            set
            {
                if (_memo == value) return;

                _memo = value;
                this.NotifyPropertyChanged("Memo");
            }
        }
        public bool IsNew { get; set; }

        public bool IsDirty { get; set; }

        public Owner(string ownerid)
        {

            this._owner_id = ownerid;
        }
    }

}
