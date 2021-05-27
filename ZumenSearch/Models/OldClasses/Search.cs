using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Reps.Models
{
    public abstract class Search
    {
        protected string searchText;

        // コンストラクタ
        public Search()
        {
            this.searchText = string.Empty;
        }

        protected string SearchText
        {
            get { return this.searchText; }
            set { this.searchText = value; }
        }

        #region method
        
        public virtual bool DoSearch(string searchText)
        {
            this.searchText = SearchText;
            return true;
        }
        
        #endregion
    }
}