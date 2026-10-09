#pragma warning disable IDE0079 // Remove unnecessary suppression
#pragma warning disable IDE0290 // Use primary constructor

namespace ZumenSearch.Models.Base;
// Aggregate Root entity.

// <summary>
// Base class for Aggregate Root entities that coincidentaly all have name properties such as properties, listings, and persons.
// Provides common properties and methods for managing entity state.
// </summary>
// <remarks>
// </remarks>
public abstract class EntityAggregateBase : EntityBase
{
    protected EntityAggregateBase(string id, EntityStatus staus) : base(id, staus)
    {
        //
    }

    // Non-ANEMIC/Rich property
    public string Name
    {
        get => field ?? string.Empty;
        private set
        {
            field = value;
            IsModified = true;
        }
    }

    public void SetName(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Name cannot be empty.", nameof(name));// 
        // InvalidOperationException 
        Name = name.Trim();
    }
};
