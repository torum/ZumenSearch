#pragma warning disable IDE0079 // Remove unnecessary suppression
#pragma warning disable IDE0290 // Use primary constructor

using ZumenSearch.Domain.Entities.Base;
using ZumenSearch.Domain.Enums;

namespace ZumenSearch.Domain.Entities.Person;

// Aggregate Root entity.
// 自然人を表すエンティティ

public partial class PersonNatural : PersonBase
{
    private string? _nameField;

    public PersonNatural(string id, EntityStatus status) : base(id, status, PersonKindType.Natural)
    {
        //
    }

    public string NameFirst
    {
        get => field ?? string.Empty;
        set
        {
            field = value;
            _nameField = $"{NameLast} {NameFirst}";

            try
            {
                if (!string.IsNullOrWhiteSpace(_nameField))
                {
                    SetName(_nameField);
                }
            }
            finally
            {
                IsModified = true;
            }
        }
    }

    public string NameLast
    {
        get => field ?? string.Empty;
        set
        {
            field = value;
            _nameField = $"{NameLast} {NameFirst}";

            try
            {
                if (!string.IsNullOrWhiteSpace(_nameField))
                {
                    SetName(_nameField);
                }
            }
            finally
            {
                IsModified = true;
            }
        }
    }

};
