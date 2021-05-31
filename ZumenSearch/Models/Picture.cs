using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ZumenSearch.Common;

namespace ZumenSearch.Models
{
    // 物件の写真の基底クラス
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

        // For display.
        private ImageSource _pictureThumb;
        public ImageSource PictureThumb
        {
            get
            {
                return _pictureThumb;
            }
            set
            {
                if (_pictureThumb == value) return;

                _pictureThumb = value;
                this.NotifyPropertyChanged("PictureThumb");
            }
        }

        private byte[] _PictureThumbData;
        public byte[] PictureThumbData
        {
            get
            {
                return _PictureThumbData;
            }
            set
            {
                if (_PictureThumbData == value) return;

                _PictureThumbData = value;
                this.NotifyPropertyChanged("PictureThumbData");
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

        // メイン画像フラグ。
        private bool _pictureIsMain;
        public bool PictureIsMain
        {
            get
            {
                return _pictureIsMain;
            }
            set
            {
                if (_pictureIsMain == value) 
                    return;

                _pictureIsMain = value;
                NotifyPropertyChanged("PictureIsMain");
            }
        }

        // 新規か編集（保存済み）かどうかのフラグ。
        private bool _isNew;
        public bool IsNew
        {
            get
            {
                return _isNew;
            }
            set
            {
                if (_isNew == value) return;

                _isNew = value;
                NotifyPropertyChanged("IsNew");
                //NotifyPropertyChanged("IsEdit");
                NotifyPropertyChanged("StatusIsNew");
                //NotifyPropertyChanged("StatusIsDirty");
            }
        }

        // 表示用のステータス
        public string StatusIsNew
        {
            get
            {
                if (IsNew)
                    return "新規";
                else
                    return "更新";
            }
        }

        // 変更があったかどうかの（DB更新用）フラグ。
        private bool _isModified = false;
        public bool IsModified
        {
            get
            {
                return _isModified;
            }
            set
            {
                if (_isModified == value) return;

                _isModified = value;
                NotifyPropertyChanged("IsModified");
            }
        }
    }

    // 賃貸住居用物件の写真クラス（建物外観写真・地図等）
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

        public Dictionary<string, string> PictureTypes { get; set; } = new Dictionary<string, string>()
        {
            {"Gaikan", "外観"},
            {"Chizu", "周辺"},
            {"Other", "その他"},
        };

        private string _pictureType;
        public string PictureType
        {
            get
            {
                return _pictureType;
            }
            set
            {
                if (_pictureType == value) 
                    return;

                _pictureType = value;
                NotifyPropertyChanged("PictureType");
                NotifyPropertyChanged("PictureTypeLabel");

                IsModified = true;
            }
        }

        public string PictureTypeLabel
        {
            get
            {
                if (PictureType == null)
                    return "";

                if (PictureTypes.ContainsKey(PictureType))
                    return
                        PictureTypes[PictureType];
                else
                    return "";
            }
        }

        private string _pictureDescription;
        public string PictureDescription
        {
            get
            {
                return _pictureDescription;
            }
            set
            {
                if (_pictureDescription == value)
                    return;

                _pictureDescription = value;
                NotifyPropertyChanged("PictureDescription");

                IsModified = true;
            }
        }

        public RentLivingPicture(string rentid, string rentlivingid, string rentlivingpictureid)
        {
            _rentId = rentid;
            _rentLivingId = rentlivingid;
            _rentPictureId = rentlivingpictureid;
        }
    }

    // 賃貸事業用物件の写真クラス

    // ・・・

    // 部屋・区画の写真基底クラス
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

        private byte[] _PictureThumbData;
        public byte[] PictureThumbData
        {
            get
            {
                return _PictureThumbData;
            }
            set
            {
                if (_PictureThumbData == value) return;

                _PictureThumbData = value;
                this.NotifyPropertyChanged("PictureThumbData");
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

    // 賃貸住居用物件の写真クラス（室内内観写真・設備写真・間取り図等）
    public class RentLivingRoomPicture : RentSectionPicture
    {
        protected string _rentLivingSectionId;
        public string RentLivingRoomId
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

        public RentLivingRoomPicture(string rentid, string rentlivingid, string rentlivingsectionid, string rentlivingsectionpictureid)
        {
            this._rentId = rentid;
            this._rentLivingId = rentlivingid;
            this._rentLivingSectionId = rentlivingsectionid;

            this._rentSectionPictureId = rentlivingsectionpictureid;
        }
    }


}
