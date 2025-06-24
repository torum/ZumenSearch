using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Data;
using ZumenSearch.Models;
using ZumenSearch.Models.Rent.Residentials;

namespace ZumenSearch.Services;

public interface IDataAccessService
{
    SqliteDataAccessResultWrapper InitializeDatabase(string dataBaseFilePath);

    SqliteDataAccessResultWrapper InsertRentResidential(EntryResidentialFull entry);

    SqliteDataAccessResultWrapper UpdateRentResidential(EntryResidentialFull entry);

    SqliteDataAccessResultWrapper DeleteRentResidential(string rentId);

    SqliteDataAccessSelectRentResidentialResultWrapper SelectRentResidentialsByNameKeyword(string keyword);

    SqliteDataAccessSelectRentResidentialFullResultWrapper SelectRentResidentialById(string id);


    /*
    SqliteDataAccessResultWrapper InsertFeed(string feedId, Uri feedUri, string feedName, string feedTitle, string feedDescription, DateTime updated, Uri? htmlUri);

    SqliteDataAccessResultWrapper UpdateFeed(string feedId, Uri feedUri, string feedName, string feedTitle, string feedDescription, DateTime updated, Uri? htmlUri);

    SqliteDataAccessResultWrapper DeleteFeed(string feedId);

    SqliteDataAccessSelectResultWrapper SelectEntriesByFeedId(string feedId, bool IsUnarchivedOnly = true);
    
    SqliteDataAccessSelectResultWrapper SelectEntriesByFeedIds(List<string> feedIds, bool IsUnarchivedOnly = true);


    //SqliteDataAccessSelectImageResultWrapper SelectImageByImageId(string imageId);

    SqliteDataAccessInsertResultWrapper InsertEntries(List<EntryItem> entries, string feedId, string feedName, string feedTitle, string feedDescription, DateTime updated, Uri htmlUri);


    //SqliteDataAccessInsertResultWrapper InsertImages(List<EntryItem> entries);

    SqliteDataAccessResultWrapper UpdateAllEntriesAsArchived(List<string> feedIds);

    SqliteDataAccessResultWrapper UpdateEntryReadStatus(string? entryId, ReadStatus readStatus);


    //SqliteDataAccessResultWrapper UpdateEntryStatus(EntryItem entry);

    SqliteDataAccessResultWrapper DeleteEntriesByFeedIds(List<string> feedIds);
    */
}



