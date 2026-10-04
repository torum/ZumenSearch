using ZumenSearch.Models.Base;

namespace ZumenSearch.Models.Person;

#pragma warning disable IDE0079 // Remove unnecessary suppression
#pragma warning disable IDE0290 // Use primary constructor

// Entity

public partial class NaturalPersonClass : PersonBase
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

    public NaturalPersonClass(string id, EnumEntityStatus status) : base(id, status, EnumPersonKind.Natural)
    {
        //
    }
};
