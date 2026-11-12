using Microsoft.Data.Sqlite;
using System.Data;
using System.Globalization;
using ZumenSearch.Helpers;
using ZumenSearch.Models;
using ZumenSearch.Models.Base;
using ZumenSearch.Models.SearchResult;
using ZumenSearch.Services.Contracts;

namespace ZumenSearch.Services;

// Repository

// <summary>
// DataAccessService provides data access functionalities for managing properties and listings and related data in the application.
// </summary>

// TODO:
// * 

public sealed partial class DataAccessService : IDataAccessService, IDisposable
{
    private SqliteConnectionStringBuilder _connectionStringBuilder = [];

    private readonly ReaderWriterLockSlim _readerWriterLock = new();

    #region == Initialization ==

    public ResultWrapper InitializeDatabase(string dataBaseFilePath)
    {
        var res = new ResultWrapper();

        try
        {
            // Don't think WAL mode is needed for this application. It is single-user, single-process, single-threading app with ReaderWriterLock.
            // WAL mode is more suitable for multi-threaded or multi-process scenarios where concurrent reads and writes are expected.
            _connectionStringBuilder = new SqliteConnectionStringBuilder("Data Source=" + dataBaseFilePath);//+ ";Pooling=false" 

            // Known issue. I personaly reported the issue to Microsoft.
            // https://github.com/dotnet/efcore/issues/38275
            // It is not a problem for packaged apps, but it is a problem for unpackaged apps. Database initialization succeeds with the exceptions, but it the debugger stops at the exception.
            // The issue is that Microsoft.Data.Sqlite SqliteConnection() calls Windows.Storage.ApplicationData.Current.get() which results in System.InvalidOperationException "Operation is not valid due to the current state of the object."
            // To avoid the debugger stopping here, change the Visual Studio exception settings for System.InvalidOperationException and System.Reflection.TargetInvocationException so it does not break when thrown; avoid disabling thrown-exception breaks globally if you still need them for other exceptions.
            using var connection = new SqliteConnection(_connectionStringBuilder.ConnectionString);
            if (!RuntimeHelper.IsMSIX)
            {
                // https://github.com/dotnet/efcore/issues/38275
                Debug.WriteLine("* Microsoft.Data.Sqlite SqliteConnection() calls Windows.Storage.ApplicationData.Current.get() results in System.InvalidOperationException \"Operation is not valid due to the current state of the object.\" Since we are in unpackaged, we can safely ignore the exception.");
            }

            connection.Open();

            using var tableCmd = connection.CreateCommand();
            tableCmd.Transaction = connection.BeginTransaction();
            try
            {
                tableCmd.CommandText = "CREATE TABLE IF NOT EXISTS properties (" +
                    "property_id TEXT NOT NULL PRIMARY KEY," +
                    "property_kind TEXT NOT NULL DEFAULT ''," +
                    "name TEXT NOT NULL DEFAULT ''," +
                    "thumbnail_filename TEXT NOT NULL DEFAULT ''," +

                #region == Properties location ==
                    "location_machiaza_id TEXT NOT NULL DEFAULT ''," +
                    "location_prefecture_code TEXT NOT NULL DEFAULT ''," + // MunicipalityCode - rename it?
                    "location_prefecture TEXT NOT NULL DEFAULT ''," +
                    "location_county TEXT NOT NULL DEFAULT ''," +
                    "location_city TEXT NOT NULL DEFAULT ''," +
                    "location_ward TEXT NOT NULL DEFAULT ''," +
                    "location_oaza_cho TEXT NOT NULL DEFAULT ''," +
                    "location_choume TEXT NOT NULL DEFAULT ''," +
                    "location_edaban TEXT NOT NULL DEFAULT ''," +
                    "location_postal_code TEXT NOT NULL DEFAULT ''," +
                    "location_full TEXT NOT NULL DEFAULT ''," +
                    "location_latitude TEXT NOT NULL DEFAULT ''," +
                    "location_longitude TEXT NOT NULL DEFAULT ''," +

                    "train1_line_code TEXT NOT NULL DEFAULT ''," +
                    "train1_line_name TEXT NOT NULL DEFAULT ''," +
                    "train1_station_code TEXT NOT NULL DEFAULT ''," +
                    "train1_station_name TEXT NOT NULL DEFAULT ''," +
                    "train1_ekitoho INTEGER NOT NULL DEFAULT 0," +
                    "buss1_stop_name TEXT NOT NULL DEFAULT ''," +
                    "buss1_Jyousya INTEGER NOT NULL DEFAULT 0," +
                    "buss1_Toho INTEGER NOT NULL DEFAULT 0," +

                    "train2_line_code TEXT NOT NULL DEFAULT ''," +
                    "train2_line_name TEXT NOT NULL DEFAULT ''," +
                    "train2_station_code TEXT NOT NULL DEFAULT ''," +
                    "train2_station_name TEXT NOT NULL DEFAULT ''," +
                    "train2_ekitoho INTEGER NOT NULL DEFAULT 0," +
                    "buss2_stop_name TEXT NOT NULL DEFAULT ''," +
                    "buss2_Jyousya INTEGER NOT NULL DEFAULT 0," +
                    "buss2_Toho INTEGER NOT NULL DEFAULT 0," +

                    "train3_line_code TEXT NOT NULL DEFAULT ''," +
                    "train3_line_name TEXT NOT NULL DEFAULT ''," +
                    "train3_station_code TEXT NOT NULL DEFAULT ''," +
                    "train3_station_name TEXT NOT NULL DEFAULT ''," +
                    "train3_ekitoho INTEGER NOT NULL DEFAULT 0," +
                    "buss3_stop_name TEXT NOT NULL DEFAULT ''," +
                    "buss3_Jyousya INTEGER NOT NULL DEFAULT 0," +
                    "buss3_Toho INTEGER NOT NULL DEFAULT 0," +

                    "train4_line_code TEXT NOT NULL DEFAULT ''," +
                    "train4_line_name TEXT NOT NULL DEFAULT ''," +
                    "train4_station_code TEXT NOT NULL DEFAULT ''," +
                    "train4_station_name TEXT NOT NULL DEFAULT ''," +
                    "train4_ekitoho INTEGER NOT NULL DEFAULT 0," +
                    "buss4_stop_name TEXT NOT NULL DEFAULT ''," +
                    "buss4_Jyousya INTEGER NOT NULL DEFAULT 0," +
                    "buss4_Toho INTEGER NOT NULL DEFAULT 0," +
                #endregion

                    // TODO: add more columns.


                    "updated_at TEXT NOT NULL DEFAULT (DATETIME('now', 'utc'))," +
                    "created_at TEXT NOT NULL DEFAULT (DATETIME('now', 'utc'))" + // Last column, no comma
                    ")";
                tableCmd.ExecuteNonQuery();

                #region == Rent Residential ==

                tableCmd.CommandText = "CREATE TABLE IF NOT EXISTS rent_residentials (" +
                    "property_id TEXT NOT NULL PRIMARY KEY," +
                    //"residential_id TEXT NOT NULL," +
                    "property_type TEXT NOT NULL DEFAULT ''," +
                    "is_unit_ownership INTEGER NOT NULL DEFAULT 0," +
                    "property_structure TEXT NOT NULL DEFAULT ''," +
                    "floor_count_above_ground INTEGER NOT NULL DEFAULT 0," +
                    "floor_count_basement INTEGER NOT NULL DEFAULT 0," +
                    "total_unit_count INTEGER NOT NULL DEFAULT 0," +
                    "built_year_month TEXT NOT NULL DEFAULT ''," +
                    "fudousan_id TEXT NOT NULL DEFAULT ''," +
                    "fudousan_id_additional_code TEXT NOT NULL DEFAULT ''," +
                    "remarks TEXT NOT NULL DEFAULT ''," +
                    "has_autolock INTEGER NOT NULL DEFAULT 0," +
                    "has_elevator INTEGER NOT NULL DEFAULT 0," +
                    "has_security_camera INTEGER NOT NULL DEFAULT 0," +
                    "has_parcel_locker INTEGER NOT NULL DEFAULT 0," +
                    "has_electric INTEGER NOT NULL DEFAULT 0," +
                    "property_electric_kind TEXT NOT NULL DEFAULT '" + nameof(Models.Rent.Residentials.Property.EnumElectricType.Unspecified) + "'," +
                    "electric_detail TEXT NOT NULL DEFAULT ''," +
                    "has_gas INTEGER NOT NULL DEFAULT 0," +
                    "gas_type TEXT NOT NULL DEFAULT '" + nameof(Models.Rent.Residentials.FacilityGasType.Unspecified) + "'," +
                    "gas_detail TEXT NOT NULL DEFAULT ''," +
                    "tap_water_type TEXT NOT NULL DEFAULT '" + nameof(Models.Rent.Residentials.FacilityTapWaterType.Unspecified) + "'," +
                    "tap_water_detail TEXT NOT NULL DEFAULT ''," +
                    "sewerage_type TEXT NOT NULL DEFAULT '" + nameof(Models.Rent.Residentials.FacilitySewerageType.Unspecified) + "'," +
                    "sewerage_detail TEXT NOT NULL DEFAULT ''," +
                    "ground_digital INTEGER NOT NULL DEFAULT 0," +
                    "bs_antenna INTEGER NOT NULL DEFAULT 0," +
                    "cs_antenna INTEGER NOT NULL DEFAULT 0," +
                    "catv INTEGER NOT NULL DEFAULT 0," +
                    "usen INTEGER NOT NULL DEFAULT 0," +
                    "internet INTEGER NOT NULL DEFAULT 0," +
                    "fiber INTEGER NOT NULL DEFAULT 0," +
                    "internet_free INTEGER NOT NULL DEFAULT 0," +
                    "tv_antenna_detail TEXT NOT NULL DEFAULT ''," +
                    "internet_detail TEXT NOT NULL DEFAULT ''," +
                    "has_parking INTEGER NOT NULL DEFAULT 0," +
                    "parking_contract_type TEXT NOT NULL DEFAULT '" + nameof(Models.Rent.Residentials.FacilityParkingContractType.Optional) + "'," +
                    "parking_availability TEXT NOT NULL DEFAULT '" + nameof(Models.Rent.Residentials.FacilityAvailabilityType.Unspecified) + "'," +
                    "is_parking_free INTEGER NOT NULL DEFAULT 0," +
                    "parking_fee TEXT NOT NULL DEFAULT ''," +
                    "parking_fee_tax_type TEXT NOT NULL DEFAULT '" + nameof(Models.Rent.Residentials.FacilityTaxType.Included) + "'," +
                    "parking_spaces TEXT NOT NULL DEFAULT ''," +
                    "parking_distance TEXT NOT NULL DEFAULT ''," +
                    "parking_notes TEXT NOT NULL DEFAULT ''," +
                    "has_bicycle_parking INTEGER NOT NULL DEFAULT 0," +
                    "bicycle_parking_availability TEXT NOT NULL DEFAULT '" + nameof(Models.Rent.Residentials.FacilityAvailabilityType.Unspecified) + "'," +
                    "is_bicycle_parking_free INTEGER NOT NULL DEFAULT 0," +
                    "bicycle_parking_fee TEXT NOT NULL DEFAULT ''," +
                    "bicycle_parking_fee_tax_type TEXT NOT NULL DEFAULT '" + nameof(Models.Rent.Residentials.FacilityTaxType.Included) + "'," +
                    "bicycle_parking_notes TEXT NOT NULL DEFAULT ''," +
                    "has_motorcycle_parking INTEGER NOT NULL DEFAULT 0," +
                    "motorcycle_parking_availability TEXT NOT NULL DEFAULT '" + nameof(Models.Rent.Residentials.FacilityAvailabilityType.Unspecified) + "'," +
                    "is_motorcycle_parking_free INTEGER NOT NULL DEFAULT 0," +
                    "motorcycle_parking_fee TEXT NOT NULL DEFAULT ''," +
                    "motorcycle_parking_fee_tax_type TEXT NOT NULL DEFAULT '" + nameof(Models.Rent.Residentials.FacilityTaxType.Included) + "'," +
                    "motorcycle_parking_notes TEXT NOT NULL DEFAULT ''," +


                    //"updated_at TEXT NOT NULL DEFAULT (DATETIME('now', 'utc'))," +
                    "FOREIGN KEY (property_id) REFERENCES properties(property_id) ON DELETE CASCADE" +
                    ")";
                tableCmd.ExecuteNonQuery();

                tableCmd.CommandText = "CREATE TABLE IF NOT EXISTS rent_residential_pictures (" +
                    "picture_id TEXT NOT NULL PRIMARY KEY," +
                    "property_id TEXT NOT NULL DEFAULT ''," +
                    "filename TEXT NOT NULL DEFAULT ''," +
                    "type TEXT NOT NULL DEFAULT ''," +
                    "description TEXT NOT NULL DEFAULT ''," +
                    "is_main INTEGER NOT NULL DEFAULT 0," +
                    "FOREIGN KEY (property_id) REFERENCES rent_residentials(property_id) ON DELETE CASCADE," + //?
                    "FOREIGN KEY (property_id) REFERENCES properties(property_id) ON DELETE CASCADE" +
                    " )";
                tableCmd.ExecuteNonQuery();

                tableCmd.CommandText = "CREATE TABLE IF NOT EXISTS rent_residential_pdfs (" +
                    "pdf_id TEXT NOT NULL PRIMARY KEY," +
                    "property_id TEXT NOT NULL DEFAULT ''," +
                    "filename TEXT NOT NULL DEFAULT ''," +
                    "thumbnail_filename TEXT NOT NULL DEFAULT ''," +
                    "type TEXT NOT NULL DEFAULT ''," +
                    "description TEXT NOT NULL DEFAULT ''," +
                    "is_main INTEGER  NOT NULL DEFAULT 0," +
                    "created_at TEXT NOT NULL DEFAULT (DATETIME('now', 'utc'))," +
                    "updated_at TEXT NOT NULL DEFAULT (DATETIME('now', 'utc'))," +
                    "FOREIGN KEY (property_id) REFERENCES rent_residentials(property_id) ON DELETE CASCADE," + //?
                    "FOREIGN KEY (property_id) REFERENCES properties(property_id) ON DELETE CASCADE" +
                    " )";
                tableCmd.ExecuteNonQuery();

                // Listing
                tableCmd.CommandText = "CREATE TABLE IF NOT EXISTS rent_residential_listings (" +
                    "listing_id TEXT NOT NULL PRIMARY KEY," +
                    "property_id TEXT NOT NULL DEFAULT ''," +
                    "is_property_unit_ownership INTEGER NOT NULL DEFAULT 0," +
                    "name TEXT NOT NULL DEFAULT ''," +

                    "floor_plan_type TEXT NOT NULL DEFAULT ''," +
                    "floor_plan_extra_type TEXT NOT NULL DEFAULT ''," +
                    "floor_area TEXT NOT NULL DEFAULT ''," +
                    "floor_type TEXT NOT NULL DEFAULT ''," +
                    "floor_number TEXT NOT NULL DEFAULT ''," +
                    "is_corner_room INTEGER NOT NULL DEFAULT 0," +
                    "main_exposure_direction TEXT NOT NULL DEFAULT ''," +
                    "availability_status TEXT NOT NULL DEFAULT ''," +
                    "current_status TEXT NOT NULL DEFAULT ''," +
                    "is_recruiting INTEGER NOT NULL DEFAULT 0," +
                    "available_from_month TEXT NOT NULL DEFAULT ''," +
                    "available_from_period TEXT NOT NULL DEFAULT ''," +
                    "is_immediate_occupancy INTEGER NOT NULL DEFAULT 0," +
                    "current_status_checked_at TEXT NOT NULL DEFAULT ''," +

                    "management_fee TEXT NOT NULL DEFAULT ''," +
                    "management_fee_unit TEXT NOT NULL DEFAULT 'Yen'," +
                    "security_deposit TEXT NOT NULL DEFAULT ''," +
                    "security_deposit_unit TEXT NOT NULL DEFAULT 'Months'," +
                    "key_money TEXT NOT NULL DEFAULT ''," +
                    "key_money_unit TEXT NOT NULL DEFAULT 'Months'," +
                    "contract_type TEXT NOT NULL DEFAULT 'Unspecified'," +
                    "contract_period_years TEXT NOT NULL DEFAULT ''," +
                    "renewal_fee TEXT NOT NULL DEFAULT ''," +
                    "renewal_fee_unit TEXT NOT NULL DEFAULT 'Months'," +
                    "recontract_fee TEXT NOT NULL DEFAULT ''," +
                    "recontract_fee_unit TEXT NOT NULL DEFAULT 'Yen'," +
                    "guarantor_company TEXT NOT NULL DEFAULT ''," +
                    "housing_insurance TEXT NOT NULL DEFAULT ''," +
                    "other_fees TEXT NOT NULL DEFAULT ''," +
                    "pet_friendly INTEGER NOT NULL DEFAULT 0," +
                    "two_person_occupancy INTEGER NOT NULL DEFAULT 0," +
                    "no_guarantor_needed INTEGER NOT NULL DEFAULT 0," +
                    "office_use_allowed INTEGER NOT NULL DEFAULT 0," +
                    "other_conditions TEXT NOT NULL DEFAULT ''," +
                    "contract_remarks TEXT NOT NULL DEFAULT ''," +
                    "handling_type TEXT NOT NULL DEFAULT 'Unspecified'," +
                    "transaction_type TEXT NOT NULL DEFAULT 'Unspecified'," +
                    "lessor_commission TEXT NOT NULL DEFAULT ''," +
                    "lessor_commission_unit TEXT NOT NULL DEFAULT 'Percent'," +
                    "lessee_commission TEXT NOT NULL DEFAULT ''," +
                    "lessee_commission_unit TEXT NOT NULL DEFAULT 'Percent'," +
                    "listing_broker_commission TEXT NOT NULL DEFAULT ''," +
                    "listing_broker_commission_unit TEXT NOT NULL DEFAULT 'Percent'," +
                    "tenant_broker_commission TEXT NOT NULL DEFAULT ''," +
                    "tenant_broker_commission_unit TEXT NOT NULL DEFAULT 'Percent'," +
                    "transaction_remarks TEXT NOT NULL DEFAULT ''," +

                    "has_air_conditioner INTEGER NOT NULL DEFAULT 0," +
                    "has_wood_flooring INTEGER NOT NULL DEFAULT 0," +
                    "has_video_intercom INTEGER NOT NULL DEFAULT 0," +
                    "has_bath_reheating INTEGER NOT NULL DEFAULT 0," +
                    "has_gas_stove INTEGER NOT NULL DEFAULT 0," +
                    "has_induction_cooktop INTEGER NOT NULL DEFAULT 0," +
                    "has_multiple_burners INTEGER NOT NULL DEFAULT 0," +
                    "has_system_kitchen INTEGER NOT NULL DEFAULT 0," +
                    "has_loft INTEGER NOT NULL DEFAULT 0," +
                    "has_indoor_washing_machine_space INTEGER NOT NULL DEFAULT 0," +
                    "has_bathroom_dryer INTEGER NOT NULL DEFAULT 0," +
                    "has_separate_bath_and_toilet INTEGER NOT NULL DEFAULT 0," +
                    "has_separate_washbasin INTEGER NOT NULL DEFAULT 0," +
                    "has_warm_water_toilet_seat INTEGER NOT NULL DEFAULT 0," +
                    "appliance_remarks TEXT NOT NULL DEFAULT ''," +

                    "remarks TEXT NOT NULL DEFAULT ''," +

                    "chinryou INTEGER NOT NULL DEFAULT 0," +

                    "created_at TEXT NOT NULL DEFAULT (DATETIME('now'))," +
                    "updated_at TEXT NOT NULL DEFAULT (DATETIME('now'))," +
                    "FOREIGN KEY (property_id) REFERENCES rent_residentials(property_id) ON DELETE CASCADE," + //?
                    "FOREIGN KEY (property_id) REFERENCES properties(property_id) ON DELETE CASCADE" +
                    " )";
                tableCmd.ExecuteNonQuery();

                tableCmd.CommandText = "CREATE TABLE IF NOT EXISTS rent_residential_listing_pictures (" +
                    "picture_id TEXT NOT NULL PRIMARY KEY," +
                    "listing_id TEXT NOT NULL DEFAULT ''," +
                    "property_id TEXT NOT NULL DEFAULT ''," +
                    "filename TEXT NOT NULL DEFAULT ''," +
                    "type TEXT NOT NULL DEFAULT ''," +
                    "description TEXT NOT NULL DEFAULT ''," +
                    "is_main INTEGER  NOT NULL DEFAULT 0," +
                    "FOREIGN KEY (listing_id) REFERENCES rent_residential_listings(listing_id) ON DELETE CASCADE," +
                    "FOREIGN KEY (property_id) REFERENCES rent_residentials(property_id) ON DELETE CASCADE," + //?
                    "FOREIGN KEY (property_id) REFERENCES properties(property_id) ON DELETE CASCADE" +
                    " )";
                tableCmd.ExecuteNonQuery();

                tableCmd.CommandText = "CREATE TABLE IF NOT EXISTS rent_residential_listing_pdfs (" +
                    "pdf_id TEXT NOT NULL PRIMARY KEY," +
                    "listing_id TEXT NOT NULL DEFAULT ''," +
                    "property_id TEXT NOT NULL DEFAULT ''," +
                    "filename TEXT NOT NULL DEFAULT ''," +
                    "thumbnail_filename TEXT NOT NULL DEFAULT ''," +
                    "type TEXT NOT NULL DEFAULT ''," +
                    "description TEXT NOT NULL DEFAULT ''," +
                    "is_main INTEGER  NOT NULL DEFAULT 0," +
                    "created_at TEXT NOT NULL DEFAULT (DATETIME('now', 'utc'))," +
                    "updated_at TEXT NOT NULL DEFAULT (DATETIME('now', 'utc'))," +
                    "FOREIGN KEY (listing_id) REFERENCES rent_residential_listings(listing_id) ON DELETE CASCADE," +
                    "FOREIGN KEY (property_id) REFERENCES rent_residentials(property_id) ON DELETE CASCADE," + //?
                    "FOREIGN KEY (property_id) REFERENCES properties(property_id) ON DELETE CASCADE" +
                    " )";
                tableCmd.ExecuteNonQuery();

                tableCmd.CommandText = """
                    CREATE INDEX IF NOT EXISTS ix_properties_updated_at
                        ON properties(updated_at);

                    CREATE INDEX IF NOT EXISTS ix_properties_kind_name
                        ON properties(property_kind, name);

                    CREATE INDEX IF NOT EXISTS ix_rent_residential_pictures_property_id
                        ON rent_residential_pictures(property_id);

                    CREATE INDEX IF NOT EXISTS ix_rent_residential_pdfs_property_id
                        ON rent_residential_pdfs(property_id);
                    
                    CREATE INDEX IF NOT EXISTS ix_rent_residential_rooms_property_id
                        ON rent_residential_listings(property_id);

                    CREATE INDEX IF NOT EXISTS ix_rent_residential_room_pictures_listing_id
                        ON rent_residential_listing_pictures(listing_id);

                    CREATE INDEX IF NOT EXISTS ix_rent_residential_room_pdfs_listing_id
                        ON rent_residential_listing_pdfs(listing_id);
                    """;
                // TODO: more?
                tableCmd.ExecuteNonQuery();

                #endregion

                #region == Rent Commercial ==

                tableCmd.CommandText = """
    CREATE TABLE IF NOT EXISTS rent_commercials (
        property_id TEXT NOT NULL PRIMARY KEY,
        commercial_kind TEXT NOT NULL DEFAULT '',
        is_unit_ownership INTEGER NOT NULL DEFAULT 0,
        property_structure TEXT NOT NULL DEFAULT '',
        floor_count_above_ground INTEGER NOT NULL DEFAULT 0,
        floor_count_basement INTEGER NOT NULL DEFAULT 0,
        total_floor_area NUMERIC NOT NULL DEFAULT 0,
        built_year_month TEXT NOT NULL DEFAULT '',
        fudousan_id TEXT NOT NULL DEFAULT '',
        fudousan_id_additional_code TEXT NOT NULL DEFAULT '',
        remarks TEXT NOT NULL DEFAULT '',
        created_at TEXT NOT NULL DEFAULT (DATETIME('now', 'utc')),
        updated_at TEXT NOT NULL DEFAULT (DATETIME('now', 'utc')),
        FOREIGN KEY (property_id)
            REFERENCES properties(property_id)
            ON DELETE CASCADE
    );

    CREATE TABLE IF NOT EXISTS rent_commercial_units (
        listing_id TEXT NOT NULL PRIMARY KEY,
        property_id TEXT NOT NULL DEFAULT '',
        is_property_unit_ownership INTEGER NOT NULL DEFAULT 0,
        name TEXT NOT NULL DEFAULT '',
        chinryou NUMERIC NOT NULL DEFAULT 0,
        kyoueki_fee NUMERIC NOT NULL DEFAULT 0,
        shikikin NUMERIC NOT NULL DEFAULT 0,
        shikikin_unit TEXT NOT NULL DEFAULT '',
        reikin NUMERIC NOT NULL DEFAULT 0,
        reikin_unit TEXT NOT NULL DEFAULT '',
        renewal_fee NUMERIC NOT NULL DEFAULT 0,
        renewal_fee_unit TEXT NOT NULL DEFAULT '',
        recontract_fee NUMERIC NOT NULL DEFAULT 0,
        recontract_fee_unit TEXT NOT NULL DEFAULT '',
        floor_area NUMERIC NOT NULL DEFAULT 0,
        usage TEXT NOT NULL DEFAULT '',
        business_hours TEXT NOT NULL DEFAULT '',
        parking_available INTEGER NOT NULL DEFAULT 0,
        other_conditions TEXT NOT NULL DEFAULT '',
        remarks TEXT NOT NULL DEFAULT '',
        created_at TEXT NOT NULL DEFAULT (DATETIME('now', 'utc')),
        updated_at TEXT NOT NULL DEFAULT (DATETIME('now', 'utc')),
        FOREIGN KEY (property_id)
            REFERENCES rent_commercials(property_id)
            ON DELETE CASCADE
    );

    CREATE INDEX IF NOT EXISTS
        ix_rent_commercial_units_property_id
        ON rent_commercial_units(property_id);


    CREATE TABLE IF NOT EXISTS rent_commercial_pictures (
        picture_id TEXT NOT NULL PRIMARY KEY,
        property_id TEXT NOT NULL,
        filename TEXT NOT NULL,
        type TEXT NOT NULL,
        description TEXT NOT NULL,
        is_main INTEGER NOT NULL DEFAULT 0,
        FOREIGN KEY (property_id) REFERENCES rent_commercials(property_id) ON DELETE CASCADE
    );

    CREATE TABLE IF NOT EXISTS rent_commercial_pdfs (
        pdf_id TEXT NOT NULL PRIMARY KEY,
        property_id TEXT NOT NULL,
        filename TEXT NOT NULL,
        thumbnail_filename TEXT NOT NULL,
        type TEXT NOT NULL,
        description TEXT NOT NULL,
        is_main INTEGER NOT NULL DEFAULT 0,
        created_at TEXT NOT NULL DEFAULT (DATETIME('now', 'utc')),
        updated_at TEXT NOT NULL DEFAULT (DATETIME('now', 'utc')),
        FOREIGN KEY (property_id) REFERENCES rent_commercials(property_id) ON DELETE CASCADE
    );

    CREATE INDEX IF NOT EXISTS ix_rent_commercial_pictures_property_id
        ON rent_commercial_pictures(property_id);

    CREATE INDEX IF NOT EXISTS ix_rent_commercial_pdfs_property_id
        ON rent_commercial_pdfs(property_id);
    """;

                tableCmd.ExecuteNonQuery();

                #endregion

                #region == Rent Lessor ==

                tableCmd.CommandText = "CREATE TABLE IF NOT EXISTS rent_lessors (" +
                    "lessor_id TEXT NOT NULL PRIMARY KEY," +
                    "name TEXT NOT NULL DEFAULT ''," +
                    "person_kind TEXT NOT NULL DEFAULT ''," +
                    "name_last TEXT NOT NULL DEFAULT ''," +
                    "name_first TEXT NOT NULL DEFAULT ''," +
                    "name_company TEXT NOT NULL DEFAULT ''," +
                    "name_company_type TEXT NOT NULL DEFAULT ''," +
                    "name_company_type_position INTEGER NOT NULL DEFAULT 0," +

                    // Phone numbers
                    // Address
                    "remarks TEXT NOT NULL DEFAULT ''," +

                    "updated_at TEXT NOT NULL DEFAULT (DATETIME('now', 'utc'))," +
                    "created_at TEXT NOT NULL DEFAULT (DATETIME('now', 'utc'))" + // Last column, no comma
                    ")";
                tableCmd.ExecuteNonQuery();

                // A composite primary key
                tableCmd.CommandText = "CREATE TABLE IF NOT EXISTS rent_lessors_properties_listings (" +
                    "lessor_id TEXT NOT NULL DEFAULT ''," +
                    "property_id TEXT NOT NULL DEFAULT ''," +
                    "property_kind TEXT NOT NULL DEFAULT ''," +
                    "listing_id TEXT NOT NULL DEFAULT ''," +

                    "PRIMARY KEY (lessor_id, property_id, listing_id)," +

                    "FOREIGN KEY (lessor_id) REFERENCES rent_lessors(lessor_id) ON DELETE CASCADE," +
                    "FOREIGN KEY (property_id) REFERENCES properties(property_id) ON DELETE CASCADE" +
                    ")";
                tableCmd.ExecuteNonQuery();
                /*
                // Or
                tableCmd.CommandText = "CREATE TABLE IF NOT EXISTS rent_lessors_properties_listings (" +
                    "id INTEGER PRIMARY KEY AUTOINCREMENT" +
                    "lessor_id TEXT NOT NULL," +
                    "property_id TEXT NOT NULL," +
                    "property_kind TEXT NOT NULL," +
                    "listing_id TEXT NOT NULL," +

                    "UNIQUE(lessor_id, property_id, listing_id)" +
                    ")";
                tableCmd.ExecuteNonQuery();
                */

                tableCmd.CommandText = """
                    CREATE INDEX IF NOT EXISTS ix_rent_lessors_properties_property_id
                        ON rent_lessors_properties_listings(property_id);

                    CREATE INDEX IF NOT EXISTS ix_rent_lessors_properties_lessor_id
                        ON rent_lessors_properties_listings(lessor_id);
                    """;
                tableCmd.ExecuteNonQuery();

                #endregion

                #region == Sale Residentials ==

                // Sale tables
                tableCmd.CommandText = """
    CREATE TABLE IF NOT EXISTS sale_residentials (
        property_id TEXT NOT NULL PRIMARY KEY,
        property_type TEXT NOT NULL DEFAULT '',
        is_unit_ownership INTEGER NOT NULL DEFAULT 0,
        property_structure TEXT NOT NULL DEFAULT '',
        floor_count_above_ground INTEGER NOT NULL DEFAULT 0,
        floor_count_basement INTEGER NOT NULL DEFAULT 0,
        total_unit_count INTEGER NOT NULL DEFAULT 0,
        built_year_month TEXT NOT NULL,
        fudousan_id TEXT NOT NULL,
        fudousan_id_additional_code TEXT NOT NULL,
        remarks TEXT NOT NULL DEFAULT '',
        created_at TEXT NOT NULL DEFAULT (DATETIME('now', 'utc')),
        updated_at TEXT NOT NULL DEFAULT (DATETIME('now', 'utc')),
        FOREIGN KEY (property_id)
            REFERENCES properties(property_id)
            ON DELETE CASCADE
    );

    CREATE TABLE IF NOT EXISTS sale_residential_pictures (
        picture_id TEXT NOT NULL PRIMARY KEY,
        property_id TEXT NOT NULL DEFAULT '',
        filename TEXT NOT NULL DEFAULT '',
        type TEXT NOT NULL DEFAULT '',
        description TEXT NOT NULL DEFAULT '',
        is_main INTEGER NOT NULL DEFAULT 0,
        created_at TEXT NOT NULL DEFAULT (DATETIME('now', 'utc')),
        updated_at TEXT NOT NULL DEFAULT (DATETIME('now', 'utc')),
        FOREIGN KEY (property_id)
            REFERENCES sale_residentials(property_id)
            ON DELETE CASCADE
    );

    CREATE TABLE IF NOT EXISTS sale_residential_pdfs (
        pdf_id TEXT NOT NULL PRIMARY KEY,
        property_id TEXT NOT NULL DEFAULT '',
        filename TEXT NOT NULL DEFAULT '',
        thumbnail_filename TEXT NOT NULL DEFAULT '',
        type TEXT NOT NULL DEFAULT '',
        description TEXT NOT NULL DEFAULT '',
        is_main INTEGER NOT NULL DEFAULT 0,
        created_at TEXT NOT NULL DEFAULT (DATETIME('now', 'utc')),
        updated_at TEXT NOT NULL DEFAULT (DATETIME('now', 'utc')),
        FOREIGN KEY (property_id)
            REFERENCES sale_residentials(property_id)
            ON DELETE CASCADE
    );

    CREATE TABLE IF NOT EXISTS sale_residential_units (
        listing_id TEXT NOT NULL PRIMARY KEY,
        property_id TEXT NOT NULL DEFAULT '',
        is_property_unit_ownership INTEGER NOT NULL DEFAULT 0,
        name TEXT NOT NULL DEFAULT '',
        sale_price NUMERIC NOT NULL DEFAULT 0,
        management_fee NUMERIC NOT NULL DEFAULT 0,
        repair_reserve_fund NUMERIC NOT NULL DEFAULT 0,
        ownership_type TEXT NOT NULL DEFAULT '',
        occupancy_status TEXT NOT NULL DEFAULT '',
        delivery_timing TEXT NOT NULL DEFAULT '',
        remarks TEXT NOT NULL DEFAULT '',
        created_at TEXT NOT NULL DEFAULT (DATETIME('now', 'utc')),
        updated_at TEXT NOT NULL DEFAULT (DATETIME('now', 'utc')),
        FOREIGN KEY (property_id)
            REFERENCES sale_residentials(property_id)
            ON DELETE CASCADE
    );

    CREATE TABLE IF NOT EXISTS sale_residential_unit_pictures (
        picture_id TEXT NOT NULL PRIMARY KEY,
        listing_id TEXT NOT NULL DEFAULT '',
        property_id TEXT NOT NULL DEFAULT '',
        filename TEXT NOT NULL DEFAULT '',
        type TEXT NOT NULL DEFAULT '',
        description TEXT NOT NULL DEFAULT '',
        is_main INTEGER NOT NULL DEFAULT 0,
        created_at TEXT NOT NULL DEFAULT (DATETIME('now', 'utc')),
        updated_at TEXT NOT NULL DEFAULT (DATETIME('now', 'utc')),
        FOREIGN KEY (listing_id)
            REFERENCES sale_residential_units(listing_id)
            ON DELETE CASCADE,
        FOREIGN KEY (property_id)
            REFERENCES sale_residentials(property_id)
            ON DELETE CASCADE
    );

    CREATE TABLE IF NOT EXISTS sale_residential_unit_pdfs (
        pdf_id TEXT NOT NULL PRIMARY KEY,
        listing_id TEXT NOT NULL DEFAULT '',
        property_id TEXT NOT NULL DEFAULT '',
        filename TEXT NOT NULL DEFAULT '',
        thumbnail_filename TEXT NOT NULL DEFAULT '',
        type TEXT NOT NULL DEFAULT '',
        description TEXT NOT NULL DEFAULT '',
        is_main INTEGER NOT NULL DEFAULT 0,
        created_at TEXT NOT NULL DEFAULT (DATETIME('now', 'utc')),
        updated_at TEXT NOT NULL DEFAULT (DATETIME('now', 'utc')),
        FOREIGN KEY (listing_id)
            REFERENCES sale_residential_units(listing_id)
            ON DELETE CASCADE,
        FOREIGN KEY (property_id)
            REFERENCES sale_residentials(property_id)
            ON DELETE CASCADE
    );

    CREATE INDEX IF NOT EXISTS ix_sale_residential_units_property_id
        ON sale_residential_units(property_id);

    CREATE INDEX IF NOT EXISTS ix_sale_residential_pictures_property_id
        ON sale_residential_pictures(property_id);

    CREATE INDEX IF NOT EXISTS ix_sale_residential_pdfs_property_id
        ON sale_residential_pdfs(property_id);
    """;

                tableCmd.ExecuteNonQuery();

                #endregion

                #region == Broker ==

                tableCmd.CommandText = """
                    CREATE TABLE IF NOT EXISTS brokers (
                        broker_id TEXT NOT NULL PRIMARY KEY,
                        name TEXT NOT NULL DEFAULT '',
                        person_kind TEXT NOT NULL,
                        name_last TEXT NOT NULL DEFAULT '',
                        name_first TEXT NOT NULL DEFAULT '',
                        name_company TEXT NOT NULL DEFAULT '',
                        name_company_type TEXT NOT NULL DEFAULT '',
                        name_company_type_position INTEGER NOT NULL DEFAULT 0,
                        remarks TEXT NOT NULL DEFAULT '',
                        updated_at TEXT NOT NULL DEFAULT (DATETIME('now', 'utc')),
                        created_at TEXT NOT NULL DEFAULT (DATETIME('now', 'utc'))
                    );
                    """;
                tableCmd.ExecuteNonQuery();

                tableCmd.CommandText = """
                    CREATE TABLE IF NOT EXISTS brokers_properties_listings (
                        broker_id TEXT NOT NULL DEFAULT '',
                        property_id TEXT NOT NULL DEFAULT '',
                        property_kind TEXT NOT NULL DEFAULT '',
                        listing_id TEXT NOT NULL DEFAULT '',
                        PRIMARY KEY (broker_id, property_id, listing_id),
                        FOREIGN KEY (broker_id)
                            REFERENCES brokers(broker_id)
                            ON DELETE CASCADE
                    );

                    CREATE INDEX IF NOT EXISTS
                        ix_brokers_properties_listings_property_id
                        ON brokers_properties_listings(property_id);
                    """;

                tableCmd.ExecuteNonQuery();

                #endregion


                // 
                AddColumnsIfNotExist(connection);

                tableCmd.Transaction.Commit();
            }

            catch (Exception ex)
            {
                tableCmd.Transaction.Rollback();

                SetDatabaseError(res, ex, "transaction.Commit", "Failed to initialize database tables. Transaction.Rollback()", nameof(InitializeDatabase));
            }
        }
        catch (Exception ex)
        {
            SetDatabaseError(res, ex, "connection.Open", "Failed to Connect to a SQLite database file", nameof(InitializeDatabase));
        }

        return res;
    }

