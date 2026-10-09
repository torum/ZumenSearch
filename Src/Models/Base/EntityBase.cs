using CommunityToolkit.Mvvm.ComponentModel;

#pragma warning disable IDE0079 // Remove unnecessary suppression
#pragma warning disable IDE0290 // Use primary constructor

namespace ZumenSearch.Models.Base;

// <summary>
// Base class for all entities like properties, listings, persons, pictures, and PDFs that are going to be saved in the database independently .
// Provides common properties and methods for managing entity state.
// </summary>
// <remarks>
// 
// </remarks>
public abstract class EntityBase : ObservableObject
{
    protected EntityBase(string id, EntityStatus status)
    {
        Id = id;
        Status = status;
    }

    public string Id { get; private set; } = string.Empty;

    // Non-ANEMIC/Rich property
    public EntityStatus Status { get; protected set; } = EntityStatus.New;

    // Non-ANEMIC/Rich property
    public bool IsModified { get; protected set; }

    #region == Public Methods ==

    public void SetIsModified(bool isModified)
    {
        IsModified = isModified;
    }

    public void SetStatus(EntityStatus status)
    {
        Status = status;
        // No don't. IsModified = true;
    }

    /*
    public void ClearId()
    {
        Id = string.Empty;
    }

    public void SetId(string id)
    {
        if (string.IsNullOrEmpty(id))
        {
            throw new ArgumentException("Id cannot be null or empty.", nameof(id));
        }

        Id = id;
    }
    */

    #endregion
}



