using CommunityToolkit.Mvvm.ComponentModel;

namespace ZumenSearch.Models.Base;

#pragma warning disable IDE0079 // Remove unnecessary suppression
#pragma warning disable IDE0290 // Use primary constructor

// <summary>
// Represents the status of an entity in the application, indicating whether it is saved in the database or new.
// It is used to track the state of entities such as properties, listings, persons, pictures, and PDFs.
// </summary>
// <remarks>
// </remarks>
public enum EnumEntityStatus
{
    Saved,
    New,
}

// <summary>
// Base class for all entities in the application including properties, listings, persons, pictures, and PDFs.
// Provides common properties and methods for managing entity state.
// </summary>
public abstract class Entity : ObservableObject
{
    public EnumEntityStatus Status { get; set; } = EnumEntityStatus.New;

    public string Id { get; private set; } = string.Empty;

    public bool IsModified { get; set; } = false;

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

    protected Entity(string id, EnumEntityStatus staus)
    {
        Id = id;
        Status = staus;
    }

    #region == Public Methods ==

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

    #endregion
};
