using CommunityToolkit.Mvvm.ComponentModel;

namespace ZumenSearch.Models.Base;

#pragma warning disable IDE0079 // Remove unnecessary suppression
#pragma warning disable IDE0290 // Use primary constructor

// <summary>
// Base class for all entities in the application that are going to be saved in the database including properties, listings, and persons.
// Provides common properties and methods for managing entity state.
// </summary>
// <remarks>
// </remarks>
public abstract class Entity : EntityBase
{
    protected Entity(string id, EnumEntityStatus staus) : base(id, staus)
    {
        //
    }

    public string Name
    {
        get => field ?? string.Empty;
        set
        {
            if (SetProperty(ref field, value))
            {
                IsModified = true;
            }
        }
    }
};
