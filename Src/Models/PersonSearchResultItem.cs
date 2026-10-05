using System;
using System.Collections.Generic;
using System.Text;
using ZumenSearch.Models.Base;
using ZumenSearch.Models.Enums;

#pragma warning disable IDE0079 // Remove unnecessary suppression
#pragma warning disable IDE0290 // Use primary constructor

namespace ZumenSearch.Models;

// 貸主検索結果一覧表示用 
public sealed partial class PersonSearchResultItem : PersonBase
{
    // TODO: more

    public string CreatedAt = string.Empty;
    public string UpdatedAt = string.Empty;

    public PersonSearchResultItem(string id, EnumPersonKind personKind) : base(id, EnumEntityStatus.Saved, personKind)
    {
        //
    }
}
