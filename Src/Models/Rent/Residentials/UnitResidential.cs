using System;
using System.Collections.Generic;
using System.Text;
using ZumenSearch.Models.Base;
using static System.Net.Mime.MediaTypeNames;

namespace ZumenSearch.Models.Rent.Residentials;

public partial class UnitResidentialSearchResult : UnitBase
{
    public string EntryId { get; init; }

    public string EntryName
    {
        get => field ?? string.Empty;
        set
        {
            if (SetProperty(ref field, value))
            {

            }
        }
    }

    public UnitResidentialSearchResult(string id, string entryId) : base(id)
    {
        EntryId = entryId;
    }
}

public partial class UnitResidential : UnitBase
{
    public int Chinryou
    {
        get;
        set
        {
            if (field == value)
            {
                return;
            }

            if (value > -1) 
            {
                field = value;
                OnPropertyChanged();
                IsModified = true;
            }
            else
            {
                // TODO: show error
                //field = string.Empty;
                throw new ArgumentOutOfRangeException("Chinryou", "Must be at least 0.");
            }
        }
    }

    public UnitResidential(string id) : base(id)
    {
        //
    }
}
