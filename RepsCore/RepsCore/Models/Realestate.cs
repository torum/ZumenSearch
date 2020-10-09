
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reps.Models
{
    public abstract class Realestate
    {

        // コンストラクタ
        public Realestate()
        {

        }

        #region プロパティ

        public string GUID
        {
            get { return this.RealestateGUID; }
        }

        public string Name { get; set; }

        public string NameKana { get; set; }

        public string AddressFullString { get; set; }

        public string AddressPrefString { get; set; }

        public string AddressCityString { get; set; }

        public string AddressTownString { get; set; }

        public int AddressPostalCode { get; set; }

        public int AddressPrefCode { get; set; }

        public int AddressCityCode { get; set; }

        public string AddressChoume { get; set; }

        public string AddressBanchi { get; set; }

        public string Memo { get; set; }

        public string ErrorString { get; set; }

        protected string RealestateGUID { get; set; }

        protected string UpdateGUID { get; set; }

        #endregion

        #region メソッド

        public virtual bool RealestateOpen(string realestateGUID)
        {
            return true;
        }

        public virtual bool RealestateReOpen()
        {
            return true;
        }

        public virtual bool RealestateSectionReOpen()
        {
            return true;
        }
        
        public virtual bool RealestateSave()
        {
            return true;
        }

        public virtual bool RealestateSaveAsNew()
        {
            return true;
        }

        public virtual bool RealestateDelete()
        {
            return true;
        }

        public virtual void RealestateReset()
        { }

        protected virtual bool ExecuteSelectRealestate(string queryString, string realestateGUID)
        {
            return true;
        }

        protected virtual bool ExecuteSelectSections(string queryString)
        {
            return true;
        }

        protected virtual bool ExecuteSelectPictures(string queryString)
        {
            return true;
        }

        protected virtual bool ExecuteInsertOrUpdateRealestate(string queryString, string realestateGUID, string tmpUpdateGUID)
        {
            return true;
        }

        #endregion
    }
}
