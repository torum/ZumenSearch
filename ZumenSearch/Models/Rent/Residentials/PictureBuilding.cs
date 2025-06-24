using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZumenSearch.Views.Rent.Residentials.Editor.Modal;

namespace ZumenSearch.Models.Rent.Residentials
{
    // 賃貸住居用物件の「構造」クラス
    public class PictureBuilding : PictureBase
    {
        public PictureBuilding(string imageLocation)
        {
            ImageLocation = imageLocation;
        }

    };
}
