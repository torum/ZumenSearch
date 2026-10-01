using ZumenSearch.Models.Base;

namespace ZumenSearch.Models.Person;

#pragma warning disable IDE0079 // Remove unnecessary suppression
#pragma warning disable IDE0290 // Use primary constructor

public partial class PersonNatural : PersonBase
{
    public string NameFirst
    {
        get => field ?? string.Empty;
        set
        {
            if (SetProperty(ref field, value))
            {
                Name = $"{NameLast} {NameFirst}";
                IsModified = true;
            }
        }
    }

    public string NameLast
    {
        get => field ?? string.Empty;
        set
        {
            if (SetProperty(ref field, value))
            {
                Name = $"{NameLast} {NameFirst}";
                IsModified = true;
            }
        }
    }

    public PersonNatural(string id, EnumEntryStatus status) : base(id, status, EnumPersonKind.Natural)
    {
        //
    }
};
