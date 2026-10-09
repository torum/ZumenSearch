using ZumenSearch.Models.Base;

namespace ZumenSearch.Models.Person;

#pragma warning disable IDE0079 // Remove unnecessary suppression
#pragma warning disable IDE0290 // Use primary constructor

// Entity

public partial class LegalPerson : PersonBase
{
    private string? _nameField;

    public LegalPerson(string id, EntityStatus status) : base(id, status, PersonKindType.Legal)
    {
        //
    }

    public string NameCompany
    {
        get => field ?? string.Empty;
        set
        {
            field = value;
            if (NameCompanyTypePosition == 0)
            {
                _nameField = $"{NameCompanyType}{NameCompany}";
            }
            else
            {
                _nameField = $"{NameCompany}{NameCompanyType}";
            }

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

    public string NameCompanyType
    {
        get => field ?? string.Empty;
        set
        {
            field = value;
            if (NameCompanyTypePosition == 0)
            {
                _nameField = $"{NameCompanyType}{NameCompany}";
            }
            else
            {
                _nameField = $"{NameCompany}{NameCompanyType}";
            }

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

    // 0 前付け、1 後付け
    public int NameCompanyTypePosition
    {
        get;
        set
        {
            field = value;
            if (value == 0)
            {
                _nameField = $"{NameCompanyType}{NameCompany}";
            }
            else
            {
                _nameField = $"{NameCompany}{NameCompanyType}";
            }

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
