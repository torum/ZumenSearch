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
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Xml;
using System.Xml.Linq;
using ZumenSearch.Common;
using ZumenSearch.ViewModels;


namespace ZumenSearch.Models
{
    public enum ResultTypes
    {
        Owner, RentLiving, RentBussiness, RentParking
    }

    public class MainSearchResult : ViewModelBase
    {
        protected string _indexId;
        public string IndexId
        {
            get
            {
                return _indexId;
            }
        }

        protected string _dataId;
        public string DataId
        {
            get
            {
                return _dataId;
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
                this.NotifyPropertyChanged(nameof(Name));
            }
        }

        private ResultTypes _resultType;
        public ResultTypes ResultType
        {
            get
            {
                return _resultType;
            }
            set
            {
                if (_resultType == value) return;

                _resultType = value;
                this.NotifyPropertyChanged(nameof(ResultType));
            }
        }

        public Dictionary<ResultTypes, string> ResultTypeToLabel { get; } = new Dictionary<ResultTypes, string>()
        {
            {ResultTypes.Owner, "オーナー（貸主）"},
            {ResultTypes.RentLiving, "賃貸（住居用）"},
            {ResultTypes.RentBussiness, "賃貸（事業用）"},
            {ResultTypes.RentParking, "駐車場"},
        };

        public string ResultTypeLabel
        {
            get
            {
                return ResultTypeToLabel[_resultType];
            }
        }

        private string _location;
        public string Location
        {
            get
            {
                return _location;
            }
            set
            {
                if (_location == value) return;

                _location = value;
                this.NotifyPropertyChanged(nameof(Location));
            }
        }

        private string _trainLine;
        public string TrainLine
        {
            get
            {
                return _trainLine;
            }
            set
            {
                if (_trainLine == value) return;

                _trainLine = value;
                this.NotifyPropertyChanged(nameof(TrainLine));
            }
        }

        private string _trainStation;
        public string TrainStation
        {
            get
            {
                return _trainStation;
            }
            set
            {
                if (_trainStation == value) return;

                _trainStation = value;
                this.NotifyPropertyChanged(nameof(TrainStation));
            }
        }


        // コンストラクタ
        public MainSearchResult(string id, string dataid, ResultTypes type)
        {
            _indexId = id;
            _dataId = dataid;
            _resultType = type;
        }
    }
}