namespace ZumenSearch.Models.Base;

#pragma warning disable IDE0079 // Remove unnecessary suppression
#pragma warning disable IDE0290 // Use primary constructor

// Aggregate Root entity.

// <summary>
// Base class for all Person entities such as lessors(landlords), and real estate agents.
// Use LegalPersonClass for legal entities such as corporations, and NaturalPersonClass for natural persons unless abstract PersonBase is needed.
// </summary>
// <remarks>
// 
// </remarks>
public abstract class PersonBase : EntityAggregateBase
{
    public EnumPersonKind PersonKind { get; set; }

    // Do not use SetProperty.
    public string Remarks
    {
        get;
        set
        {
            if (field == value) return;

            if (value is not null)
            {
                // Set this before raize PropertyChanged.
                IsModified = true;

                // Raize PropertyChanged event here.
                field = value;
            }
            else
            {
                field = string.Empty;
            }

            OnPropertyChanged();
        }
    } = string.Empty;

    protected PersonBase(string id, EnumEntityStatus status, EnumPersonKind personKind) : base(id, status)
    {
        PersonKind = personKind;
    }
};


public enum EnumPersonKind
{
    Natural,
    Legal,
    Undetermined
}
