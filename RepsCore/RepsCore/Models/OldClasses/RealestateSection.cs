using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reps.Models
{
    public abstract class RealestateSection
    {
        #region フィールド
        /*
        protected string sectionGUID;
        protected string realestateGUID;
        protected string updateGUID;

        protected string sectionName;
        protected string sectionMemo;
        protected bool isVacant;
        */

        #endregion

        // コンストラクタ
        public RealestateSection()
        {
            this.IsVacant = false;
        }

        #region プロパティ
        public string GUID
        {
            get { return this.SectionGUID; }
            set { this.SectionGUID = value; }
        }

        public string Name { get; set; }

        public bool IsVacant { get; set; }

        public string Memo { get; set; }

        protected string ErrorString { get; set; }

        public string RealestateGUID { get; set; }

        protected string SectionGUID { get; set; }

        public string UpdateGUID { get; set; }

        #endregion

        #region method

        public virtual bool SectionOpen(string sectionGUID)
        {
            return true;
        }

        public virtual bool SectionReOpen()
        {
            return true;
        }

        public virtual bool SectionSave()
        {
            return true;
        }

        public virtual bool SectionSaveAsNew()
        {
            return true;
        }

        public virtual bool SectionDelete()
        {
            return true;
        }

        public virtual void SectionReset()
        {
        }

        protected virtual bool ExecuteInsertOrUpdateSection(string queryString, string sectionGUID, string updateGUID)
        {
            return true;
        }

        protected virtual bool ExecuteSelectSection(string queryString, string sectionGUID)
        {
            return true;
        }

        #endregion
    }
}
