using ZumenSearch.Models.Base;
using ZumenSearch.Models.Enums;

namespace ZumenSearch.Models.Person;

#pragma warning disable IDE0079 // Remove unnecessary suppression
#pragma warning disable IDE0290 // Use primary constructor

// Entity

public partial class NaturalPerson : PersonBase
{
    private string? _nameField;

    public string NameFirst
    {
        get => field ?? string.Empty;
        set
        {
            if (SetProperty(ref field, value))
            {
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
    }

    public string NameLast
    {
        get => field ?? string.Empty;
        set
        {
            if (SetProperty(ref field, value))
            {
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
    }

    public NaturalPerson(string id, EntityStatus status) : base(id, status, PersonKind.Natural)
    {
        //
    }
};
