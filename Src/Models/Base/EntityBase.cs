using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Text;
using ZumenSearch.Models.Enums;

namespace ZumenSearch.Models.Base;

#pragma warning disable IDE0079 // Remove unnecessary suppression
#pragma warning disable IDE0290 // Use primary constructor

// Entity

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

    // ANEMIC property, but we simply want to keep track of whether the entity has been modified since it was loaded from the database.
    public EntityStatus Status { get; set; } = EntityStatus.New;

    // ANEMIC property, but ...
    public bool IsModified { get; set; } = false;

    #region == Public Methods ==

    public void SetIsModified(bool isModified)
    {
        IsModified = isModified;
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



