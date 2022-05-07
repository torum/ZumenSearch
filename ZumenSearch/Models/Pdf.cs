using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using ZumenSearch.Common;

namespace ZumenSearch.Models
{
    // 図面の基底クラス
    public class RentPDF : ViewModelBase
    {
        protected string _RentPdfId;
        public string RentPdfId
        {
            get
            {
                return _RentPdfId;
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

        private byte[] _pdfData;
        public byte[] PdfData
        {
            get
            {
                return _pdfData;
            }
            set
            {
                if (_pdfData == value) return;

                _pdfData = value;
                this.NotifyPropertyChanged("PdfData");
            }
        }

        private byte[] _pdfThumbData;
        public byte[] PdfThumbData
        {
            get
            {
                return _pdfThumbData;
            }
            set
            {
                if (_pdfThumbData == value) return;

                _pdfThumbData = value;
                this.NotifyPropertyChanged("PdfThumbData");
            }
        }

        // 登録日
        protected DateTime _dateTimeAdded;
        public DateTime DateTimeAdded
        {
            get
            {
                return _dateTimeAdded;
            }
            set
            {
                if (_dateTimeAdded == value) return;

                _dateTimeAdded = value;
                this.NotifyPropertyChanged("DateTimeAdded");
            }
        }

        // 情報公開日
        private DateTime _dateTimePublished;
        public DateTime DateTimePublished
        {
            get
            {
                return _dateTimePublished;
            }
            set
            {
                if (_dateTimePublished == value) return;

                _dateTimePublished = value;
                this.NotifyPropertyChanged("DateTimePublished");

                //this.IsDirty = true;
            }
        }

        // 最終確認日
        private DateTime _dateTimeVerified;
        public DateTime DateTimeVerified
        {
            get
            {
                return _dateTimeVerified;
            }
            set
            {
                if (_dateTimeVerified == value) return;

                _dateTimeVerified = value;
                this.NotifyPropertyChanged("DateTimeVerified");

                //this.IsDirty = true;
            }
        }

        // ファイルサイズ
        public long _fileSize;
        public long FileSize
        {
            get
            {
                return _fileSize;
            }
            set
            {
                if (_fileSize == value) return;

                _fileSize = value;
                this.NotifyPropertyChanged("FileSize");
                this.NotifyPropertyChanged("FileSizeLabel");
            }
        }

        public string FileSizeLabel
        {
            get
            {
                if (FileSize > 0)
                {
                    string[] sizes = { "B", "KB", "MB", "GB", "TB" };
                    double len = FileSize;
                    int order = 0;
                    while (len >= 1024 && order < sizes.Length - 1)
                    {
                        order++;
                        len = len / 1024;
                    }

                    // Adjust the format string to your preferences. For example "{0:0.#}{1}" would
                    // show a single decimal place, and no space.
                    return String.Format("{0:0.##} {1}", len, sizes[order]);
                }
                else
                {
                    return "";
                }


            }
        }

        // メインPDFフラグ。
        private bool _pdfIsMain;
        public bool PdfIsMain
        {
            get
            {
                return _pdfIsMain;
            }
            set
            {
                if (_pdfIsMain == value)
                    return;

                _pdfIsMain = value;
                NotifyPropertyChanged("PdfIsMain");
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
                    return "：新規";
                else
                    return "：更新";
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

    // 賃貸住居用物件の図面クラス
    public class RentLivingPdf : RentPDF
    {
        protected string _rentLivingId;
        public string RentLivingId
        {
            get
            {
                return _rentLivingId;
            }
        }

        public Dictionary<string, string> PdfTypes { get; set; } = new Dictionary<string, string>()
        {
            {"BoshuuZumen", "募集図面"},
            {"KenchikuZumen", "建築図面"},
            {"Touhon", "登記簿謄本"},
            {"Other", "その他"},
        };

        private string _pdfType;
        public string PdfType
        {
            get
            {
                return _pdfType;
            }
            set
            {
                if (_pdfType == value)
                    return;

                _pdfType = value;
                NotifyPropertyChanged("PdfType");
                NotifyPropertyChanged("PdfTypeLabel");

                IsModified = true;
            }
        }

        public string PdfTypeLabel
        {
            get
            {
                if (PdfType == null)
                    return "";

                if (PdfTypes.ContainsKey(PdfType))
                    return
                        PdfTypes[PdfType];
                else
                    return "";
            }
        }

        private string _pdfDescription;
        public string PdfDescription
        {
            get
            {
                return _pdfDescription;
            }
            set
            {
                if (_pdfDescription == value)
                    return;

                _pdfDescription = value;
                NotifyPropertyChanged("PdfDescription");

                IsModified = true;
            }
        }

        public RentLivingPdf(string rentid, string rentlivingid, string rentlivingpdfid)
        {
            _rentId = rentid;
            _rentLivingId = rentlivingid;

            _RentPdfId = rentlivingpdfid;

            // 一応
            _dateTimeAdded = DateTime.Now;
        }
    }

    // TODO: 賃貸事業用の図面クラス

    // 賃貸住居用の部屋・区画の図面基底クラス
    public class RentSectionPdf : ViewModelBase
    {
        protected string _rentSectionPdfId;
        public string RentSectionPdfId
        {
            get
            {
                return _rentSectionPdfId;
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

        private byte[] _pdfData;
        public byte[] PdfData
        {
            get
            {
                return _pdfData;
            }
            set
            {
                if (_pdfData == value) return;

                _pdfData = value;
                this.NotifyPropertyChanged("PdfData");
            }
        }

        private byte[] _pdfThumbData;
        public byte[] PdfThumbData
        {
            get
            {
                return _pdfThumbData;
            }
            set
            {
                if (_pdfThumbData == value) return;

                _pdfThumbData = value;
                this.NotifyPropertyChanged("PdfThumbData");
            }
        }

        // 登録日
        protected DateTime _dateTimeAdded;
        public DateTime DateTimeAdded
        {
            get
            {
                return _dateTimeAdded;
            }
            set
            {
                if (_dateTimeAdded == value) return;

                _dateTimeAdded = value;
                this.NotifyPropertyChanged("DateTimeAdded");
            }
        }

        // 情報公開日
        private DateTime _dateTimePublished;
        public DateTime DateTimePublished
        {
            get
            {
                return _dateTimePublished;
            }
            set
            {
                if (_dateTimePublished == value) return;

                _dateTimePublished = value;
                this.NotifyPropertyChanged("DateTimePublished");

                //this.IsDirty = true;
            }
        }

        // 最終確認日
        private DateTime _dateTimeVerified;
        public DateTime DateTimeVerified
        {
            get
            {
                return _dateTimeVerified;
            }
            set
            {
                if (_dateTimeVerified == value) return;

                _dateTimeVerified = value;
                this.NotifyPropertyChanged("DateTimeVerified");

                //this.IsDirty = true;
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
                NotifyPropertyChanged(nameof(IsNew));
                //NotifyPropertyChanged("IsEdit");
                NotifyPropertyChanged(nameof(StatusIsNew));
                //NotifyPropertyChanged("StatusIsDirty");
            }
        }

        // 表示用のステータス
        public string StatusIsNew
        {
            get
            {
                if (IsNew)
                    return "：新規";
                else
                    return "：更新";
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
                NotifyPropertyChanged(nameof(IsModified));
            }
        }
    }

    // 賃貸住居用の部屋の図面クラス
    public class RentLivingRoomPdf : RentSectionPdf
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

        public Dictionary<string, string> PdfTypes { get; set; } = new Dictionary<string, string>()
        {
            {"BoshuuZumen", "募集図面"},
            {"KenchikuZumen", "建築図面"},
            {"Touhon", "登記簿謄本"},
            {"Other", "その他"},
        };

        private string _pdfType;
        public string PdfType
        {
            get
            {
                return _pdfType;
            }
            set
            {
                if (_pdfType == value)
                    return;

                _pdfType = value;
                NotifyPropertyChanged("PdfType");
                NotifyPropertyChanged("PdfTypeLabel");

                IsModified = true;
            }
        }

        public string PdfTypeLabel
        {
            get
            {
                if (PdfType == null)
                    return "";

                if (PdfTypes.ContainsKey(PdfType))
                    return
                        PdfTypes[PdfType];
                else
                    return "";
            }
        }

        private string _pdfDescription;
        public string PdfDescription
        {
            get
            {
                return _pdfDescription;
            }
            set
            {
                if (_pdfDescription == value)
                    return;

                _pdfDescription = value;
                NotifyPropertyChanged("PdfDescription");

                IsModified = true;
            }
        }

        // メインPDFフラグ。
        private bool _pdfIsMain;
        public bool PdfIsMain
        {
            get
            {
                return _pdfIsMain;
            }
            set
            {
                if (_pdfIsMain == value)
                    return;

                _pdfIsMain = value;
                NotifyPropertyChanged("PdfIsMain");
            }
        }

        //(string rentid, string rentlivingid, string rentlivingsectionid, string rentlivingsectionpictureid)
        public RentLivingRoomPdf(string rentid, string rentlivingid, string rentlivingsectionid, string rentlivingsectionpdfid)
        {
            _rentId = rentid;
            _rentLivingId = rentlivingid;
            _rentLivingSectionId = rentlivingsectionid;
            _rentSectionPdfId = rentlivingsectionpdfid;

            // 一応
            _dateTimeAdded = DateTime.Now;
        }
    }

    // TODO: 賃貸事業用の区画の図面クラス
}