    private static void AddColumnsIfNotExist(SqliteConnection conn)
    {
        _ = conn; // Just to avoid the warning. The code below is commented out for now, but it may be useful in the future if we need to add columns to existing tables.
        //var cmd = conn.CreateCommand();
        //bool exists = false;

        #region == add to properties ==
        /*
        cmd.CommandText = "PRAGMA table_info(properties);";
        
        using (var reader = cmd.ExecuteReader())
        {
            // created_at
            while (reader.Read())
            {
                if (reader.GetString(1) == "created_at")
                {
                    exists = true;
                    break;
                }
            }
            if (!exists)
            {
                var altcmd = conn.CreateCommand();

                try
                {
                    altcmd.CommandText = "ALTER TABLE properties ADD COLUMN created_at TEXT NOT NULL DEFAULT (DATETIME('now', 'utc'));";
                    altcmd.ExecuteNonQuery();
                }
                catch (SqliteException ex)
                {
                    Debug.WriteLine("SqliteException on ADD COLUMN created_at @InitializeDatabase: " + ex.Message);
                }
            }

            reader.Close();
        }

        using (var reader = cmd.ExecuteReader())
        {
            exists = false;
            while (reader.Read())
            {
                if (reader.GetString(1) == "updated_at")
                {
                    exists = true;
                    break;
                }
            }
            if (!exists)
            {
                var altcmd = conn.CreateCommand();

                try
                {
                    altcmd.CommandText = "ALTER TABLE properties ADD COLUMN updated_at TEXT NOT NULL DEFAULT (DATETIME('now', 'utc'));";
                    altcmd.ExecuteNonQuery();
                }
                catch (SqliteException ex)
                {
                    Debug.WriteLine("SqliteException on ADD COLUMN updated_at @InitializeDatabase: " + ex.Message);
                }
            }
            reader.Close();
        }
        */
        #endregion

        #region == add to rent_residentials ==
        /*
        cmd.CommandText = "PRAGMA table_info(rent_residentials);";
        using (var reader = cmd.ExecuteReader())
        {
            exists = false;
            while (reader.Read())
            {
                if (reader.GetString(1) == "updated_at")
                {
                    exists = true;
                    break;
                }
            }
            if (!exists)
            {
                var altcmd = conn.CreateCommand();

                try
                {
                    altcmd.CommandText = "ALTER TABLE rent_residentials ADD COLUMN updated_at TEXT NOT NULL DEFAULT '';";
                    altcmd.ExecuteNonQuery();
                }
                catch (SqliteException ex)
                {
                    Debug.WriteLine("SqliteException on ADD COLUMN updated_at @InitializeDatabase: " + ex.Message);
                }
            }
            reader.Close();
        }
        */
        #endregion

        #region == add to rent_residential_listings ==
        /*
        cmd.CommandText = "PRAGMA table_info(rent_residential_listings);";
        
        using (var reader = cmd.ExecuteReader())
        {
            exists = false;
            while (reader.Read())
            {
                if (reader.GetString(1) == "chinryou")
                {
                    exists = true;
                    break;
                }
            }
            if (!exists)
            {
                var altcmd = conn.CreateCommand();

                try
                {
                    altcmd.CommandText = "ALTER TABLE rent_residential_listings ADD COLUMN chinryou INTEGER NOT NULL DEFAULT 0;";
                    altcmd.ExecuteNonQuery();
                }
                catch (SqliteException ex)
                {
                    Debug.WriteLine("SqliteException on ADD COLUMN chinryou @InitializeDatabase: " + ex.Message);
                }
            }

            reader.Close();
        }

        using (var reader = cmd.ExecuteReader())
        {
            exists = false;
            while (reader.Read())
            {
                if (reader.GetString(1) == "created_at")
                {
                    exists = true;
                    break;
                }
            }

            if (!exists)
            {
                var altcmd = conn.CreateCommand();

                try
                {
                    altcmd.CommandText = "ALTER TABLE rent_residential_listings ADD COLUMN created_at TEXT NOT NULL DEFAULT '';";
                    altcmd.ExecuteNonQuery();
                }
                catch (SqliteException ex)
                {
                    Debug.WriteLine("SqliteException on ADD COLUMN created_at @InitializeDatabase: " + ex.Message);
                }
            }

            reader.Close();
        }

        using (var reader = cmd.ExecuteReader())
        {
            exists = false;
            while (reader.Read())
            {
                if (reader.GetString(1) == "updated_at")
                {
                    exists = true;
                    break;
                }
            }

            if (!exists)
            {
                var altcmd = conn.CreateCommand();

                try
                {
                    altcmd.CommandText = "ALTER TABLE rent_residential_listings ADD COLUMN updated_at TEXT NOT NULL DEFAULT '';";
                    altcmd.ExecuteNonQuery();
                }
                catch (SqliteException ex)
                {
                    Debug.WriteLine("SqliteException on ADD COLUMN updated_at @InitializeDatabase: " + ex.Message);
                }
            }

            reader.Close();
        }
        */
        #endregion

        #region == add to rent_residential_pdfs ==
        /*
        cmd.CommandText = "PRAGMA table_info(rent_residential_pdfs);";
        using (var reader = cmd.ExecuteReader())
        {
            exists = false;

            while (reader.Read())
            {
                if (reader.GetString(1) == "created_at")
                {
                    exists = true;
                    break;
                }
            }

            if (!exists)
            {
                var altcmd = conn.CreateCommand();

                try
                {
                    altcmd.CommandText = "ALTER TABLE rent_residential_pdfs ADD COLUMN created_at TEXT NOT NULL DEFAULT '';";
                    altcmd.ExecuteNonQuery();
                }
                catch (SqliteException ex)
                {
                    Debug.WriteLine("SqliteException on ADD COLUMN created_at @InitializeDatabase: " + ex.Message);
                }
            }

            reader.Close();
        }

        using (var reader = cmd.ExecuteReader())
        {
            exists = false;
            while (reader.Read())
            {
                if (reader.GetString(1) == "updated_at")
                {
                    exists = true;
                    break;
                }
            }

            if (!exists)
            {
                var altcmd = conn.CreateCommand();

                try
                {
                    altcmd.CommandText = "ALTER TABLE rent_residential_pdfs ADD COLUMN updated_at TEXT NOT NULL DEFAULT '';";
                    altcmd.ExecuteNonQuery();
                }
                catch (SqliteException ex)
                {
                    Debug.WriteLine("SqliteException on ADD COLUMN updated_at @InitializeDatabase: " + ex.Message);
                }
            }

            reader.Close();
        }

        using (var reader = cmd.ExecuteReader())
        {
            exists = false;
            while (reader.Read())
            {
                if (reader.GetString(1) == "type")
                {
                    exists = true;
                    break;
                }
            }

            if (!exists)
            {
                var altcmd = conn.CreateCommand();

                try
                {
                    altcmd.CommandText = "ALTER TABLE rent_residential_pdfs ADD COLUMN type TEXT NOT NULL DEFAULT '';";
                    altcmd.ExecuteNonQuery();
                }
                catch (SqliteException ex)
                {
                    Debug.WriteLine("SqliteException on ADD COLUMN type @InitializeDatabase: " + ex.Message);
                }
            }

            reader.Close();
        }
        */
        #endregion

        #region == add to rent_residential_listing_pictures ==
        /*
        cmd.CommandText = "PRAGMA table_info(rent_residential_listing_pictures);";
        exists = false;
        using (var reader = cmd.ExecuteReader())
        {
            while (reader.Read())
            {
                if (reader.GetString(1) == "listing_id")
                {
                    exists = true;
                    break;
                }
            }

            if (!exists)
            {
                var altcmd = conn.CreateCommand();

                try
                {
                    altcmd.CommandText = "ALTER TABLE rent_residential_listing_pictures ADD COLUMN listing_id TEXT NOT NULL DEFAULT '';";
                    altcmd.ExecuteNonQuery();
                }
                catch (SqliteException ex)
                {
                    Debug.WriteLine("SqliteException on ADD COLUMN listing_id @InitializeDatabase: " + ex.Message);
                }
            }
            reader.Close();
        }
        */
        #endregion
    }

    #endregion

    #region == Properties ==

    public PropertySearchResultWrapper SelectRecentProperties()
    {
        var res = new PropertySearchResultWrapper();

        _readerWriterLock.EnterReadLock();
        try
        {
            using var connection = new SqliteConnection(_connectionStringBuilder.ConnectionString);
            connection.Open();
            using var cmd = connection.CreateCommand();
            cmd.CommandText = "SELECT * FROM properties ORDER BY updated_at DESC LIMIT 10"; // limit 10 for now.

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                var id = reader.GetString(reader.GetOrdinal("property_id")) ?? string.Empty; //Convert.ToString(reader["property_id"]);
                if (string.IsNullOrEmpty(id))
                {
                    Debug.WriteLine("DataAccess::SelectRecentProperties: property_id is null or empty .");
                    continue;
                }

                var enumKind = PropertyContextType.Unknown;
                var kind = reader.GetString(reader.GetOrdinal("property_kind")) ?? string.Empty;
                if (!string.IsNullOrEmpty(kind))
                {
                    if (Enum.TryParse<PropertyContextType>(kind, out var parsedKind))
                    {
                        enumKind = parsedKind;
                    }
                    else
                    {
                        Debug.WriteLine($"property_kind ({kind}) parse failed. @SelectRecentProperties()");
                    }
                }

                var item = new Models.SearchResult.PropertySearchResultItem(id, enumKind);

                var name = reader.GetString(reader.GetOrdinal("name")) ?? string.Empty;//Convert.ToString(reader["name"]) ?? "";
                item.SetName(name);

                var thumb = reader.GetString(reader.GetOrdinal("thumbnail_filename")) ?? string.Empty;
                item.ThumbnailFilename = thumb;

                var createdAt = reader.GetString(reader.GetOrdinal("created_at")) ?? string.Empty;//Convert.ToString(reader["created_at"]) ?? string.Empty;
                item.CreatedAt = createdAt;
                var updatedAt = reader.GetString(reader.GetOrdinal("updated_at")) ?? string.Empty;//Convert.ToString(reader["updated_at"]) ?? string.Empty;
                item.UpdatedAt = updatedAt;

                //res.AffectedCount++;

                res.PropertySearchResult.Add(item);
            }
        }
        catch (Exception ex)
        {
            SetDatabaseError(res, ex, "connection.Open(), reader.Read()", "Failed to connect to or read a SQLite database file", nameof(SelectRecentProperties));
        }
        finally
        {
            _readerWriterLock.ExitReadLock();
        }

