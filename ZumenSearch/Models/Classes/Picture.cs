using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ZumenSearch.Common;

namespace ZumenSearch.Models.Classes
{

    // TODO:

    /// <summary>
    /// 物件の写真の基底クラス
    /// </summary>
    public class RentPicture : ViewModelBase
    {
        protected string _rentPictureId;
        public string RentPictureId
        {
            get
            {
                return _rentPictureId;
            }
        }

        protected string _rentId;
        public string RentId
        {
            get
            {
                return _rentId;
            }
        }

        // For display.
        private ImageSource _picture;
        public ImageSource Picture
        {
            get
            {
                return _picture;
            }
            set
            {
                if (_picture == value) return;

                _picture = value;
                this.NotifyPropertyChanged("Picture");
            }
        }

        private byte[] _pictureThumbW200xData;
        public byte[] PictureThumbW200xData
        {
            get
            {
                return _pictureThumbW200xData;
            }
            set
            {
                if (_pictureThumbW200xData == value) return;

                _pictureThumbW200xData = value;
                this.NotifyPropertyChanged("PictureThumbW200xData");
            }
        }

        private byte[] _pictureData;
        public byte[] PictureData
        {
            get
            {
                return _pictureData;
            }
            set
            {
                if (_pictureData == value) return;

                _pictureData = value;
                this.NotifyPropertyChanged("PictureData");
            }
        }

        private string _pictureFileExt;
        public string PictureFileExt
        {
            get
            {
                return _pictureFileExt;
            }
            set
            {
                if (_pictureFileExt == value) return;

                _pictureFileExt = value;
                this.NotifyPropertyChanged("PictureFileExt");
            }
        }

        // 新規追加された画像（要保存）
        public bool IsNew { get; set; }

        // 画像が差し替えなど、変更された（要保存）
        public bool IsModified { get; set; }

    }

    /// <summary>
    /// 賃貸住居用物件の写真クラス（外観等）
    /// </summary>
    public class RentLivingPicture : RentPicture
    {
        protected string _rentLivingId;
        public string RentLivingId
        {
            get
            {
                return _rentLivingId;
            }
        }

        public RentLivingPicture(string rentid, string rentlivingid, string rentlivingpictureid)
        {
            this._rentId = rentid;
            this._rentLivingId = rentlivingid;

            this._rentPictureId = rentlivingpictureid;
        }
    }



    /// <summary>
    /// 部屋・区画の写真基底クラス
    /// </summary>
    public class RentSectionPicture : ViewModelBase
    {
        protected string _rentSectionPictureId;
        public string RentSectionPictureId
        {
            get
            {
                return _rentSectionPictureId;
            }
        }

        protected string _rentId;
        public string RentId
        {
            get
            {
                return _rentId;
            }
        }

        // For display.
        private ImageSource _picture;
        public ImageSource Picture
        {
            get
            {
                return _picture;
            }
            set
            {
                if (_picture == value) return;

                _picture = value;
                this.NotifyPropertyChanged("Picture");
            }
        }

        private byte[] _pictureThumbW200xData;
        public byte[] PictureThumbW200xData
        {
            get
            {
                return _pictureThumbW200xData;
            }
            set
            {
                if (_pictureThumbW200xData == value) return;

                _pictureThumbW200xData = value;
                this.NotifyPropertyChanged("PictureThumbW200xData");
            }
        }

        private byte[] _pictureData;
        public byte[] PictureData
        {
            get
            {
                return _pictureData;
            }
            set
            {
                if (_pictureData == value) return;

                _pictureData = value;
                this.NotifyPropertyChanged("PictureData");
            }
        }

        private string _pictureFileExt;
        public string PictureFileExt
        {
            get
            {
                return _pictureFileExt;
            }
            set
            {
                if (_pictureFileExt == value) return;

                _pictureFileExt = value;
                this.NotifyPropertyChanged("PictureFileExt");
            }
        }

        // 新規に追加されたので、まだDBに保存されていない。
        public bool IsNew { get; set; }

        // 保存されていてIDは固定だが、内容が変更されているのでUPDATEが必要。
        public bool IsModified { get; set; }

    }

    /// <summary>
    /// 賃貸住居用物件の写真クラス（室内・設備写真等）
    /// </summary>
    public class RentLivingSectionPicture : RentSectionPicture
    {
        protected string _rentLivingSectionId;
        public string RentLivingSectionId
        {
            get
            {
                return _rentLivingSectionId;
            }
        }

        protected string _rentLivingId;
        public string RentLivingId
        {
            get
            {
                return _rentLivingId;
            }
        }

        public RentLivingSectionPicture(string rentid, string rentlivingid, string rentlivingsectionid, string rentlivingsectionpictureid)
        {
            this._rentId = rentid;
            this._rentLivingId = rentlivingid;
            this._rentLivingSectionId = rentlivingsectionid;

            this._rentSectionPictureId = rentlivingsectionpictureid;
        }
    }


}
