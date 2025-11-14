using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using ZumenSearch.Views.Rent.Residentials.Bldg.Unit;

namespace ZumenSearch.Models.Rent.Residentials;

public enum EnumBuildingPictureType
{
    //Unspecified, Madori, Gaikan, Situnai, LivingDining, Bedroom, Kitchen, Bathroom, Restroom, Washroom, StorageSpace, Appliance, FrontDoor, Balcony, Entrance, Neighborhood, Other
    Unspecified, Madori, Gaikan, Entrance, Neighborhood, Other
}

public class BuildingPictureType(EnumBuildingPictureType key)
{
    private Dictionary<EnumBuildingPictureType, string> BuildingPictureTypeDictionary
    {
        get;
    } = new Dictionary<EnumBuildingPictureType, string>()
    {
                {EnumBuildingPictureType.Unspecified, "未指定"},
                {EnumBuildingPictureType.Madori, "間取り図"},
                {EnumBuildingPictureType.Gaikan, "外観"},
                //{EnumBuildingPictureType.Situnai, "室内"},
                //{EnumBuildingPictureType.LivingDining, "リビング・ダイニング"},
                //{EnumBuildingPictureType.Bedroom, "寝室"},
                //{EnumBuildingPictureType.Kitchen, "キッチン"},
                //{EnumBuildingPictureType.Bathroom, "浴室"},
                //{EnumBuildingPictureType.Restroom, "トイレ"},
                //{EnumBuildingPictureType.Washroom, "洗面"},
                //{EnumBuildingPictureType.StorageSpace, "収納"},
                //{EnumBuildingPictureType.Appliance, "設備"},
                //{EnumBuildingPictureType.FrontDoor, "玄関"},
                //{EnumBuildingPictureType.Balcony, "バルコニー"},
                {EnumBuildingPictureType.Entrance, "エントランス"},
                {EnumBuildingPictureType.Neighborhood, "周辺"},
                {EnumBuildingPictureType.Other, "その他"},
            };

    public string Text => BuildingPictureTypeDictionary[Key];

    public EnumBuildingPictureType Key { get; set; } = key;
};


public partial class PictureBuilding : PictureBase
{
    private BuildingPictureType _pictureType = new(EnumBuildingPictureType.Unspecified);
    public BuildingPictureType PictureType
    {
        get => _pictureType;
        set
        {
            if (SetProperty(ref _pictureType, value))
            {
            }
        }
    }

    private string _description = string.Empty;
    public string Description
    {
        get => _description;
        set
        {
            if (SetProperty(ref _description, value))
            {
            }
        }
    }

    private bool _isMain;
    public bool IsMain
    {
        get => _isMain;
        set
        {
            if (SetProperty(ref _isMain, value))
            {
            }
        }
    }

    public PictureBuilding(string imageLocation)
    {
        ImageLocation = imageLocation;
    }

    public EnumBuildingPictureType? SetLabelFromString(string titleStr)
    {
        if (Enum.TryParse<EnumBuildingPictureType>(titleStr, out var result))
        {
            PictureType = new(result);
            Debug.WriteLine($"SetLabelFromString: {titleStr} -> {PictureType.Text}");
            return result;
        }
        else
        {
            PictureType = new(EnumBuildingPictureType.Unspecified);
            return EnumBuildingPictureType.Unspecified;
        }
    }

};
