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
    public string NameCompany
    {
        get => field ?? string.Empty;
        set
        {
            if (SetProperty(ref field, value))
            {
                if (NameCompanyTypePosition == 0)
                {
                    Name = $"{NameCompanyType}{NameCompany}";
                }
                else
                {
                    Name = $"{NameCompany}{NameCompanyType}";
                }
                IsModified = true;
            }
        }
    }

    public string NameCompanyType
    {
        get => field ?? string.Empty;
        set
        {
            if (SetProperty(ref field, value))
            {
                if (NameCompanyTypePosition == 0)
                {
                    Name = $"{NameCompanyType}{NameCompany}";
                }
                else
                {
                    Name = $"{NameCompany}{NameCompanyType}";
                }
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
            if (SetProperty(ref field, value))
            {
                if (value == 0)
                {
                    Name = $"{NameCompanyType}{NameCompany}";
                }
                else
                {
                    Name = $"{NameCompany}{NameCompanyType}";
                }
                IsModified = true;
            }
        }
    } = 0;

    public PersonLegal(string id, EnumEntryStatus status) : base(id, status, EnumPersonKind.Legal)
    {
        //
    }
};
