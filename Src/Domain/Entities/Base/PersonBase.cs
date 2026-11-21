#pragma warning disable IDE0079 // Remove unnecessary suppression
#pragma warning disable IDE0290 // Use primary constructor

using ZumenSearch.Domain.Enums;
using ZumenSearch.Domain.ValueObjects.Location;

namespace ZumenSearch.Domain.Entities.Base;

// Aggregate Root entity.

// <summary>
// Base class for all Person entities such as lessors(landlords), and real estate agents.
// Use PersonLegal for legal entities such as corporations, and PersonNatural for natural persons unless abstract PersonBase is needed.
// </summary>
// <remarks>
// 
// </remarks>
public abstract class PersonBase : EntityAggregateBase
{
    protected PersonBase(string id, EntityStatus status, PersonKindType personKind) : base(id, status)
    {
        PersonKind = personKind;
    }

    public PersonKindType PersonKind { get; set; }

    public string PhoneNumber
    {
        get;
        set
        {
            if (field == value) return;
            field = value;
            IsModified = true;
        }
    } = string.Empty;


    public string PostalCode
    {
        get;
        set
        {
            if (field == value) return;
            field = value;
            IsModified = true;
        }
    } = string.Empty;

    public string Address
    {
        get;
        set
        {
            if (field == value) return;
            field = value;
            IsModified = true;
        }
    } = string.Empty;

    public Address AddressLocation { get; } = new Address();

    public string Remarks
    {
        get;
        set
        {
            if (field == value) return;

            if (value is not null)
            {
                IsModified = true;

                field = value;
            }
            else
            {
                field = string.Empty;
            }

        }
    } = string.Empty;

};


