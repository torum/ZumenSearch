using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RepsCore.ViewModels
{
    /// <summary>
    /// 物件の種別などの Enum （ViewのXAMLで参照するのでここで定義）
    /// </summary>
    #region == グローバル定数 ==

    // 賃貸物件のタイプ（住居用・事業用・駐車場）
    public enum RentTypes
    {
        RentLiving, RentBussiness, RentParking
    }

    // 賃貸住居用物件種別（アパート・マンション・一戸建て・他）
    public enum RentLivingKinds
    {
        Apartment, Mansion, House, Other
    }

    // 一括所有の建物か、区分所有か
    public enum RentOwnerships
    {
        All, Unit
    }

    #endregion

}
