using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Xml;
using System.Xml.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using ZumenSearch.Models;
using ZumenSearch.ViewModels;

namespace ZumenSearch.Models;

public abstract partial class EntryBase : ObservableObject
{
    public bool IsDirty
    {
        get => field;
        set
        {
            field = value;
            OnPropertyChanged(nameof(IsDirty));
        }
    }

    protected string _id;
    public string Id => _id;

    public string Name
    {
        get => field ?? string.Empty; // Ensure a non-null value is returned
        set
        {
            if (SetProperty(ref field, value))
            {
                IsDirty = true;
            }
        }
    }

    #region == 所在地 ==

    public string LocPrefId
    {
        get => field ?? string.Empty;
        set => SetProperty(ref field, value);
    }

    public string LocPrefecture
    {
        get => field ?? string.Empty;
        set => SetProperty(ref field, value);
    }

    public string LocMachiazaId
    {
        get => field ?? string.Empty;
        set => SetProperty(ref field, value);
    }

    public string LocCounty
    {
        get => field ?? string.Empty;
        set => SetProperty(ref field, value);
    }

    public string LocCity
    {
        get => field ?? string.Empty;
        set => SetProperty(ref field, value);
    }

    public string LocWard
    {
        get => field ?? string.Empty;
        set => SetProperty(ref field, value);
    }

    public string LocOazaCho
    {
        get => field ?? string.Empty;
        set => SetProperty(ref field, value);
    }

    public string LocChoume
    {
        get => field ?? string.Empty;
        set => SetProperty(ref field, value);
    }

    public string LocEdaban
    {
        get => field ?? string.Empty;
        set => SetProperty(ref field, value);
    }

    public string LocLocationFull
    {
        get => field ?? string.Empty;
        set => SetProperty(ref field, value);
    }

    #endregion

    protected EntryBase()
    {
        // Instead of using a GUID, we initialize _id to an empty string to indicate that the entry is NEW.
        _id = string.Empty;//Guid.CreateVersion7() //Guid.NewGuid().ToString();
    }

    protected EntryBase(string id)
    {
        _id = id;
    }

    #region == Public Methods ==

    public void ClearId()
    {
        _id = string.Empty;
    }

    public void SetId(string id)
    {
        if (string.IsNullOrEmpty(id))
        {
            throw new ArgumentException("Id cannot be null or empty.", nameof(id));
        }
        _id = id;
    }

    #endregion
}
