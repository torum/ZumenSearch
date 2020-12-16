using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RepsCore.Common;

namespace RepsCore.Models.Classes
{

    public class Zumen
    {

        // TODO:
    }

    /// <summary>
    /// 図面の基底クラス
    /// </summary>
    public class RentZumenPDF : ViewModelBase
    {
        protected string _rentZumenPDF_id;
        public string RentZumenPDF_ID
        {
            get
            {
                return _rentZumenPDF_id;
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

        private byte[] _pdfData;
        public byte[] PDFData
        {
            get
            {
                return _pdfData;
            }
            set
            {
                if (_pdfData == value) return;

                _pdfData = value;
                this.NotifyPropertyChanged("PDFData");
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

                this.IsDirty = true;
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

                this.IsDirty = true;
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

        // 新規追加なので、DBにINSERTが必要
        public bool IsNew { get; set; }

        // 日付などが変更された（DBのUPDATEが必要）
        public bool IsDirty { get; set; }
    }

    /// <summary>
    /// 賃貸住居用物件の図面クラス
    /// </summary>
    public class RentLivingZumenPDF : RentZumenPDF
    {
        protected string _rentLiving_id;
        public string RentLiving_ID
        {
            get
            {
                return _rentLiving_id;
            }
        }

        public RentLivingZumenPDF(string rentid, string rentlivingid, string rentlivingzumenid)
        {
            this._rent_id = rentid;
            this._rentLiving_id = rentlivingid;

            this._rentZumenPDF_id = rentlivingzumenid;

            // 一応
            this._dateTimeAdded = DateTime.Now;
        }
    }


}
