using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using System.Diagnostics;
using ZumenSearch.Models.Base;

namespace ZumenSearch.Models;

#pragma warning disable IDE0079 // Remove unnecessary suppression
#pragma warning disable IDE0290 // Use primary constructor

public partial class PersonLegal : PersonBase
{
    /*
法人格「会社の種類」または「会社形態」


株式会社
合同会社
有限会社
合資会社
合名会社
その他（一般社団法人、NPO法人など）


「前付け・後付け」

表示位置（または法人格の位置）
◯ 前（例：株式会社〇〇）
◯ 後（例：〇〇株式会社）
    */

    public PersonLegal(string id, EnumEntryStatus status) : base(id, status, EnumPersonKind.Legal)
    {
        //
    }
};
