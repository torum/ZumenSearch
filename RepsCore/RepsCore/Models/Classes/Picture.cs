using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using RepsCore.Common;

namespace RepsCore.Models.Classes
{

    // TODO:

    /// <summary>
    /// 物件の写真の基底クラス
    /// </summary>
    public class RentPicture : ViewModelBase
    {
        protected string _rentPicture_id;
        public string RentPicture_ID
        {
            get
            {
                return _rentPicture_id;
            }
        }

        protected string _rent_id;
        public string Rent_ID
        {
            get
            {
                return _rent_id;
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
        protected string _rentLiving_id;
        public string RentLiving_ID
        {
            get
            {
                return _rentLiving_id;
            }
        }

        public RentLivingPicture(string rentid, string rentlivingid, string rentlivingpictureid)
        {
            this._rent_id = rentid;
            this._rentLiving_id = rentlivingid;

            this._rentPicture_id = rentlivingpictureid;
        }
    }



    /// <summary>
    /// 部屋・区画の写真基底クラス
    /// </summary>
    public class RentSectionPicture : ViewModelBase
    {
        protected string _rentSectionPicture_id;
        public string RentSectionPicture_ID
        {
            get
            {
                return _rentSectionPicture_id;
            }
        }

        protected string _rent_id;
        public string Rent_ID
        {
            get
            {
                return _rent_id;
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
        protected string _rentLivingSection_id;
        public string RentLivingSection_ID
        {
            get
            {
                return _rentLivingSection_id;
            }
        }

        protected string _rentLiving_id;
        public string RentLiving_ID
        {
            get
            {
                return _rentLiving_id;
            }
        }

        public RentLivingSectionPicture(string rentid, string rentlivingid, string rentlivingsectionid, string rentlivingsectionpictureid)
        {
            this._rent_id = rentid;
            this._rentLiving_id = rentlivingid;
            this._rentLivingSection_id = rentlivingsectionid;

            this._rentSectionPicture_id = rentlivingsectionpictureid;
        }
    }


}
