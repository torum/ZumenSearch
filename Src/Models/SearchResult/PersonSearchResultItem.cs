using ZumenSearch.Models.Base;
using ZumenSearch.Models.Person;

#pragma warning disable IDE0079 // Remove unnecessary suppression
#pragma warning disable IDE0290 // Use primary constructor

namespace ZumenSearch.Models.SearchResult;

// 貸主検索結果一覧表示用 
public sealed partial class PersonSearchResultItem : PersonBase
{
    public PersonSearchResultItem(string id, PersonKindType personKind) : base(id, EntityStatus.Saved, personKind)
    {
        //
    }


    // TODO: more

    public string CreatedAt { get; set; } = string.Empty;
    public string UpdatedAt { get; set; } = string.Empty;

}
