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

    public string CreatedAt { get; set; } = string.Empty;
    public string UpdatedAt { get; set; } = string.Empty;

    public PersonSearchResultItem(string id, PersonKind personKind) : base(id, EntityStatus.Saved, personKind)
    {
        //
    }
}