        return res;
    }

    public PropertySearchResultWrapper SelectPropertiesByKeyword(string keyword) // TODO: add what type of keyword eg. name/adderss/lessor.
    {
        var res = new PropertySearchResultWrapper();

        if (string.IsNullOrEmpty(keyword))
        {
            keyword = "*";
        }

        _readerWriterLock.EnterReadLock();
        try
        {
            using var connection = new SqliteConnection(_connectionStringBuilder.ConnectionString);
            connection.Open();

            using var cmd = connection.CreateCommand();
            if (keyword == "*")
            {
                //cmd.CommandText = "SELECT properties.name as propertyName, properties.property_kind as propertyKind, rent_residentials.remarks as remarks, properties.property_id as propertyId FROM rent_residentials INNER JOIN properties USING (property_id)";
                cmd.CommandText = "SELECT * FROM properties ORDER BY updated_at DESC";
            }
            else
            {
                //cmd.CommandText = string.Format("SELECT properties.name as propertyName, properties.property_kind as propertyKind, rent_residentials.remarks as remarks, properties.property_id as propertyId FROM rent_residentials INNER JOIN properties USING (property_id) WHERE properties.name LIKE '%{0}%'", keyword);
                cmd.CommandText = $"SELECT * FROM properties WHERE name LIKE '%{keyword}%' ORDER BY updated_at DESC";
            }

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                var s = Convert.ToString(reader["property_id"], CultureInfo.InvariantCulture);
                if (string.IsNullOrEmpty(s))
                {
                    Debug.WriteLine("DataAccess::SelectPropertiesByKeyword: propertyId is null or empty for a rent residential.");
                    continue;
                }

                var enumKind = PropertyContextType.Unknown;
                var kind = reader.GetString(reader.GetOrdinal("property_kind")) ?? string.Empty;
                if (!string.IsNullOrEmpty(kind))
                {
                    if (Enum.TryParse<PropertyContextType>(kind, out var parsedKind))
                    {
                        enumKind = parsedKind;
                    }
                }

                var item = new Models.SearchResult.PropertySearchResultItem(s, enumKind);
                // try...
                item.SetName(reader.GetString(reader.GetOrdinal("name")) ?? string.Empty);

                //Debug.WriteLine($"Found item: {item.Name} @SelectPropertiesByKeyword() in DataAccessService");

                // Reset item Isdirty flag.
                item.SetIsModified(false);

                //res.AffectedCount++;

                res.PropertySearchResult.Add(item);
            }
        }
        catch (Exception ex)
        {
            SetDatabaseError(res, ex, "connection.Open(), reader.Read()", "Failed to connect to or read a SQLite database file", nameof(SelectPropertiesByKeyword));
        }
        finally
        {
            _readerWriterLock.ExitReadLock();
        }

        return res;
    }

    #endregion

    #region == Rent Residential ==

    public ResultWrapper UpsertRentResidential(Models.Rent.Residentials.Property building)
    {
        var res = new ResultWrapper();

        if (string.IsNullOrEmpty(building.Id))
        {
            res.IsError = true;
            // TODO:
            return res;
        }

        _readerWriterLock.EnterWriteLock();
        try
        {
            // System.Data.SQLite
            //using var connection = new SQLiteConnection(_connectionStringBuilder.ConnectionString);
            // Microsoft.Data.Sqlite
            using var connection = new SqliteConnection(_connectionStringBuilder.ConnectionString);
            connection.Open();

            using var cmd = connection.CreateCommand();
            cmd.Transaction = connection.BeginTransaction();
            try
            {
                // Main property table
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Clear();
                // Insert
                //cmd.CommandText = "INSERT INTO properties (property_id, name, property_kind, thumbnail_filename, location_prefecture_code, location_prefecture, location_machiaza_id, location_county, location_city, location_ward, location_oaza_cho, location_choume, location_edaban, location_full, updated_at) " +
                //  "VALUES (@RentId, @Name, @PropertyContextType, @Thumb, @LocPrefId, @LocPrefecture, @LocMachiazaId, @LocCounty, @LocCity, @LocWard, @LocOazaCho, @LocChoume, @LocEdaban, @LocLocationFull, @updated_at)";
                // Upsert
                var sqlUpsert = "INSERT INTO properties (property_id, name, property_kind, thumbnail_filename, location_prefecture_code, location_prefecture, location_machiaza_id, location_county, location_city, location_ward, location_oaza_cho, location_choume, location_edaban, location_postal_code, location_full, location_latitude, location_longitude, " +
                    "train1_line_code, train1_line_name, train1_station_code, train1_station_name, train1_ekitoho, buss1_stop_name, buss1_Jyousya, buss1_Toho, " +
                    "train2_line_code, train2_line_name, train2_station_code, train2_station_name, train2_ekitoho, buss2_stop_name, buss2_Jyousya, buss2_Toho, " +
                    "train3_line_code, train3_line_name, train3_station_code, train3_station_name, train3_ekitoho, buss3_stop_name, buss3_Jyousya, buss3_Toho, " +
                    "train4_line_code, train4_line_name, train4_station_code, train4_station_name, train4_ekitoho, buss4_stop_name, buss4_Jyousya, buss4_Toho, updated_at) ";

                sqlUpsert += "VALUES (@propertyId, @name, @propertyKind, @thumbnailPath, @locPrefId, @locPrefecture, @locMachiazaId, @locCounty, @locCity, @locWard, @locOazaCho, @locChoume, @locEdaban, @locPostalCode, @locLocationFull, @locationLatitude, @locationLongitude, " +
                    "@train1_line_code, @train1_line_name, @train1_station_code, @train1_station_name, @train1_ekitoho, @buss1_stop_name, @buss1_Jyousya, @buss1_Toho, " +
                    "@train2_line_code, @train2_line_name, @train2_station_code, @train2_station_name, @train2_ekitoho, @buss2_stop_name, @buss2_Jyousya, @buss2_Toho, " +
                    "@train3_line_code, @train3_line_name, @train3_station_code, @train3_station_name, @train3_ekitoho, @buss3_stop_name, @buss3_Jyousya, @buss3_Toho, " +
                    "@train4_line_code, @train4_line_name, @train4_station_code, @train4_station_name, @train4_ekitoho, @buss4_stop_name, @buss4_Jyousya, @buss4_Toho, @updated_at) ";

                sqlUpsert += "ON CONFLICT (property_id) DO UPDATE SET " +
                    "name = @name, property_kind = @propertyKind, thumbnail_filename = @thumbnailPath, " +
                    "location_prefecture_code = @locPrefId, location_prefecture = @locPrefecture, location_machiaza_id = @locMachiazaId, " +
                    "location_county = @locCounty, location_city = @locCity, location_ward = @locWard, location_oaza_cho = @locOazaCho, " +
                    "location_choume = @locChoume, location_edaban = @locEdaban, location_postal_code = @locPostalCode, location_full = @locLocationFull, " +
                    "location_latitude = @locationLatitude, location_longitude = @locationLongitude, " +
                    "train1_line_code = @train1_line_code, train1_line_name = @train1_line_name, train1_station_code = @train1_station_code, train1_station_name = @train1_station_name, train1_ekitoho = @train1_ekitoho, buss1_stop_name = @buss1_stop_name, buss1_Jyousya = @buss1_Jyousya, buss1_Toho = @buss1_Toho, " +
                    "train2_line_code = @train2_line_code, train2_line_name = @train2_line_name, train2_station_code = @train2_station_code, train2_station_name = @train2_station_name, train2_ekitoho = @train2_ekitoho, buss2_stop_name = @buss2_stop_name, buss2_Jyousya = @buss2_Jyousya, buss2_Toho = @buss2_Toho, " +
                    "train3_line_code = @train3_line_code, train3_line_name = @train3_line_name, train3_station_code = @train3_station_code, train3_station_name = @train3_station_name, train3_ekitoho = @train3_ekitoho, buss3_stop_name = @buss3_stop_name, buss3_Jyousya = @buss3_Jyousya, buss3_Toho = @buss3_Toho, " +
                    "train4_line_code = @train4_line_code, train4_line_name = @train4_line_name, train4_station_code = @train4_station_code, train4_station_name = @train4_station_name, train4_ekitoho = @train4_ekitoho, buss4_stop_name = @buss4_stop_name, buss4_Jyousya = @buss4_Jyousya, buss4_Toho = @buss4_Toho, " +
                    "updated_at = @updated_at";

                cmd.CommandText = sqlUpsert;
                
                cmd.Parameters.AddWithValue("@propertyId", building.Id);
                cmd.Parameters.AddWithValue("@name", building.Name);
                cmd.Parameters.AddWithValue("@propertyKind", building.PropertyContextType.ToString());
                cmd.Parameters.AddWithValue("@thumbnailPath", building.ThumbnailFilename);
                // Address/Location
                cmd.Parameters.AddWithValue("@locPrefId", building.Address.Prefecture?.MunicipalityCode ?? string.Empty);
                cmd.Parameters.AddWithValue("@locPrefecture", building.Address.Prefecture?.Name ?? string.Empty);
                cmd.Parameters.AddWithValue("@locMachiazaId", building.Address.MachiazaId ?? string.Empty);
                cmd.Parameters.AddWithValue("@locCounty", building.Address.CountyAndCity?.County ?? string.Empty);
                cmd.Parameters.AddWithValue("@locCity", building.Address.CountyAndCity?.City ?? string.Empty);
                cmd.Parameters.AddWithValue("@locWard", building.Address.WardAndOaza?.Ward ?? string.Empty);
                cmd.Parameters.AddWithValue("@locOazaCho", building.Address.WardAndOaza?.Oaza ?? string.Empty);
                cmd.Parameters.AddWithValue("@locChoume", building.Address.Choume?.Chou ?? string.Empty);
                cmd.Parameters.AddWithValue("@locEdaban", building.Address.Edaban ?? string.Empty);
                cmd.Parameters.AddWithValue("@locPostalCode", building.Address.PostalCode ?? string.Empty);
                cmd.Parameters.AddWithValue("@locLocationFull", building.Address.AddressFull ?? string.Empty);
                cmd.Parameters.AddWithValue("@locationLatitude", building.Address.LocationLatitude);
                cmd.Parameters.AddWithValue("@locationLongitude", building.Address.LocationLongitude);
                // Train1
                cmd.Parameters.AddWithValue("@train1_line_code", building.Train1.RailLine.LineCode);
                cmd.Parameters.AddWithValue("@train1_line_name", building.Train1.RailLine.LineName);
                cmd.Parameters.AddWithValue("@train1_station_code", building.Train1.RailStation.StationCode);
                cmd.Parameters.AddWithValue("@train1_station_name", building.Train1.RailStation.StationName);
                cmd.Parameters.AddWithValue("@train1_ekitoho", building.Train1.EkiToho); //int
                // Buss1
                cmd.Parameters.AddWithValue("@buss1_stop_name", building.BusStop1);
                cmd.Parameters.AddWithValue("@buss1_Jyousya", building.BusJyousya1);
                cmd.Parameters.AddWithValue("@buss1_Toho", building.BusStopToho1);

                // Train2
                cmd.Parameters.AddWithValue("@train2_line_code", building.Train2.RailLine.LineCode);
                cmd.Parameters.AddWithValue("@train2_line_name", building.Train2.RailLine.LineName);
                cmd.Parameters.AddWithValue("@train2_station_code", building.Train2.RailStation.StationCode);
                cmd.Parameters.AddWithValue("@train2_station_name", building.Train2.RailStation.StationName);
                cmd.Parameters.AddWithValue("@train2_ekitoho", building.Train2.EkiToho); //int
                // Buss2
                cmd.Parameters.AddWithValue("@buss2_stop_name", building.BusStop2);
                cmd.Parameters.AddWithValue("@buss2_Jyousya", building.BusJyousya2);
                cmd.Parameters.AddWithValue("@buss2_Toho", building.BusStopToho2);
                
                // Train3
                cmd.Parameters.AddWithValue("@train3_line_code", building.Train3.RailLine.LineCode);
                cmd.Parameters.AddWithValue("@train3_line_name", building.Train3.RailLine.LineName);
                cmd.Parameters.AddWithValue("@train3_station_code", building.Train3.RailStation.StationCode);
                cmd.Parameters.AddWithValue("@train3_station_name", building.Train3.RailStation.StationName);
                cmd.Parameters.AddWithValue("@train3_ekitoho", building.Train3.EkiToho); //int
                // Buss3
                cmd.Parameters.AddWithValue("@buss3_stop_name", building.BusStop3);
                cmd.Parameters.AddWithValue("@buss3_Jyousya", building.BusJyousya3);
                cmd.Parameters.AddWithValue("@buss3_Toho", building.BusStopToho3);

                // Train4
                cmd.Parameters.AddWithValue("@train4_line_code", building.Train4.RailLine.LineCode);
                cmd.Parameters.AddWithValue("@train4_line_name", building.Train4.RailLine.LineName);
                cmd.Parameters.AddWithValue("@train4_station_code", building.Train4.RailStation.StationCode);
                cmd.Parameters.AddWithValue("@train4_station_name", building.Train4.RailStation.StationName);
                cmd.Parameters.AddWithValue("@train4_ekitoho", building.Train4.EkiToho); //int
                // Buss4
                cmd.Parameters.AddWithValue("@buss4_stop_name", building.BusStop4);
                cmd.Parameters.AddWithValue("@buss4_Jyousya", building.BusJyousya4);
                cmd.Parameters.AddWithValue("@buss4_Toho", building.BusStopToho4);

                // TODO: more

                cmd.Parameters.AddWithValue("@updated_at", DateTimeOffset.UtcNow.ToString("s"));
                res.AffectedCount = cmd.ExecuteNonQuery();

                // rent_residentials
                cmd.Parameters.Clear();
                // Insert
                //cmd.CommandText = "INSERT INTO rent_residentials (property_id, property_type, is_unit_ownership, property_structure, floor_count_above_ground, floor_count_basement, total_unit_count, built_year_month, fudousan_id, fudousan_id_additional_code, remarks) " +
                //    "VALUES (@RentId, @BuildingKind, @IsUnitOwnership, @PropertyStructure, @FloorCountAboveGround, @FloorCountBasement, @TotalUnitCount, @BuiltYearMonth, @FudousanId, @FudousanIdAdditionalCode, @Remarks)";
                // Upsert
                sqlUpsert = "INSERT INTO rent_residentials (property_id, property_type, is_unit_ownership, property_structure, floor_count_above_ground, floor_count_basement, total_unit_count, built_year_month, fudousan_id, fudousan_id_additional_code, remarks, " +
                    "has_autolock, has_elevator, has_security_camera, has_parcel_locker, has_electric, " +
                    "property_electric_kind, electric_detail, has_gas, gas_type, gas_detail, " +
                    "tap_water_type, tap_water_detail, sewerage_type, sewerage_detail, ground_digital, " +
                    "bs_antenna, cs_antenna, catv, usen, internet, " +
                    "fiber, internet_free, tv_antenna_detail, internet_detail, has_parking, " +
                    "parking_contract_type, parking_availability, is_parking_free, parking_fee, parking_fee_tax_type, " +
                    "parking_spaces, parking_distance, parking_notes, has_bicycle_parking, bicycle_parking_availability, " +
                    "is_bicycle_parking_free, bicycle_parking_fee, bicycle_parking_fee_tax_type, bicycle_parking_notes, has_motorcycle_parking, " +
                    "motorcycle_parking_availability, is_motorcycle_parking_free, motorcycle_parking_fee, motorcycle_parking_fee_tax_type, motorcycle_parking_notes) ";
                sqlUpsert += "VALUES (@propertyId, @buildingType, @isUnitOwnership, @buildingStructure, @floorCountAboveGround, @floorCountBasement, @totalUnitCount, @builtYearMonth, @fudousanId, @fudousanIdAdditionalCode, @remarks, " +
                    "@facility_has_autolock, @facility_has_elevator, @facility_has_security_camera, @facility_has_parcel_locker, @facility_has_electric, " +
                    "@facility_property_electric_kind, @facility_electric_detail, @facility_has_gas, @facility_gas_type, @facility_gas_detail, " +
                    "@facility_tap_water_type, @facility_tap_water_detail, @facility_sewerage_type, @facility_sewerage_detail, @facility_ground_digital, " +
                    "@facility_bs_antenna, @facility_cs_antenna, @facility_catv, @facility_usen, @facility_internet, " +
                    "@facility_fiber, @facility_internet_free, @facility_tv_antenna_detail, @facility_internet_detail, @facility_has_parking, " +
                    "@facility_parking_contract_type, @facility_parking_availability, @facility_is_parking_free, @facility_parking_fee, @facility_parking_fee_tax_type, " +
                    "@facility_parking_spaces, @facility_parking_distance, @facility_parking_notes, @facility_has_bicycle_parking, @facility_bicycle_parking_availability, " +
                    "@facility_is_bicycle_parking_free, @facility_bicycle_parking_fee, @facility_bicycle_parking_fee_tax_type, @facility_bicycle_parking_notes, @facility_has_motorcycle_parking, " +
                    "@facility_motorcycle_parking_availability, @facility_is_motorcycle_parking_free, @facility_motorcycle_parking_fee, @facility_motorcycle_parking_fee_tax_type, @facility_motorcycle_parking_notes) ";
                sqlUpsert += "ON CONFLICT(property_id) ";
                sqlUpsert += "DO UPDATE SET property_type = @buildingType, is_unit_ownership = @isUnitOwnership, property_structure = @buildingStructure, floor_count_above_ground = @floorCountAboveGround, floor_count_basement = @floorCountBasement, total_unit_count = @totalUnitCount, built_year_month = @builtYearMonth, fudousan_id = @fudousanId, fudousan_id_additional_code = @fudousanIdAdditionalCode, remarks = @remarks, " +
                    "has_autolock = excluded.has_autolock, has_elevator = excluded.has_elevator, has_security_camera = excluded.has_security_camera, has_parcel_locker = excluded.has_parcel_locker, has_electric = excluded.has_electric, " +
                    "property_electric_kind = excluded.property_electric_kind, electric_detail = excluded.electric_detail, has_gas = excluded.has_gas, gas_type = excluded.gas_type, gas_detail = excluded.gas_detail, " +
                    "tap_water_type = excluded.tap_water_type, tap_water_detail = excluded.tap_water_detail, sewerage_type = excluded.sewerage_type, sewerage_detail = excluded.sewerage_detail, ground_digital = excluded.ground_digital, " +
                    "bs_antenna = excluded.bs_antenna, cs_antenna = excluded.cs_antenna, catv = excluded.catv, usen = excluded.usen, internet = excluded.internet, " +
                    "fiber = excluded.fiber, internet_free = excluded.internet_free, tv_antenna_detail = excluded.tv_antenna_detail, internet_detail = excluded.internet_detail, has_parking = excluded.has_parking, " +
                    "parking_contract_type = excluded.parking_contract_type, parking_availability = excluded.parking_availability, is_parking_free = excluded.is_parking_free, parking_fee = excluded.parking_fee, parking_fee_tax_type = excluded.parking_fee_tax_type, " +
                    "parking_spaces = excluded.parking_spaces, parking_distance = excluded.parking_distance, parking_notes = excluded.parking_notes, has_bicycle_parking = excluded.has_bicycle_parking, bicycle_parking_availability = excluded.bicycle_parking_availability, " +
                    "is_bicycle_parking_free = excluded.is_bicycle_parking_free, bicycle_parking_fee = excluded.bicycle_parking_fee, bicycle_parking_fee_tax_type = excluded.bicycle_parking_fee_tax_type, bicycle_parking_notes = excluded.bicycle_parking_notes, has_motorcycle_parking = excluded.has_motorcycle_parking, " +
                    "motorcycle_parking_availability = excluded.motorcycle_parking_availability, is_motorcycle_parking_free = excluded.is_motorcycle_parking_free, motorcycle_parking_fee = excluded.motorcycle_parking_fee, motorcycle_parking_fee_tax_type = excluded.motorcycle_parking_fee_tax_type, motorcycle_parking_notes = excluded.motorcycle_parking_notes";

                cmd.CommandText = sqlUpsert;

                cmd.Parameters.AddWithValue("@propertyId", building.Id);
                cmd.Parameters.AddWithValue("@buildingType", building.PropertyKind.Key.ToString());
                cmd.Parameters.AddWithValue("@isUnitOwnership", building.IsUnitOwnership ? 1 : 0); // bool to int
                cmd.Parameters.AddWithValue("@buildingStructure", building.PropertyStructure.Key.ToString());
                cmd.Parameters.AddWithValue("@floorCountAboveGround", building.FloorCountAboveGround);// int
                cmd.Parameters.AddWithValue("@floorCountBasement", building.FloorCountBasement);// int
                cmd.Parameters.AddWithValue("@totalUnitCount", building.TotalUnitCount);// int
                cmd.Parameters.AddWithValue("@builtYearMonth", building.BuiltYearAndMonth.ToString("s"));
                cmd.Parameters.AddWithValue("@fudousanId", building.FudousanId);
                cmd.Parameters.AddWithValue("@fudousanIdAdditionalCode", building.FudousanIdAdditionalCode);
                cmd.Parameters.AddWithValue("@remarks", building.Remarks);
                cmd.Parameters.AddWithValue("@facility_has_autolock", building.HasAutolock ? 1 : 0);
                cmd.Parameters.AddWithValue("@facility_has_elevator", building.HasElevator ? 1 : 0);
                cmd.Parameters.AddWithValue("@facility_has_security_camera", building.HasSecurityCamera ? 1 : 0);
                cmd.Parameters.AddWithValue("@facility_has_parcel_locker", building.HasParcelLocker ? 1 : 0);
                cmd.Parameters.AddWithValue("@facility_has_electric", building.HasElectric ? 1 : 0);
                cmd.Parameters.AddWithValue("@facility_property_electric_kind", FormatFacilityEnum(building.PropertyElectricKind));
                cmd.Parameters.AddWithValue("@facility_electric_detail", building.ElectricDetail ?? string.Empty);
                cmd.Parameters.AddWithValue("@facility_has_gas", building.HasGas ? 1 : 0);
                cmd.Parameters.AddWithValue("@facility_gas_type", FormatFacilityEnum(building.GasType));
                cmd.Parameters.AddWithValue("@facility_gas_detail", building.GasDetail ?? string.Empty);
                cmd.Parameters.AddWithValue("@facility_tap_water_type", FormatFacilityEnum(building.TapWaterType));
                cmd.Parameters.AddWithValue("@facility_tap_water_detail", building.TapWaterDetail ?? string.Empty);
                cmd.Parameters.AddWithValue("@facility_sewerage_type", FormatFacilityEnum(building.SewerageType));
                cmd.Parameters.AddWithValue("@facility_sewerage_detail", building.SewerageDetail ?? string.Empty);
                cmd.Parameters.AddWithValue("@facility_ground_digital", building.GroundDigital ? 1 : 0);
                cmd.Parameters.AddWithValue("@facility_bs_antenna", building.BsAntenna ? 1 : 0);
                cmd.Parameters.AddWithValue("@facility_cs_antenna", building.CsAntenna ? 1 : 0);
                cmd.Parameters.AddWithValue("@facility_catv", building.Catv ? 1 : 0);
                cmd.Parameters.AddWithValue("@facility_usen", building.Usen ? 1 : 0);
                cmd.Parameters.AddWithValue("@facility_internet", building.Internet ? 1 : 0);
                cmd.Parameters.AddWithValue("@facility_fiber", building.Fiber ? 1 : 0);
                cmd.Parameters.AddWithValue("@facility_internet_free", building.InternetFree ? 1 : 0);
                cmd.Parameters.AddWithValue("@facility_tv_antenna_detail", building.TvAntennaDetail ?? string.Empty);
                cmd.Parameters.AddWithValue("@facility_internet_detail", building.InternetDetail ?? string.Empty);
                cmd.Parameters.AddWithValue("@facility_has_parking", building.HasParking ? 1 : 0);
                cmd.Parameters.AddWithValue("@facility_parking_contract_type", FormatFacilityEnum(building.ParkingContractType));
                cmd.Parameters.AddWithValue("@facility_parking_availability", FormatFacilityEnum(building.ParkingAvailability));
                cmd.Parameters.AddWithValue("@facility_is_parking_free", building.IsParkingFree ? 1 : 0);
                cmd.Parameters.AddWithValue("@facility_parking_fee", building.ParkingFee ?? string.Empty);
                cmd.Parameters.AddWithValue("@facility_parking_fee_tax_type", FormatFacilityEnum(building.ParkingFeeTaxType));
                cmd.Parameters.AddWithValue("@facility_parking_spaces", building.ParkingSpaces ?? string.Empty);
                cmd.Parameters.AddWithValue("@facility_parking_distance", building.ParkingDistance ?? string.Empty);
                cmd.Parameters.AddWithValue("@facility_parking_notes", building.ParkingNotes ?? string.Empty);
                cmd.Parameters.AddWithValue("@facility_has_bicycle_parking", building.HasBicycleParking ? 1 : 0);
                cmd.Parameters.AddWithValue("@facility_bicycle_parking_availability", FormatFacilityEnum(building.BicycleParkingAvailability));
                cmd.Parameters.AddWithValue("@facility_is_bicycle_parking_free", building.IsBicycleParkingFree ? 1 : 0);
                cmd.Parameters.AddWithValue("@facility_bicycle_parking_fee", building.BicycleParkingFee ?? string.Empty);
                cmd.Parameters.AddWithValue("@facility_bicycle_parking_fee_tax_type", FormatFacilityEnum(building.BicycleParkingFeeTaxType));
                cmd.Parameters.AddWithValue("@facility_bicycle_parking_notes", building.BicycleParkingNotes ?? string.Empty);
                cmd.Parameters.AddWithValue("@facility_has_motorcycle_parking", building.HasMotorcycleParking ? 1 : 0);
                cmd.Parameters.AddWithValue("@facility_motorcycle_parking_availability", FormatFacilityEnum(building.MotorcycleParkingAvailability));
                cmd.Parameters.AddWithValue("@facility_is_motorcycle_parking_free", building.IsMotorcycleParkingFree ? 1 : 0);
                cmd.Parameters.AddWithValue("@facility_motorcycle_parking_fee", building.MotorcycleParkingFee ?? string.Empty);
                cmd.Parameters.AddWithValue("@facility_motorcycle_parking_fee_tax_type", FormatFacilityEnum(building.MotorcycleParkingFeeTaxType));
                cmd.Parameters.AddWithValue("@facility_motorcycle_parking_notes", building.MotorcycleParkingNotes ?? string.Empty);
                // TODO: more

                cmd.ExecuteNonQuery();

                cmd.Parameters.Clear();

                // 写真（建物）rent_residential_pictures
                if (building.Pictures.Count > 0)
                {
                    foreach (var pic in building.Pictures)
                    {
                        //var sqlInsertIntoRentLivingPicture = "INSERT INTO rent_residential_pictures (picture_id, property_id, filename, type, description, is_main) " +
                        //    "VALUES (@PicId, @RentId, @Path, @Type, @Desc, @Main)";

                        var sqlUpsertPicture = "INSERT INTO rent_residential_pictures (picture_id, property_id, filename, type, description, is_main) ";
                        sqlUpsertPicture += "VALUES (@pictureId, @propertyId, @filePath, @type, @description, @isMain) ";
                        sqlUpsertPicture += "ON CONFLICT (picture_id) ";
                        sqlUpsertPicture += "DO UPDATE SET filename = @filePath, type = @type, description = @description, is_main = @isMain";

                        cmd.CommandText = sqlUpsertPicture;

                        // ループなので、前のパラメーターをクリアする。
                        cmd.Parameters.Clear();

                        cmd.Parameters.AddWithValue("@pictureId", pic.Id);
                        //cmd.Parameters.AddWithValue("@RentResidentialId", building.Id + "_1");
                        cmd.Parameters.AddWithValue("@propertyId", building.Id);
                        cmd.Parameters.AddWithValue("@filePath", pic.ImageFilename);
                        cmd.Parameters.AddWithValue("@type", pic.PictureType.Key.ToString());
                        cmd.Parameters.AddWithValue("@description", pic.Description);

                        //Debug.WriteLine($"Inserting picture: {pic.ImageLocation}, {pic.Id}, isMain: {pic.IsMain} @DataAccess::InsertRentResidential");

                        var paramIsMain = new SqliteParameter("@isMain", System.Data.DbType.Int32)
                        {
                            Value = pic.IsMain ? 1 : 0
                        };
                        cmd.Parameters.Add(paramIsMain);

                        var r = cmd.ExecuteNonQuery();
                        if (r > 0)
                        {
                            //pic.IsNew = false;
                            pic.SetStatus(EntityStatus.Saved);
                            pic.SetIsModified(false);
                        }
                    }
                }

                cmd.Parameters.Clear();

                // 物件写真の削除リストを処理
                if (building.PicturesToBeDeleted.Count > 0)
                {
                    foreach (var delp in building.PicturesToBeDeleted)
                    {
                        // 削除
                        var sqlDeleteRentLivingPicture = string.Format(CultureInfo.InvariantCulture, "DELETE FROM rent_residential_pictures WHERE picture_id = '{0}'", delp.Id);

                        cmd.CommandText = sqlDeleteRentLivingPicture;
                        var DelRentLivingPicResult = cmd.ExecuteNonQuery();
                        if (DelRentLivingPicResult > 0)
                        {
                            // TODO:
                            Debug.WriteLine("Picture deleted");
                        }
                    }

                    // let's not. Needs this to clean up the file.
                    //building.BuildingPicturesToBeDeleted.Clear();
                }

                cmd.Parameters.Clear();

                // PDF（建物）rent_residential_pdfs
                if (building.Pdfs.Count > 0)
                {
                    foreach (var pic in building.Pdfs)
                    {
                        //var sqlInsertIntoRentLivingPicture = "INSERT INTO rent_residential_pdfs (pdf_id, property_id, filename, thumbnail_filename, type, description, is_main) " +
                        //    "VALUES (@PdfId, @RentId, @Path, @Thumb, @Type, @Desc, @Main)";
                        var sqlUpsertPdf = "INSERT INTO rent_residential_pdfs (pdf_id, property_id, filename, thumbnail_filename, type, description, is_main) ";
                        sqlUpsertPdf += "VALUES (@pdfId, @propertyId, @filePath, @thumbnailPath, @type, @description, @isMain) ";
                        sqlUpsertPdf += "ON CONFLICT (pdf_id) ";
                        sqlUpsertPdf += "DO UPDATE SET filename = @filePath, thumbnail_filename = @thumbnailPath, type = @type, description = @description, is_main = @isMain";

                        cmd.CommandText = sqlUpsertPdf;

                        // ループなので、前のパラメーターをクリアする。
                        cmd.Parameters.Clear();

                        cmd.Parameters.AddWithValue("@pdfId", pic.Id);
                        cmd.Parameters.AddWithValue("@propertyId", building.Id);
                        cmd.Parameters.AddWithValue("@filePath", pic.PdfFilename);
                        cmd.Parameters.AddWithValue("@thumbnailPath", pic.ThumbnailFilename);
                        cmd.Parameters.AddWithValue("@type", pic.PdfType.Key.ToString());
                        cmd.Parameters.AddWithValue("@description", pic.Description);

                        //Debug.WriteLine($"Inserting picture: {pic.ImageLocation}, {pic.Id}, isMain: {pic.IsMain} @DataAccess::InsertRentResidential");

                        var paramIsMain = new SqliteParameter("@isMain", System.Data.DbType.Int32)
                        {
                            Value = pic.IsMain ? 1 : 0
                        };
                        cmd.Parameters.Add(paramIsMain);

                        var r = cmd.ExecuteNonQuery();
                        if (r > 0)
                        {
                            //pic.IsNew = false;
                            pic.SetStatus(EntityStatus.Saved);
                            pic.SetIsModified(false);
                        }
                    }
                }

                cmd.Parameters.Clear();

                // PDF（建物）の削除リストを処理
                if (building.PdfsToBeDeleted.Count > 0)
                {
                    foreach (var delp in building.PdfsToBeDeleted)
                    {
                        // 削除
                        var sqlDeleteRentLivingPdf = string.Format(CultureInfo.InvariantCulture, "DELETE FROM rent_residential_pdfs WHERE pdf_id = '{0}'", delp.Id);

                        cmd.CommandText = sqlDeleteRentLivingPdf;
                        var DelRentLivingPdfResult = cmd.ExecuteNonQuery();
                        if (DelRentLivingPdfResult > 0)
                        {
                            // TODO:
                            Debug.WriteLine("Pdf deleted");
                        }
                    }

                    // let's not. Needs this to clean up the file.
                    //building.BuildingPdfsToBeDeleted.Clear();
                }

                cmd.Parameters.Clear();

                // 貸主（建物）rent_lessors_properties_listings
                if (building.Lessors.Count > 0)
                {
                    foreach (var psn in building.Lessors)
                    {
                        //var sqlInsertInto = "INSERT INTO rent_lessors_properties_listings (lessor_id, property_id, property_kind, listing_id) " +
                        //                "VALUES (@lessor_id, @property_id, @property_kind, @listing_id)";
                        var sqlUpsertLessor = "INSERT INTO rent_lessors_properties_listings (lessor_id, property_id, property_kind, listing_id) ";
                        sqlUpsertLessor += "VALUES (@lessor_id, @property_id, @property_kind, @listing_id) ";
                        sqlUpsertLessor += "ON CONFLICT (lessor_id, property_id, listing_id) ";
                        sqlUpsertLessor += "DO NOTHING";

                        cmd.CommandText = sqlUpsertLessor;

                        // ループなので、前のパラメーターをクリアする。
                        cmd.Parameters.Clear();

                        cmd.Parameters.AddWithValue("@lessor_id", psn.Id);
                        cmd.Parameters.AddWithValue("@property_id", building.Id);
                        cmd.Parameters.AddWithValue("@property_kind", building.PropertyContextType.ToString());
                        cmd.Parameters.AddWithValue("@listing_id", string.Empty);// since this is building.

                        cmd.ExecuteNonQuery();
                    }
                }

                cmd.Parameters.Clear();

                // 貸主（建物）の削除リストを処理
                if (building.LessorsToBeDeleted.Count > 0)
                {
                    foreach (var psn in building.LessorsToBeDeleted)
                    {
                        // 削除
                        var sqlDelete = ($"DELETE FROM rent_lessors_properties_listings WHERE lessor_id = '{psn.Id}' AND property_id = '{building.Id}' AND listing_id = '{string.Empty}'");

                        cmd.CommandText = sqlDelete;
                        var sqlResult = cmd.ExecuteNonQuery();
                        if (sqlResult > 0)
                        {
                            // TODO:
                            Debug.WriteLine("Lessor deleted");
                        }
                    }
                    // TODO: should I?
                    building.LessorsToBeDeleted.Clear();
                }

                cmd.Parameters.Clear();

                // 宅建業者（建物）brokers_properties_listings
                if (building.Brokers.Count > 0)
                {
                    foreach (var psn in building.Brokers)
                    {
                        var sqlUpsertBroker = "INSERT INTO brokers_properties_listings (broker_id, property_id, property_kind, listing_id) ";
                        sqlUpsertBroker += "VALUES (@broker_id, @property_id, @property_kind, @listing_id) ";
                        sqlUpsertBroker += "ON CONFLICT (broker_id, property_id, listing_id) ";
                        sqlUpsertBroker += "DO NOTHING";

                        cmd.CommandText = sqlUpsertBroker;

                        // ループなので、前のパラメーターをクリアする。
                        cmd.Parameters.Clear();

                        cmd.Parameters.AddWithValue("@broker_id", psn.Id);
                        cmd.Parameters.AddWithValue("@property_id", building.Id);
                        cmd.Parameters.AddWithValue("@property_kind", building.PropertyContextType.ToString());
                        cmd.Parameters.AddWithValue("@listing_id", string.Empty);// since this is building.

                        cmd.ExecuteNonQuery();
                    }
                }

                cmd.Parameters.Clear();

                // 宅建業者（建物）の削除リストを処理
                if (building.BrokersToBeDeleted.Count > 0)
                {
                    foreach (var psn in building.BrokersToBeDeleted)
                    {
                        // 削除
                        var sqlDelete = ($"DELETE FROM brokers_properties_listings WHERE broker_id = '{psn.Id}' AND property_id = '{building.Id}' AND listing_id = '{string.Empty}'");

                        cmd.CommandText = sqlDelete;
                        var sqlResult = cmd.ExecuteNonQuery();
                        if (sqlResult > 0)
                        {
                            // TODO:
                            Debug.WriteLine("Broker deleted");
                        }
                    }
                    // TODO: should I?
                    building.BrokersToBeDeleted.Clear();
                }

                cmd.Parameters.Clear();

                // 部屋 rent_residential_listings
                if (building.Rooms.Count > 0)
                {
                    foreach (var unit in building.Rooms)
                    {
                        // Reuse the code for upsert listing.
                        _ = DoUpsertRentResidentialListing(cmd, building.Id, unit);
                    }
                }

                cmd.Parameters.Clear();

                // 部屋の削除リストを処理
                if (building.RoomsToBeDeleted.Count > 0)
                {
                    foreach (var delr in building.RoomsToBeDeleted)
                    {
                        // 削除
                        var sqlDeleteRentLivingRoom = string.Format(CultureInfo.InvariantCulture, "DELETE FROM rent_residential_listings WHERE listing_id = '{0}'", delr.Id);

                        cmd.CommandText = sqlDeleteRentLivingRoom;
                        var delRentLivingRoomResult = cmd.ExecuteNonQuery();
                        if (delRentLivingRoomResult > 0)
                        {
                            // TODO:
                            Debug.WriteLine("Room deleted @UpdateRentResidential in DataAccessService");
                        }
                    }
                    // Let's not
                    //building.RoomsToBeDeleted.Clear();
                }

                // commit
                cmd.Transaction.Commit();
            }
            catch (Exception ex)
            {
                cmd.Transaction.Rollback();

                SetDatabaseError(res, ex, "cmd.ExecuteNonQuery(), cmd.Transaction.Commit", "Failed to update database tables. Transaction.Rollback()", nameof(UpsertRentResidential));
                return res;
            }
        }
        catch (Exception ex)
        {
            SetDatabaseError(res, ex, "connection.Open()", "Failed to connect to a SQLite database file", nameof(UpsertRentResidential));
        }
        finally
        {
            _readerWriterLock.ExitWriteLock();
        }

        //Debug.WriteLine(string.Format("{0} Entries Inserted to DB", res.AffectedCount.ToString()));

        building.SetIsModified(false);
        building.SetStatus(EntityStatus.Saved);

        return res;
    }

    public Models.Rent.Residentials.PropertyResultWrapper SelectRentResidentialById(string id)
    {
        var res = new Models.Rent.Residentials.PropertyResultWrapper();

        var property = new Models.Rent.Residentials.Property(id, EntityStatus.Saved);

        if (string.IsNullOrEmpty(id))
        {
            res.IsError = true;
            // TODO:
            return res;
        }

        _readerWriterLock.EnterReadLock();
        try
        {
            using var connection = new SqliteConnection(_connectionStringBuilder.ConnectionString);
            connection.Open();

            using var cmd = connection.CreateCommand();
            cmd.CommandText = "SELECT properties.name as propertyName, " +
                "properties.property_kind as propertyKind, " +
                "properties.location_prefecture_code as locPrefId, " +
                "properties.location_prefecture as locPrefecture, " +
                "properties.location_machiaza_id as locMachiazaId, " +
                "properties.location_county as locCounty, " +
                "properties.location_city as locCity, " +
                "properties.location_ward as locWard, " +
                "properties.location_oaza_cho as locOazaCho, " +
                "properties.location_choume as locChoume, " +
                "properties.location_edaban as locEdaban, " +
                "properties.location_postal_code as locPostalCode, " +
                "properties.location_full as locLocationFull, " +
                "properties.location_latitude as locationLatitude, " +
                "properties.location_longitude as locationLongitude, " +
                "properties.train1_line_code as train1LineCode, " +
                "properties.train1_line_name as train1LineName, " +
                "properties.train1_station_code as train1StationCode, " +
                "properties.train1_station_name as train1StationName, " +
                "properties.train1_ekitoho as train1EkiToho, " +
                "properties.buss1_stop_name as buss1StopName, " +
                "properties.buss1_Jyousya as buss1Jyousya, " +
                "properties.buss1_Toho as buss1Toho, " +
                "properties.train2_line_code as train2LineCode, " +
                "properties.train2_line_name as train2LineName, " +
                "properties.train2_station_code as train2StationCode, " +
                "properties.train2_station_name as train2StationName, " +
                "properties.train2_ekitoho as train2EkiToho, " +
                "properties.buss2_stop_name as buss2StopName, " +
                "properties.buss2_Jyousya as buss2Jyousya, " +
                "properties.buss2_Toho as buss2Toho, " +
                "properties.train3_line_code as train3LineCode, " +
                "properties.train3_line_name as train3LineName, " +
                "properties.train3_station_code as train3StationCode, " +
                "properties.train3_station_name as train3StationName, " +
                "properties.train3_ekitoho as train3EkiToho, " +
                "properties.buss3_stop_name as buss3StopName, " +
                "properties.buss3_Jyousya as buss3Jyousya, " +
                "properties.buss3_Toho as buss3Toho, " +
                "properties.train4_line_code as train4LineCode, " +
                "properties.train4_line_name as train4LineName, " +
                "properties.train4_station_code as train4StationCode, " +
                "properties.train4_station_name as train4StationName, " +
                "properties.train4_ekitoho as train4EkiToho, " +
                "properties.buss4_stop_name as buss4StopName, " +
                "properties.buss4_Jyousya as buss4Jyousya, " +
                "properties.buss4_Toho as buss4Toho, " +
                "properties.updated_at as UpdatedAt, " +

                "rent_residentials.property_type as resiBuildingKind, " +
                "rent_residentials.is_unit_ownership as resiUnitOwnership, " +
                "rent_residentials.property_structure as resiBuildingStructure, " +
                "rent_residentials.floor_count_above_ground as resiAboveGroundFloorCount, " +
                "rent_residentials.floor_count_basement as resiBasementFloorCount, " +
                "rent_residentials.total_unit_count as resiTotalUnitCount, " +
                "rent_residentials.built_year_month as resiBuiltYearMonth, " +
                "rent_residentials.fudousan_id as resiFudousanId, " +
                "rent_residentials.fudousan_id_additional_code as resiFudousanIdAdditionalCode, " +
                "rent_residentials.remarks as resiRemarks, " +
                "rent_residentials.has_autolock as has_autolock, " +
                "rent_residentials.has_elevator as has_elevator, " +
                "rent_residentials.has_security_camera as has_security_camera, " +
                "rent_residentials.has_parcel_locker as has_parcel_locker, " +
                "rent_residentials.has_electric as has_electric, " +
                "rent_residentials.property_electric_kind as property_electric_kind, " +
                "rent_residentials.electric_detail as electric_detail, " +
                "rent_residentials.has_gas as has_gas, " +
                "rent_residentials.gas_type as gas_type, " +
                "rent_residentials.gas_detail as gas_detail, " +
                "rent_residentials.tap_water_type as tap_water_type, " +
                "rent_residentials.tap_water_detail as tap_water_detail, " +
                "rent_residentials.sewerage_type as sewerage_type, " +
                "rent_residentials.sewerage_detail as sewerage_detail, " +
                "rent_residentials.ground_digital as ground_digital, " +
                "rent_residentials.bs_antenna as bs_antenna, " +
                "rent_residentials.cs_antenna as cs_antenna, " +
                "rent_residentials.catv as catv, " +
                "rent_residentials.usen as usen, " +
                "rent_residentials.internet as internet, " +
                "rent_residentials.fiber as fiber, " +
                "rent_residentials.internet_free as internet_free, " +
                "rent_residentials.tv_antenna_detail as tv_antenna_detail, " +
                "rent_residentials.internet_detail as internet_detail, " +
                "rent_residentials.has_parking as has_parking, " +
                "rent_residentials.parking_contract_type as parking_contract_type, " +
                "rent_residentials.parking_availability as parking_availability, " +
                "rent_residentials.is_parking_free as is_parking_free, " +
                "rent_residentials.parking_fee as parking_fee, " +
                "rent_residentials.parking_fee_tax_type as parking_fee_tax_type, " +
                "rent_residentials.parking_spaces as parking_spaces, " +
                "rent_residentials.parking_distance as parking_distance, " +
                "rent_residentials.parking_notes as parking_notes, " +
                "rent_residentials.has_bicycle_parking as has_bicycle_parking, " +
                "rent_residentials.bicycle_parking_availability as bicycle_parking_availability, " +
                "rent_residentials.is_bicycle_parking_free as is_bicycle_parking_free, " +
                "rent_residentials.bicycle_parking_fee as bicycle_parking_fee, " +
                "rent_residentials.bicycle_parking_fee_tax_type as bicycle_parking_fee_tax_type, " +
                "rent_residentials.bicycle_parking_notes as bicycle_parking_notes, " +
                "rent_residentials.has_motorcycle_parking as has_motorcycle_parking, " +
                "rent_residentials.motorcycle_parking_availability as motorcycle_parking_availability, " +
                "rent_residentials.is_motorcycle_parking_free as is_motorcycle_parking_free, " +
                "rent_residentials.motorcycle_parking_fee as motorcycle_parking_fee, " +
                "rent_residentials.motorcycle_parking_fee_tax_type as motorcycle_parking_fee_tax_type, " +
                "rent_residentials.motorcycle_parking_notes as motorcycle_parking_notes, " +
                // TODO: more fields to be added here.

                //"rent_residentials.updated_at as UpdatedAt, " +
                "properties.property_id as propertyId " +
                "FROM rent_residentials INNER JOIN properties USING (property_id) WHERE properties.property_id = @propertyId";
            cmd.Parameters.Clear();
            cmd.Parameters.AddWithValue("@propertyId", id);

            bool isFound = false;

            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    var pId = Convert.ToString(reader["propertyId"], CultureInfo.InvariantCulture);

                    if (!id.Equals(pId, StringComparison.Ordinal))
                    {
                        Debug.WriteLine("DataAccess::SelectRentResidentialsById: propertyId is null or empty for a rent residential entry.");
                        continue;
                    }

                    isFound = true;

                    property.SetName(Convert.ToString(reader["propertyName"], CultureInfo.InvariantCulture) ?? "");
                    // Address/Location
                    property.Address.SetMachiazaId(Convert.ToString(reader["locMachiazaId"], CultureInfo.InvariantCulture) ?? "");
                    property.Address.SetPrefectureByMunicipalityCode(Convert.ToString(reader["locPrefId"], CultureInfo.InvariantCulture) ?? "",
                        Convert.ToString(reader["locPrefecture"], CultureInfo.InvariantCulture) ?? "");
                    property.Address.SetCountyAndCity(property.Address.MachiazaId,
                        Convert.ToString(reader["locCounty"], CultureInfo.InvariantCulture) ?? "",
                        Convert.ToString(reader["locCity"], CultureInfo.InvariantCulture) ?? "");

                    property.Address.SetWardAndOaza(property.Address.MachiazaId,
                        Convert.ToString(reader["locWard"], CultureInfo.InvariantCulture) ?? "",
                        Convert.ToString(reader["locOazaCho"], CultureInfo.InvariantCulture) ?? "");

                    property.Address.SetChoume(property.Address.MachiazaId,
                        Convert.ToString(reader["locChoume"], CultureInfo.InvariantCulture) ?? "");

                    property.Address.SetEdaban(Convert.ToString(reader["locEdaban"], CultureInfo.InvariantCulture) ?? "");
                    property.Address.SetPostalCode(Convert.ToString(reader["locPostalCode"], CultureInfo.InvariantCulture) ?? string.Empty);
                    //property.SetLocLocationFull(Convert.ToString(reader["locLocationFull"], CultureInfo.InvariantCulture) ?? "");
                    property.Address.SetLocationLatitude(Convert.ToString(reader["locationLatitude"], CultureInfo.InvariantCulture) ?? string.Empty);
                    property.Address.SetLocationLongitude(Convert.ToString(reader["locationLongitude"], CultureInfo.InvariantCulture) ?? string.Empty);

                    //property.Train1.RailLine.SetLineCode(Convert.ToString(reader["train1LineCode"], CultureInfo.InvariantCulture) ?? string.Empty);
                    //property.Train1.RailLine.SetLineName(Convert.ToString(reader["train1LineName"], CultureInfo.InvariantCulture) ?? string.Empty);
                    //property.Train1.RailStation.SetLineCode(property.Train1.RailLine.LineCode);
                    //property.Train1.RailStation.SetStationCode(Convert.ToString(reader["train1StationCode"], CultureInfo.InvariantCulture) ?? string.Empty);
                    //property.Train1.RailStation.SetStationName(Convert.ToString(reader["train1StationName"], CultureInfo.InvariantCulture) ?? string.Empty);
                    //property.Train1.SetEkiToho(Convert.ToInt32(reader["train1EkiToho"], CultureInfo.InvariantCulture));

                    property.Train1.SetRailLine(Convert.ToString(reader["train1LineCode"], CultureInfo.InvariantCulture) ?? "",Convert.ToString(reader["train1LineName"], CultureInfo.InvariantCulture) ?? "");
                    property.Train1.SetRailStation(property.Train1.RailLine.LineCode, Convert.ToString(reader["train1StationCode"], CultureInfo.InvariantCulture) ?? "", Convert.ToString(reader["train1StationName"], CultureInfo.InvariantCulture) ?? "");
                    property.SetBusStop1(Convert.ToString(reader["buss1StopName"], CultureInfo.InvariantCulture) ?? "");
                    property.SetBusJyousya1(Convert.ToString(reader["buss1Jyousya"], CultureInfo.InvariantCulture) ?? "");
                    property.SetBusStopToho1(Convert.ToString(reader["buss1Toho"], CultureInfo.InvariantCulture) ?? "");

                    property.Train2.SetRailLine(Convert.ToString(reader["train2LineCode"], CultureInfo.InvariantCulture) ?? "", Convert.ToString(reader["train2LineName"], CultureInfo.InvariantCulture) ?? "");
                    property.Train2.SetRailStation(property.Train2.RailLine.LineCode, Convert.ToString(reader["train2StationCode"], CultureInfo.InvariantCulture) ?? "", Convert.ToString(reader["train2StationName"], CultureInfo.InvariantCulture) ?? "");
                    property.SetBusStop2(Convert.ToString(reader["buss2StopName"], CultureInfo.InvariantCulture) ?? "");
                    property.SetBusJyousya2(Convert.ToString(reader["buss2Jyousya"], CultureInfo.InvariantCulture) ?? "");
                    property.SetBusStopToho2(Convert.ToString(reader["buss2Toho"], CultureInfo.InvariantCulture) ?? "");
                    property.Train2.SetEkiToho(Convert.ToInt32(reader["train2EkiToho"], CultureInfo.InvariantCulture));

                    property.Train3.SetRailLine(Convert.ToString(reader["train3LineCode"], CultureInfo.InvariantCulture) ?? "", Convert.ToString(reader["train3LineName"], CultureInfo.InvariantCulture) ?? "");
                    property.Train3.SetRailStation(property.Train3.RailLine.LineCode, Convert.ToString(reader["train3StationCode"], CultureInfo.InvariantCulture) ?? "", Convert.ToString(reader["train3StationName"], CultureInfo.InvariantCulture) ?? "");
                    property.SetBusStop3(Convert.ToString(reader["buss3StopName"], CultureInfo.InvariantCulture) ?? "");
                    property.SetBusJyousya3(Convert.ToString(reader["buss3Jyousya"], CultureInfo.InvariantCulture) ?? "");
                    property.SetBusStopToho3(Convert.ToString(reader["buss3Toho"], CultureInfo.InvariantCulture) ?? "");
                    property.Train3.SetEkiToho(Convert.ToInt32(reader["train3EkiToho"], CultureInfo.InvariantCulture));

                    property.Train4.SetRailLine(Convert.ToString(reader["train4LineCode"], CultureInfo.InvariantCulture) ?? "", Convert.ToString(reader["train4LineName"], CultureInfo.InvariantCulture) ?? "");
                    property.Train4.SetRailStation(property.Train4.RailLine.LineCode, Convert.ToString(reader["train4StationCode"], CultureInfo.InvariantCulture) ?? "", Convert.ToString(reader["train4StationName"], CultureInfo.InvariantCulture) ?? "");
                    property.SetBusStop4(Convert.ToString(reader["buss4StopName"], CultureInfo.InvariantCulture) ?? "");
                    property.SetBusJyousya4(Convert.ToString(reader["buss4Jyousya"], CultureInfo.InvariantCulture) ?? "");
                    property.SetBusStopToho4(Convert.ToString(reader["buss4Toho"], CultureInfo.InvariantCulture) ?? "");
                    property.Train4.SetEkiToho(Convert.ToInt32(reader["train4EkiToho"], CultureInfo.InvariantCulture));

                    // TODO: more.


                    property.SetPropertyTypeFromString(Convert.ToString(reader["resiBuildingKind"], CultureInfo.InvariantCulture) ?? "");
                    property.SetIsUnitOwnership(Convert.ToInt32(reader["resiUnitOwnership"], CultureInfo.InvariantCulture) != 0); // int to bool
                    property.SetStructureTypeFromString(Convert.ToString(reader["resiBuildingStructure"], CultureInfo.InvariantCulture) ?? "");
                    property.SetFloorCountAboveGround(Convert.ToInt32(reader["resiAboveGroundFloorCount"], CultureInfo.InvariantCulture));
                    property.SetFloorCountBasement(Convert.ToInt32(reader["resiBasementFloorCount"], CultureInfo.InvariantCulture));
                    property.SetTotalUnitCount(Convert.ToInt32(reader["resiTotalUnitCount"], CultureInfo.InvariantCulture));
                    property.SetBuiltYearAndMonthFromString(Convert.ToString(reader["resiBuiltYearMonth"], CultureInfo.InvariantCulture) ?? "");
                    property.SetFudousanId(Convert.ToString(reader["resiFudousanId"], CultureInfo.InvariantCulture) ?? "");
                    property.SetFudousanIdAdditionalCode(Convert.ToString(reader["resiFudousanIdAdditionalCode"], CultureInfo.InvariantCulture) ?? "");
                    property.SetRemarks(Convert.ToString(reader["resiRemarks"], CultureInfo.InvariantCulture) ?? "");

                    property.HasAutolock = Convert.ToInt32(reader["has_autolock"], CultureInfo.InvariantCulture) != 0;
                    property.HasElevator = Convert.ToInt32(reader["has_elevator"], CultureInfo.InvariantCulture) != 0;
                    property.HasSecurityCamera = Convert.ToInt32(reader["has_security_camera"], CultureInfo.InvariantCulture) != 0;
                    property.HasParcelLocker = Convert.ToInt32(reader["has_parcel_locker"], CultureInfo.InvariantCulture) != 0;
                    property.HasElectric = Convert.ToInt32(reader["has_electric"], CultureInfo.InvariantCulture) != 0;
                    property.PropertyElectricKind = ParseFacilityEnum<Models.Rent.Residentials.Property.EnumElectricType>(reader, "property_electric_kind");
                    property.ElectricDetail = reader.GetString(reader.GetOrdinal("electric_detail"));
                    property.HasGas = Convert.ToInt32(reader["has_gas"], CultureInfo.InvariantCulture) != 0;
                    property.GasType = ParseFacilityEnum<Models.Rent.Residentials.FacilityGasType>(reader, "gas_type");
                    property.GasDetail = reader.GetString(reader.GetOrdinal("gas_detail"));
                    property.TapWaterType = ParseFacilityEnum<Models.Rent.Residentials.FacilityTapWaterType>(reader, "tap_water_type");
                    property.TapWaterDetail = reader.GetString(reader.GetOrdinal("tap_water_detail"));
                    property.SewerageType = ParseFacilityEnum<Models.Rent.Residentials.FacilitySewerageType>(reader, "sewerage_type");
                    property.SewerageDetail = reader.GetString(reader.GetOrdinal("sewerage_detail"));
                    property.GroundDigital = Convert.ToInt32(reader["ground_digital"], CultureInfo.InvariantCulture) != 0;
                    property.BsAntenna = Convert.ToInt32(reader["bs_antenna"], CultureInfo.InvariantCulture) != 0;
                    property.CsAntenna = Convert.ToInt32(reader["cs_antenna"], CultureInfo.InvariantCulture) != 0;
                    property.Catv = Convert.ToInt32(reader["catv"], CultureInfo.InvariantCulture) != 0;
                    property.Usen = Convert.ToInt32(reader["usen"], CultureInfo.InvariantCulture) != 0;
                    property.Internet = Convert.ToInt32(reader["internet"], CultureInfo.InvariantCulture) != 0;
                    property.Fiber = Convert.ToInt32(reader["fiber"], CultureInfo.InvariantCulture) != 0;
                    property.InternetFree = Convert.ToInt32(reader["internet_free"], CultureInfo.InvariantCulture) != 0;
                    property.TvAntennaDetail = reader.GetString(reader.GetOrdinal("tv_antenna_detail"));
                    property.InternetDetail = reader.GetString(reader.GetOrdinal("internet_detail"));
                    property.HasParking = Convert.ToInt32(reader["has_parking"], CultureInfo.InvariantCulture) != 0;
                    property.ParkingContractType = ParseFacilityEnum<Models.Rent.Residentials.FacilityParkingContractType>(reader, "parking_contract_type");
                    property.ParkingAvailability = ParseFacilityEnum<Models.Rent.Residentials.FacilityAvailabilityType>(reader, "parking_availability");
                    property.IsParkingFree = Convert.ToInt32(reader["is_parking_free"], CultureInfo.InvariantCulture) != 0;
                    property.ParkingFee = reader.GetString(reader.GetOrdinal("parking_fee"));
                    property.ParkingFeeTaxType = ParseFacilityEnum<Models.Rent.Residentials.FacilityTaxType>(reader, "parking_fee_tax_type");
                    property.ParkingSpaces = reader.GetString(reader.GetOrdinal("parking_spaces"));
                    property.ParkingDistance = reader.GetString(reader.GetOrdinal("parking_distance"));
                    property.ParkingNotes = reader.GetString(reader.GetOrdinal("parking_notes"));
                    property.HasBicycleParking = Convert.ToInt32(reader["has_bicycle_parking"], CultureInfo.InvariantCulture) != 0;
                    property.BicycleParkingAvailability = ParseFacilityEnum<Models.Rent.Residentials.FacilityAvailabilityType>(reader, "bicycle_parking_availability");
                    property.IsBicycleParkingFree = Convert.ToInt32(reader["is_bicycle_parking_free"], CultureInfo.InvariantCulture) != 0;
                    property.BicycleParkingFee = reader.GetString(reader.GetOrdinal("bicycle_parking_fee"));
                    property.BicycleParkingFeeTaxType = ParseFacilityEnum<Models.Rent.Residentials.FacilityTaxType>(reader, "bicycle_parking_fee_tax_type");
                    property.BicycleParkingNotes = reader.GetString(reader.GetOrdinal("bicycle_parking_notes"));
                    property.HasMotorcycleParking = Convert.ToInt32(reader["has_motorcycle_parking"], CultureInfo.InvariantCulture) != 0;
                    property.MotorcycleParkingAvailability = ParseFacilityEnum<Models.Rent.Residentials.FacilityAvailabilityType>(reader, "motorcycle_parking_availability");
                    property.IsMotorcycleParkingFree = Convert.ToInt32(reader["is_motorcycle_parking_free"], CultureInfo.InvariantCulture) != 0;
                    property.MotorcycleParkingFee = reader.GetString(reader.GetOrdinal("motorcycle_parking_fee"));
                    property.MotorcycleParkingFeeTaxType = ParseFacilityEnum<Models.Rent.Residentials.FacilityTaxType>(reader, "motorcycle_parking_fee_tax_type");
                    property.MotorcycleParkingNotes = reader.GetString(reader.GetOrdinal("motorcycle_parking_notes"));

                    // TODO: more.


                    property.SetIsModified(false);

                    // TODO: more.

                    //res.AffectedCount++;

                    //break; // Assuming we only want the first match
                }
            }

            if (!isFound)
            {
                // TODO: IsError?
                return res;
            }

            // 物件写真（建物）
            cmd.CommandText = "SELECT * FROM rent_residential_pictures WHERE property_id = @propertyId";
            cmd.Parameters.Clear();
            cmd.Parameters.AddWithValue("@propertyId", id);
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    var picid = Convert.ToString(reader["picture_id"], CultureInfo.InvariantCulture) ?? string.Empty;
                    var picpath = Convert.ToString(reader["filename"], CultureInfo.InvariantCulture) ?? string.Empty;
                    if (!string.IsNullOrEmpty(picid) && !string.IsNullOrEmpty(picpath))
                    {
                        var rlpic = new Models.Rent.Residentials.PropertyPicture(picid, picpath, EntityStatus.Saved)
                        {
                            Description = Convert.ToString(reader["description"], CultureInfo.InvariantCulture) ?? string.Empty,
                        };

                        rlpic.SetIsModified(false);

                        var strType = Convert.ToString(reader["type"], CultureInfo.InvariantCulture) ?? string.Empty;
                        rlpic.SetLabelFromString(strType);

                        rlpic.IsMain = Convert.ToInt32(reader["is_main"], CultureInfo.InvariantCulture) != 0; // int to bool

                        property.Pictures.Add(rlpic);
                    }
                    else
                    {
                        Debug.WriteLine("picture_id or filename is null/empty.");
                    }
                }
            }

            // PDF（建物）
            cmd.CommandText = "SELECT * FROM rent_residential_pdfs WHERE property_id = @propertyId";
            cmd.Parameters.Clear();
            cmd.Parameters.AddWithValue("@propertyId", id);
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    var pdfid = Convert.ToString(reader["pdf_id"], CultureInfo.InvariantCulture) ?? string.Empty;
                    var pdfpath = Convert.ToString(reader["filename"], CultureInfo.InvariantCulture) ?? string.Empty;
                    var thumbpath = Convert.ToString(reader["thumbnail_filename"], CultureInfo.InvariantCulture) ?? string.Empty;
                    if (!string.IsNullOrEmpty(pdfid) && !string.IsNullOrEmpty(pdfpath) && !string.IsNullOrEmpty(thumbpath))
                    {
                        var rlpdf = new Models.Rent.Residentials.PropertyPdf(pdfid, pdfpath, thumbpath, EntityStatus.Saved)
                        {
                            Description = Convert.ToString(reader["description"], CultureInfo.InvariantCulture) ?? string.Empty
                        };

                        rlpdf.SetIsModified(false);

                        var strType = Convert.ToString(reader["type"], CultureInfo.InvariantCulture) ?? string.Empty;
                        rlpdf.SetTypeFromString(strType);

                        rlpdf.IsMain = Convert.ToInt32(reader["is_main"], CultureInfo.InvariantCulture) != 0; // int to bool

                        property.Pdfs.Add(rlpdf);
                    }
                    else
                    {
                        Debug.WriteLine("pdf_id or filename or thumbnail_filename is null/empty.");
                    }
                }
            }

            // 貸主（建物）
            var lessorIdList = new List<string>();
            cmd.CommandText = "SELECT * FROM rent_lessors_properties_listings WHERE property_id = @propertyId AND listing_id = ''";
            cmd.Parameters.Clear();
            cmd.Parameters.AddWithValue("@propertyId", id);
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    var lessorId = Convert.ToString(reader["lessor_id"], CultureInfo.InvariantCulture) ?? string.Empty;
                    if (!string.IsNullOrEmpty(lessorId))
                    {
                        lessorIdList.Add(lessorId);
                    }
                    else
                    {
                        Debug.WriteLine("lessor_id is null/empty.");
                    }
                }
            }
            if (lessorIdList.Count > 0)
            {
                foreach (var lessId in lessorIdList)
                {
                    // Get actuall lessors
                    cmd.CommandText = "SELECT lessor_id, name, person_kind, name_last, name_first, name_company, name_company_type, name_company_type_position, remarks FROM rent_lessors WHERE lessor_id = @lessorId";
                    cmd.Parameters.Clear();
                    cmd.Parameters.AddWithValue("@lessorId", lessId);
#pragma warning disable IDE0063 // Use simple 'using' statement
                    using (var reader2 = cmd.ExecuteReader())
                    {
                        while (reader2.Read())
                        {
                            var s = Convert.ToString(reader2["lessor_id"], CultureInfo.InvariantCulture);
                            if (string.IsNullOrEmpty(s))
                            {
                                Debug.WriteLine("DataAccess::SelectRentResidentialById: lessor_id is null or empty.");
                                continue;
                            }

                            var lessor = GetPerson(reader2, lessId);

                            if (lessor is not null)
                            {
                                property.Lessors.Add(lessor);
                            }

                            //break; // Assuming we only want the first match
                        }
                    }
#pragma warning restore IDE0063 // Use simple 'using' statement
                }
            }

            // 宅建業者（建物）
            var brokerIdList = new List<string>();
            cmd.CommandText = "SELECT * FROM brokers_properties_listings WHERE property_id = @propertyId AND listing_id = ''";
            cmd.Parameters.Clear();
            cmd.Parameters.AddWithValue("@propertyId", id);
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    var brokerId = Convert.ToString(reader["broker_id"], CultureInfo.InvariantCulture) ?? string.Empty;
                    if (!string.IsNullOrEmpty(brokerId))
                    {
                        brokerIdList.Add(brokerId);
                    }
                    else
                    {
                        Debug.WriteLine("broker_id is null/empty.");
                    }
                }
            }
            if (brokerIdList.Count > 0)
            {
                foreach (var brokerId in brokerIdList)
                {
                    // Get actuall brokers
                    cmd.CommandText = "SELECT broker_id, name, person_kind, name_last, name_first, name_company, name_company_type, name_company_type_position, remarks FROM brokers WHERE broker_id = @brokerId";
                    cmd.Parameters.Clear();
                    cmd.Parameters.AddWithValue("@brokerId", brokerId);
#pragma warning disable IDE0063 // Use simple 'using' statement
                    using (var reader2 = cmd.ExecuteReader())
                    {
                        while (reader2.Read())
                        {
                            var s = Convert.ToString(reader2["broker_id"], CultureInfo.InvariantCulture);
                            if (string.IsNullOrEmpty(s))
                            {
                                Debug.WriteLine("DataAccess::SelectRentResidentialById: broker_id is null or empty.");
                                continue;
                            }

                            var broker = GetPerson(reader2, brokerId);

                            if (broker is not null)
                            {
                                property.Brokers.Add(broker);
                            }

                            //break; // Assuming we only want the first match
                        }
                    }
#pragma warning restore IDE0063 // Use simple 'using' statement
                }
            }

            // 部屋
            cmd.CommandText = "SELECT * FROM rent_residential_listings WHERE property_id = @propertyId";
            cmd.Parameters.Clear();
            cmd.Parameters.AddWithValue("@propertyId", id);
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    var resRoom = GetRentResidentialListing(reader, id, property.Name);
                    if (resRoom is not null)
                    {
                        property.Rooms.Add(resRoom);
                    }
                }
            }

            foreach (var room in property.Rooms)
            {
                SetRentResidentialListingChildValues(cmd, room);
            }

            // Reset property Isdirty flag.
            property.SetStatus(EntityStatus.Saved);
            property.SetIsModified(false);

            res.Building = property;
        }
        catch (Exception ex)
        {
            SetDatabaseError(res, ex, "connection.Open(), reader.Read()", "Failed to connect to or read a SQLite database file", nameof(SelectRentResidentialById));
        }
        finally
        {
            _readerWriterLock.ExitReadLock();
        }

        return res;
    }

    private static string FormatFacilityEnum<TEnum>(TEnum value) where TEnum : struct, Enum
    {
        if (!Enum.IsDefined(value))
        {
            throw new ArgumentOutOfRangeException(nameof(value), value, "Invalid residential facility enum value.");
        }

        return value.ToString();
    }

    private static TEnum ParseFacilityEnum<TEnum>(SqliteDataReader reader, string column) where TEnum : struct, Enum
    {
        var text = reader.GetString(reader.GetOrdinal(column));
        if (!Enum.TryParse<TEnum>(text, out var value) || !Enum.IsDefined(value) || !string.Equals(text, value.ToString(), StringComparison.Ordinal))
        {
            throw new FormatException($"Invalid residential facility enum value in {column}: {text}");
        }

        return value;
    }

    public ResultWrapper DeleteRentResidential(string propertyId)
    {
        var res = new ResultWrapper();

        if (string.IsNullOrEmpty(propertyId))
        {
            res.IsError = true;
            // TODO:
            return res;
        }

        _readerWriterLock.EnterWriteLock();
        try
        {
            // System.Data.SQLite
            //using var connection = new SQLiteConnection(_connectionStringBuilder.ConnectionString);
            // Microsoft.Data.Sqlite
            using var connection = new SqliteConnection(_connectionStringBuilder.ConnectionString);
            connection.Open();

            using var cmd = connection.CreateCommand();

            cmd.Transaction = connection.BeginTransaction();
            try
            {
                cmd.CommandText = string.Format(CultureInfo.InvariantCulture, "DELETE FROM properties WHERE property_id = '{0}';", propertyId);
                res.AffectedCount = cmd.ExecuteNonQuery();

                cmd.Transaction.Commit();
            }
            catch (Exception ex)
            {
                cmd.Transaction.Rollback();

                SetDatabaseError(res, ex, "cmd.ExecuteNonQuery(), cmd.Transaction.Commit", "Failed to delete database recode. Transaction.Rollback()", nameof(DeleteRentResidential));
                return res;
            }
        }
        catch (Exception ex)
        {
            SetDatabaseError(res, ex, "Connection.Open", "Failed to Connect to a SQLite database file", nameof(DeleteRentResidential));

            return res;
        }
        finally
        {
            _readerWriterLock.ExitWriteLock();
        }

        //Debug.WriteLine(string.Format("{0} feed Deleted from DB", res.AffectedCount));

        return res;
    }

    #endregion

    #region == Rent Residential Listing ==

    private static int DoUpsertRentResidentialListing(SqliteCommand cmd, string propertyId, Models.Rent.Residentials.Listing room)
    {
        cmd.CommandText = """
            INSERT INTO rent_residential_listings (
                listing_id, property_id, is_property_unit_ownership, name,
                floor_plan_type, floor_plan_extra_type, floor_area, floor_type, floor_number,
                is_corner_room, main_exposure_direction, availability_status, current_status,
                is_recruiting, available_from_month, available_from_period,
                is_immediate_occupancy, current_status_checked_at, remarks, chinryou,
                management_fee, management_fee_unit, security_deposit, security_deposit_unit,
                key_money, key_money_unit, contract_type, contract_period_years,
                renewal_fee, renewal_fee_unit, recontract_fee, recontract_fee_unit,
                guarantor_company, housing_insurance, other_fees, pet_friendly,
                two_person_occupancy, no_guarantor_needed, office_use_allowed, other_conditions, contract_remarks,
                handling_type, transaction_type, lessor_commission, lessor_commission_unit,
                lessee_commission, lessee_commission_unit, listing_broker_commission, listing_broker_commission_unit,
                tenant_broker_commission, tenant_broker_commission_unit, transaction_remarks,
                has_air_conditioner, has_wood_flooring, has_video_intercom, has_bath_reheating,
                has_gas_stove, has_induction_cooktop, has_multiple_burners, has_system_kitchen,
                has_loft, has_indoor_washing_machine_space, has_bathroom_dryer, has_separate_bath_and_toilet,
                has_separate_washbasin, has_warm_water_toilet_seat, appliance_remarks)
            VALUES (
                @listing_id, @property_id, @is_property_unit_ownership, @name,
                @floor_plan_type, @floor_plan_extra_type, @floor_area, @floor_type, @floor_number,
                @is_corner_room, @main_exposure_direction, @availability_status, @current_status,
                @is_recruiting, @available_from_month, @available_from_period,
                @is_immediate_occupancy, @current_status_checked_at, @remarks, @chinryou,
                @management_fee, @management_fee_unit, @security_deposit, @security_deposit_unit,
                @key_money, @key_money_unit, @contract_type, @contract_period_years,
                @renewal_fee, @renewal_fee_unit, @recontract_fee, @recontract_fee_unit,
                @guarantor_company, @housing_insurance, @other_fees, @pet_friendly,
                @two_person_occupancy, @no_guarantor_needed, @office_use_allowed, @other_conditions, @contract_remarks,
                @handling_type, @transaction_type, @lessor_commission, @lessor_commission_unit,
                @lessee_commission, @lessee_commission_unit, @listing_broker_commission, @listing_broker_commission_unit,
                @tenant_broker_commission, @tenant_broker_commission_unit, @transaction_remarks,
                @has_air_conditioner, @has_wood_flooring, @has_video_intercom, @has_bath_reheating,
                @has_gas_stove, @has_induction_cooktop, @has_multiple_burners, @has_system_kitchen,
                @has_loft, @has_indoor_washing_machine_space, @has_bathroom_dryer, @has_separate_bath_and_toilet,
                @has_separate_washbasin, @has_warm_water_toilet_seat, @appliance_remarks)
            ON CONFLICT(listing_id) DO UPDATE SET
                is_property_unit_ownership = @is_property_unit_ownership,
                name = @name,
                floor_plan_type = @floor_plan_type,
                floor_plan_extra_type = @floor_plan_extra_type,
                floor_area = @floor_area,
                floor_type = @floor_type,
                floor_number = @floor_number,
                is_corner_room = @is_corner_room,
                main_exposure_direction = @main_exposure_direction,
                availability_status = @availability_status,
                current_status = @current_status,
                is_recruiting = @is_recruiting,
                available_from_month = @available_from_month,
                available_from_period = @available_from_period,
                is_immediate_occupancy = @is_immediate_occupancy,
                current_status_checked_at = @current_status_checked_at,
                remarks = @remarks,
                chinryou = @chinryou,
                management_fee = @management_fee,
                management_fee_unit = @management_fee_unit,
                security_deposit = @security_deposit,
                security_deposit_unit = @security_deposit_unit,
                key_money = @key_money,
                key_money_unit = @key_money_unit,
                contract_type = @contract_type,
                contract_period_years = @contract_period_years,
                renewal_fee = @renewal_fee,
                renewal_fee_unit = @renewal_fee_unit,
                recontract_fee = @recontract_fee,
                recontract_fee_unit = @recontract_fee_unit,
                guarantor_company = @guarantor_company,
                housing_insurance = @housing_insurance,
                other_fees = @other_fees,
                pet_friendly = @pet_friendly,
                two_person_occupancy = @two_person_occupancy,
                no_guarantor_needed = @no_guarantor_needed,
                office_use_allowed = @office_use_allowed,
                other_conditions = @other_conditions,
                contract_remarks = @contract_remarks,
                handling_type = @handling_type,
                transaction_type = @transaction_type,
                lessor_commission = @lessor_commission,
                lessor_commission_unit = @lessor_commission_unit,
                lessee_commission = @lessee_commission,
                lessee_commission_unit = @lessee_commission_unit,
                listing_broker_commission = @listing_broker_commission,
                listing_broker_commission_unit = @listing_broker_commission_unit,
                tenant_broker_commission = @tenant_broker_commission,
                tenant_broker_commission_unit = @tenant_broker_commission_unit,
                transaction_remarks = @transaction_remarks,
                has_air_conditioner = @has_air_conditioner,
                has_wood_flooring = @has_wood_flooring,
                has_video_intercom = @has_video_intercom,
                has_bath_reheating = @has_bath_reheating,
                has_gas_stove = @has_gas_stove,
                has_induction_cooktop = @has_induction_cooktop,
                has_multiple_burners = @has_multiple_burners,
                has_system_kitchen = @has_system_kitchen,
                has_loft = @has_loft,
                has_indoor_washing_machine_space = @has_indoor_washing_machine_space,
                has_bathroom_dryer = @has_bathroom_dryer,
                has_separate_bath_and_toilet = @has_separate_bath_and_toilet,
                has_separate_washbasin = @has_separate_washbasin,
                has_warm_water_toilet_seat = @has_warm_water_toilet_seat,
                appliance_remarks = @appliance_remarks,
                updated_at = @updated_at;
            """;

        // Clear parameters before setting new ones because this method can be called multiple times in a loop.
        cmd.Parameters.Clear();

        cmd.Parameters.AddWithValue("@listing_id", room.Id);
        cmd.Parameters.AddWithValue("@property_id", propertyId);
        cmd.Parameters.AddWithValue("@is_property_unit_ownership", room.IsPropertyUnitOwnership ? 1 : 0); // bool to int
        cmd.Parameters.AddWithValue("@name", room.Name);

        cmd.Parameters.AddWithValue("@floor_plan_type", room.FloorPlan.Key.ToString());
        cmd.Parameters.AddWithValue("@floor_plan_extra_type", room.FloorPlanExtra.Key.ToString());
        cmd.Parameters.AddWithValue("@floor_area", room.FloorExclusiveAreaSqm);
        cmd.Parameters.AddWithValue("@floor_type", room.FloorGround.Key.ToString());
        cmd.Parameters.AddWithValue("@floor_number", room.FloorNumber);
        cmd.Parameters.AddWithValue("@is_corner_room", room.IsCornerRoom ? 1 : 0);
        cmd.Parameters.AddWithValue("@main_exposure_direction", room.SelectedExposureDirectionType.Key.ToString());
        cmd.Parameters.AddWithValue("@availability_status", room.SelectedAvailabilityStatusType.Key.ToString());
        cmd.Parameters.AddWithValue("@current_status", room.SelectedOccupancyStatus.Key.ToString());
        cmd.Parameters.AddWithValue("@is_recruiting", room.IsAvailableForRentNow ? 1 : 0);
        cmd.Parameters.AddWithValue("@available_from_month", room.SelectedAvailabilityMonthType.Key.ToString());
        cmd.Parameters.AddWithValue("@available_from_period", room.SelectedAvailabilityPeriodType.Key.ToString());
        cmd.Parameters.AddWithValue("@is_immediate_occupancy", room.IsAvailableForImmediateMoveIn ? 1 : 0);
        cmd.Parameters.AddWithValue("@current_status_checked_at", room.OccupancyStatusCheckedAt?.ToString("O", CultureInfo.InvariantCulture) ?? "");

        cmd.Parameters.AddWithValue("@remarks", room.Remarks ?? string.Empty);


        cmd.Parameters.AddWithValue("@chinryou", room.Chinryou);

        cmd.Parameters.AddWithValue("@management_fee", room.ManagementFee);
        cmd.Parameters.AddWithValue("@management_fee_unit", room.ManagementFeeUnit.Key.ToString());
        cmd.Parameters.AddWithValue("@security_deposit", room.SecurityDeposit);
        cmd.Parameters.AddWithValue("@security_deposit_unit", room.SecurityDepositUnit.Key.ToString());
        cmd.Parameters.AddWithValue("@key_money", room.KeyMoney);
        cmd.Parameters.AddWithValue("@key_money_unit", room.KeyMoneyUnit.Key.ToString());
        cmd.Parameters.AddWithValue("@contract_type", room.ContractType.Key.ToString());
        cmd.Parameters.AddWithValue("@contract_period_years", room.ContractPeriodYears);
        cmd.Parameters.AddWithValue("@renewal_fee", room.RenewalFee);
        cmd.Parameters.AddWithValue("@renewal_fee_unit", room.RenewalFeeUnit.Key.ToString());
        cmd.Parameters.AddWithValue("@recontract_fee", room.RecontractFee);
        cmd.Parameters.AddWithValue("@recontract_fee_unit", room.RecontractFeeUnit.Key.ToString());
        cmd.Parameters.AddWithValue("@guarantor_company", room.GuarantorCompany);
        cmd.Parameters.AddWithValue("@housing_insurance", room.HousingInsurance);
        cmd.Parameters.AddWithValue("@other_fees", room.OtherFees);
        cmd.Parameters.AddWithValue("@pet_friendly", room.PetFriendly ? 1 : 0);
        cmd.Parameters.AddWithValue("@two_person_occupancy", room.TwoPersonOccupancy ? 1 : 0);
        cmd.Parameters.AddWithValue("@no_guarantor_needed", room.NoGuarantorNeeded ? 1 : 0);
        cmd.Parameters.AddWithValue("@office_use_allowed", room.OfficeUseAllowed ? 1 : 0);
        cmd.Parameters.AddWithValue("@other_conditions", room.OtherConditions);
        cmd.Parameters.AddWithValue("@contract_remarks", room.ContractRemarks);
        cmd.Parameters.AddWithValue("@handling_type", room.HandlingType.Key.ToString());
        cmd.Parameters.AddWithValue("@transaction_type", room.TransactionType.Key.ToString());
        cmd.Parameters.AddWithValue("@lessor_commission", room.LessorCommission);
        cmd.Parameters.AddWithValue("@lessor_commission_unit", room.LessorCommissionUnit.Key.ToString());
        cmd.Parameters.AddWithValue("@lessee_commission", room.LesseeCommission);
        cmd.Parameters.AddWithValue("@lessee_commission_unit", room.LesseeCommissionUnit.Key.ToString());
        cmd.Parameters.AddWithValue("@listing_broker_commission", room.ListingBrokerCommission);
        cmd.Parameters.AddWithValue("@listing_broker_commission_unit", room.ListingBrokerCommissionUnit.Key.ToString());
        cmd.Parameters.AddWithValue("@tenant_broker_commission", room.TenantBrokerCommission);
        cmd.Parameters.AddWithValue("@tenant_broker_commission_unit", room.TenantBrokerCommissionUnit.Key.ToString());
        cmd.Parameters.AddWithValue("@transaction_remarks", room.TransactionRemarks);
        cmd.Parameters.AddWithValue("@has_air_conditioner", room.HasAirConditioner ? 1 : 0);
        cmd.Parameters.AddWithValue("@has_wood_flooring", room.HasWoodFlooring ? 1 : 0);
        cmd.Parameters.AddWithValue("@has_video_intercom", room.HasVideoIntercom ? 1 : 0);
        cmd.Parameters.AddWithValue("@has_bath_reheating", room.HasBathReheating ? 1 : 0);
        cmd.Parameters.AddWithValue("@has_gas_stove", room.HasGasStove ? 1 : 0);
        cmd.Parameters.AddWithValue("@has_induction_cooktop", room.HasInductionCooktop ? 1 : 0);
        cmd.Parameters.AddWithValue("@has_multiple_burners", room.HasMultipleBurners ? 1 : 0);
        cmd.Parameters.AddWithValue("@has_system_kitchen", room.HasSystemKitchen ? 1 : 0);
        cmd.Parameters.AddWithValue("@has_loft", room.HasLoft ? 1 : 0);
        cmd.Parameters.AddWithValue("@has_indoor_washing_machine_space", room.HasIndoorWashingMachineSpace ? 1 : 0);
        cmd.Parameters.AddWithValue("@has_bathroom_dryer", room.HasBathroomDryer ? 1 : 0);
        cmd.Parameters.AddWithValue("@has_separate_bath_and_toilet", room.HasSeparateBathAndToilet ? 1 : 0);
        cmd.Parameters.AddWithValue("@has_separate_washbasin", room.HasSeparateWashbasin ? 1 : 0);
        cmd.Parameters.AddWithValue("@has_warm_water_toilet_seat", room.HasWarmWaterToiletSeat ? 1 : 0);
        cmd.Parameters.AddWithValue("@appliance_remarks", room.ApplianceRemarks);

        cmd.Parameters.AddWithValue("@updated_at", DateTimeOffset.UtcNow.ToString("s"));

        var result = cmd.ExecuteNonQuery();
        if (result > 0)
        {
            //room.IsNew = false;
            room.SetIsModified(false);
            room.SetStatus(EntityStatus.Saved);
            room.SetPropertyStatus(EntityStatus.Saved);
        }
        //res.AffectedCount = result;

        cmd.Parameters.Clear();

        // 部屋写真 rent_residential_listing_pictures table - Insert or Update
        if (room.Pictures.Count > 0)
        {
            foreach (var pic in room.Pictures)
            {
                // Upsert
                var sqlUpsertRoom = "INSERT INTO rent_residential_listing_pictures (picture_id, listing_id, property_id, filename, type, description, is_main) VALUES (@picture_id, @listing_id, @property_id, @filename, @type, @description, @is_main) ";
                sqlUpsertRoom += "ON CONFLICT(picture_id) ";
                sqlUpsertRoom += "DO UPDATE SET filename = @filename, type = @type, description = @description, is_main = @is_main";
                var exec = true;

                cmd.CommandText = sqlUpsertRoom;

                if (exec)
                {
                    // ループなので、前のパラメーターをクリアする。
                    cmd.Parameters.Clear();

                    cmd.Parameters.AddWithValue("@picture_id", pic.Id);
                    cmd.Parameters.AddWithValue("@listing_id", room.Id);
                    cmd.Parameters.AddWithValue("@property_id", propertyId);
                    cmd.Parameters.AddWithValue("@filename", pic.ImageFilename);
                    cmd.Parameters.AddWithValue("@type", pic.PictureType.Key.ToString());
                    cmd.Parameters.AddWithValue("@description", pic.Description);
                    /*
                    var paramIsMain = new SqliteParameter("@is_main", System.Data.DbType.Int32);
                    if (pic.IsMain)
                    {
                        paramIsMain.Value = 1;
                    }
                    else
                    {
                        paramIsMain.Value = 0;
                    }
                    cmd.Parameters.Add(paramIsMain);
                    */
                    cmd.Parameters.AddWithValue("@is_main", pic.IsMain ? 1 : 0); // bool to int

                    result = cmd.ExecuteNonQuery();
                    if (result > 0)
                    {
                        //pic.IsNew = false;
                        pic.SetStatus(EntityStatus.Saved);
                        pic.SetIsModified(false);
                    }
                }

            }
        }

        // 部屋写真の削除リストを処理
        if (room.PicturesToBeDeleted.Count > 0)
        {
            foreach (var delr in room.PicturesToBeDeleted)
            {
                // 削除
                var sqlDeleteRentLivingRoom = string.Format(CultureInfo.InvariantCulture, "DELETE FROM rent_residential_listing_pictures WHERE picture_id = '{0}'", delr.Id);

                cmd.CommandText = sqlDeleteRentLivingRoom;
                var DelRentLivingRoomResult = cmd.ExecuteNonQuery();
                if (DelRentLivingRoomResult > 0)
                {
                    // TODO:
                    Debug.WriteLine("Room Pic deleted");
                }
            }
            // let's not do this.
            //room.UnitPicturesToBeDeleted.Clear();
        }

        // 部屋図面 rent_residential_listing_pdfs table - Insert or Update
        if (room.Pdfs.Count > 0)
        {
            foreach (var pdf in room.Pdfs)
            {
                // Upsert
                var sqlUpsertRoom = "INSERT INTO rent_residential_listing_pdfs (pdf_id, listing_id, property_id, filename, thumbnail_filename, type, description, is_main) VALUES (@pdf_id, @listing_id, @property_id, @filename, @thumbnail_filename, @type, @description, @is_main) ";
                sqlUpsertRoom += "ON CONFLICT(pdf_id) ";
                sqlUpsertRoom += "DO UPDATE SET filename = @filename, thumbnail_filename = @thumbnail_filename, type = @type, description = @description, is_main = @is_main";
                var exec = true;

                cmd.CommandText = sqlUpsertRoom;

                if (exec)
                {
                    // ループなので、前のパラメーターをクリアする。
                    cmd.Parameters.Clear();

                    cmd.Parameters.AddWithValue("@pdf_id", pdf.Id);
                    cmd.Parameters.AddWithValue("@listing_id", room.Id);
                    cmd.Parameters.AddWithValue("@property_id", propertyId);
                    cmd.Parameters.AddWithValue("@filename", pdf.PdfFilename);
                    cmd.Parameters.AddWithValue("@thumbnail_filename", pdf.ThumbnailFilename);
                    cmd.Parameters.AddWithValue("@type", pdf.PdfType.Key.ToString());
                    cmd.Parameters.AddWithValue("@description", pdf.Description);
                    var paramIsMain = new SqliteParameter("@is_main", System.Data.DbType.Int32);
                    if (pdf.IsMain)
                    {
                        paramIsMain.Value = 1;
                    }
                    else
                    {
                        paramIsMain.Value = 0;
                    }
                    cmd.Parameters.Add(paramIsMain);

                    result = cmd.ExecuteNonQuery();
                    if (result > 0)
                    {
                        //pdf.IsNew = false;
                        pdf.SetStatus(EntityStatus.Saved);
                        pdf.SetIsModified(false);
                    }
                }

            }
        }

        // 部屋図面の削除リストを処理
        if (room.PdfsToBeDeleted.Count > 0)
        {
            foreach (var delr in room.PdfsToBeDeleted)
            {
                // 削除
                var sqlDeleteRentLivingRoom = string.Format(CultureInfo.InvariantCulture, "DELETE FROM rent_residential_listing_pdfs WHERE pdf_id = '{0}'", delr.Id);

                cmd.CommandText = sqlDeleteRentLivingRoom;
                var DelRentLivingRoomResult = cmd.ExecuteNonQuery();
                if (DelRentLivingRoomResult > 0)
                {
                    // TODO:
                    Debug.WriteLine("Room Pdf deleted @UpdateRentResidential in DataAccessService");
                }
            }
            // let's not do this.
            //room.UnitPicturesToBeDeleted.Clear();
        }

        // 部屋貸主 rent_lessors_properties_listings - Insert or Update
        if (room.Lessors.Count > 0)
        {
            foreach (var psn in room.Lessors)
            {
                //var sqlInsertInto = "INSERT INTO rent_lessors_properties_listings (lessor_id, property_id, property_kind, listing_id) " +
                //                "VALUES (@lessor_id, @property_id, @property_kind, @listing_id)";

                // Upsert 
                var sqlUpsert = "INSERT INTO rent_lessors_properties_listings (lessor_id, property_id, property_kind, listing_id) VALUES (@lessor_id, @property_id, @property_kind, @listing_id) ";
                sqlUpsert += "ON CONFLICT(lessor_id, property_id, listing_id) ";
                sqlUpsert += "DO NOTHING";//"DO UPDATE SET lessor_id = @lessor_id, property_id = @property_id, property_kind = @property_kind, listing_id = @listing_id";

                cmd.CommandText = sqlUpsert;

                // ループなので、前のパラメーターをクリアする。
                cmd.Parameters.Clear();

                cmd.Parameters.AddWithValue("@lessor_id", psn.Id);
                cmd.Parameters.AddWithValue("@property_id", room.PropertyId);
                cmd.Parameters.AddWithValue("@property_kind", room.PropertyContextType.ToString());
                cmd.Parameters.AddWithValue("@listing_id", room.Id);

                cmd.ExecuteNonQuery();
            }
        }

        // 部屋貸主の削除リストを処理
        if (room.LessorsToBeDeleted.Count > 0)
        {
            foreach (var psn in room.LessorsToBeDeleted)
            {
                // 削除
                var sqlDelete = ($"DELETE FROM rent_lessors_properties_listings WHERE lessor_id = '{psn.Id}' AND property_id = '{room.PropertyId}' AND listing_id = '{room.Id}'");

                cmd.CommandText = sqlDelete;
                var sqlResult = cmd.ExecuteNonQuery();
                if (sqlResult > 0)
                {
                    // TODO:
                    Debug.WriteLine("Lessor deleted");
                }
            }
        }

        // 部屋宅建業者 brokers_properties_listings - Insert or Update
        foreach (var broker in room.Brokers)
        {
            cmd.Parameters.Clear();
            cmd.CommandText = """
                INSERT INTO brokers_properties_listings
                    (broker_id, property_id, property_kind, listing_id)
                VALUES
                    (@brokerId, @propertyId, @propertyKind, @listingId)
                ON CONFLICT (broker_id, property_id, listing_id) DO NOTHING;
                """;
            cmd.Parameters.AddWithValue("@brokerId", broker.Id);
            cmd.Parameters.AddWithValue("@propertyId", room.PropertyId);
            cmd.Parameters.AddWithValue("@propertyKind", room.PropertyContextType.ToString());
            cmd.Parameters.AddWithValue("@listingId", room.Id);
            cmd.ExecuteNonQuery();
        }

        // 部屋宅建業者の削除リストを処理
        foreach (var broker in room.BrokersToBeDeleted)
        {
            cmd.Parameters.Clear();
            cmd.CommandText = """
                DELETE FROM brokers_properties_listings
                WHERE broker_id = @brokerId
                  AND property_id = @propertyId
                  AND listing_id = @listingId;
                """;
            cmd.Parameters.AddWithValue("@brokerId", broker.Id);
            cmd.Parameters.AddWithValue("@propertyId", room.PropertyId);
            cmd.Parameters.AddWithValue("@listingId", room.Id);
            cmd.ExecuteNonQuery();
        }


        return result;
    }

    public ResultWrapper UpsertRentResidentialListing(string propertyId, Models.Rent.Residentials.Listing room)
    {
        var res = new ResultWrapper();

        if (string.IsNullOrEmpty(propertyId))
        {
            res.IsError = true;
            // TODO:
            return res;
        }

        _readerWriterLock.EnterWriteLock();
        try
        {
            using var connection = new SqliteConnection(_connectionStringBuilder.ConnectionString);
            connection.Open();

            using var cmd = connection.CreateCommand();
            cmd.Transaction = connection.BeginTransaction();
            try
            {
                cmd.CommandType = CommandType.Text;

                // Update updated_at in the properties table.
                cmd.CommandText = "UPDATE properties SET updated_at = @updatedAt WHERE property_id = @propertyId;";
                cmd.Parameters.Add("@updatedAt", SqliteType.Text).Value = DateTimeOffset.UtcNow.ToString("s", CultureInfo.InvariantCulture);
                cmd.Parameters.Add("@propertyId", SqliteType.Text).Value = propertyId;

                cmd.ExecuteNonQuery();
                /*
                
                // Upsert into rent_residential_listings
                cmd.CommandText = """
                    INSERT INTO rent_residential_listings (
                        listing_id, property_id, is_property_unit_ownership, name, chinryou,
                        room_count, floor_plan_type, floor_plan_extra_type, floor_area, floor_type, floor_number,
                        is_corner_room, main_exposure_direction, current_status, is_recruiting,
                        available_from_month, available_from_period, is_immediate_occupancy,
                        current_status_checked_at, remarks)
                    VALUES (
                        @listing_id, @property_id, @is_property_unit_ownership, @name, @chinryou,
                        @room_count, @floor_plan_type, @floor_plan_extra_type, @floor_area, @floor_type, @floor_number,
                        @is_corner_room, @main_exposure_direction, @current_status, @is_recruiting,
                        @available_from_month, @available_from_period, @is_immediate_occupancy,
                        @current_status_checked_at, @remarks)
                    ON CONFLICT(listing_id) DO UPDATE SET
                        is_property_unit_ownership = @is_property_unit_ownership,
                        name = @name,
                        chinryou = @chinryou,
                        room_count = @room_count,
                        floor_plan_type = @floor_plan_type,
                        floor_plan_extra_type = @floor_plan_extra_type,
                        floor_area = @floor_area,
                        floor_type = @floor_type,
                        floor_number = @floor_number,
                        is_corner_room = @is_corner_room,
                        main_exposure_direction = @main_exposure_direction,
                        current_status = @current_status,
                        is_recruiting = @is_recruiting,
                        available_from_month = @available_from_month,
                        available_from_period = @available_from_period,
                        is_immediate_occupancy = @is_immediate_occupancy,
                        current_status_checked_at = @current_status_checked_at,
                        remarks = @remarks,
                        updated_at = @updated_at;
                    """;
                cmd.Parameters.Clear();

                */
                //
                res.AffectedCount = DoUpsertRentResidentialListing(cmd, propertyId, room);

                // Commit
                cmd.Transaction.Commit();

                // Clear deletion queues only after the commit succeeds.
                room.LessorsToBeDeleted.Clear();
                room.BrokersToBeDeleted.Clear();
            }
            catch (Exception ex)
            {
                cmd.Transaction.Rollback();

                SetDatabaseError(res, ex, "cmd.ExecuteNonQuery(), cmd.Transaction.Commit", "Failed to update database tables. Transaction.Rollback()", nameof(UpsertRentResidentialListing));

                return res;
            }
        }
        catch (Exception ex)
        {
            SetDatabaseError(res, ex, "connection.Open", "Failed to Connect to a SQLite database file", nameof(UpsertRentResidentialListing));
            return res;
        }
        finally
        {
            _readerWriterLock.ExitWriteLock();
        }

        //Debug.WriteLine(string.Format("{0} Entries Inserted to DB", res.AffectedCount.ToString()));

        return res;
    }

    public ListingSearchResultWrapper SelectRentResidentialListings(Models.Rent.Residentials.ListingSearchCriteria criteria)
    {
        var res = new ListingSearchResultWrapper();

        _readerWriterLock.EnterReadLock();
        try
        {
            using var connection = new SqliteConnection(_connectionStringBuilder.ConnectionString);
            connection.Open();

            using var cmd = connection.CreateCommand();
            var conditions = new List<string>();

            void AddTextFilter(string value, string expression, string parameterName)
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    return;
                }

                conditions.Add(expression.Replace("@value", "@" + parameterName, StringComparison.Ordinal));
                var escaped = value.Trim().Replace("\\", "\\\\", StringComparison.Ordinal)
                    .Replace("%", "\\%", StringComparison.Ordinal)
                    .Replace("_", "\\_", StringComparison.Ordinal);
                cmd.Parameters.AddWithValue("@" + parameterName, "%" + escaped + "%");
            }

            void AddNumberFilter(double value, string expression, string parameterName, string comparison)
            {
                if (!double.IsFinite(value))
                {
                    return;
                }

                conditions.Add($"{expression} {comparison} @{parameterName}");
                cmd.Parameters.AddWithValue("@" + parameterName, value);
            }

            void AddEnumFilter(string column, IEnumerable<string> values, string parameterName)
            {
                var selectedValues = values.ToArray();
                if (selectedValues.Length == 0)
                {
                    return;
                }

                var parameters = selectedValues.Select((_, index) => "@" + parameterName + index.ToString(CultureInfo.InvariantCulture)).ToArray();
                conditions.Add(column + " IN (" + string.Join(", ", parameters) + ")");
                for (var index = 0; index < selectedValues.Length; index++)
                {
                    cmd.Parameters.AddWithValue(parameters[index], selectedValues[index]);
                }
            }

            void AddFlag(bool selected, string column)
            {
                if (selected)
                {
                    conditions.Add(column + " = 1");
                }
            }

            AddTextFilter(criteria.PropertyName, "p.name LIKE @value ESCAPE '\\'", "propertyName");
            AddTextFilter(criteria.Address, "p.location_full LIKE @value ESCAPE '\\'", "address");
            AddTextFilter(criteria.LessorName, "EXISTS (SELECT 1 FROM rent_lessors_properties_listings AS relation INNER JOIN rent_lessors AS lessor ON lessor.lessor_id = relation.lessor_id WHERE relation.property_id = p.property_id AND (relation.listing_id = '' OR relation.listing_id = l.listing_id) AND lessor.name LIKE @value ESCAPE '\\')", "lessorName");
            AddTextFilter(criteria.FudousanId, "r.fudousan_id LIKE @value ESCAPE '\\'", "fudousanId");

            AddNumberFilter(criteria.MinimumRent, "l.chinryou", "minimumRent", ">=");
            AddNumberFilter(criteria.MaximumRent, "l.chinryou", "maximumRent", "<=");
            AddNumberFilter(criteria.MaximumManagementFee, "CAST(REPLACE(l.management_fee, ',', '') AS REAL)", "maximumManagementFee", "<=");
            if (criteria.NoSecurityDeposit)
            {
                conditions.Add("CAST(REPLACE(l.security_deposit, ',', '') AS REAL) = 0");
            }
            if (criteria.NoKeyMoney)
            {
                conditions.Add("CAST(REPLACE(l.key_money, ',', '') AS REAL) = 0");
            }

            var hasStationFilter = !string.IsNullOrWhiteSpace(criteria.Station);
            var hasLineFilter = !string.IsNullOrWhiteSpace(criteria.Line);
            var hasWalkFilter = double.IsFinite(criteria.MaximumStationWalkMinutes);
            if (hasStationFilter || hasLineFilter || hasWalkFilter)
            {
                if (hasStationFilter)
                {
                    cmd.Parameters.AddWithValue("@station", "%" + criteria.Station.Trim().Replace("\\", "\\\\", StringComparison.Ordinal).Replace("%", "\\%", StringComparison.Ordinal).Replace("_", "\\_", StringComparison.Ordinal) + "%");
                }
                if (hasLineFilter)
                {
                    cmd.Parameters.AddWithValue("@line", "%" + criteria.Line.Trim().Replace("\\", "\\\\", StringComparison.Ordinal).Replace("%", "\\%", StringComparison.Ordinal).Replace("_", "\\_", StringComparison.Ordinal) + "%");
                }
                if (hasWalkFilter)
                {
                    cmd.Parameters.AddWithValue("@maximumWalk", criteria.MaximumStationWalkMinutes);
                }

                var routeConditions = new List<string>();
                for (var index = 1; index <= 4; index++)
                {
                    var route = new List<string>();
                    if (hasStationFilter)
                    {
                        route.Add($"p.train{index}_station_name LIKE @station ESCAPE '\\'");
                    }
                    if (hasLineFilter)
                    {
                        route.Add($"p.train{index}_line_name LIKE @line ESCAPE '\\'");
                    }
                    if (hasWalkFilter)
                    {
                        route.Add($"p.train{index}_ekitoho > 0 AND p.train{index}_ekitoho <= @maximumWalk");
                    }
                    routeConditions.Add("(" + string.Join(" AND ", route) + ")");
                }
                conditions.Add("(" + string.Join(" OR ", routeConditions) + ")");
            }

            AddEnumFilter("r.property_type", criteria.PropertyKinds.Select(value => value.ToString()), "propertyKind");
            AddEnumFilter("r.property_structure", criteria.PropertyStructures.Select(value => value.ToString()), "propertyStructure");
            AddNumberFilter(DateTime.Now.Year - criteria.MaximumBuildingAgeYears, "CAST(substr(r.built_year_month, 1, 4) AS REAL)", "minimumBuiltYear", ">=");
            AddEnumFilter("l.floor_plan_type", criteria.FloorPlans.Select(value => value.ToString()), "floorPlan");
            AddNumberFilter(criteria.MinimumFloorArea, "CAST(REPLACE(l.floor_area, ',', '') AS REAL)", "minimumFloorArea", ">=");
            AddNumberFilter(criteria.MaximumFloorArea, "CAST(REPLACE(l.floor_area, ',', '') AS REAL)", "maximumFloorArea", "<=");

            AddFlag(criteria.PetFriendly, "l.pet_friendly");
            AddFlag(criteria.TwoPersonOccupancy, "l.two_person_occupancy");
            AddFlag(criteria.NoGuarantorNeeded, "l.no_guarantor_needed");
            AddFlag(criteria.OfficeUseAllowed, "l.office_use_allowed");
            if (criteria.HandlingType is not null)
            {
                conditions.Add("l.handling_type = @handlingType");
                cmd.Parameters.AddWithValue("@handlingType", criteria.HandlingType.Key.ToString());
            }
            if (criteria.TransactionType is not null)
            {
                conditions.Add("l.transaction_type = @transactionType");
                cmd.Parameters.AddWithValue("@transactionType", criteria.TransactionType.Key.ToString());
            }

            AddCommissionFilter(criteria.MaximumLessorCommission, criteria.LessorCommissionUnit, "l.lessor_commission", "l.lessor_commission_unit", "lessorCommission");
            AddCommissionFilter(criteria.MaximumLesseeCommission, criteria.LesseeCommissionUnit, "l.lessee_commission", "l.lessee_commission_unit", "lesseeCommission");
            AddCommissionFilter(criteria.MaximumListingBrokerCommission, criteria.ListingBrokerCommissionUnit, "l.listing_broker_commission", "l.listing_broker_commission_unit", "listingBrokerCommission");
            AddCommissionFilter(criteria.MaximumTenantBrokerCommission, criteria.TenantBrokerCommissionUnit, "l.tenant_broker_commission", "l.tenant_broker_commission_unit", "tenantBrokerCommission");

            AddFlag(criteria.IsRecruiting, "l.is_recruiting");
            AddFlag(criteria.ImmediateOccupancy, "l.is_immediate_occupancy");
            AddFlag(criteria.CornerRoom, "l.is_corner_room");
            AddFlag(criteria.HasAutolock, "r.has_autolock");
            AddFlag(criteria.HasElevator, "r.has_elevator");
            AddFlag(criteria.HasParcelLocker, "r.has_parcel_locker");
            AddFlag(criteria.HasFreeInternet, "r.internet_free");
            AddFlag(criteria.HasAirConditioner, "l.has_air_conditioner");
            AddFlag(criteria.HasSeparateBathAndToilet, "l.has_separate_bath_and_toilet");
            AddFlag(criteria.HasParking, "r.has_parking");

            void AddCommissionFilter(double maximum, Models.Rent.Residentials.ListingCommissionUnitTypeLabel? unit, string amountColumn, string unitColumn, string parameterName)
            {
                if (unit is not null)
                {
                    conditions.Add(unitColumn + " = @" + parameterName + "Unit");
                    cmd.Parameters.AddWithValue("@" + parameterName + "Unit", unit.Key.ToString());
                }
                if (double.IsFinite(maximum))
                {
                    conditions.Add("CAST(REPLACE(" + amountColumn + ", ',', '') AS REAL) <= @" + parameterName);
                    cmd.Parameters.AddWithValue("@" + parameterName, maximum);
                }
            }

            cmd.CommandText = "SELECT p.name AS propertyName, l.name AS roomName, l.listing_id AS roomId, l.property_id AS propertyId " +
                "FROM rent_residential_listings AS l LEFT JOIN properties AS p USING (property_id) LEFT JOIN rent_residentials AS r USING (property_id)" +
                (conditions.Count == 0 ? string.Empty : " WHERE " + string.Join(" AND ", conditions)) +
                " ORDER BY p.name, l.name";

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                var eid = Convert.ToString(reader["propertyId"], CultureInfo.InvariantCulture);
                if (string.IsNullOrEmpty(eid))
                {
                    Debug.WriteLine("DataAccess::SelectRentResidentialListings: propertyId is null or empty for a rent residential.");
                    continue;
                }

                var rid = Convert.ToString(reader["roomId"], CultureInfo.InvariantCulture);
                if (string.IsNullOrEmpty(rid))
                {
                    Debug.WriteLine("DataAccess::SelectRentResidentialListings: roomId is null or empty for a rent residential room.");
                    continue;
                }

                var unit = new Models.SearchResult.ListingSearchResultItem(rid, eid, PropertyContextType.RentResidential);
                unit.SetName(Convert.ToString(reader["roomName"], CultureInfo.InvariantCulture) ?? string.Empty);

                var propertyName = Convert.ToString(reader["propertyName"], CultureInfo.InvariantCulture);
                if (!string.IsNullOrEmpty(propertyName))
                {
                    unit.PropertyName = propertyName;
                }

                unit.SetIsModified(false);
                res.ListingSearchResult.Add(unit);
            }
        }
        catch (Exception ex)
        {
            SetDatabaseError(res, ex, "connection.Open(), reader.Read()", "Failed to connect to or read a SQLite database file", nameof(SelectRentResidentialListings));
        }
        finally
        {
            _readerWriterLock.ExitReadLock();
        }

        return res;
    }

    private static Models.Rent.Residentials.Listing? GetRentResidentialListing(SqliteDataReader reader, string propertyId, string propertyName)
    {
        var listingId = reader.GetString(reader.GetOrdinal("listing_id")) ?? string.Empty;
        if (string.IsNullOrEmpty(listingId))
        {
            Debug.WriteLine("DataAccess::GetRentResidentialListing: listing_id is null or empty.");
            return null;
        }

        var pId = reader.GetString(reader.GetOrdinal("property_id")) ?? string.Empty;
        if (!propertyId.Equals(pId, StringComparison.Ordinal))
        {
            Debug.WriteLine("DataAccess::GetRentResidentialListing: property_id is not Equals to given param.");
            return null;
        }

        var isUnitOwnership = Convert.ToInt32(reader["is_property_unit_ownership"], CultureInfo.InvariantCulture) != 0;
        // Create listing.
        var room = new Models.Rent.Residentials.Listing(listingId, EntityStatus.Saved, propertyId, EntityStatus.Saved, isUnitOwnership, propertyName);
        // try
        room.SetName(reader.GetString(reader.GetOrdinal("name")) ?? string.Empty);
        room.SetFloorPlanTypeLabelFromString(Convert.ToString(reader["floor_plan_type"], CultureInfo.InvariantCulture) ?? string.Empty);
        room.SetFloorPlanExtraTypeLabelFromString(Convert.ToString(reader["floor_plan_extra_type"], CultureInfo.InvariantCulture) ?? string.Empty);
        room.SetFloorGroundTypeLabelFromString(Convert.ToString(reader["floor_type"], CultureInfo.InvariantCulture) ?? string.Empty);
        room.SetFloorExclusiveAreaSqm(Convert.ToString(reader["floor_area"], CultureInfo.InvariantCulture) ?? string.Empty);
        room.SetFloorNumber(Convert.ToString(reader["floor_number"], CultureInfo.InvariantCulture) ?? string.Empty);
        room.SetIsCornerRoom(Convert.ToInt32(reader["is_corner_room"], CultureInfo.InvariantCulture) != 0);
        room.SetExposureDirectionTypeLabelFromString(Convert.ToString(reader["main_exposure_direction"], CultureInfo.InvariantCulture) ?? string.Empty);
        room.SetAvailabilityStatusTypeLabelFromString(Convert.ToString(reader["availability_status"], CultureInfo.InvariantCulture) ?? string.Empty);
        room.SetOccupancyStatusLabelFromString(Convert.ToString(reader["current_status"], CultureInfo.InvariantCulture) ?? string.Empty);
        room.SetIsAvailableForRentNow(Convert.ToInt32(reader["is_recruiting"], CultureInfo.InvariantCulture) != 0);
        room.SetAvailabilityMonthTypeLabelFromString(Convert.ToString(reader["available_from_month"], CultureInfo.InvariantCulture) ?? string.Empty);
        room.SetAvailabilityPeriodTypeLabelFromString(Convert.ToString(reader["available_from_period"], CultureInfo.InvariantCulture) ?? string.Empty);
        room.SetIsAvailableForImmediateMoveIn(Convert.ToInt32(reader["is_immediate_occupancy"], CultureInfo.InvariantCulture) != 0);
        room.SetOccupancyStatusCheckedAtFromString(Convert.ToString(reader["current_status_checked_at"], CultureInfo.InvariantCulture) ?? string.Empty);

        room.SetRemarks(Convert.ToString(reader["remarks"], CultureInfo.InvariantCulture) ?? string.Empty);
        room.SetChinryouFromString(Convert.ToString(reader["chinryou"], CultureInfo.InvariantCulture) ?? string.Empty);

        room.SetManagementFee(Convert.ToString(reader["management_fee"], CultureInfo.InvariantCulture) ?? string.Empty);
        room.SetManagementFeeUnitTypeLabelFromString(Convert.ToString(reader["management_fee_unit"], CultureInfo.InvariantCulture) ?? string.Empty);
        room.SetSecurityDeposit(Convert.ToString(reader["security_deposit"], CultureInfo.InvariantCulture) ?? string.Empty);
        room.SetSecurityDepositUnitTypeLabelFromString(Convert.ToString(reader["security_deposit_unit"], CultureInfo.InvariantCulture) ?? string.Empty);
        room.SetKeyMoney(Convert.ToString(reader["key_money"], CultureInfo.InvariantCulture) ?? string.Empty);
        room.SetKeyMoneyUnitTypeLabelFromString(Convert.ToString(reader["key_money_unit"], CultureInfo.InvariantCulture) ?? string.Empty);
        room.SetContractTypeLabelFromString(Convert.ToString(reader["contract_type"], CultureInfo.InvariantCulture) ?? string.Empty);
        room.SetContractPeriodYears(Convert.ToString(reader["contract_period_years"], CultureInfo.InvariantCulture) ?? string.Empty);
        room.SetRenewalFee(Convert.ToString(reader["renewal_fee"], CultureInfo.InvariantCulture) ?? string.Empty);
        room.SetRenewalFeeUnitTypeLabelFromString(Convert.ToString(reader["renewal_fee_unit"], CultureInfo.InvariantCulture) ?? string.Empty);
        room.SetRecontractFee(Convert.ToString(reader["recontract_fee"], CultureInfo.InvariantCulture) ?? string.Empty);
        room.SetRecontractFeeUnitTypeLabelFromString(Convert.ToString(reader["recontract_fee_unit"], CultureInfo.InvariantCulture) ?? string.Empty);
        room.SetGuarantorCompany(Convert.ToString(reader["guarantor_company"], CultureInfo.InvariantCulture) ?? string.Empty);
        room.SetHousingInsurance(Convert.ToString(reader["housing_insurance"], CultureInfo.InvariantCulture) ?? string.Empty);
        room.SetOtherFees(Convert.ToString(reader["other_fees"], CultureInfo.InvariantCulture) ?? string.Empty);
        room.SetPetFriendly(Convert.ToInt32(reader["pet_friendly"], CultureInfo.InvariantCulture) != 0);
        room.SetTwoPersonOccupancy(Convert.ToInt32(reader["two_person_occupancy"], CultureInfo.InvariantCulture) != 0);
        room.SetNoGuarantorNeeded(Convert.ToInt32(reader["no_guarantor_needed"], CultureInfo.InvariantCulture) != 0);
        room.SetOfficeUseAllowed(Convert.ToInt32(reader["office_use_allowed"], CultureInfo.InvariantCulture) != 0);
        room.SetOtherConditions(Convert.ToString(reader["other_conditions"], CultureInfo.InvariantCulture) ?? string.Empty);
        room.SetContractRemarks(Convert.ToString(reader["contract_remarks"], CultureInfo.InvariantCulture) ?? string.Empty);
        room.SetHandlingTypeLabelFromString(reader.GetString(reader.GetOrdinal("handling_type")));
        room.SetTransactionTypeLabelFromString(reader.GetString(reader.GetOrdinal("transaction_type")));
        room.SetLessorCommission(reader.GetString(reader.GetOrdinal("lessor_commission")));
        room.SetLessorCommissionUnitTypeLabelFromString(reader.GetString(reader.GetOrdinal("lessor_commission_unit")));
        room.SetLesseeCommission(reader.GetString(reader.GetOrdinal("lessee_commission")));
        room.SetLesseeCommissionUnitTypeLabelFromString(reader.GetString(reader.GetOrdinal("lessee_commission_unit")));
        room.SetListingBrokerCommission(reader.GetString(reader.GetOrdinal("listing_broker_commission")));
        room.SetListingBrokerCommissionUnitTypeLabelFromString(reader.GetString(reader.GetOrdinal("listing_broker_commission_unit")));
        room.SetTenantBrokerCommission(reader.GetString(reader.GetOrdinal("tenant_broker_commission")));
        room.SetTenantBrokerCommissionUnitTypeLabelFromString(reader.GetString(reader.GetOrdinal("tenant_broker_commission_unit")));
        room.SetTransactionRemarks(reader.GetString(reader.GetOrdinal("transaction_remarks")));
        room.SetHasAirConditioner(Convert.ToInt32(reader["has_air_conditioner"], CultureInfo.InvariantCulture) != 0);
        room.SetHasWoodFlooring(Convert.ToInt32(reader["has_wood_flooring"], CultureInfo.InvariantCulture) != 0);
        room.SetHasVideoIntercom(Convert.ToInt32(reader["has_video_intercom"], CultureInfo.InvariantCulture) != 0);
        room.SetHasBathReheating(Convert.ToInt32(reader["has_bath_reheating"], CultureInfo.InvariantCulture) != 0);
        room.SetHasGasStove(Convert.ToInt32(reader["has_gas_stove"], CultureInfo.InvariantCulture) != 0);
        room.SetHasInductionCooktop(Convert.ToInt32(reader["has_induction_cooktop"], CultureInfo.InvariantCulture) != 0);
        room.SetHasMultipleBurners(Convert.ToInt32(reader["has_multiple_burners"], CultureInfo.InvariantCulture) != 0);
        room.SetHasSystemKitchen(Convert.ToInt32(reader["has_system_kitchen"], CultureInfo.InvariantCulture) != 0);
        room.SetHasLoft(Convert.ToInt32(reader["has_loft"], CultureInfo.InvariantCulture) != 0);
        room.SetHasIndoorWashingMachineSpace(Convert.ToInt32(reader["has_indoor_washing_machine_space"], CultureInfo.InvariantCulture) != 0);
        room.SetHasBathroomDryer(Convert.ToInt32(reader["has_bathroom_dryer"], CultureInfo.InvariantCulture) != 0);
        room.SetHasSeparateBathAndToilet(Convert.ToInt32(reader["has_separate_bath_and_toilet"], CultureInfo.InvariantCulture) != 0);
        room.SetHasSeparateWashbasin(Convert.ToInt32(reader["has_separate_washbasin"], CultureInfo.InvariantCulture) != 0);
        room.SetHasWarmWaterToiletSeat(Convert.ToInt32(reader["has_warm_water_toilet_seat"], CultureInfo.InvariantCulture) != 0);
        room.SetApplianceRemarks(reader.GetString(reader.GetOrdinal("appliance_remarks")));

        room.SetIsModified(false);

        //Debug.WriteLine($"Room ID: {room.Id}, Room Name: {room.RoomName}");

        return room;
    }

    private static void SetRentResidentialListingChildValues(SqliteCommand cmd, Models.Rent.Residentials.Listing room)
    {
        // 部屋写真
        cmd.CommandText = "SELECT * FROM rent_residential_listing_pictures WHERE listing_id = @listingId";
        cmd.Parameters.Clear();
        cmd.Parameters.AddWithValue("@listingId", room.Id);
        using (var reader = cmd.ExecuteReader())
        {
            while (reader.Read())
            {
                var picid = Convert.ToString(reader["picture_id"], CultureInfo.InvariantCulture) ?? string.Empty;
                var picpath = Convert.ToString(reader["filename"], CultureInfo.InvariantCulture) ?? string.Empty;
                if (!string.IsNullOrEmpty(picid) && !string.IsNullOrEmpty(picpath))
                {
                    var rlpic = new Models.Rent.Residentials.ListingPicture(picid, picpath, EntityStatus.Saved)
                    {
                        Description = Convert.ToString(reader["description"], CultureInfo.InvariantCulture) ?? string.Empty
                    };

                    rlpic.SetIsModified(false);

                    var strType = Convert.ToString(reader["type"], CultureInfo.InvariantCulture) ?? string.Empty;
                    rlpic.SetLabelFromString(strType);

                    rlpic.IsMain = Convert.ToInt32(reader["is_main"], CultureInfo.InvariantCulture) != 0; // int to bool

                    room.Pictures.Add(rlpic);
                }
                else
                {
                    Debug.WriteLine("picture_id or filename is null/empty.");
                }
            }
        }

        // 部屋PDF
        cmd.CommandText = "SELECT * FROM rent_residential_listing_pdfs WHERE listing_id = @listingId";
        cmd.Parameters.Clear();
        cmd.Parameters.AddWithValue("@listingId", room.Id);
        using (var reader = cmd.ExecuteReader())
        {
            while (reader.Read())
            {
                var pdfid = Convert.ToString(reader["pdf_id"], CultureInfo.InvariantCulture) ?? string.Empty;
                var pdfpath = Convert.ToString(reader["filename"], CultureInfo.InvariantCulture) ?? string.Empty;
                var thumbpath = Convert.ToString(reader["thumbnail_filename"], CultureInfo.InvariantCulture) ?? string.Empty;
                if (!string.IsNullOrEmpty(pdfid) && !string.IsNullOrEmpty(pdfpath) && !string.IsNullOrEmpty(thumbpath))
                {
                    var rlpdf = new Models.Rent.Residentials.ListingPdf(pdfid, pdfpath, thumbpath, EntityStatus.Saved)
                    {
                        Description = Convert.ToString(reader["description"], CultureInfo.InvariantCulture) ?? string.Empty
                    };

                    rlpdf.SetIsModified(false);

                    var strType = Convert.ToString(reader["type"], CultureInfo.InvariantCulture) ?? string.Empty;
                    rlpdf.SetTypeFromString(strType);

                    rlpdf.IsMain = Convert.ToInt32(reader["is_main"], CultureInfo.InvariantCulture) != 0; // int to bool

                    room.Pdfs.Add(rlpdf);
                }
                else
                {
                    Debug.WriteLine("pdf_id or filename or thumbnail_filename is null/empty.");
                }
            }
        }

        // 部屋貸主
        var lessorIds = new List<string>();
        cmd.CommandText = "SELECT * FROM rent_lessors_properties_listings WHERE listing_id = @listingId";
        cmd.Parameters.Clear();
        cmd.Parameters.AddWithValue("@listingId", room.Id);
        using (var reader = cmd.ExecuteReader())
        {
            while (reader.Read())
            {
                var lessorId = Convert.ToString(reader["lessor_id"], CultureInfo.InvariantCulture) ?? string.Empty;
                if (!string.IsNullOrEmpty(lessorId))
                {
                    lessorIds.Add(lessorId);
                }
                else
                {
                    Debug.WriteLine("lessor_id is null/empty.");
                }
            }
        }
        if (lessorIds.Count > 0)
        {
            foreach (var lessId in lessorIds)
            {
                // Get actuall lessors
                cmd.CommandText = "SELECT lessor_id, name, name_last, name_first, remarks FROM rent_lessors WHERE lessor_id = @lessorId";
                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("@lessorId", lessId);
#pragma warning disable IDE0063 // Use simple 'using' statement
                using (var reader2 = cmd.ExecuteReader())
                {
                    while (reader2.Read())
                    {
                        var s = Convert.ToString(reader2["lessor_id"], CultureInfo.InvariantCulture);
                        if (string.IsNullOrEmpty(s))
                        {
                            Debug.WriteLine("DataAccess::SelectRentResidentialById: lessor_id is null or empty.");
                            continue;
                        }

                        // TODO: IF natural
                        //var lessor = new Models.Rent.Lessors.Person(lessId, EnumEntityStatus.Saved);
                        var lessor = new Models.Person.NaturalPerson(lessId, EntityStatus.Saved)
                        {
                            NameLast = Convert.ToString(reader2["name_last"], CultureInfo.InvariantCulture) ?? "",
                            NameFirst = Convert.ToString(reader2["name_first"], CultureInfo.InvariantCulture) ?? "",
                            Remarks = Convert.ToString(reader2["remarks"], CultureInfo.InvariantCulture) ?? ""
                        };
                        // Try
                        lessor.SetName(Convert.ToString(reader2["name"], CultureInfo.InvariantCulture) ?? "");

                        // TODO: more.

                        room.Lessors.Add(lessor);

                        //break; // Assuming we only want the first match
                    }
                }
#pragma warning restore IDE0063 // Use simple 'using' statement
            }
        }

        // 宅建業者（部屋）
        cmd.CommandText = """
            SELECT b.broker_id, b.name, b.person_kind, b.name_last, b.name_first, b.name_company, b.name_company_type, b.name_company_type_position, b.remarks
            FROM brokers_properties_listings AS association
            INNER JOIN brokers AS b ON b.broker_id = association.broker_id
            WHERE association.property_id = @propertyId AND association.listing_id = @listingId;
            """;
        cmd.Parameters.Clear();
        cmd.Parameters.AddWithValue("@propertyId", room.PropertyId);
        cmd.Parameters.AddWithValue("@listingId", room.Id);

        using (var reader = cmd.ExecuteReader())
        {
            while (reader.Read())
            {
                var brokerId = Convert.ToString(reader["broker_id"], CultureInfo.InvariantCulture);
                if (string.IsNullOrWhiteSpace(brokerId))
                {
                    continue;
                }

                var broker = GetPerson(reader, brokerId);
                if (broker is not null)
                {
                    room.Brokers.Add(broker);
                }
            }
        }

        cmd.Parameters.Clear();
    }

    public Models.Rent.Residentials.ListingResultWrapper SelectRentResidentialListingById(string propertyId, string roomId)
    {
        var res = new Models.Rent.Residentials.ListingResultWrapper();

        if (string.IsNullOrEmpty(roomId))
        {
            res.IsError = true;
            // TODO:
            return res;
        }

        if (string.IsNullOrEmpty(propertyId))
        {
            res.IsError = true;
            // TODO:
            return res;
        }

        _readerWriterLock.EnterReadLock();
        try
        {
            using var connection = new SqliteConnection(_connectionStringBuilder.ConnectionString);
            connection.Open();

            using var cmd = connection.CreateCommand();

            var buildingName = string.Empty;
            var isFound = false;

            cmd.CommandText = string.Format(CultureInfo.InvariantCulture, "SELECT name FROM properties WHERE property_id = '{0}'", propertyId);
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    buildingName = reader.GetString(reader.GetOrdinal("name")) ?? string.Empty;
                    if (!string.IsNullOrEmpty(buildingName))
                    {
                        isFound = true;
                    }
                }
            }

            if (isFound)
            {
                cmd.CommandText = string.Format(CultureInfo.InvariantCulture, "SELECT * FROM rent_residential_listings WHERE listing_id = '{0}'", roomId);
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        res.Room = GetRentResidentialListing(reader, propertyId, buildingName);
                    }
                }

                if (res.Room is not null)
                {
                    SetRentResidentialListingChildValues(cmd, res.Room);
                }
            }
        }
        catch (Exception ex)
        {
            SetDatabaseError(res, ex, "connection.Open(), reader.Read()", "Failed to connect to or read a SQLite database file", nameof(SelectRentResidentialListingById));
        }
        finally
        {
            _readerWriterLock.ExitReadLock();
        }

        return res;
    }

    public ResultWrapper DeleteRentResidentialListing(string roomId)
    {
        var res = new ResultWrapper();

        if (string.IsNullOrEmpty(roomId))
        {
            res.IsError = true;
            // TODO:
            return res;
        }

        _readerWriterLock.EnterWriteLock();
        try
        {
            // System.Data.SQLite
            //using var connection = new SQLiteConnection(_connectionStringBuilder.ConnectionString);
            // Microsoft.Data.Sqlite
            using var connection = new SqliteConnection(_connectionStringBuilder.ConnectionString);
            connection.Open();

            using var cmd = connection.CreateCommand();

            cmd.Transaction = connection.BeginTransaction();
            try
            {
                cmd.CommandText = string.Format(CultureInfo.InvariantCulture, "DELETE FROM rent_residential_listings WHERE listing_id = '{0}';", roomId);
                res.AffectedCount = cmd.ExecuteNonQuery();

                cmd.Transaction.Commit();
            }
            catch (Exception ex)
            {
                cmd.Transaction.Rollback();

                SetDatabaseError(res, ex, "cmd.ExecuteNonQuery(), cmd.Transaction.Commit", "Failed to delete database record. Transaction.Rollback()", nameof(DeleteRentResidentialListing));
                return res;
            }
        }
        catch (Exception ex)
        {
            SetDatabaseError(res, ex, "connection.Open(), reader.Read()", "Failed to connect to a SQLite database file", nameof(DeleteRentResidentialListing));
            return res;
        }
        finally
        {
            _readerWriterLock.ExitWriteLock();
        }

        //Debug.WriteLine(string.Format("{0} feed Deleted from DB", res.AffectedCount));

        return res;
    }

    #endregion

    #region == Rent Commercial ==

    public ResultWrapper UpsertRentCommercial(Models.Rent.Commercials.Property building)
    {
        var result = new ResultWrapper();

        if (string.IsNullOrWhiteSpace(building.Id))
        {
            result.IsError = true;
            result.Error.Description = "Commercial property ID is empty.";
            // TODO:
            return result;
        }

        _readerWriterLock.EnterWriteLock();

        try
        {
            using var connection =
                new SqliteConnection(_connectionStringBuilder.ConnectionString);

            connection.Open();

            // TODO: try catch Transaction.Rollback()

            using var transaction = connection.BeginTransaction();
            using var command = connection.CreateCommand();

            command.Transaction = transaction;

            command.CommandText = """
            INSERT INTO properties (
                property_id,
                name,
                property_kind,
                thumbnail_filename,
                location_prefecture_code,
                location_prefecture,
                location_machiaza_id,
                location_county,
                location_city,
                location_ward,
                location_oaza_cho,
                location_choume,
                location_edaban,
                location_full,
                location_latitude,
                location_longitude,
                updated_at
            )
            VALUES (
                @propertyId,
                @name,
                @propertyKind,
                @thumbnailFilename,
                @locPrefId,
                @locPrefecture,
                @locMachiazaId,
                @locCounty,
                @locCity,
                @locWard,
                @locOazaCho,
                @locChoume,
                @locEdaban,
                @locLocationFull,
                @locationLatitude,
                @locationLongitude,
                @updatedAt
            )
            ON CONFLICT(property_id) DO UPDATE SET
                name = excluded.name,
                property_kind = excluded.property_kind,
                thumbnail_filename = excluded.thumbnail_filename,
                location_prefecture_code = excluded.location_prefecture_code,
                location_prefecture = excluded.location_prefecture,
                location_machiaza_id = excluded.location_machiaza_id,
                location_county = excluded.location_county,
                location_city = excluded.location_city,
                location_ward = excluded.location_ward,
                location_oaza_cho = excluded.location_oaza_cho,
                location_choume = excluded.location_choume,
                location_edaban = excluded.location_edaban,
                location_full = excluded.location_full,
                location_latitude = excluded.location_latitude,
                location_longitude = excluded.location_longitude,
                updated_at = excluded.updated_at;
            """;

            command.Parameters.AddWithValue("@propertyId", building.Id);
            command.Parameters.AddWithValue("@name", building.Name);
            command.Parameters.AddWithValue("@propertyKind",PropertyContextType.RentCommercial.ToString());
            command.Parameters.AddWithValue("@thumbnailFilename",building.ThumbnailFilename);

            // Address/Location
            command.Parameters.AddWithValue("@locPrefId", building.Address.Prefecture?.MunicipalityCode ?? string.Empty);
            command.Parameters.AddWithValue("@locPrefecture", building.Address.Prefecture?.Name ?? string.Empty);
            command.Parameters.AddWithValue("@locMachiazaId", building.Address.MachiazaId ?? string.Empty);
            command.Parameters.AddWithValue("@locCounty", building.Address.CountyAndCity?.County ?? string.Empty);
            command.Parameters.AddWithValue("@locCity", building.Address.CountyAndCity?.City ?? string.Empty);
            command.Parameters.AddWithValue("@locWard", building.Address.WardAndOaza?.Ward ?? string.Empty);
            command.Parameters.AddWithValue("@locOazaCho", building.Address.WardAndOaza?.Oaza ?? string.Empty);
            command.Parameters.AddWithValue("@locChoume", building.Address.Choume?.Chou ?? string.Empty);
            command.Parameters.AddWithValue("@locEdaban", building.Address.Edaban ?? string.Empty);
            command.Parameters.AddWithValue("@locLocationFull", building.Address.AddressFull ?? string.Empty);
            command.Parameters.AddWithValue("@locationLatitude", building.Address.LocationLatitude);
            command.Parameters.AddWithValue("@locationLongitude", building.Address.LocationLongitude);

            command.Parameters.AddWithValue("@updatedAt",DateTimeOffset.UtcNow.ToString("s"));

            command.ExecuteNonQuery();

            command.Parameters.Clear();

            command.CommandText = """
            INSERT INTO rent_commercials (
                property_id,
                commercial_kind,
                is_unit_ownership,
                property_structure,
                floor_count_above_ground,
                floor_count_basement,
                total_floor_area,
                built_year_month,
                fudousan_id,
                fudousan_id_additional_code,
                remarks,
                updated_at
            )
            VALUES (
                @propertyId,
                @commercialKind,
                @isUnitOwnership,
                @buildingStructure,
                @floorCountAboveGround,
                @floorCountBasement,
                @totalFloorArea,
                @builtYearMonth,
                @fudousanId,
                @fudousanIdAdditionalCode,
                @remarks,
                @updatedAt
            )
            ON CONFLICT(property_id) DO UPDATE SET
                commercial_kind = excluded.commercial_kind,
                is_unit_ownership = excluded.is_unit_ownership,
                property_structure = excluded.property_structure,
                floor_count_above_ground =
                    excluded.floor_count_above_ground,
                floor_count_basement =
                    excluded.floor_count_basement,
                total_floor_area = excluded.total_floor_area,
                built_year_month = excluded.built_year_month,
                fudousan_id = excluded.fudousan_id,
                fudousan_id_additional_code =
                    excluded.fudousan_id_additional_code,
                remarks = excluded.remarks,
                updated_at = excluded.updated_at;
            """;

            command.Parameters.AddWithValue("@propertyId", building.Id);
            command.Parameters.AddWithValue(
                "@commercialKind",
                building.CommercialKind.Key.ToString());
            command.Parameters.AddWithValue(
                "@isUnitOwnership",
                building.IsUnitOwnership ? 1 : 0);
            command.Parameters.AddWithValue(
                "@buildingStructure",
                building.PropertyStructure.Key.ToString());
            command.Parameters.AddWithValue(
                "@floorCountAboveGround",
                building.FloorCountAboveGround);
            command.Parameters.AddWithValue(
                "@floorCountBasement",
                building.FloorCountBasement);
            command.Parameters.AddWithValue(
                "@totalFloorArea",
                building.TotalFloorArea);
            command.Parameters.AddWithValue(
                "@builtYearMonth",
                building.BuiltYearAndMonth.ToString("s"));
            command.Parameters.AddWithValue(
                "@fudousanId",
                building.FudousanId);
            command.Parameters.AddWithValue(
                "@fudousanIdAdditionalCode",
                building.FudousanIdAdditionalCode);
            command.Parameters.AddWithValue("@remarks", building.Remarks);
            command.Parameters.AddWithValue(
                "@updatedAt",
                DateTimeOffset.UtcNow.ToString("s"));

            result.AffectedCount = command.ExecuteNonQuery();

            SaveRentCommercialMedia(command, building);

            // lessor
            command.Parameters.Clear();

            foreach (var person in building.Lessors)
            {
                command.CommandText = """
        INSERT INTO rent_lessors_properties_listings
            (lessor_id, property_id, property_kind, listing_id)
        VALUES
            (@lessorId, @propertyId, @propertyKind, @listingId)
        ON CONFLICT (lessor_id, property_id, listing_id)
        DO NOTHING;
        """;

                command.Parameters.Clear();
                command.Parameters.AddWithValue("@lessorId", person.Id);
                command.Parameters.AddWithValue("@propertyId", building.Id);
                command.Parameters.AddWithValue(
                    "@propertyKind",
                    PropertyContextType.RentCommercial.ToString());
                command.Parameters.AddWithValue("@listingId", string.Empty);

                command.ExecuteNonQuery();
            }

            foreach (var person in building.LessorsToBeDeleted)
            {
                command.CommandText = """
        DELETE FROM rent_lessors_properties_listings
        WHERE lessor_id = @lessorId
          AND property_id = @propertyId
          AND property_kind = @propertyKind
          AND listing_id = @listingId;
        """;

                command.Parameters.Clear();
                command.Parameters.AddWithValue("@lessorId", person.Id);
                command.Parameters.AddWithValue("@propertyId", building.Id);
                command.Parameters.AddWithValue(
                    "@propertyKind",
                    PropertyContextType.RentCommercial.ToString());
                command.Parameters.AddWithValue("@listingId", string.Empty);

                command.ExecuteNonQuery();
            }


            //
            command.Parameters.Clear();

            foreach (var person in building.Brokers)
            {
                command.CommandText = """
        INSERT INTO brokers_properties_listings
            (broker_id, property_id, property_kind, listing_id)
        VALUES
            (@brokerId, @propertyId, @propertyKind, @listingId)
        ON CONFLICT (broker_id, property_id, listing_id)
        DO NOTHING;
        """;

                command.Parameters.Clear();
                command.Parameters.AddWithValue("@brokerId", person.Id);
                command.Parameters.AddWithValue("@propertyId", building.Id);
                command.Parameters.AddWithValue(
                    "@propertyKind",
                    PropertyContextType.RentCommercial.ToString());
                command.Parameters.AddWithValue("@listingId", string.Empty);

                command.ExecuteNonQuery();
            }

            foreach (var person in building.BrokersToBeDeleted)
            {
                command.CommandText = """
        DELETE FROM brokers_properties_listings
        WHERE broker_id = @brokerId
          AND property_id = @propertyId
          AND property_kind = @propertyKind
          AND listing_id = @listingId;
        """;

                command.Parameters.Clear();
                command.Parameters.AddWithValue("@brokerId", person.Id);
                command.Parameters.AddWithValue("@propertyId", building.Id);
                command.Parameters.AddWithValue(
                    "@propertyKind",
                    PropertyContextType.RentCommercial.ToString());
                command.Parameters.AddWithValue("@listingId", string.Empty);

                command.ExecuteNonQuery();
            }

            // Units
            foreach (var unit in building.Units)
            {
                command.CommandText = """
        INSERT INTO rent_commercial_units (
            listing_id,
            property_id,
            is_property_unit_ownership,
            name,
            chinryou,
            kyoueki_fee,
            shikikin,
            shikikin_unit,
            reikin,
            reikin_unit,
            renewal_fee,
            renewal_fee_unit,
            recontract_fee,
            recontract_fee_unit,
            floor_area,
            usage,
            business_hours,
            parking_available,
            other_conditions,
            remarks,
            updated_at
        )
        VALUES (
            @listingId,
            @propertyId,
            @isUnitOwnership,
            @name,
            @chinryou,
            @kyouekiFee,
            @shikikin,
            @shikikinUnit,
            @reikin,
            @reikinUnit,
            @renewalFee,
            @renewalFeeUnit,
            @recontractFee,
            @recontractFeeUnit,
            @floorArea,
            @usage,
            @businessHours,
            @parkingAvailable,
            @otherConditions,
            @remarks,
            @updatedAt
        )
        ON CONFLICT(listing_id) DO UPDATE SET
            property_id = excluded.property_id,
            is_property_unit_ownership = excluded.is_property_unit_ownership,
            name = excluded.name,
            chinryou = excluded.chinryou,
            kyoueki_fee = excluded.kyoueki_fee,
            shikikin = excluded.shikikin,
            shikikin_unit = excluded.shikikin_unit,
            reikin = excluded.reikin,
            reikin_unit = excluded.reikin_unit,
            renewal_fee = excluded.renewal_fee,
            renewal_fee_unit = excluded.renewal_fee_unit,
            recontract_fee = excluded.recontract_fee,
            recontract_fee_unit = excluded.recontract_fee_unit,
            floor_area = excluded.floor_area,
            usage = excluded.usage,
            business_hours = excluded.business_hours,
            parking_available = excluded.parking_available,
            other_conditions = excluded.other_conditions,
            remarks = excluded.remarks,
            updated_at = excluded.updated_at;
        """;

                command.Parameters.Clear();
                command.Parameters.AddWithValue("@listingId", unit.Id);
                command.Parameters.AddWithValue("@propertyId", building.Id);
                command.Parameters.AddWithValue("@isUnitOwnership", unit.IsPropertyUnitOwnership ? 1 : 0);
                command.Parameters.AddWithValue("@name", unit.Name);
                command.Parameters.AddWithValue("@chinryou", unit.Chinryou);
                /*
                command.Parameters.AddWithValue("@kyouekiFee", unit.KyouekiFee);
                command.Parameters.AddWithValue("@shikikin", unit.Shikikin);
                command.Parameters.AddWithValue("@shikikinUnit", unit.ShikikinUnit);
                command.Parameters.AddWithValue("@reikin", unit.Reikin);
                command.Parameters.AddWithValue("@reikinUnit", unit.ReikinUnit);
                command.Parameters.AddWithValue("@renewalFee", unit.RenewalFee);
                command.Parameters.AddWithValue("@renewalFeeUnit", unit.RenewalFeeUnit);
                command.Parameters.AddWithValue("@recontractFee", unit.RecontractFee);
                command.Parameters.AddWithValue("@recontractFeeUnit", unit.RecontractFeeUnit);
                command.Parameters.AddWithValue("@floorArea", unit.FloorArea);
                command.Parameters.AddWithValue("@usage", unit.Usage);
                command.Parameters.AddWithValue("@businessHours", unit.BusinessHours);
                command.Parameters.AddWithValue("@parkingAvailable", unit.ParkingAvailable ? 1 : 0);
                command.Parameters.AddWithValue("@otherConditions", unit.OtherConditions);
                */
                command.Parameters.AddWithValue("@remarks", unit.Remarks);
                command.Parameters.AddWithValue(
                    "@updatedAt",
                    DateTimeOffset.UtcNow.ToString("s"));

                command.ExecuteNonQuery();
            }

            // process queued unit deletions
            foreach (var unit in building.UnitsToBeDeleted)
            {
                command.CommandText = """
        DELETE FROM rent_commercial_units
        WHERE property_id = @propertyId AND listing_id = @listingId;
        """;
                command.Parameters.Clear();
                command.Parameters.AddWithValue("@propertyId", building.Id);
                command.Parameters.AddWithValue("@listingId", unit.Id);
                command.ExecuteNonQuery();
            }


            // Commit transaction
            transaction.Commit();

            // TODO: Check if this is necessary or already done in Save() in viewmodel.
            foreach (var picture in building.Pictures)
            {
                //picture.IsNew = false;
                picture.SetStatus(EntityStatus.Saved);
                picture.SetIsModified(false);
            }

            foreach (var pdf in building.Pdfs)
            {
                //pdf.IsNew = false;
                pdf.SetStatus(EntityStatus.Saved);
                pdf.SetIsModified(false);
            }

            building.SetStatus(EntityStatus.Saved);
            building.SetIsModified(false);

        }
        catch (Exception ex)
        {
            SetDatabaseError(result, ex, "connection.Open(), cmd.ExecuteNonQuery(), cmd.Transaction.Commit", "Failed to update database tables. Transaction.Rollback()", nameof(UpsertRentCommercial));
        }
        finally
        {
            _readerWriterLock.ExitWriteLock();
        }

        return result;
    }

    private static void SaveRentCommercialMedia(SqliteCommand command, Models.Rent.Commercials.Property building)
    {
        foreach (var picture in building.Pictures)
        {
            command.CommandText = """
            INSERT INTO rent_commercial_pictures
                (picture_id, property_id, filename, type, description, is_main)
            VALUES
                (@id, @propertyId, @filename, @type, @description, @isMain)
            ON CONFLICT(picture_id) DO UPDATE SET
                property_id = excluded.property_id,
                filename = excluded.filename,
                type = excluded.type,
                description = excluded.description,
                is_main = excluded.is_main;
            """;

            command.Parameters.Clear();
            command.Parameters.AddWithValue("@id", picture.Id);
            command.Parameters.AddWithValue("@propertyId", building.Id);
            command.Parameters.AddWithValue("@filename", picture.ImageFilename);
            command.Parameters.AddWithValue(
                "@type",
                picture.PictureType.Key.ToString());
            command.Parameters.AddWithValue(
                "@description",
                picture.Description);
            command.Parameters.AddWithValue(
                "@isMain",
                picture.IsMain ? 1 : 0);
            command.ExecuteNonQuery();
        }

        foreach (var pdf in building.Pdfs)
        {
            command.CommandText = """
            INSERT INTO rent_commercial_pdfs
                (pdf_id, property_id, filename, thumbnail_filename,
                 type, description, is_main)
            VALUES
                (@id, @propertyId, @filename, @thumbnailFilename,
                 @type, @description, @isMain)
            ON CONFLICT(pdf_id) DO UPDATE SET
                property_id = excluded.property_id,
                filename = excluded.filename,
                thumbnail_filename = excluded.thumbnail_filename,
                type = excluded.type,
                description = excluded.description,
                is_main = excluded.is_main,
                updated_at = DATETIME('now', 'utc');
            """;

            command.Parameters.Clear();
            command.Parameters.AddWithValue("@id", pdf.Id);
            command.Parameters.AddWithValue("@propertyId", building.Id);
            command.Parameters.AddWithValue("@filename", pdf.PdfFilename);
            command.Parameters.AddWithValue(
                "@thumbnailFilename",
                pdf.ThumbnailFilename);
            command.Parameters.AddWithValue(
                "@type",
                pdf.PdfType.Key.ToString());
            command.Parameters.AddWithValue("@description", pdf.Description);
            command.Parameters.AddWithValue("@isMain", pdf.IsMain ? 1 : 0);
            command.ExecuteNonQuery();
        }

        foreach (var picture in building.PicturesToBeDeleted)
        {
            command.CommandText = """
            DELETE FROM rent_commercial_pictures
            WHERE picture_id = @id
              AND property_id = @propertyId;
            """;

            command.Parameters.Clear();
            command.Parameters.AddWithValue("@id", picture.Id);
            command.Parameters.AddWithValue("@propertyId", building.Id);
            command.ExecuteNonQuery();
        }

        foreach (var pdf in building.PdfsToBeDeleted)
        {
            command.CommandText = """
            DELETE FROM rent_commercial_pdfs
            WHERE pdf_id = @id
              AND property_id = @propertyId;
            """;

            command.Parameters.Clear();
            command.Parameters.AddWithValue("@id", pdf.Id);
            command.Parameters.AddWithValue("@propertyId", building.Id);
            command.ExecuteNonQuery();
        }

        command.Parameters.Clear();
    }

    /*
    public PropertiesResultWrapper SelectRentCommercialsByNameKeyword(string keyword)
    {
        var result = new PropertiesResultWrapper();

        _readerWriterLock.EnterReadLock();

        try
        {
            using var connection =
                new SqliteConnection(_connectionStringBuilder.ConnectionString);

            connection.Open();

            using var command = connection.CreateCommand();

            var searchAll = string.IsNullOrWhiteSpace(keyword) ||
                            keyword.Trim() == "*";

            command.CommandText = searchAll
                ? """
              SELECT
                  p.property_id,
                  p.name,
                  p.property_kind
              FROM rent_commercials AS c
              INNER JOIN properties AS p
                  ON p.property_id = c.property_id
              WHERE p.property_kind = @propertyKind
              ORDER BY p.name;
              """
                : """
              SELECT
                  p.property_id,
                  p.name,
                  p.property_kind
              FROM rent_commercials AS c
              INNER JOIN properties AS p
                  ON p.property_id = c.property_id
              WHERE p.property_kind = @propertyKind
                AND p.name LIKE @keyword
              ORDER BY p.name;
              """;

            command.Parameters.AddWithValue(
                "@propertyKind",
                EnumPropertyKind.RentCommercial.ToString());

            if (!searchAll)
            {
                command.Parameters.AddWithValue(
                    "@keyword",
                    $"%{keyword.Trim()}%");
            }

            using var reader = command.ExecuteReader();

            while (reader.Read())
            {
                var propertyId =
                    Convert.ToString(reader["property_id"]);

                if (string.IsNullOrWhiteSpace(propertyId))
                {
                    continue;
                }

                var item = new Models.PropertySearchResultItem(
                    propertyId,
                    EnumPropertyKind.RentCommercial)
                {
                    Name = Convert.ToString(reader["name"])
                        ?? string.Empty,
                    IsModified = false
                };

                result.PropertySearchResult.Add(item);
            }
        }
        catch (Exception ex)
        {
            SetDatabaseError(result, ex, "connection.Open(), reader.Read()", "Failed to connect to or read a SQLite database file", nameof(SelectRentCommercialsByNameKeyword));
        }
        finally
        {
            _readerWriterLock.ExitReadLock();
        }

        return result;
    }
    */
    public Models.Rent.Commercials.PropertyResultWrapper SelectRentCommercialById(string id)
    {
        var result = new Models.Rent.Commercials.PropertyResultWrapper();

        if (string.IsNullOrWhiteSpace(id))
        {
            result.IsError = true;
            result.Error.Description = "Commercial property ID is empty.";
            return result;
        }

        _readerWriterLock.EnterReadLock();

        try
        {
            using var connection =
                new SqliteConnection(_connectionStringBuilder.ConnectionString);

            connection.Open();

            using var command = connection.CreateCommand();

            command.CommandText = """
            SELECT
                p.property_id,
                p.name,
                p.thumbnail_filename,
                p.location_prefecture_code,
                p.location_prefecture,
                p.location_machiaza_id,
                p.location_county,
                p.location_city,
                p.location_ward,
                p.location_oaza_cho,
                p.location_choume,
                p.location_edaban,
                p.location_full,
                p.location_latitude,
                p.location_longitude,
                c.commercial_kind,
                c.is_unit_ownership,
                c.property_structure,
                c.floor_count_above_ground,
                c.floor_count_basement,
                c.total_floor_area,
                c.built_year_month,
                c.fudousan_id,
                c.fudousan_id_additional_code,
                c.remarks
            FROM rent_commercials AS c
            INNER JOIN properties AS p
                ON p.property_id = c.property_id
            WHERE c.property_id = @propertyId
              AND p.property_kind = @propertyKind;
            """;

            command.Parameters.AddWithValue("@propertyId", id);
            command.Parameters.AddWithValue(
                "@propertyKind",
                PropertyContextType.RentCommercial.ToString());

            using var reader = command.ExecuteReader();

            if (!reader.Read())
            {
                return result;
            }

            var building = new Models.Rent.Commercials.Property(Convert.ToString(reader["property_id"], CultureInfo.InvariantCulture)!,EntityStatus.Saved)
                {
                    ThumbnailFilename =Convert.ToString(reader["thumbnail_filename"], CultureInfo.InvariantCulture)?? string.Empty,

                    IsUnitOwnership =
                        Convert.ToInt32(reader["is_unit_ownership"], CultureInfo.InvariantCulture) != 0,
                    FloorCountAboveGround =
                        Convert.ToInt32(reader["floor_count_above_ground"], CultureInfo.InvariantCulture),
                    FloorCountBasement =
                        Convert.ToInt32(reader["floor_count_basement"], CultureInfo.InvariantCulture),
                    TotalFloorArea =
                        Convert.ToDecimal(reader["total_floor_area"], CultureInfo.InvariantCulture),
                    FudousanId =
                        Convert.ToString(reader["fudousan_id"], CultureInfo.InvariantCulture)
                        ?? string.Empty,
                    FudousanIdAdditionalCode =
                        Convert.ToString(reader["fudousan_id_additional_code"], CultureInfo.InvariantCulture)
                        ?? string.Empty,
                    Remarks =
                        Convert.ToString(reader["remarks"], CultureInfo.InvariantCulture) ?? string.Empty
                };

            // Try
            building.SetName(Convert.ToString(reader["name"], CultureInfo.InvariantCulture) ?? string.Empty);
            // Address/Location
            building.Address.SetMachiazaId(Convert.ToString(reader["location_machiaza_id"], CultureInfo.InvariantCulture) ?? "");
            building.Address.SetPrefectureByMunicipalityCode(Convert.ToString(reader["location_prefecture_code"], CultureInfo.InvariantCulture) ?? "",
                Convert.ToString(reader["location_prefecture"], CultureInfo.InvariantCulture) ?? "");
            building.Address.SetCountyAndCity(building.Address.MachiazaId,
                Convert.ToString(reader["location_county"], CultureInfo.InvariantCulture) ?? "",
                Convert.ToString(reader["location_city"], CultureInfo.InvariantCulture) ?? "");

            building.Address.SetWardAndOaza(building.Address.MachiazaId,
                Convert.ToString(reader["location_ward"], CultureInfo.InvariantCulture) ?? "",
                Convert.ToString(reader["location_oaza_cho"], CultureInfo.InvariantCulture) ?? "");

            building.Address.SetChoume(building.Address.MachiazaId,
                Convert.ToString(reader["location_choume"], CultureInfo.InvariantCulture) ?? "");

            building.Address.SetEdaban(Convert.ToString(reader["location_edaban"], CultureInfo.InvariantCulture) ?? "");
            //building.Address.SetLocLocationFull(Convert.ToString(reader["locLocationFull"], CultureInfo.InvariantCulture) ?? "");
            building.Address.SetLocationLatitude(Convert.ToString(reader["location_latitude"], CultureInfo.InvariantCulture) ?? string.Empty);
            building.Address.SetLocationLongitude(Convert.ToString(reader["location_longitude"], CultureInfo.InvariantCulture) ?? string.Empty);

            building.SetCommercialKindFromString(
                Convert.ToString(reader["commercial_kind"], CultureInfo.InvariantCulture)
                ?? string.Empty);

            building.SetStructureTypeFromString(
                Convert.ToString(reader["property_structure"], CultureInfo.InvariantCulture)
                ?? string.Empty);

            building.SetBuildYearMonthFromString(
                Convert.ToString(reader["built_year_month"], CultureInfo.InvariantCulture)
                ?? string.Empty);


            reader.Close();

            //
            LoadRentCommercialMedia(command, building);

            // lessors
            command.Parameters.Clear();
            command.CommandText = """
    SELECT
        l.lessor_id,
        l.name,
        l.person_kind,
        l.name_last,
        l.name_first,
        l.name_company,
        l.name_company_type,
        l.name_company_type_position,
        l.remarks
    FROM rent_lessors_properties_listings AS r
    INNER JOIN rent_lessors AS l
        ON l.lessor_id = r.lessor_id
    WHERE r.property_id = @propertyId
      AND r.property_kind = @propertyKind
      AND r.listing_id = @listingId;
    """;

            command.Parameters.AddWithValue("@propertyId", building.Id);
            command.Parameters.AddWithValue(
                "@propertyKind",
                PropertyContextType.RentCommercial.ToString());
            command.Parameters.AddWithValue("@listingId", string.Empty);

            using var lessorReader = command.ExecuteReader();

            while (lessorReader.Read())
            {
                var personId =
                    Convert.ToString(lessorReader["lessor_id"], CultureInfo.InvariantCulture);

                if (string.IsNullOrWhiteSpace(personId))
                {
                    continue;
                }

                var person = GetPerson(lessorReader, personId);

                if (person is not null)
                {
                    building.Lessors.Add(person);
                }
            }

            lessorReader.Close();

            // brokers
            command.Parameters.Clear();

            command.CommandText = """
    SELECT
        b.broker_id AS broker_id,
        b.name,
        b.person_kind,
        b.name_last,
        b.name_first,
        b.name_company,
        b.name_company_type,
        b.name_company_type_position,
        b.remarks
    FROM brokers_properties_listings AS r
    INNER JOIN brokers AS b
        ON b.broker_id = r.broker_id
    WHERE r.property_id = @propertyId
      AND r.property_kind = @propertyKind
      AND r.listing_id = @listingId;
    """;

            command.Parameters.AddWithValue("@propertyId", building.Id);
            command.Parameters.AddWithValue(
                "@propertyKind",
                PropertyContextType.RentCommercial.ToString());
            command.Parameters.AddWithValue("@listingId", string.Empty);

            using var brokerReader = command.ExecuteReader();

            while (brokerReader.Read())
            {
                var brokerId =
                    Convert.ToString(brokerReader["broker_id"], CultureInfo.InvariantCulture);

                if (string.IsNullOrWhiteSpace(brokerId))
                {
                    continue;
                }

                var broker = GetPerson(brokerReader, brokerId);

                if (broker is not null)
                {
                    building.Brokers.Add(broker);
                }
            }

            brokerReader.Close();

            // Units
            command.Parameters.Clear();
            command.CommandText = """
    SELECT listing_id, is_property_unit_ownership, name, chinryou, kyoueki_fee,
           shikikin, shikikin_unit, reikin, reikin_unit, renewal_fee,
           renewal_fee_unit, recontract_fee, recontract_fee_unit, floor_area,
           usage, business_hours, parking_available, other_conditions, remarks
    FROM rent_commercial_units
    WHERE property_id = @propertyId
    ORDER BY name;
    """;
            command.Parameters.AddWithValue("@propertyId", building.Id);

            using (var unitReader = command.ExecuteReader())
            {
                while (unitReader.Read())
                {
                    var listingId = Convert.ToString(unitReader["listing_id"], CultureInfo.InvariantCulture) ?? string.Empty;
                    if (string.IsNullOrWhiteSpace(listingId))
                    {
                        continue;
                    }

                    var unit = new Models.Rent.Commercials.Listing.Listing(
                        listingId,
                        EntityStatus.Saved,
                        building.Id,
                        EntityStatus.Saved,
                        Convert.ToInt32(unitReader["is_property_unit_ownership"], CultureInfo.InvariantCulture) != 0,
                        building.Name)/*
                    {
                        Chinryou = Convert.ToDecimal(unitReader["chinryou"], CultureInfo.InvariantCulture),
                        KyouekiFee = Convert.ToDecimal(unitReader["kyoueki_fee"], CultureInfo.InvariantCulture),
                        Shikikin = Convert.ToDecimal(unitReader["shikikin"], CultureInfo.InvariantCulture),
                        ShikikinUnit = Convert.ToString(unitReader["shikikin_unit"], CultureInfo.InvariantCulture) ?? "ヵ月",
                        Reikin = Convert.ToDecimal(unitReader["reikin"], CultureInfo.InvariantCulture),
                        ReikinUnit = Convert.ToString(unitReader["reikin_unit"], CultureInfo.InvariantCulture) ?? "ヵ月",
                        RenewalFee = Convert.ToDecimal(unitReader["renewal_fee"], CultureInfo.InvariantCulture),
                        RenewalFeeUnit = Convert.ToString(unitReader["renewal_fee_unit"], CultureInfo.InvariantCulture) ?? "ヵ月",
                        RecontractFee = Convert.ToDecimal(unitReader["recontract_fee"], CultureInfo.InvariantCulture),
                        RecontractFeeUnit = Convert.ToString(unitReader["recontract_fee_unit"], CultureInfo.InvariantCulture) ?? "円",
                        FloorArea = Convert.ToDecimal(unitReader["floor_area"], CultureInfo.InvariantCulture),
                        Usage = Convert.ToString(unitReader["usage"], CultureInfo.InvariantCulture) ?? "未指定",
                        BusinessHours = Convert.ToString(unitReader["business_hours"], CultureInfo.InvariantCulture) ?? string.Empty,
                        ParkingAvailable = Convert.ToInt32(unitReader["parking_available"], CultureInfo.InvariantCulture) != 0,
                        OtherConditions = Convert.ToString(unitReader["other_conditions"], CultureInfo.InvariantCulture) ?? string.Empty,
                        Remarks = Convert.ToString(unitReader["remarks"], CultureInfo.InvariantCulture) ?? string.Empty
                    }*/;

                    unit.SetIsModified(false);

                    building.SetName(Convert.ToString(unitReader["name"], CultureInfo.InvariantCulture) ?? string.Empty);

                    building.Units.Add(unit);
                }
            }



            // set result
            result.Building = building;
        }
        catch (Exception ex)
        {
            SetDatabaseError(result, ex, "connection.Open(), reader.Read()", "Failed to connect to or read a SQLite database file", nameof(SelectRentCommercialById));
        }
        finally
        {
            _readerWriterLock.ExitReadLock();
        }

        return result;
    }

    private static void LoadRentCommercialMedia(SqliteCommand command, Models.Rent.Commercials.Property building)
    {
        command.Parameters.Clear();
        command.CommandText = """
        SELECT picture_id, filename, type, description, is_main
        FROM rent_commercial_pictures
        WHERE property_id = @propertyId
        ORDER BY picture_id;
        """;
        command.Parameters.AddWithValue("@propertyId", building.Id);

        using (var reader = command.ExecuteReader())
        {
            while (reader.Read())
            {
                var id = Convert.ToString(reader["picture_id"], CultureInfo.InvariantCulture) ?? string.Empty;
                var filename = Convert.ToString(reader["filename"], CultureInfo.InvariantCulture) ?? string.Empty;

                if (string.IsNullOrWhiteSpace(id) ||
                    string.IsNullOrWhiteSpace(filename))
                {
                    continue;
                }

                var picture = new Models.Rent.Commercials.PropertyPicture(id, filename, EntityStatus.Saved)
                {
                    Description =
                        Convert.ToString(reader["description"], CultureInfo.InvariantCulture) ?? string.Empty,
                    IsMain = Convert.ToInt32(reader["is_main"], CultureInfo.InvariantCulture) != 0
                };

                picture.SetLabelFromString(
                    Convert.ToString(reader["type"], CultureInfo.InvariantCulture) ?? string.Empty);
                picture.SetIsModified(false);

                building.Pictures.Add(picture);
            }
        }

        command.Parameters.Clear();
        command.CommandText = """
        SELECT pdf_id, filename, thumbnail_filename, type, description,
               is_main
        FROM rent_commercial_pdfs
        WHERE property_id = @propertyId
        ORDER BY pdf_id;
        """;
        command.Parameters.AddWithValue("@propertyId", building.Id);

        using (var reader = command.ExecuteReader())
        {
            while (reader.Read())
            {
                var id = Convert.ToString(reader["pdf_id"], CultureInfo.InvariantCulture) ?? string.Empty;
                var filename = Convert.ToString(reader["filename"], CultureInfo.InvariantCulture) ?? string.Empty;
                var thumbnailFilename =
                    Convert.ToString(reader["thumbnail_filename"], CultureInfo.InvariantCulture)
                    ?? string.Empty;

                if (string.IsNullOrWhiteSpace(id) ||
                    string.IsNullOrWhiteSpace(filename) ||
                    string.IsNullOrWhiteSpace(thumbnailFilename))
                {
                    continue;
                }

                var pdf = new Models.Rent.Commercials.PropertyPdf(
                    id,
                    filename,
                    thumbnailFilename, EntityStatus.Saved)
                {
                    Description =
                        Convert.ToString(reader["description"], CultureInfo.InvariantCulture) ?? string.Empty,
                    IsMain = Convert.ToInt32(reader["is_main"], CultureInfo.InvariantCulture) != 0
                };

                pdf.SetTypeFromString(
                    Convert.ToString(reader["type"], CultureInfo.InvariantCulture) ?? string.Empty);
                pdf.SetIsModified(false);

                building.Pdfs.Add(pdf);
            }
        }

        command.Parameters.Clear();
    }

    public ResultWrapper DeleteRentCommercial(string commercialId)
    {
        var result = new ResultWrapper();

        if (string.IsNullOrWhiteSpace(commercialId))
        {
            result.IsError = true;
            result.Error.Description = "Commercial property ID is empty.";
            return result;
        }

        _readerWriterLock.EnterWriteLock();

        try
        {
            using var connection =
                new SqliteConnection(_connectionStringBuilder.ConnectionString);

            connection.Open();

            // try catch rollback

            using var transaction = connection.BeginTransaction();
            using var command = connection.CreateCommand();

            command.Transaction = transaction;
            command.CommandText = """
            DELETE FROM properties
            WHERE property_id = @propertyId
              AND property_kind = @propertyKind;
            """;

            command.Parameters.AddWithValue("@propertyId", commercialId);
            command.Parameters.AddWithValue(
                "@propertyKind",
                PropertyContextType.RentCommercial.ToString());

            result.AffectedCount = command.ExecuteNonQuery();

            transaction.Commit();
        }
        catch (Exception ex)
        {
            SetDatabaseError(result, ex, "cmd.ExecuteNonQuery(), cmd.Transaction.Commit", "Failed to delete database record. Transaction.Rollback()", nameof(DeleteRentCommercial));
        }
        finally
        {
            _readerWriterLock.ExitWriteLock();
        }

        return result;
    }

    public ResultWrapper UpsertRentCommercialListing(string commercialId, Models.Rent.Commercials.Listing.Listing room)
    {
        var result = new ResultWrapper();

        if (string.IsNullOrWhiteSpace(commercialId) ||
            string.IsNullOrWhiteSpace(room.Id))
        {
            result.IsError = true;
            result.Error.Description =
                "Commercial property or unit ID is empty.";
            return result;
        }

        _readerWriterLock.EnterWriteLock();

        try
        {
            using var connection =
                new SqliteConnection(_connectionStringBuilder.ConnectionString);

            connection.Open();

            // TODO: try catch rollback

            using var transaction = connection.BeginTransaction();
            using var command = connection.CreateCommand();

            command.Transaction = transaction;

            command.CommandText = """
            UPDATE properties
            SET updated_at = @updatedAt
            WHERE property_id = @propertyId
              AND property_kind = @propertyKind;

            INSERT INTO rent_commercial_units (
                listing_id,
                property_id,
                is_property_unit_ownership,
                name,
                chinryou,
                kyoueki_fee,
                shikikin,
                shikikin_unit,
                reikin,
                reikin_unit,
                renewal_fee,
                renewal_fee_unit,
                recontract_fee,
                recontract_fee_unit,
                floor_area,
                usage,
                business_hours,
                parking_available,
                other_conditions,
                remarks,
                updated_at
            )
            VALUES (
                @listingId,
                @propertyId,
                @isUnitOwnership,
                @name,
                @chinryou,
                @kyouekiFee,
                @shikikin,
                @shikikinUnit,
                @reikin,
                @reikinUnit,
                @renewalFee,
                @renewalFeeUnit,
                @recontractFee,
                @recontractFeeUnit,
                @floorArea,
                @usage,
                @businessHours,
                @parkingAvailable,
                @otherConditions,
                @remarks,
                @updatedAt
            )
            ON CONFLICT(listing_id) DO UPDATE SET
                property_id = excluded.property_id,
                is_property_unit_ownership =
                    excluded.is_property_unit_ownership,
                name = excluded.name,
                chinryou = excluded.chinryou,
                kyoueki_fee = excluded.kyoueki_fee,
                shikikin = excluded.shikikin,
                shikikin_unit = excluded.shikikin_unit,
                reikin = excluded.reikin,
                reikin_unit = excluded.reikin_unit,
                renewal_fee = excluded.renewal_fee,
                renewal_fee_unit = excluded.renewal_fee_unit,
                recontract_fee = excluded.recontract_fee,
                recontract_fee_unit = excluded.recontract_fee_unit,
                floor_area = excluded.floor_area,
                usage = excluded.usage,
                business_hours = excluded.business_hours,
                parking_available = excluded.parking_available,
                other_conditions = excluded.other_conditions,
                remarks = excluded.remarks,
                updated_at = excluded.updated_at;
            """;

            command.Parameters.AddWithValue("@propertyId", commercialId);
            command.Parameters.AddWithValue(
                "@propertyKind",
                PropertyContextType.RentCommercial.ToString());
            command.Parameters.AddWithValue(
                "@updatedAt",
                DateTimeOffset.UtcNow.ToString("s"));
            command.Parameters.AddWithValue("@listingId", room.Id);
            command.Parameters.AddWithValue(
                "@isUnitOwnership",
                room.IsPropertyUnitOwnership ? 1 : 0);
            command.Parameters.AddWithValue("@name", room.Name);
            command.Parameters.AddWithValue("@chinryou", room.Chinryou);
            /*
            command.Parameters.AddWithValue(
                "@kyouekiFee",
                room.KyouekiFee);
            command.Parameters.AddWithValue("@shikikin", room.Shikikin);
            command.Parameters.AddWithValue(
                "@shikikinUnit",
                room.ShikikinUnit);
            command.Parameters.AddWithValue("@reikin", room.Reikin);
            command.Parameters.AddWithValue(
                "@reikinUnit",
                room.ReikinUnit);
            command.Parameters.AddWithValue(
                "@renewalFee",
                room.RenewalFee);
            command.Parameters.AddWithValue(
                "@renewalFeeUnit",
                room.RenewalFeeUnit);
            command.Parameters.AddWithValue(
                "@recontractFee",
                room.RecontractFee);
            command.Parameters.AddWithValue(
                "@recontractFeeUnit",
                room.RecontractFeeUnit);
            command.Parameters.AddWithValue(
                "@floorArea",
                room.FloorArea);
            command.Parameters.AddWithValue("@usage", room.Usage);
            command.Parameters.AddWithValue(
                "@businessHours",
                room.BusinessHours);
            command.Parameters.AddWithValue(
                "@parkingAvailable",
                room.ParkingAvailable ? 1 : 0);
            command.Parameters.AddWithValue(
                "@otherConditions",
                room.OtherConditions);
            */
            command.Parameters.AddWithValue("@remarks", room.Remarks);

            result.AffectedCount = command.ExecuteNonQuery();

            transaction.Commit();

            room.SetStatus(EntityStatus.Saved);
            room.SetIsModified(false);
        }
        catch (Exception ex)
        {
            SetDatabaseError(result, ex, "cmd.ExecuteNonQuery(), cmd.Transaction.Commit", "Failed to update database tables. Transaction.Rollback()", nameof(UpsertRentCommercialListing));
        }
        finally
        {
            _readerWriterLock.ExitWriteLock();
        }

        return result;
    }

    public ListingSearchResultWrapper SelectRentCommercialListings()
    {
        var result = new ListingSearchResultWrapper();

        _readerWriterLock.EnterReadLock();

        try
        {
            using var connection =
                new SqliteConnection(_connectionStringBuilder.ConnectionString);

            connection.Open();

            using var command = connection.CreateCommand();

            command.CommandText = """
            SELECT
                p.property_id,
                p.name AS property_name,
                u.listing_id,
                u.name AS unit_name
            FROM rent_commercial_units AS u
            INNER JOIN rent_commercials AS c
                ON c.property_id = u.property_id
            INNER JOIN properties AS p
                ON p.property_id = u.property_id
            WHERE p.property_kind = @propertyKind
            ORDER BY p.name, u.name;
            """;

            command.Parameters.AddWithValue(
                "@propertyKind",
                PropertyContextType.RentCommercial.ToString());

            using var reader = command.ExecuteReader();

            while (reader.Read())
            {
                var propertyId =
                    Convert.ToString(reader["property_id"], CultureInfo.InvariantCulture);

                var listingId =
                    Convert.ToString(reader["listing_id"], CultureInfo.InvariantCulture);

                if (string.IsNullOrWhiteSpace(propertyId) ||
                    string.IsNullOrWhiteSpace(listingId))
                {
                    continue;
                }

                var item = new Models.SearchResult.ListingSearchResultItem(
                    listingId,
                    propertyId,
                    PropertyContextType.RentCommercial)
                {
                    PropertyName =
                        Convert.ToString(reader["property_name"], CultureInfo.InvariantCulture)
                        ?? string.Empty
                };
                // Try
                item.SetName(Convert.ToString(reader["unit_name"], CultureInfo.InvariantCulture) ?? string.Empty);


                item.SetIsModified(false);

                result.ListingSearchResult.Add(item);
            }
        }
        catch (Exception ex)
        {
            SetDatabaseError(result, ex, "connection.Open(), reader.Read()", "Failed to connect to or read a SQLite database file", nameof(SelectRentCommercialListings));
        }
        finally
        {
            _readerWriterLock.ExitReadLock();
        }

        return result;
    }

    public Models.Rent.Commercials.ListingResultWrapper SelectRentCommercialListingById(string commercialId, string roomId)
    {
        var result = new Models.Rent.Commercials.ListingResultWrapper();

        if (string.IsNullOrWhiteSpace(commercialId) ||
            string.IsNullOrWhiteSpace(roomId))
        {
            result.IsError = true;
            result.Error.Description =
                "Commercial property or unit ID is empty.";
            return result;
        }

        _readerWriterLock.EnterReadLock();

        try
        {
            using var connection =
                new SqliteConnection(_connectionStringBuilder.ConnectionString);

            connection.Open();

            using var command = connection.CreateCommand();

            command.CommandText = """
            SELECT
                p.property_id,
                p.name AS property_name,
                u.listing_id,
                u.is_property_unit_ownership,
                u.name AS unit_name,
                u.chinryou,
                u.kyoueki_fee,
                u.shikikin,
                u.shikikin_unit,
                u.reikin,
                u.reikin_unit,
                u.renewal_fee,
                u.renewal_fee_unit,
                u.recontract_fee,
                u.recontract_fee_unit,
                u.floor_area,
                u.usage,
                u.business_hours,
                u.parking_available,
                u.other_conditions,
                u.remarks
            FROM rent_commercial_units AS u
            INNER JOIN rent_commercials AS c
                ON c.property_id = u.property_id
            INNER JOIN properties AS p
                ON p.property_id = u.property_id
            WHERE u.property_id = @propertyId
              AND u.listing_id = @listingId
              AND p.property_kind = @propertyKind;
            """;

            command.Parameters.AddWithValue(
                "@propertyId",
                commercialId);
            command.Parameters.AddWithValue(
                "@listingId",
                roomId);
            command.Parameters.AddWithValue(
                "@propertyKind",
                PropertyContextType.RentCommercial.ToString());

            using var reader = command.ExecuteReader();

            if (!reader.Read())
            {
                return result;
            }

            var propertyId =
                Convert.ToString(reader["property_id"], CultureInfo.InvariantCulture)
                ?? string.Empty;

            var listingId =
                Convert.ToString(reader["listing_id"], CultureInfo.InvariantCulture)
                ?? string.Empty;

            var propertyName =
                Convert.ToString(reader["property_name"], CultureInfo.InvariantCulture)
                ?? string.Empty;

            var room = new Models.Rent.Commercials.Listing.Listing(
                listingId,
                EntityStatus.Saved,
                propertyId,
                EntityStatus.Saved,
                Convert.ToInt32(
                    reader["is_property_unit_ownership"], CultureInfo.InvariantCulture) != 0,
                propertyName)/*
            {
                Chinryou = Convert.ToDecimal(reader["chinryou"], CultureInfo.InvariantCulture),
                KyouekiFee =
                    Convert.ToDecimal(reader["kyoueki_fee"], CultureInfo.InvariantCulture),
                Shikikin =
                    Convert.ToDecimal(reader["shikikin"], CultureInfo.InvariantCulture),
                ShikikinUnit =
                    Convert.ToString(reader["shikikin_unit"], CultureInfo.InvariantCulture)
                    ?? "ヵ月",
                Reikin =
                    Convert.ToDecimal(reader["reikin"], CultureInfo.InvariantCulture),
                ReikinUnit =
                    Convert.ToString(reader["reikin_unit"], CultureInfo.InvariantCulture)
                    ?? "ヵ月",
                RenewalFee =
                    Convert.ToDecimal(reader["renewal_fee"], CultureInfo.InvariantCulture),
                RenewalFeeUnit =
                    Convert.ToString(reader["renewal_fee_unit"], CultureInfo.InvariantCulture)
                    ?? "ヵ月",
                RecontractFee =
                    Convert.ToDecimal(reader["recontract_fee"], CultureInfo.InvariantCulture),
                RecontractFeeUnit =
                    Convert.ToString(reader["recontract_fee_unit"], CultureInfo.InvariantCulture)
                    ?? "円",
                FloorArea =
                    Convert.ToDecimal(reader["floor_area"], CultureInfo.InvariantCulture),
                Usage =
                    Convert.ToString(reader["usage"], CultureInfo.InvariantCulture)
                    ?? "未指定",
                BusinessHours =
                    Convert.ToString(reader["business_hours"], CultureInfo.InvariantCulture)
                    ?? string.Empty,
                ParkingAvailable =
                    Convert.ToInt32(reader["parking_available"], CultureInfo.InvariantCulture) != 0,
                OtherConditions =
                    Convert.ToString(reader["other_conditions"], CultureInfo.InvariantCulture)
                    ?? string.Empty,
                Remarks =
                    Convert.ToString(reader["remarks"], CultureInfo.InvariantCulture)
                    ?? string.Empty
            }*/;
            room.SetName(Convert.ToString(reader["unit_name"], CultureInfo.InvariantCulture) ?? string.Empty);
            room.SetIsModified(false);

            result.BuildingName = propertyName;
            result.Unit = room;
        }
        catch (Exception ex)
        {
            SetDatabaseError(result, ex, "connection.Open(), reader.Read()", "Failed to connect to or read a SQLite database file", nameof(SelectRentCommercialListingById));
        }
        finally
        {
            _readerWriterLock.ExitReadLock();
        }

        return result;
    }

    public ResultWrapper DeleteRentCommercialListing(string roomId)
    {
        var result = new ResultWrapper();

        if (string.IsNullOrWhiteSpace(roomId))
        {
            result.IsError = true;
            result.Error.Description = "Commercial unit ID is empty.";
            return result;
        }

        _readerWriterLock.EnterWriteLock();

        try
        {
            using var connection =
                new SqliteConnection(_connectionStringBuilder.ConnectionString);

            connection.Open();

            // TODO: try catch rollback

            using var command = connection.CreateCommand();

            command.CommandText = """
            DELETE FROM rent_commercial_units
            WHERE listing_id = @listingId;
            """;

            command.Parameters.AddWithValue(
                "@listingId",
                roomId);

            result.AffectedCount = command.ExecuteNonQuery();
        }
        catch (Exception ex)
        {
            SetDatabaseError(result, ex, "cmd.ExecuteNonQuery(), cmd.Transaction.Commit", "Failed to update database tables. Transaction.Rollback()", nameof(DeleteRentCommercialListing));
        }
        finally
        {
            _readerWriterLock.ExitWriteLock();
        }

        return result;
    }

    #endregion

    #region == Rent Lessor ==

    public ResultWrapper UpsertRentLessor(Models.Base.PersonBase lessor)
    {
        var res = new ResultWrapper();

        if (string.IsNullOrEmpty(lessor.Id))
        {
            res.IsError = true;
            // TODO:
            return res;
        }

        _readerWriterLock.EnterWriteLock();
        try
        {
            using var connection = new SqliteConnection(_connectionStringBuilder.ConnectionString);
            connection.Open();

            using var cmd = connection.CreateCommand();
            cmd.Transaction = connection.BeginTransaction();
            try
            {
                cmd.CommandType = CommandType.Text;

                // Upsert into rent_lessor
                var sqlInsertIntoRentLivingRoom = "INSERT INTO rent_lessors (lessor_id, name, person_kind, name_last, name_first, name_company, name_company_type, name_company_type_position, remarks) VALUES (@lessor_id, @name, @personKind, @name_last, @name_first, @name_company, @name_company_type, @name_company_type_position, @remarks) ";
                sqlInsertIntoRentLivingRoom += "ON CONFLICT(lessor_id) ";
                sqlInsertIntoRentLivingRoom += "DO UPDATE SET name = @name, person_kind = @personKind, name_last = @name_last, name_first = @name_first, name_company = @name_company, name_company_type = @name_company_type, name_company_type_position = @name_company_type_position, remarks = @remarks, updated_at = @updated_at";

                cmd.CommandText = sqlInsertIntoRentLivingRoom;

                cmd.Parameters.AddWithValue("@lessor_id", lessor.Id);
                cmd.Parameters.AddWithValue("@name", lessor.Name);
                cmd.Parameters.AddWithValue("@personKind", lessor.PersonKind.ToString());

                if (lessor is Models.Person.NaturalPerson naturalPerson)
                {
                    cmd.Parameters.AddWithValue("@name_last", naturalPerson.NameLast);
                    cmd.Parameters.AddWithValue("@name_first", naturalPerson.NameFirst);
                    // Clear legalPerson values
                    cmd.Parameters.AddWithValue("@name_company", string.Empty);
                    cmd.Parameters.AddWithValue("@name_company_type", string.Empty);
                    // TODO: check if int is ok
                    cmd.Parameters.AddWithValue("@name_company_type_position", 0);
                }
                else if (lessor is Models.Person.LegalPerson legalPerson)
                {
                    // Clear naturalPerson values
                    cmd.Parameters.AddWithValue("@name_last", string.Empty);
                    cmd.Parameters.AddWithValue("@name_first", string.Empty);

                    cmd.Parameters.AddWithValue("@name_company", legalPerson.NameCompany);
                    cmd.Parameters.AddWithValue("@name_company_type", legalPerson.NameCompanyType);
                    // TODO: check if int is ok
                    cmd.Parameters.AddWithValue("@name_company_type_position", legalPerson.NameCompanyTypePosition);
                }
                cmd.Parameters.AddWithValue("@remarks", lessor.Remarks);
                cmd.Parameters.AddWithValue("@updated_at", DateTimeOffset.UtcNow.ToString("s"));

                var result = cmd.ExecuteNonQuery();
                if (result > 0)
                {
                    lessor.SetStatus(EntityStatus.Saved);
                    lessor.SetIsModified(false);
                }
                res.AffectedCount = result;

                // Commit
                cmd.Transaction.Commit();
            }
            catch (Exception ex)
            {
                cmd.Transaction.Rollback();

                SetDatabaseError(res, ex, "cmd.ExecuteNonQuery(), cmd.Transaction.Commit", "Failed to update database tables. Transaction.Rollback()", nameof(UpsertRentLessor));

                return res;
            }
        }
        catch (Exception ex)
        {
            SetDatabaseError(res, ex, "connection.Open", "Failed to Connect to a SQLite database file", nameof(UpsertRentLessor));

            return res;
        }
        finally
        {
            _readerWriterLock.ExitWriteLock();
        }

        //Debug.WriteLine(string.Format("{0} Entries Inserted to DB", res.AffectedCount.ToString()));

        return res;
    }

    public PersonsSearchResultWrapper SelectRentLessorsByKeyword(string keyword)
    {
        var res = new PersonsSearchResultWrapper();

        if (string.IsNullOrEmpty(keyword))
        {
            keyword = "*";
        }

        //Debug.WriteLine($"keyword is {keyword} @SelectRentResidentialsByNameKeyword() in DataAccessService");

        _readerWriterLock.EnterReadLock();
        try
        {
            using var connection = new SqliteConnection(_connectionStringBuilder.ConnectionString);
            connection.Open();

            using var cmd = connection.CreateCommand();
            if (keyword == "*")
            {
                cmd.CommandText = "SELECT lessor_id, name, person_kind, remarks FROM rent_lessors";
            }
            else
            {
                cmd.CommandText = string.Format(CultureInfo.InvariantCulture, "SELECT lessor_id, name, person_kind, remarks FROM rent_lessors WHERE REPLACE(REPLACE(name, ' ', ''), '　', '') LIKE '%{0}%'", keyword);
            }

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                var s = Convert.ToString(reader["lessor_id"], CultureInfo.InvariantCulture);
                if (string.IsNullOrEmpty(s))
                {
                    Debug.WriteLine("DataAccess::SelectRentLessorByKeyword: lessor_id is null or empty.");
                    continue;
                }

                Models.Person.PersonKindType? enumKind = null;
                var kind = reader.GetString(reader.GetOrdinal("person_kind")) ?? string.Empty;
                if (!string.IsNullOrEmpty(kind))
                {
                    if (Enum.TryParse<Models.Person.PersonKindType>(kind, out var parsedKind))
                    {
                        enumKind = parsedKind;
                    }
                }

                if (enumKind is null)
                {
                    Debug.WriteLine("DataAccess::SelectRentLessorByKeyword: EnumPersonKind is null.");
                    continue;
                }

                Models.SearchResult.PersonSearchResultItem item;
                if (enumKind == Models.Person.PersonKindType.Natural)
                {
                    item = new Models.SearchResult.PersonSearchResultItem(s, Models.Person.PersonKindType.Natural);
                }
                else if (enumKind == Models.Person.PersonKindType.Legal)
                {
                    item = new Models.SearchResult.PersonSearchResultItem(s, Models.Person.PersonKindType.Legal);
                }
                else
                {
                    continue;
                }

                item.SetName(Convert.ToString(reader["name"], CultureInfo.InvariantCulture) ?? "");

                //Debug.WriteLine($"Found rent residential item: {item.Name} @SelectRentResidentialsByNameKeyword() in DataAccessService");

                s = Convert.ToString(reader["remarks"], CultureInfo.InvariantCulture) ?? "";
                item.Remarks = s;

                // Reset item Isdirty flag.
                item.SetIsModified(false);

                //res.AffectedCount++;

                res.PersonSearchResult.Add(item);
            }
        }
        catch (Exception ex)
        {
            SetDatabaseError(res, ex, "connection.Open(), reader.Read()", "Failed to connect to or read a SQLite database file", nameof(SelectRentLessorsByKeyword));
        }
        finally
        {
            _readerWriterLock.ExitReadLock();
        }

        return res;
    }

    public Models.Person.ResultWrapper SelectRentLessorById(string id)
    {
        var res = new Models.Person.ResultWrapper();

        Models.Base.PersonBase? person = null; //new Models.Base.PersonBase(id, EnumEntityStatus.Saved);

        if (string.IsNullOrEmpty(id))
        {
            res.IsError = true;
            // TODO:
            return res;
        }

        _readerWriterLock.EnterReadLock();
        try
        {
            using var connection = new SqliteConnection(_connectionStringBuilder.ConnectionString);
            connection.Open();

            using var cmd = connection.CreateCommand();

            cmd.CommandText = $"SELECT lessor_id, name, person_kind, name_last, name_first, name_company, name_company_type, name_company_type_position, remarks FROM rent_lessors WHERE lessor_id = '{id}'";

            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    var s = Convert.ToString(reader["lessor_id"], CultureInfo.InvariantCulture);
                    if (string.IsNullOrEmpty(s))
                    {
                        Debug.WriteLine("DataAccess::SelectRentLessorById: lessor_id is null or empty for a lessor entry.");
                        continue;
                    }

                    person = GetPerson(reader, id);

                    //res.AffectedCount++;

                    //break; // Assuming we only want the first match
                }
            }

            if (person is not null)
            {
                // Reset person Isdirty flag.
                person.SetStatus(EntityStatus.Saved);
                person.SetIsModified(false);
            }

            res.Person = person;
        }
        catch (Exception ex)
        {
            SetDatabaseError(res, ex, "connection.Open(), reader.Read()", "Failed to connect to or read a SQLite database file", nameof(SelectRentLessorById));
        }
        finally
        {
            _readerWriterLock.ExitReadLock();
        }

        return res;
    }

    private static Models.Base.PersonBase? GetPerson(SqliteDataReader reader, string personId)
    {
        // Do not try to access reader["person_id"] here because it may not be present in the SELECT query. Use the provided personId parameter instead.

        Models.Base.PersonBase? person = null;

        Models.Person.PersonKindType? enumKind = null;
        var kind = reader.GetString(reader.GetOrdinal("person_kind")) ?? string.Empty;
        if (!string.IsNullOrEmpty(kind))
        {
            if (Enum.TryParse<Models.Person.PersonKindType>(kind, out var parsedKind))
            {
                enumKind = parsedKind;
            }
        }

        if (enumKind is null)
        {
            Debug.WriteLine("DataAccess::GetPerson: EnumPersonKind is null.");
            return null;
        }

        if (enumKind == Models.Person.PersonKindType.Natural)
        {
            person = new Models.Person.NaturalPerson(personId, EntityStatus.Saved);
        }
        else if (enumKind == Models.Person.PersonKindType.Legal)
        {
            person = new Models.Person.LegalPerson(personId, EntityStatus.Saved);
        }

        if (person is null)
        {
            return null;
        }

        //s = Convert.ToString(reader["name"]) ?? "";
        person.SetName(reader.GetString(reader.GetOrdinal("name")) ?? string.Empty);

        if (person is Models.Person.NaturalPerson naturalPerson)
        {
            naturalPerson.NameLast = Convert.ToString(reader["name_last"], CultureInfo.InvariantCulture) ?? "";
            naturalPerson.NameFirst = Convert.ToString(reader["name_first"], CultureInfo.InvariantCulture) ?? "";

        }
        else if (person is Models.Person.LegalPerson legalPerson)
        {
            legalPerson.NameCompany = Convert.ToString(reader["name_company"], CultureInfo.InvariantCulture) ?? "";
            legalPerson.NameCompanyType = Convert.ToString(reader["name_company_type"], CultureInfo.InvariantCulture) ?? "";
            legalPerson.NameCompanyTypePosition = Convert.ToInt32(reader["name_company_type_position"], CultureInfo.InvariantCulture);// int

        }


        person.Remarks = Convert.ToString(reader["remarks"], CultureInfo.InvariantCulture) ?? "";

        // TODO: more.

        return person;
    }

    public ResultWrapper DeleteRentLessor(string id)
    {
        var res = new ResultWrapper();

        if (string.IsNullOrEmpty(id))
        {
            res.IsError = true;
            // TODO:
            return res;
        }

        _readerWriterLock.EnterWriteLock();
        try
        {
            // System.Data.SQLite
            //using var connection = new SQLiteConnection(_connectionStringBuilder.ConnectionString);
            // Microsoft.Data.Sqlite
            using var connection = new SqliteConnection(_connectionStringBuilder.ConnectionString);
            connection.Open();

            using var cmd = connection.CreateCommand();

            cmd.Transaction = connection.BeginTransaction();
            try
            {
                cmd.CommandText = string.Format(CultureInfo.InvariantCulture, "DELETE FROM rent_lessors WHERE lessor_id = '{0}';", id);
                res.AffectedCount = cmd.ExecuteNonQuery();

                cmd.Transaction.Commit();
            }
            catch (Exception ex)
            {
                cmd.Transaction.Rollback();

                SetDatabaseError(res, ex, "cmd.ExecuteNonQuery(), cmd.Transaction.Commit", "Failed to delete database record. Transaction.Rollback()", nameof(DeleteRentLessor));

                return res;
            }
        }
        catch (Exception ex)
        {
            SetDatabaseError(res, ex, "connection.Open(), reader.Read()", "Failed to connect to a SQLite database file", nameof(DeleteRentLessor));

            return res;
        }
        finally
        {
            _readerWriterLock.ExitWriteLock();
        }

        //Debug.WriteLine(string.Format("{0} feed Deleted from DB", res.AffectedCount));

        return res;
    }

    #endregion

    #region == Sale Residential ==

    public ResultWrapper UpsertSaleResidential(Models.Sale.Residentials.Property building)
    {
        var res = new ResultWrapper();

        if (string.IsNullOrWhiteSpace(building.Id))
        {
            res.IsError = true;
            res.Error.Description = "Sale residential ID is empty.";
            return res;
        }

        _readerWriterLock.EnterWriteLock();

        try
        {
            using var connection =
                new SqliteConnection(_connectionStringBuilder.ConnectionString);

            connection.Open();

            // TODO: try catch rollback

            using var transaction = connection.BeginTransaction();
            using var command = connection.CreateCommand();

            command.Transaction = transaction;

            command.CommandText = """
            INSERT INTO properties (
                property_id,
                name,
                property_kind,
                thumbnail_filename,
                location_prefecture_code,
                location_prefecture,
                location_machiaza_id,
                location_county,
                location_city,
                location_ward,
                location_oaza_cho,
                location_choume,
                location_edaban,
                location_full,
                updated_at
            )
            VALUES (
                @propertyId,
                @name,
                @propertyKind,
                @thumbnailFilename,
                @locPrefId,
                @locPrefecture,
                @locMachiazaId,
                @locCounty,
                @locCity,
                @locWard,
                @locOazaCho,
                @locChoume,
                @locEdaban,
                @locLocationFull,
                @updatedAt
            )
            ON CONFLICT(property_id) DO UPDATE SET
                name = excluded.name,
                property_kind = excluded.property_kind,
                thumbnail_filename = excluded.thumbnail_filename,
                location_prefecture_code = excluded.location_prefecture_code,
                location_prefecture = excluded.location_prefecture,
                location_machiaza_id = excluded.location_machiaza_id,
                location_county = excluded.location_county,
                location_city = excluded.location_city,
                location_ward = excluded.location_ward,
                location_oaza_cho = excluded.location_oaza_cho,
                location_choume = excluded.location_choume,
                location_edaban = excluded.location_edaban,
                location_full = excluded.location_full,
                updated_at = excluded.updated_at;
            """;

            command.Parameters.AddWithValue("@propertyId", building.Id);
            command.Parameters.AddWithValue("@name", building.Name);
            command.Parameters.AddWithValue("@propertyKind",building.PropertyContextType.ToString());
            command.Parameters.AddWithValue("@thumbnailFilename",building.ThumbnailFilename);

            // Address/Location
            command.Parameters.AddWithValue("@locPrefId", building.Address.Prefecture?.MunicipalityCode ?? string.Empty);
            command.Parameters.AddWithValue("@locPrefecture", building.Address.Prefecture?.Name ?? string.Empty);
            command.Parameters.AddWithValue("@locMachiazaId", building.Address.MachiazaId ?? string.Empty);
            command.Parameters.AddWithValue("@locCounty", building.Address.CountyAndCity?.County ?? string.Empty);
            command.Parameters.AddWithValue("@locCity", building.Address.CountyAndCity?.City ?? string.Empty);
            command.Parameters.AddWithValue("@locWard", building.Address.WardAndOaza?.Ward ?? string.Empty);
            command.Parameters.AddWithValue("@locOazaCho", building.Address.WardAndOaza?.Oaza ?? string.Empty);
            command.Parameters.AddWithValue("@locChoume", building.Address.Choume?.Chou ?? string.Empty);
            command.Parameters.AddWithValue("@locEdaban", building.Address.Edaban ?? string.Empty);
            command.Parameters.AddWithValue("@locLocationFull", building.Address.AddressFull ?? string.Empty);
            command.Parameters.AddWithValue("@locationLatitude", building.Address.LocationLatitude);
            command.Parameters.AddWithValue("@locationLongitude", building.Address.LocationLongitude);

            command.Parameters.AddWithValue("@updatedAt", DateTimeOffset.UtcNow.ToString("s"));

            command.ExecuteNonQuery();

            command.Parameters.Clear();

            command.CommandText = """
            INSERT INTO sale_residentials (
                property_id,
                property_type,
                is_unit_ownership,
                property_structure,
                floor_count_above_ground,
                floor_count_basement,
                total_unit_count,
                built_year_month,
                fudousan_id,
                fudousan_id_additional_code,
                remarks,
                updated_at
            )
            VALUES (
                @propertyId,
                @buildingType,
                @isUnitOwnership,
                @buildingStructure,
                @floorCountAboveGround,
                @floorCountBasement,
                @totalUnitCount,
                @builtYearMonth,
                @fudousanId,
                @fudousanIdAdditionalCode,
                @remarks,
                @updatedAt
            )
            ON CONFLICT(property_id) DO UPDATE SET
                property_type = excluded.property_type,
                is_unit_ownership = excluded.is_unit_ownership,
                property_structure = excluded.property_structure,
                floor_count_above_ground = excluded.floor_count_above_ground,
                floor_count_basement = excluded.floor_count_basement,
                total_unit_count = excluded.total_unit_count,
                built_year_month = excluded.built_year_month,
                fudousan_id = excluded.fudousan_id,
                fudousan_id_additional_code =
                    excluded.fudousan_id_additional_code,
                remarks = excluded.remarks,
                updated_at = excluded.updated_at;
            """;

            command.Parameters.AddWithValue("@propertyId", building.Id);
            command.Parameters.AddWithValue(
                "@buildingType",
                building.BuildingKind.Key.ToString());
            command.Parameters.AddWithValue(
                "@isUnitOwnership",
                building.IsUnitOwnership ? 1 : 0);
            command.Parameters.AddWithValue(
                "@buildingStructure",
                building.PropertyStructure.Key.ToString());
            command.Parameters.AddWithValue(
                "@floorCountAboveGround",
                building.FloorCountAboveGround);
            command.Parameters.AddWithValue(
                "@floorCountBasement",
                building.FloorCountBasement);
            command.Parameters.AddWithValue(
                "@totalUnitCount",
                building.TotalUnitCount);
            command.Parameters.AddWithValue(
                "@builtYearMonth",
                building.BuiltYearAndMonth.ToString("s"));
            command.Parameters.AddWithValue(
                "@fudousanId",
                building.FudousanId);
            command.Parameters.AddWithValue(
                "@fudousanIdAdditionalCode",
                building.FudousanIdAdditionalCode);
            command.Parameters.AddWithValue("@remarks", building.Remarks);
            command.Parameters.AddWithValue(
                "@updatedAt",
                DateTimeOffset.UtcNow.ToString("s"));

            res.AffectedCount = command.ExecuteNonQuery();

            transaction.Commit();

            building.SetStatus(EntityStatus.Saved);
            building.SetIsModified(false);
        }
        catch (Exception ex)
        {
            SetDatabaseError(res, ex, "cmd.ExecuteNonQuery(), cmd.Transaction.Commit", "Failed to update database tables. Transaction.Rollback()", nameof(UpsertSaleResidential));
        }
        finally
        {
            _readerWriterLock.ExitWriteLock();
        }

        return res;
    }

    public PropertySearchResultWrapper SelectSaleResidentialsByNameKeyword(string keyword)
    {
        var result = new PropertySearchResultWrapper();

        _readerWriterLock.EnterReadLock();

        try
        {
            using var connection =
                new SqliteConnection(_connectionStringBuilder.ConnectionString);

            connection.Open();

            using var command = connection.CreateCommand();

            var searchAll = string.IsNullOrWhiteSpace(keyword) ||
                            keyword.Trim() == "*";

            command.CommandText = searchAll
                ? """
              SELECT
                  p.property_id,
                  p.name,
                  p.property_kind
              FROM sale_residentials AS s
              INNER JOIN properties AS p
                  ON p.property_id = s.property_id
              WHERE p.property_kind = @propertyKind
              ORDER BY p.name;
              """
                : """
              SELECT
                  p.property_id,
                  p.name,
                  p.property_kind
              FROM sale_residentials AS s
              INNER JOIN properties AS p
                  ON p.property_id = s.property_id
              WHERE p.property_kind = @propertyKind
                AND p.name LIKE @keyword
              ORDER BY p.name;
              """;

            command.Parameters.AddWithValue(
                "@propertyKind",
                PropertyContextType.SaleResidential.ToString());

            if (!searchAll)
            {
                command.Parameters.AddWithValue(
                    "@keyword",
                    $"%{keyword.Trim()}%");
            }

            using var reader = command.ExecuteReader();

            while (reader.Read())
            {
                var propertyId =
                    Convert.ToString(reader["property_id"], CultureInfo.InvariantCulture);

                if (string.IsNullOrWhiteSpace(propertyId))
                {
                    continue;
                }

                var item = new Models.SearchResult.PropertySearchResultItem(
                    propertyId,
                    PropertyContextType.SaleResidential)
                {
                };
                item.SetName(Convert.ToString(reader["name"], CultureInfo.InvariantCulture) ?? string.Empty);
                item.SetIsModified(false);

                result.PropertySearchResult.Add(item);
            }
        }
        catch (Exception ex)
        {
            SetDatabaseError(result, ex, "connection.Open(), reader.Read()", "Failed to connect to or read a SQLite database file", nameof(SelectSaleResidentialsByNameKeyword));
        }
        finally
        {
            _readerWriterLock.ExitReadLock();
        }

        return result;
    }

    public Models.Sale.Residentials.PropertyResultWrapper SelectSaleResidentialById(string id)
    {
        var res = new Models.Sale.Residentials.PropertyResultWrapper();

        if (string.IsNullOrWhiteSpace(id))
        {
            res.IsError = true;
            res.Error.Description = "Sale residential ID is empty.";
            return res;
        }

        _readerWriterLock.EnterReadLock();

        try
        {
            using var connection =
                new SqliteConnection(_connectionStringBuilder.ConnectionString);

            connection.Open();

            using var command = connection.CreateCommand();

            command.CommandText = """
            SELECT
                p.property_id,
                p.name,
                p.thumbnail_filename,
                p.location_prefecture_code,
                p.location_prefecture,
                p.location_machiaza_id,
                p.location_county,
                p.location_city,
                p.location_ward,
                p.location_oaza_cho,
                p.location_choume,
                p.location_edaban,
                p.location_full,
                p.location_latitude,
                p.location_longitude,
                s.property_type,
                s.is_unit_ownership,
                s.property_structure,
                s.floor_count_above_ground,
                s.floor_count_basement,
                s.total_unit_count,
                s.built_year_month,
                s.fudousan_id,
                s.fudousan_id_additional_code,
                s.remarks
            FROM sale_residentials AS s
            INNER JOIN properties AS p
                ON p.property_id = s.property_id
            WHERE s.property_id = @propertyId;
            """;

            command.Parameters.AddWithValue("@propertyId", id);

            using var reader = command.ExecuteReader();

            if (!reader.Read())
            {
                return res;
            }

            var building =
                new Models.Sale.Residentials.Property(
                    Convert.ToString(reader["property_id"], CultureInfo.InvariantCulture)!,
                    EntityStatus.Saved)
                {
                    ThumbnailFilename =
                        Convert.ToString(reader["thumbnail_filename"], CultureInfo.InvariantCulture)
                        ?? string.Empty,

                    IsUnitOwnership =
                        Convert.ToInt32(reader["is_unit_ownership"], CultureInfo.InvariantCulture) != 0,
                    FloorCountAboveGround =
                        Convert.ToInt32(reader["floor_count_above_ground"], CultureInfo.InvariantCulture),
                    FloorCountBasement =
                        Convert.ToInt32(reader["floor_count_basement"], CultureInfo.InvariantCulture),
                    TotalUnitCount =
                        Convert.ToInt32(reader["total_unit_count"], CultureInfo.InvariantCulture),
                    FudousanId =
                        Convert.ToString(reader["fudousan_id"], CultureInfo.InvariantCulture)
                        ?? string.Empty,
                    FudousanIdAdditionalCode =
                        Convert.ToString(reader["fudousan_id_additional_code"], CultureInfo.InvariantCulture)
                        ?? string.Empty,
                    Remarks =
                        Convert.ToString(reader["remarks"], CultureInfo.InvariantCulture)
                        ?? string.Empty
                };

            building.SetName(Convert.ToString(reader["name"], CultureInfo.InvariantCulture) ?? string.Empty);
            // Address/Location
            building.Address.SetMachiazaId(Convert.ToString(reader["locMachiazaId"], CultureInfo.InvariantCulture) ?? "");
            building.Address.SetPrefectureByMunicipalityCode(Convert.ToString(reader["locPrefId"], CultureInfo.InvariantCulture) ?? "",
                Convert.ToString(reader["locPrefecture"], CultureInfo.InvariantCulture) ?? "");
            building.Address.SetCountyAndCity(building.Address.MachiazaId,
                Convert.ToString(reader["locCounty"], CultureInfo.InvariantCulture) ?? "",
                Convert.ToString(reader["locCity"], CultureInfo.InvariantCulture) ?? "");

            building.Address.SetWardAndOaza(building.Address.MachiazaId,
                Convert.ToString(reader["locWard"], CultureInfo.InvariantCulture) ?? "",
                Convert.ToString(reader["locOazaCho"], CultureInfo.InvariantCulture) ?? "");

            building.Address.SetChoume(building.Address.MachiazaId,
                Convert.ToString(reader["locChoume"], CultureInfo.InvariantCulture) ?? "");

            building.Address.SetEdaban(Convert.ToString(reader["locEdaban"], CultureInfo.InvariantCulture) ?? "");
            //building.Address.SetLocLocationFull(Convert.ToString(reader["locLocationFull"], CultureInfo.InvariantCulture) ?? "");
            building.Address.SetLocationLatitude(Convert.ToString(reader["locationLatitude"], CultureInfo.InvariantCulture) ?? string.Empty);
            building.Address.SetLocationLongitude(Convert.ToString(reader["locationLongitude"], CultureInfo.InvariantCulture) ?? string.Empty);

            building.SetKindTypeFromString(
                Convert.ToString(reader["property_type"], CultureInfo.InvariantCulture) ?? string.Empty);

            building.SetStructureTypeFromString(
                Convert.ToString(reader["property_structure"], CultureInfo.InvariantCulture) ?? string.Empty);

            building.SetBuildYearMonthFromString(
                Convert.ToString(reader["built_year_month"], CultureInfo.InvariantCulture) ?? string.Empty);

            res.Building = building;
        }
        catch (Exception ex)
        {
            SetDatabaseError(res, ex, "connection.Open(), reader.Read()", "Failed to connect to or read a SQLite database file", nameof(SelectSaleResidentialById));
        }
        finally
        {
            _readerWriterLock.ExitReadLock();
        }

        return res;
    }

    public ResultWrapper DeleteSaleResidential(string saleId)
    {
        var res = new ResultWrapper();

        if (string.IsNullOrWhiteSpace(saleId))
        {
            res.IsError = true;
            res.Error.Description = "Sale residential ID is empty.";
            return res;
        }

        _readerWriterLock.EnterWriteLock();

        try
        {
            using var connection =
                new SqliteConnection(_connectionStringBuilder.ConnectionString);

            connection.Open();

            // try catch rollback

            using var transaction = connection.BeginTransaction();
            using var command = connection.CreateCommand();

            command.Transaction = transaction;
            command.CommandText = """
            DELETE FROM properties
            WHERE property_id = @propertyId
              AND property_kind = @propertyKind;
            """;

            command.Parameters.AddWithValue("@propertyId", saleId);
            command.Parameters.AddWithValue(
                "@propertyKind",
                PropertyContextType.SaleResidential.ToString());

            res.AffectedCount = command.ExecuteNonQuery();

            transaction.Commit();
        }
        catch (Exception ex)
        {
            // TODO:
            SetDatabaseError(res, ex, "cmd.ExecuteNonQuery(), cmd.Transaction.Commit", "Failed to delete database record. Transaction.Rollback()", nameof(DeleteSaleResidential));
        }
        finally
        {
            _readerWriterLock.ExitWriteLock();
        }

        return res;
    }

    public ResultWrapper UpsertSaleResidentialListing(string saleId, Models.Sale.Residentials.Listing room)
    {
        var res = new ResultWrapper();

        if (string.IsNullOrWhiteSpace(saleId) ||
            string.IsNullOrWhiteSpace(room.Id))
        {
            res.IsError = true;
            res.Error.Description = "Sale residential or unit ID is empty.";
            return res;
        }

        _readerWriterLock.EnterWriteLock();

        try
        {
            using var connection =
                new SqliteConnection(_connectionStringBuilder.ConnectionString);

            connection.Open();

            // TODO: try catch rollback

            using var transaction = connection.BeginTransaction();
            using var command = connection.CreateCommand();

            command.Transaction = transaction;

            command.CommandText = """
            UPDATE properties
            SET updated_at = @updatedAt
            WHERE property_id = @propertyId
              AND property_kind = @propertyKind;

            INSERT INTO sale_residential_units (
                listing_id,
                property_id,
                is_property_unit_ownership,
                name,
                sale_price,
                management_fee,
                repair_reserve_fund,
                ownership_type,
                occupancy_status,
                delivery_timing,
                remarks,
                updated_at
            )
            VALUES (
                @listingId,
                @propertyId,
                @isUnitOwnership,
                @name,
                @salePrice,
                @managementFee,
                @repairReserveFund,
                @ownershipType,
                @occupancyStatus,
                @deliveryTiming,
                @remarks,
                @updatedAt
            )
            ON CONFLICT(listing_id) DO UPDATE SET
                property_id = excluded.property_id,
                is_property_unit_ownership =
                    excluded.is_property_unit_ownership,
                name = excluded.name,
                sale_price = excluded.sale_price,
                management_fee = excluded.management_fee,
                repair_reserve_fund = excluded.repair_reserve_fund,
                ownership_type = excluded.ownership_type,
                occupancy_status = excluded.occupancy_status,
                delivery_timing = excluded.delivery_timing,
                remarks = excluded.remarks,
                updated_at = excluded.updated_at;
            """;

            command.Parameters.AddWithValue("@propertyId", saleId);
            command.Parameters.AddWithValue(
                "@propertyKind",
                PropertyContextType.SaleResidential.ToString());
            command.Parameters.AddWithValue(
                "@updatedAt",
                DateTimeOffset.UtcNow.ToString("s"));
            command.Parameters.AddWithValue("@listingId", room.Id);
            command.Parameters.AddWithValue(
                "@isUnitOwnership",
                room.IsPropertyUnitOwnership ? 1 : 0);
            command.Parameters.AddWithValue("@name", room.Name);
            /*
            command.Parameters.AddWithValue("@salePrice", room.SalePrice);
            command.Parameters.AddWithValue(
                "@managementFee",
                room.ManagementFee);
            command.Parameters.AddWithValue(
                "@repairReserveFund",
                room.RepairReserveFund);
            command.Parameters.AddWithValue(
                "@ownershipType",
                room.OwnershipType);
            command.Parameters.AddWithValue(
                "@occupancyStatus",
                room.OccupancyStatus);
            command.Parameters.AddWithValue(
                "@deliveryTiming",
                room.DeliveryTiming);
            */
            command.Parameters.AddWithValue("@remarks", room.Remarks);

            res.AffectedCount = command.ExecuteNonQuery();

            transaction.Commit();

            room.SetStatus(EntityStatus.Saved);
            room.SetIsModified(false);
        }
        catch (Exception ex)
        {
            // TODO:
            SetDatabaseError(res, ex, "cmd.ExecuteNonQuery(), cmd.Transaction.Commit", "Failed to update database tables. Transaction.Rollback()", nameof(UpsertSaleResidentialListing));
        }
        finally
        {
            _readerWriterLock.ExitWriteLock();
        }

        return res;
    }

    public ListingSearchResultWrapper SelectSaleResidentialListings()
    {
        var res = new ListingSearchResultWrapper();

        _readerWriterLock.EnterReadLock();

        try
        {
            using var connection =
                new SqliteConnection(_connectionStringBuilder.ConnectionString);

            connection.Open();

            using var command = connection.CreateCommand();

            command.CommandText = """
            SELECT
                p.property_id,
                p.name AS property_name,
                u.listing_id,
                u.name AS unit_name
            FROM sale_residential_units AS u
            INNER JOIN properties AS p
                ON p.property_id = u.property_id
            WHERE p.property_kind = @propertyKind
            ORDER BY p.name, u.name;
            """;

            command.Parameters.AddWithValue(
                "@propertyKind",
                PropertyContextType.SaleResidential.ToString());

            using var reader = command.ExecuteReader();

            while (reader.Read())
            {
                var propertyId =
                    Convert.ToString(reader["property_id"], CultureInfo.InvariantCulture);

                var listingId =
                    Convert.ToString(reader["listing_id"], CultureInfo.InvariantCulture);

                if (string.IsNullOrWhiteSpace(propertyId) ||
                    string.IsNullOrWhiteSpace(listingId))
                {
                    continue;
                }

                var item = new Models.SearchResult.ListingSearchResultItem(
                    listingId,
                    propertyId,
                    PropertyContextType.SaleResidential)
                {
                    PropertyName = Convert.ToString(reader["property_name"], CultureInfo.InvariantCulture)
                        ?? string.Empty
                };

                item.SetName(Convert.ToString(reader["unit_name"], CultureInfo.InvariantCulture) ?? string.Empty);
                item.SetIsModified(false);

                res.ListingSearchResult.Add(item);
            }
        }
        catch (Exception ex)
        {
            SetDatabaseError(res, ex, "connection.Open(), reader.Read()", "Failed to connect to or read a SQLite database file", nameof(SelectSaleResidentialListings));
        }
        finally
        {
            _readerWriterLock.ExitReadLock();
        }

        return res;
    }

    public ResultWrapper DeleteSaleResidentialListing(string roomId)
    {
        var res = new ResultWrapper();

        if (string.IsNullOrWhiteSpace(roomId))
        {
            res.IsError = true;
            res.Error.Description = "Sale residential unit ID is empty.";
            return res;
        }

        _readerWriterLock.EnterWriteLock();

        try
        {
            using var connection =
                new SqliteConnection(_connectionStringBuilder.ConnectionString);

            connection.Open();

            using var command = connection.CreateCommand();

            command.CommandText = """
            DELETE FROM sale_residential_units
            WHERE listing_id = @listingId;
            """;

            command.Parameters.AddWithValue("@listingId", roomId);

            res.AffectedCount = command.ExecuteNonQuery();
        }
        catch (Exception ex)
        {
            // TODO:
            SetDatabaseError(res, ex, "cmd.ExecuteNonQuery(), cmd.Transaction.Commit", "Failed to delete database record. Transaction.Rollback()", nameof(DeleteSaleResidentialListing));
        }
        finally
        {
            _readerWriterLock.ExitWriteLock();
        }

        return res;
    }

    public Models.Sale.Residentials.ListingResultWrapper SelectSaleResidentialListingById(string saleId, string roomId)
    {
        var res = new Models.Sale.Residentials.ListingResultWrapper();

        if (string.IsNullOrWhiteSpace(saleId) ||
            string.IsNullOrWhiteSpace(roomId))
        {
            res.IsError = true;
            res.Error.Description = "Sale residential or unit ID is empty.";
            return res;
        }

        _readerWriterLock.EnterReadLock();

        try
        {
            using var connection =
                new SqliteConnection(_connectionStringBuilder.ConnectionString);

            connection.Open();

            using var command = connection.CreateCommand();

            command.CommandText = """
            SELECT
                p.property_id,
                p.name AS property_name,
                u.listing_id,
                u.is_property_unit_ownership,
                u.name AS unit_name,
                u.sale_price,
                u.management_fee,
                u.repair_reserve_fund,
                u.ownership_type,
                u.occupancy_status,
                u.delivery_timing,
                u.remarks
            FROM sale_residential_units AS u
            INNER JOIN sale_residentials AS s
                ON s.property_id = u.property_id
            INNER JOIN properties AS p
                ON p.property_id = u.property_id
            WHERE u.property_id = @propertyId
              AND u.listing_id = @listingId
              AND p.property_kind = @propertyKind;
            """;

            command.Parameters.AddWithValue("@propertyId", saleId);
            command.Parameters.AddWithValue("@listingId", roomId);
            command.Parameters.AddWithValue(
                "@propertyKind",
                PropertyContextType.SaleResidential.ToString());

            using var reader = command.ExecuteReader();

            if (!reader.Read())
            {
                return res;
            }

            var propertyId =
                Convert.ToString(reader["property_id"], CultureInfo.InvariantCulture) ?? string.Empty;

            var listingId =
                Convert.ToString(reader["listing_id"], CultureInfo.InvariantCulture) ?? string.Empty;

            var propertyName =
                Convert.ToString(reader["property_name"], CultureInfo.InvariantCulture) ?? string.Empty;

            var room = new Models.Sale.Residentials.Listing(
                listingId,
                EntityStatus.Saved,
                propertyId,
                EntityStatus.Saved,
                Convert.ToInt32(
                    reader["is_property_unit_ownership"], CultureInfo.InvariantCulture) != 0,
                propertyName)/*
            {
                SalePrice = Convert.ToDecimal(reader["sale_price"], CultureInfo.InvariantCulture),
                ManagementFee =
                    Convert.ToDecimal(reader["management_fee"], CultureInfo.InvariantCulture),
                RepairReserveFund =
                    Convert.ToDecimal(reader["repair_reserve_fund"], CultureInfo.InvariantCulture),
                OwnershipType =
                    Convert.ToString(reader["ownership_type"], CultureInfo.InvariantCulture)
                    ?? "所有権",
                OccupancyStatus =
                    Convert.ToString(reader["occupancy_status"], CultureInfo.InvariantCulture)
                    ?? "空室",
                DeliveryTiming =
                    Convert.ToString(reader["delivery_timing"], CultureInfo.InvariantCulture)
                    ?? "相談",
                Remarks =
                    Convert.ToString(reader["remarks"], CultureInfo.InvariantCulture)
                    ?? string.Empty
            }*/;

            room.SetName(Convert.ToString(reader["unit_name"], CultureInfo.InvariantCulture) ?? string.Empty);


            room.SetIsModified(false);

            res.BuildingName = propertyName;
            res.Room = room;
        }
        catch (Exception ex)
        {
            SetDatabaseError(res, ex, "connection.Open(), reader.Read()", "Failed to connect to or read a SQLite database file", nameof(SelectSaleResidentialListingById));
        }
        finally
        {
            _readerWriterLock.ExitReadLock();
        }

        return res;
    }

    #endregion

    #region == Broker ==

    public ResultWrapper UpsertBroker(Models.Base.PersonBase broker)
    {
        var res = new ResultWrapper();

        if (broker is null || string.IsNullOrWhiteSpace(broker.Id))
        {
            res.IsError = true;
            res.Error.Description = "Broker ID is empty.";
            return res;
        }

        _readerWriterLock.EnterWriteLock();

        try
        {
            using var connection =
                new SqliteConnection(_connectionStringBuilder.ConnectionString);

            connection.Open();

            using var command = connection.CreateCommand();
            using var transaction = connection.BeginTransaction();

            command.Transaction = transaction;
            command.CommandText = """
                INSERT INTO brokers (
                    broker_id,
                    name,
                    person_kind,
                    name_last,
                    name_first,
                    name_company,
                    name_company_type,
                    name_company_type_position,
                    remarks,
                    updated_at
                )
                VALUES (
                    @brokerId,
                    @name,
                    @personKind,
                    @nameLast,
                    @nameFirst,
                    @nameCompany,
                    @nameCompanyType,
                    @nameCompanyTypePosition,
                    @remarks,
                    @updatedAt
                )
                ON CONFLICT (broker_id) DO UPDATE SET
                    name = excluded.name,
                    person_kind = excluded.person_kind,
                    name_last = excluded.name_last,
                    name_first = excluded.name_first,
                    name_company = excluded.name_company,
                    name_company_type = excluded.name_company_type,
                    name_company_type_position = excluded.name_company_type_position,
                    remarks = excluded.remarks,
                    updated_at = excluded.updated_at;
                """;

            command.Parameters.AddWithValue("@brokerId", broker.Id);
            command.Parameters.AddWithValue("@name", broker.Name);
            command.Parameters.AddWithValue("@personKind", broker.PersonKind.ToString());
            command.Parameters.AddWithValue("@remarks", broker.Remarks);
            command.Parameters.AddWithValue("@updatedAt", DateTimeOffset.UtcNow.ToString("s"));

            if (broker is Models.Person.NaturalPerson natural)
            {
                command.Parameters.AddWithValue("@nameLast", natural.NameLast);
                command.Parameters.AddWithValue("@nameFirst", natural.NameFirst);
                command.Parameters.AddWithValue("@nameCompany", string.Empty);
                command.Parameters.AddWithValue("@nameCompanyType", string.Empty);
                command.Parameters.AddWithValue("@nameCompanyTypePosition", 0);
            }
            else if (broker is Models.Person.LegalPerson legal)
            {
                command.Parameters.AddWithValue("@nameLast", string.Empty);
                command.Parameters.AddWithValue("@nameFirst", string.Empty);
                command.Parameters.AddWithValue("@nameCompany", legal.NameCompany);
                command.Parameters.AddWithValue("@nameCompanyType", legal.NameCompanyType);
                command.Parameters.AddWithValue(
                    "@nameCompanyTypePosition",
                    legal.NameCompanyTypePosition);
            }
            else
            {
                command.Parameters.AddWithValue("@nameLast", string.Empty);
                command.Parameters.AddWithValue("@nameFirst", string.Empty);
                command.Parameters.AddWithValue("@nameCompany", string.Empty);
                command.Parameters.AddWithValue("@nameCompanyType", string.Empty);
                command.Parameters.AddWithValue("@nameCompanyTypePosition", 0);
            }

            res.AffectedCount = command.ExecuteNonQuery();
            transaction.Commit();

            broker.SetStatus(EntityStatus.Saved);

            broker.SetIsModified(false);
        }
        catch (Exception ex)
        {
            SetDatabaseError(
                res,
                ex,
                "command.ExecuteNonQuery(), transaction.Commit()",
                "Failed to upsert broker.",
                nameof(UpsertBroker));
        }
        finally
        {
            _readerWriterLock.ExitWriteLock();
        }

        return res;
    }

    public PersonsSearchResultWrapper SelectBrokersByKeyword(string keyword)
    {
        var res = new PersonsSearchResultWrapper();
        var searchAll = string.IsNullOrWhiteSpace(keyword) ||
                        keyword.Trim() == "*";

        _readerWriterLock.EnterReadLock();

        try
        {
            using var connection =
                new SqliteConnection(_connectionStringBuilder.ConnectionString);

            connection.Open();

            using var command = connection.CreateCommand();

            command.CommandText = searchAll
                ? """
              SELECT broker_id, name, person_kind, remarks
              FROM brokers
              ORDER BY name;
              """
                : """
              SELECT broker_id, name, person_kind, remarks
              FROM brokers
              WHERE REPLACE(REPLACE(name, ' ', ''), '　', '')
                    LIKE @keyword
              ORDER BY name;
              """;

            if (!searchAll)
            {
                command.Parameters.AddWithValue(
                    "@keyword",
                    $"%{keyword.Trim()}%");
            }

            using var reader = command.ExecuteReader();

            while (reader.Read())
            {
                var brokerId = Convert.ToString(reader["broker_id"], CultureInfo.InvariantCulture);

                if (string.IsNullOrWhiteSpace(brokerId))
                {
                    continue;
                }

                var personKindText =
                    Convert.ToString(reader["person_kind"], CultureInfo.InvariantCulture) ?? string.Empty;

                if (!Enum.TryParse<Models.Person.PersonKindType>(
                        personKindText,
                        out var personKind) ||
                    personKind == Models.Person.PersonKindType.Undetermined)
                {
                    continue;
                }

                var item = new Models.SearchResult.PersonSearchResultItem(
                    brokerId,
                    personKind)
                {
                    Remarks = Convert.ToString(reader["remarks"], CultureInfo.InvariantCulture) ?? string.Empty
                };

                item.SetName(Convert.ToString(reader["name"], CultureInfo.InvariantCulture) ?? string.Empty);
                item.SetIsModified(false);

                res.PersonSearchResult.Add(item);
            }
        }
        catch (Exception ex)
        {
            SetDatabaseError(
                res,
                ex,
                "connection.Open(), reader.Read()",
                "Failed to read brokers.",
                nameof(SelectBrokersByKeyword));
        }
        finally
        {
            _readerWriterLock.ExitReadLock();
        }

        return res;
    }

    public Models.Person.ResultWrapper SelectBrokerById(string id)
    {
        var res = new Models.Person.ResultWrapper();

        if (string.IsNullOrWhiteSpace(id))
        {
            res.IsError = true;
            res.Error.Description = "Broker ID is empty.";
            return res;
        }

        _readerWriterLock.EnterReadLock();

        try
        {
            using var connection =
                new SqliteConnection(_connectionStringBuilder.ConnectionString);

            connection.Open();

            using var command = connection.CreateCommand();

            command.CommandText = """
        SELECT
            broker_id,
            name,
            person_kind,
            name_last,
            name_first,
            name_company,
            name_company_type,
            name_company_type_position,
            remarks
        FROM brokers
        WHERE broker_id = @brokerId;
        """;

            command.Parameters.AddWithValue("@brokerId", id);

            using var reader = command.ExecuteReader();

            if (reader.Read())
            {
                res.Person = GetPerson(reader, id);

                if (res.Person is not null)
                {
                    res.Person.SetStatus(EntityStatus.Saved);
                    res.Person.SetIsModified(false);
                }
            }
        }
        catch (Exception ex)
        {
            SetDatabaseError(
                res,
                ex,
                "connection.Open(), reader.Read()",
                "Failed to read broker.",
                nameof(SelectBrokerById));
        }
        finally
        {
            _readerWriterLock.ExitReadLock();
        }

        return res;
    }

    public ResultWrapper DeleteBroker(string id)
    {
        var res = new ResultWrapper();

        if (string.IsNullOrWhiteSpace(id))
        {
            res.IsError = true;
            res.Error.Description = "Broker ID is empty.";
            return res;
        }

        _readerWriterLock.EnterWriteLock();

        try
        {
            using var connection =
                new SqliteConnection(_connectionStringBuilder.ConnectionString);

            connection.Open();

            using var command = connection.CreateCommand();
            command.CommandText = """
        DELETE FROM brokers
        WHERE broker_id = @brokerId;
        """;

            command.Parameters.AddWithValue("@brokerId", id);

            res.AffectedCount = command.ExecuteNonQuery();
        }
        catch (Exception ex)
        {
            SetDatabaseError(
                res,
                ex,
                "command.ExecuteNonQuery()",
                "Failed to delete broker.",
                nameof(DeleteBroker));
        }
        finally
        {
            _readerWriterLock.ExitWriteLock();
        }

        return res;
    }

    #endregion

    #region == Errors ==

    private static void SetDatabaseError(ResultWrapperBase result, Exception exception, string operation, string description, string method)
    {
        result.IsError = true;
        result.Error.Type = ErrorInfo.ErrType.DB;
        result.Error.Code = "";

        result.Error.Title = $"Error: {exception.GetType().FullName}";

        if (exception.InnerException != null)
        {
            result.Error.Message = exception.InnerException.Message;
        }
        else
        {
            result.Error.Message = exception.Message;
        }
        result.Error.Description = description;
        result.Error.FullDump = exception.ToString();

        result.Error.OccuredAt = DateTime.Now;
        result.Error.Operation = operation;
        result.Error.MethodName = $"{nameof(DataAccessService)}.{method}";//$"{nameof(DataAccessService)}{exception.TargetSite?.Name}";

    }

    #endregion

    // Unused for now
    #region == ColumnExists check ==

    private static bool ColumnExists(IDataRecord dr, string columnName)
    {
        for (var i = 0; i < dr.FieldCount; i++)
        {
            if (dr.GetName(i).Equals(columnName, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }
        return false; ;
    }

    private static string EscapeSingleQuote(string s)
    {
        return s is null ? string.Empty : s.Replace("'", "''");
    }

    private static string RestoreSingleQuote(string s)
    {
        return s is null ? string.Empty : s.Replace("''", "'");
    }

    #endregion

    public void Dispose()
    {
        _readerWriterLock?.Dispose();

        GC.SuppressFinalize(this);

        Debug.WriteLine("DataAccessService disposed.");
    }
}
