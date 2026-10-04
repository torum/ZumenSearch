using CommunityToolkit.Mvvm.ComponentModel;

namespace ZumenSearch.Models.Base;

#pragma warning disable IDE0079 // Remove unnecessary suppression
#pragma warning disable IDE0290 // Use primary constructor

// Aggregate Root entity.

// <summary>
// Base class for Aggregate Root entities that coincidentaly all have name properties such as properties, listings, and persons.
// Provides common properties and methods for managing entity state.
// </summary>
// <remarks>
// </remarks>
public abstract class EntityAggregateBase : EntityBase
{
    protected EntityAggregateBase(string id, EnumEntityStatus staus) : base(id, staus)
    {
        //
    }

    // Non-ANEMIC
    public string Name
    {
        get => field ?? string.Empty;
        private set
        {
            if (SetProperty(ref field, value))
            {
                IsModified = true;
            }
        }
    }

    public void SetName(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Name cannot be empty.", nameof(name));
        // InvalidOperationException 
        Name = name.Trim();
    }
};
