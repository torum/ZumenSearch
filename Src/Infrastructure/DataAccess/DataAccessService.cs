using System.Data;
using System.Globalization;
using Microsoft.Data.Sqlite;
using ZumenSearch.Application.Contracts;
using ZumenSearch.Application.Results;
using ZumenSearch.Application.SearchResults;
using ZumenSearch.Domain.Entities.Person;
using ZumenSearch.Domain.Enums;
using ZumenSearch.Domain.ValueObjects.Location;
using ZumenSearch.Infrastructure.DataAccess.Contracts;
using ZumenSearch.Infrastructure.Platform;

namespace ZumenSearch.Infrastructure.DataAccess;

// Repository

// <summary>
// DataAccessService provides data access functionalities for managing properties (buildings) and listings (rooms/units) and related data in the application.
// </summary>

// TODO:
// * Translate error messages to Japanese for user-facing errors.

public sealed partial class DataAccessService : IDataAccessService, IPropertySearchRepository, IRentResidentialPropertyRepository, IPersonRepository, IRentResidentialListingRepository, IRentCommercialPropertyRepository, IRentCommercialListingRepository, IDisposable
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
            _connectionStringBuilder = new SqliteConnectionStringBuilder("Data Source=" + dataBaseFilePath)
            {
                ForeignKeys = true
            };

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
                    "property_context_type TEXT NOT NULL DEFAULT ''," +
                    "name TEXT NOT NULL DEFAULT ''," +
                    "thumbnail_filename TEXT NOT NULL DEFAULT ''," +

                    "fudousan_id TEXT NOT NULL DEFAULT ''," +
                    "fudousan_id_additional_code TEXT NOT NULL DEFAULT ''," +

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
                    "property_kind TEXT NOT NULL DEFAULT ''," +
                    "is_unit_ownership INTEGER NOT NULL DEFAULT 0," +
                    "property_structure TEXT NOT NULL DEFAULT ''," +
                    "floor_count_above_ground INTEGER NOT NULL DEFAULT 0," +
                    "floor_count_basement INTEGER NOT NULL DEFAULT 0," +
                    "total_unit_count INTEGER NOT NULL DEFAULT 0," +
                    "built_year_month TEXT NOT NULL DEFAULT ''," +
                    "remarks TEXT NOT NULL DEFAULT ''," +

                    "kanri_shutai TEXT NOT NULL DEFAULT '" + nameof(ZumenSearch.Domain.Entities.Rent.Residentials.Property.EnumKanriShutai.Unspecified) + "'," +
                    "kanri_is_maintenance_management INTEGER NOT NULL DEFAULT 0," +
                    "kanri_is_tenant_management INTEGER NOT NULL DEFAULT 0," +
                    "kanri_is_payment_management INTEGER NOT NULL DEFAULT 0," +
                    "kanri_is_other_management INTEGER NOT NULL DEFAULT 0," +
                    "kanri_remark_jisya TEXT NOT NULL DEFAULT ''," +
                    "kanri_name_of_company_tasya TEXT NOT NULL DEFAULT ''," +
                    "kanri_contact_info_of_company_tasya TEXT NOT NULL DEFAULT ''," +
                    "kanri_remark_tasya TEXT NOT NULL DEFAULT ''," +
                    "kanri_remark_kashinushi TEXT NOT NULL DEFAULT ''," +
                    "has_autolock INTEGER NOT NULL DEFAULT 0," +
                    "has_elevator INTEGER NOT NULL DEFAULT 0," +
                    "has_security_camera INTEGER NOT NULL DEFAULT 0," +
                    "has_parcel_locker INTEGER NOT NULL DEFAULT 0," +
                    "has_electric INTEGER NOT NULL DEFAULT 0," +
                    "property_electric_kind TEXT NOT NULL DEFAULT '" + nameof(ZumenSearch.Domain.Entities.Rent.Residentials.Property.EnumElectricType.Unspecified) + "'," +
                    "electric_detail TEXT NOT NULL DEFAULT ''," +
                    "has_gas INTEGER NOT NULL DEFAULT 0," +
                    "gas_type TEXT NOT NULL DEFAULT '" + nameof(ZumenSearch.Domain.Entities.Rent.Residentials.FacilityGasType.Unspecified) + "'," +
                    "gas_detail TEXT NOT NULL DEFAULT ''," +
                    "tap_water_type TEXT NOT NULL DEFAULT '" + nameof(ZumenSearch.Domain.Entities.Rent.Residentials.FacilityTapWaterType.Unspecified) + "'," +
                    "tap_water_detail TEXT NOT NULL DEFAULT ''," +
                    "sewerage_type TEXT NOT NULL DEFAULT '" + nameof(ZumenSearch.Domain.Entities.Rent.Residentials.FacilitySewerageType.Unspecified) + "'," +
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
                    "parking_contract_type TEXT NOT NULL DEFAULT '" + nameof(ZumenSearch.Domain.Entities.Rent.Residentials.FacilityParkingContractType.Optional) + "'," +
                    "parking_availability TEXT NOT NULL DEFAULT '" + nameof(ZumenSearch.Domain.Entities.Rent.Residentials.FacilityAvailabilityType.Unspecified) + "'," +
                    "is_parking_free INTEGER NOT NULL DEFAULT 0," +
                    "parking_fee TEXT NOT NULL DEFAULT ''," +
                    "parking_fee_tax_type TEXT NOT NULL DEFAULT '" + nameof(ZumenSearch.Domain.Entities.Rent.Residentials.FacilityTaxType.Included) + "'," +
                    "parking_spaces TEXT NOT NULL DEFAULT ''," +
                    "parking_distance TEXT NOT NULL DEFAULT ''," +
                    "parking_notes TEXT NOT NULL DEFAULT ''," +
                    "has_bicycle_parking INTEGER NOT NULL DEFAULT 0," +
                    "bicycle_parking_availability TEXT NOT NULL DEFAULT '" + nameof(ZumenSearch.Domain.Entities.Rent.Residentials.FacilityAvailabilityType.Unspecified) + "'," +
                    "is_bicycle_parking_free INTEGER NOT NULL DEFAULT 0," +
                    "bicycle_parking_fee TEXT NOT NULL DEFAULT ''," +
                    "bicycle_parking_fee_tax_type TEXT NOT NULL DEFAULT '" + nameof(ZumenSearch.Domain.Entities.Rent.Residentials.FacilityTaxType.Included) + "'," +
                    "bicycle_parking_notes TEXT NOT NULL DEFAULT ''," +
                    "has_motorcycle_parking INTEGER NOT NULL DEFAULT 0," +
                    "motorcycle_parking_availability TEXT NOT NULL DEFAULT '" + nameof(ZumenSearch.Domain.Entities.Rent.Residentials.FacilityAvailabilityType.Unspecified) + "'," +
                    "is_motorcycle_parking_free INTEGER NOT NULL DEFAULT 0," +
                    "motorcycle_parking_fee TEXT NOT NULL DEFAULT ''," +
                    "motorcycle_parking_fee_tax_type TEXT NOT NULL DEFAULT '" + nameof(ZumenSearch.Domain.Entities.Rent.Residentials.FacilityTaxType.Included) + "'," +
                    "motorcycle_parking_notes TEXT NOT NULL DEFAULT ''," +


                    //"updated_at TEXT NOT NULL DEFAULT (DATETIME('now', 'utc'))," + // property updated_at is already in properties table, so no need to duplicate it here.
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

                    "fudousan_id_additional_code TEXT NOT NULL DEFAULT ''," +

                    "floor_plan_type TEXT NOT NULL DEFAULT ''," +
                    "floor_plan_extra_type TEXT NOT NULL DEFAULT ''," +
                    "floor_area TEXT NOT NULL DEFAULT ''," +
                    "floor_type TEXT NOT NULL DEFAULT ''," +
                    "floor_number TEXT NOT NULL DEFAULT ''," +
                    "is_corner_room INTEGER NOT NULL DEFAULT 0," +
                    "main_exposure_direction TEXT NOT NULL DEFAULT ''," +
                    "current_status TEXT NOT NULL DEFAULT ''," +
                    "is_available_for_rent INTEGER NOT NULL DEFAULT 0," +
                    "available_from_month TEXT NOT NULL DEFAULT ''," +
                    "available_from_period TEXT NOT NULL DEFAULT ''," +
                    "is_immediate_occupancy INTEGER NOT NULL DEFAULT 0," +
                    "current_status_checked_at TEXT NOT NULL DEFAULT ''," +
                    "remarks TEXT NOT NULL DEFAULT ''," +

                    "management_fee TEXT NOT NULL DEFAULT ''," +
                    "management_fee_unit TEXT NOT NULL DEFAULT '" + nameof(ZumenSearch.Domain.Entities.Rent.Residentials.FeeUnitType.Yen) + "'," +
                    "security_deposit TEXT NOT NULL DEFAULT ''," +
                    "security_deposit_unit TEXT NOT NULL DEFAULT '" + nameof(ZumenSearch.Domain.Entities.Rent.Residentials.FeeUnitType.Months) + "'," +
                    "key_money TEXT NOT NULL DEFAULT ''," +
                    "key_money_unit TEXT NOT NULL DEFAULT '" + nameof(ZumenSearch.Domain.Entities.Rent.Residentials.FeeUnitType.Months) + "'," +
                    "contract_type TEXT NOT NULL DEFAULT '" + nameof(ZumenSearch.Domain.Entities.Rent.Residentials.ContractType.Unspecified) + "'," +
                    "contract_period_years TEXT NOT NULL DEFAULT ''," +
                    "renewal_fee TEXT NOT NULL DEFAULT ''," +
                    "renewal_fee_unit TEXT NOT NULL DEFAULT '" + nameof(ZumenSearch.Domain.Entities.Rent.Residentials.FeeUnitType.Months) + "'," +
                    "recontract_fee TEXT NOT NULL DEFAULT ''," +
                    "recontract_fee_unit TEXT NOT NULL DEFAULT '" + nameof(ZumenSearch.Domain.Entities.Rent.Residentials.FeeUnitType.Yen) + "'," +
                    "guarantor_company TEXT NOT NULL DEFAULT ''," +
                    "housing_insurance TEXT NOT NULL DEFAULT ''," +
                    "other_fees TEXT NOT NULL DEFAULT ''," +
                    "pet_friendly INTEGER NOT NULL DEFAULT 0," +
                    "two_person_occupancy INTEGER NOT NULL DEFAULT 0," +
                    "no_guarantor_needed INTEGER NOT NULL DEFAULT 0," +
                    "office_use_allowed INTEGER NOT NULL DEFAULT 0," +
                    "other_conditions TEXT NOT NULL DEFAULT ''," +
                    "contract_remarks TEXT NOT NULL DEFAULT ''," +
                    "handling_type TEXT NOT NULL DEFAULT '" + nameof(ZumenSearch.Domain.Entities.Rent.Residentials.ListingHandlingType.Unspecified) + "'," +
                    "transaction_type TEXT NOT NULL DEFAULT '" + nameof(ZumenSearch.Domain.Entities.Rent.Residentials.ListingTransactionType.Unspecified) + "'," +
                    "lessor_commission TEXT NOT NULL DEFAULT ''," +
                    "lessor_commission_unit TEXT NOT NULL DEFAULT '" + nameof(ZumenSearch.Domain.Entities.Rent.Residentials.ListingCommissionUnitType.Percent) + "'," +
                    "lessee_commission TEXT NOT NULL DEFAULT ''," +
                    "lessee_commission_unit TEXT NOT NULL DEFAULT '" + nameof(ZumenSearch.Domain.Entities.Rent.Residentials.ListingCommissionUnitType.Percent) + "'," +
                    "listing_broker_commission TEXT NOT NULL DEFAULT ''," +
                    "listing_broker_commission_unit TEXT NOT NULL DEFAULT '" + nameof(ZumenSearch.Domain.Entities.Rent.Residentials.ListingCommissionUnitType.Percent) + "'," +
                    "tenant_broker_commission TEXT NOT NULL DEFAULT ''," +
                    "tenant_broker_commission_unit TEXT NOT NULL DEFAULT '" + nameof(ZumenSearch.Domain.Entities.Rent.Residentials.ListingCommissionUnitType.Percent) + "'," +
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

                    "chinryou INTEGER NOT NULL DEFAULT 0," +

                    "created_at TEXT NOT NULL DEFAULT (DATETIME('now'))," +
                    "updated_at TEXT NOT NULL DEFAULT (DATETIME('now'))," +
                    "UNIQUE (property_id, listing_id)," +
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

                    CREATE INDEX IF NOT EXISTS ix_properties_context_type_name
                        ON properties(property_context_type, name);

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
        total_floor_area TEXT NOT NULL DEFAULT '0',
        built_year_month TEXT NOT NULL DEFAULT '',
        remarks TEXT NOT NULL DEFAULT '',
        passenger_elevator_availability INTEGER NOT NULL DEFAULT 0,
        passenger_elevator_count INTEGER,
        freight_elevator_availability INTEGER NOT NULL DEFAULT 0,
        freight_elevator_details TEXT NOT NULL DEFAULT '',
        loading_area_availability INTEGER NOT NULL DEFAULT 0,
        loading_area_details TEXT NOT NULL DEFAULT '',
        security_availability INTEGER NOT NULL DEFAULT 0,
        security_mechanical INTEGER NOT NULL DEFAULT 0,
        security_staffed INTEGER NOT NULL DEFAULT 0,
        security_autolock INTEGER NOT NULL DEFAULT 0,
        security_camera INTEGER NOT NULL DEFAULT 0,
        security_other INTEGER NOT NULL DEFAULT 0,
        security_details TEXT NOT NULL DEFAULT '',
        electricity_availability INTEGER NOT NULL DEFAULT 0,
        electricity_details TEXT NOT NULL DEFAULT '',
        gas_availability INTEGER NOT NULL DEFAULT 0,
        gas_type INTEGER NOT NULL DEFAULT 0,
        gas_details TEXT NOT NULL DEFAULT '',
        water_availability INTEGER NOT NULL DEFAULT 0,
        water_type INTEGER NOT NULL DEFAULT 0,
        water_details TEXT NOT NULL DEFAULT '',
        sewerage_type INTEGER NOT NULL DEFAULT 0,
        sewerage_details TEXT NOT NULL DEFAULT '',
        internet_availability INTEGER NOT NULL DEFAULT 0,
        fiber_availability INTEGER NOT NULL DEFAULT 0,
        internet_details TEXT NOT NULL DEFAULT '',
        fire_safety_availability INTEGER NOT NULL DEFAULT 0,
        fire_safety_details TEXT NOT NULL DEFAULT '',
        parking_availability INTEGER NOT NULL DEFAULT 0,
        parking_vacancy INTEGER NOT NULL DEFAULT 0,
        parking_spaces INTEGER,
        parking_monthly_fee TEXT,
        parking_tax INTEGER NOT NULL DEFAULT 0,
        parking_free INTEGER NOT NULL DEFAULT 0,
        parking_details TEXT NOT NULL DEFAULT '',
        management_type INTEGER NOT NULL DEFAULT 0,
        management_maintenance INTEGER NOT NULL DEFAULT 0,
        management_tenant INTEGER NOT NULL DEFAULT 0,
        management_payment INTEGER NOT NULL DEFAULT 0,
        management_other INTEGER NOT NULL DEFAULT 0,
        management_company_remarks TEXT NOT NULL DEFAULT '',
        management_external_name TEXT NOT NULL DEFAULT '',
        management_external_contact TEXT NOT NULL DEFAULT '',
        management_external_remarks TEXT NOT NULL DEFAULT '',
        management_lessor_remarks TEXT NOT NULL DEFAULT '',
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
    
        fudousan_id_additional_code TEXT NOT NULL DEFAULT '',

        chinryou NUMERIC,
        rent_tax TEXT NOT NULL DEFAULT 'Unconfirmed',
        kyoueki_fee TEXT,
        management_fee_tax TEXT NOT NULL DEFAULT 'Unconfirmed',
        shikikin TEXT,
        shikikin_unit TEXT NOT NULL DEFAULT 'Months',
        guarantee_deposit TEXT,
        guarantee_deposit_unit TEXT NOT NULL DEFAULT 'Months',
        reikin TEXT,
        reikin_unit TEXT NOT NULL DEFAULT 'Months',
        rights_money TEXT,
        rights_money_unit TEXT NOT NULL DEFAULT 'Months',
        deposit_deduction TEXT,
        deposit_deduction_unit TEXT NOT NULL DEFAULT 'Months',
        renewal_fee TEXT,
        renewal_fee_unit TEXT NOT NULL DEFAULT 'Months',
        recontract_fee TEXT,
        recontract_fee_unit TEXT NOT NULL DEFAULT 'Yen',
        contract_type TEXT NOT NULL DEFAULT 'Unspecified',
        contract_period_years TEXT,
        guarantor_company TEXT NOT NULL DEFAULT '',
        insurance TEXT NOT NULL DEFAULT '',
        other_fees TEXT NOT NULL DEFAULT '',
        contract_conditions TEXT NOT NULL DEFAULT '',
        contract_remarks TEXT NOT NULL DEFAULT '',
        handling_type TEXT NOT NULL DEFAULT 'Unspecified',
        transaction_type TEXT NOT NULL DEFAULT 'Unspecified',
        lessor_commission TEXT NOT NULL DEFAULT '',
        lessor_commission_unit TEXT NOT NULL DEFAULT 'Percent',
        lessee_commission TEXT NOT NULL DEFAULT '',
        lessee_commission_unit TEXT NOT NULL DEFAULT 'Percent',
        listing_broker_commission TEXT NOT NULL DEFAULT '',
        listing_broker_commission_unit TEXT NOT NULL DEFAULT 'Percent',
        tenant_broker_commission TEXT NOT NULL DEFAULT '',
        tenant_broker_commission_unit TEXT NOT NULL DEFAULT 'Percent',
        transaction_remarks TEXT NOT NULL DEFAULT '',
        air_conditioning INTEGER NOT NULL DEFAULT 0,
        private_toilet INTEGER NOT NULL DEFAULT 0,
        hot_water INTEGER NOT NULL DEFAULT 0,
        kitchen INTEGER NOT NULL DEFAULT 0,
        ventilation INTEGER NOT NULL DEFAULT 0,
        unit_internet INTEGER NOT NULL DEFAULT 0,
        equipment_details TEXT NOT NULL DEFAULT '',
        unit_condition TEXT NOT NULL DEFAULT 'Unspecified',
        fixture_transfer TEXT NOT NULL DEFAULT 'Unconfirmed',
        permits_heavy_food INTEGER NOT NULL DEFAULT 0,
        permits_light_food INTEGER NOT NULL DEFAULT 0,
        permits_retail INTEGER NOT NULL DEFAULT 0,
        permits_services INTEGER NOT NULL DEFAULT 0,
        permits_office INTEGER NOT NULL DEFAULT 0,
        permits_other INTEGER NOT NULL DEFAULT 0,
        permitted_business_details TEXT NOT NULL DEFAULT '',
        facilities_remarks TEXT NOT NULL DEFAULT '',
        floor_ground TEXT NOT NULL DEFAULT 'AboveGround',
        floor_number INTEGER,
        floor_area TEXT,
        usage TEXT NOT NULL DEFAULT 'Unspecified',
        use_details TEXT NOT NULL DEFAULT '',
        availability_status TEXT NOT NULL DEFAULT 'Unspecified',
        is_available_for_rent INTEGER NOT NULL DEFAULT 0,
        current_status_checked_at TEXT NOT NULL DEFAULT '',
        availability_month TEXT NOT NULL DEFAULT 'Unspecified',
        availability_period TEXT NOT NULL DEFAULT 'Unspecified',
        is_available_immediately INTEGER NOT NULL DEFAULT 0,
        business_hours TEXT NOT NULL DEFAULT '',
        parking_available INTEGER NOT NULL DEFAULT 0,
        other_conditions TEXT NOT NULL DEFAULT '',
        remarks TEXT NOT NULL DEFAULT '',
        created_at TEXT NOT NULL DEFAULT (DATETIME('now', 'utc')),
        updated_at TEXT NOT NULL DEFAULT (DATETIME('now', 'utc')),
        UNIQUE (property_id, listing_id),
        FOREIGN KEY (property_id)
            REFERENCES rent_commercials(property_id)
            ON DELETE CASCADE
    );

    CREATE INDEX IF NOT EXISTS
        ix_rent_commercial_units_property_id
        ON rent_commercial_units(property_id);

    CREATE TABLE IF NOT EXISTS rent_commercial_listing_pictures (
        picture_id TEXT NOT NULL PRIMARY KEY,
        listing_id TEXT NOT NULL,
        property_id TEXT NOT NULL,
        filename TEXT NOT NULL,
        type TEXT NOT NULL,
        description TEXT NOT NULL,
        is_main INTEGER NOT NULL DEFAULT 0,
        FOREIGN KEY (listing_id) REFERENCES rent_commercial_units(listing_id) ON DELETE CASCADE,
        FOREIGN KEY (property_id) REFERENCES rent_commercials(property_id) ON DELETE CASCADE
    );

    CREATE INDEX IF NOT EXISTS ix_rent_commercial_listing_pictures_listing_id
        ON rent_commercial_listing_pictures(listing_id);

    CREATE TABLE IF NOT EXISTS rent_commercial_listing_pdfs (
        pdf_id TEXT NOT NULL PRIMARY KEY,
        listing_id TEXT NOT NULL,
        property_id TEXT NOT NULL,
        filename TEXT NOT NULL,
        thumbnail_filename TEXT NOT NULL,
        type TEXT NOT NULL,
        description TEXT NOT NULL,
        is_main INTEGER NOT NULL DEFAULT 0,
        FOREIGN KEY (listing_id) REFERENCES rent_commercial_units(listing_id) ON DELETE CASCADE,
        FOREIGN KEY (property_id) REFERENCES rent_commercials(property_id) ON DELETE CASCADE
    );

    CREATE INDEX IF NOT EXISTS ix_rent_commercial_listing_pdfs_listing_id
        ON rent_commercial_listing_pdfs(listing_id);

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

                    "phone_number TEXT NOT NULL DEFAULT ''," +
                    "mobile_phone_number TEXT NOT NULL DEFAULT ''," +
                    "contact_name TEXT NOT NULL DEFAULT ''," +
                    "location_postal_code TEXT NOT NULL DEFAULT ''," +
                    "location_full TEXT NOT NULL DEFAULT ''," +
                    "location_machiaza_id TEXT NOT NULL DEFAULT ''," +
                    "location_prefecture TEXT NOT NULL DEFAULT ''," +
                    "location_county TEXT NOT NULL DEFAULT ''," +
                    "location_city TEXT NOT NULL DEFAULT ''," +
                    "location_ward TEXT NOT NULL DEFAULT ''," +
                    "location_oaza TEXT NOT NULL DEFAULT ''," +
                    "location_choume TEXT NOT NULL DEFAULT ''," +
                    "location_edaban TEXT NOT NULL DEFAULT ''," +
                    "remarks TEXT NOT NULL DEFAULT ''," +

                    "updated_at TEXT NOT NULL DEFAULT (DATETIME('now', 'utc'))," +
                    "created_at TEXT NOT NULL DEFAULT (DATETIME('now', 'utc'))" + // Last column, no comma
                    ")";
                tableCmd.ExecuteNonQuery();

                tableCmd.CommandText = """
                    CREATE TABLE IF NOT EXISTS rent_lessors_properties (
                        lessor_id TEXT NOT NULL,
                        property_id TEXT NOT NULL,
                        property_context_type TEXT NOT NULL DEFAULT '',
                        PRIMARY KEY (lessor_id, property_id),
                        FOREIGN KEY (lessor_id) REFERENCES rent_lessors(lessor_id) ON DELETE CASCADE,
                        FOREIGN KEY (property_id) REFERENCES properties(property_id) ON DELETE CASCADE
                    );

                    CREATE INDEX IF NOT EXISTS ix_rent_lessors_properties_property_id
                        ON rent_lessors_properties(property_id);

                    CREATE TABLE IF NOT EXISTS rent_lessors_residential_listings (
                        lessor_id TEXT NOT NULL,
                        property_id TEXT NOT NULL,
                        property_context_type TEXT NOT NULL DEFAULT '',
                        listing_id TEXT NOT NULL,
                        PRIMARY KEY (lessor_id, property_id, listing_id),
                        FOREIGN KEY (lessor_id) REFERENCES rent_lessors(lessor_id) ON DELETE CASCADE,
                        FOREIGN KEY (property_id, listing_id)
                            REFERENCES rent_residential_listings(property_id, listing_id) ON DELETE CASCADE
                    );

                    CREATE INDEX IF NOT EXISTS ix_rent_lessors_residential_listings_property_id
                        ON rent_lessors_residential_listings(property_id);

                    CREATE TABLE IF NOT EXISTS rent_lessors_commercial_listings (
                        lessor_id TEXT NOT NULL,
                        property_id TEXT NOT NULL,
                        property_context_type TEXT NOT NULL DEFAULT '',
                        listing_id TEXT NOT NULL,
                        PRIMARY KEY (lessor_id, property_id, listing_id),
                        FOREIGN KEY (lessor_id) REFERENCES rent_lessors(lessor_id) ON DELETE CASCADE,
                        FOREIGN KEY (property_id, listing_id)
                            REFERENCES rent_commercial_units(property_id, listing_id) ON DELETE CASCADE
                    );

                    CREATE INDEX IF NOT EXISTS ix_rent_lessors_commercial_listings_property_id
                        ON rent_lessors_commercial_listings(property_id);
                    """;
                tableCmd.ExecuteNonQuery();

                #endregion

                #region == Sale Residentials ==

                // Sale tables
                tableCmd.CommandText = """
    CREATE TABLE IF NOT EXISTS sale_residentials (
        property_id TEXT NOT NULL PRIMARY KEY,
        property_kind TEXT NOT NULL DEFAULT '',
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
                        phone_number TEXT NOT NULL DEFAULT '',
                        mobile_phone_number TEXT NOT NULL DEFAULT '',
                        contact_name TEXT NOT NULL DEFAULT '',
                        location_postal_code TEXT NOT NULL DEFAULT '',
                        location_full TEXT NOT NULL DEFAULT '',
                        location_machiaza_id TEXT NOT NULL DEFAULT '',
                        location_prefecture TEXT NOT NULL DEFAULT '',
                        location_county TEXT NOT NULL DEFAULT '',
                        location_city TEXT NOT NULL DEFAULT '',
                        location_ward TEXT NOT NULL DEFAULT '',
                        location_oaza TEXT NOT NULL DEFAULT '',
                        location_choume TEXT NOT NULL DEFAULT '',
                        location_edaban TEXT NOT NULL DEFAULT '',
                        remarks TEXT NOT NULL DEFAULT '',
                        updated_at TEXT NOT NULL DEFAULT (DATETIME('now', 'utc')),
                        created_at TEXT NOT NULL DEFAULT (DATETIME('now', 'utc'))
                    );
                    """;
                tableCmd.ExecuteNonQuery();

                tableCmd.CommandText = """
                    CREATE TABLE IF NOT EXISTS brokers_properties (
                        broker_id TEXT NOT NULL,
                        property_id TEXT NOT NULL,
                        property_context_type TEXT NOT NULL DEFAULT '',
                        PRIMARY KEY (broker_id, property_id),
                        FOREIGN KEY (broker_id) REFERENCES brokers(broker_id) ON DELETE CASCADE,
                        FOREIGN KEY (property_id) REFERENCES properties(property_id) ON DELETE CASCADE
                    );

                    CREATE INDEX IF NOT EXISTS ix_brokers_properties_property_id
                        ON brokers_properties(property_id);

                    CREATE TABLE IF NOT EXISTS brokers_residential_listings (
                        broker_id TEXT NOT NULL,
                        property_id TEXT NOT NULL,
                        property_context_type TEXT NOT NULL DEFAULT '',
                        listing_id TEXT NOT NULL,
                        PRIMARY KEY (broker_id, property_id, listing_id),
                        FOREIGN KEY (broker_id) REFERENCES brokers(broker_id) ON DELETE CASCADE,
                        FOREIGN KEY (property_id, listing_id)
                            REFERENCES rent_residential_listings(property_id, listing_id) ON DELETE CASCADE
                    );

                    CREATE INDEX IF NOT EXISTS ix_brokers_residential_listings_property_id
                        ON brokers_residential_listings(property_id);

                    CREATE TABLE IF NOT EXISTS brokers_commercial_listings (
                        broker_id TEXT NOT NULL,
                        property_id TEXT NOT NULL,
                        property_context_type TEXT NOT NULL DEFAULT '',
                        listing_id TEXT NOT NULL,
                        PRIMARY KEY (broker_id, property_id, listing_id),
                        FOREIGN KEY (broker_id) REFERENCES brokers(broker_id) ON DELETE CASCADE,
                        FOREIGN KEY (property_id, listing_id)
                            REFERENCES rent_commercial_units(property_id, listing_id) ON DELETE CASCADE
                    );

                    CREATE INDEX IF NOT EXISTS ix_brokers_commercial_listings_property_id
                        ON brokers_commercial_listings(property_id);
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

    public ZumenSearch.Application.Results.PropertySearchResultWrapper SelectRecentProperties()
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
                var kind = reader.GetString(reader.GetOrdinal("property_context_type")) ?? string.Empty;
                if (!string.IsNullOrEmpty(kind))
                {
                    if (Enum.TryParse<PropertyContextType>(kind, out var parsedKind))
                    {
                        enumKind = parsedKind;
                    }
                    else
                    {
                        Debug.WriteLine($"property_context_type ({kind}) parse failed. @SelectRecentProperties()");
                    }
                }

                var item = new ZumenSearch.Application.SearchResults.PropertySearchResultItem(id, enumKind);

                var name = reader.GetString(reader.GetOrdinal("name")) ?? string.Empty;//Convert.ToString(reader["name"]) ?? "";
                item.SetName(name);

                item.ThumbnailFilename = reader.GetString(reader.GetOrdinal("thumbnail_filename")) ?? string.Empty;
 
                item.CreatedAt = reader.GetString(reader.GetOrdinal("created_at")) ?? string.Empty;//Convert.ToString(reader["created_at"]) ?? string.Empty;
                item.UpdatedAt = reader.GetString(reader.GetOrdinal("updated_at")) ?? string.Empty;//Convert.ToString(reader["updated_at"]) ?? string.Empty;

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

    public ZumenSearch.Application.Results.PropertySearchResultWrapper SelectPropertiesByKeyword(string keyword, PropertySearchTarget searchTarget = PropertySearchTarget.PropertyName)
    {
        var res = new PropertySearchResultWrapper();

        if (string.IsNullOrEmpty(keyword))
        {
            keyword = "*";
        }

        var whereClause = searchTarget switch
        {
            PropertySearchTarget.PropertyName => "p.name LIKE @keyword ESCAPE '\\'",
            PropertySearchTarget.Address => "p.location_full LIKE @keyword ESCAPE '\\'",
            PropertySearchTarget.Station => "(p.train1_station_name LIKE @keyword ESCAPE '\\' OR p.train2_station_name LIKE @keyword ESCAPE '\\' OR p.train3_station_name LIKE @keyword ESCAPE '\\' OR p.train4_station_name LIKE @keyword ESCAPE '\\')",
            PropertySearchTarget.RailLine => "(p.train1_line_name LIKE @keyword ESCAPE '\\' OR p.train2_line_name LIKE @keyword ESCAPE '\\' OR p.train3_line_name LIKE @keyword ESCAPE '\\' OR p.train4_line_name LIKE @keyword ESCAPE '\\')",
            PropertySearchTarget.FudousanId => "p.fudousan_id LIKE @keyword ESCAPE '\\'",
            PropertySearchTarget.BrokerName => "EXISTS (SELECT 1 FROM brokers AS broker WHERE broker.name LIKE @keyword ESCAPE '\\' AND (EXISTS (SELECT 1 FROM brokers_properties AS relation WHERE relation.property_id = p.property_id AND relation.broker_id = broker.broker_id) OR EXISTS (SELECT 1 FROM brokers_residential_listings AS relation WHERE relation.property_id = p.property_id AND relation.broker_id = broker.broker_id) OR EXISTS (SELECT 1 FROM brokers_commercial_listings AS relation WHERE relation.property_id = p.property_id AND relation.broker_id = broker.broker_id)))",
            PropertySearchTarget.LessorName => "EXISTS (SELECT 1 FROM rent_lessors AS lessor WHERE lessor.name LIKE @keyword ESCAPE '\\' AND (EXISTS (SELECT 1 FROM rent_lessors_properties AS relation WHERE relation.property_id = p.property_id AND relation.lessor_id = lessor.lessor_id) OR EXISTS (SELECT 1 FROM rent_lessors_residential_listings AS relation WHERE relation.property_id = p.property_id AND relation.lessor_id = lessor.lessor_id) OR EXISTS (SELECT 1 FROM rent_lessors_commercial_listings AS relation WHERE relation.property_id = p.property_id AND relation.lessor_id = lessor.lessor_id)))",
            _ => throw new ArgumentOutOfRangeException(nameof(searchTarget), searchTarget, null)
        };

        _readerWriterLock.EnterReadLock();
        try
        {
            using var connection = new SqliteConnection(_connectionStringBuilder.ConnectionString);
            connection.Open();

            using var cmd = connection.CreateCommand();
            if (keyword == "*")
            {
                cmd.CommandText = "SELECT * FROM properties ORDER BY updated_at DESC";
            }
            else
            {
                cmd.CommandText = $"SELECT * FROM properties AS p WHERE {whereClause} ORDER BY p.updated_at DESC";
                cmd.Parameters.AddWithValue("@keyword", "%" + EscapeLikeValue(keyword) + "%");
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
                var kind = reader.GetString(reader.GetOrdinal("property_context_type")) ?? string.Empty;
                if (!string.IsNullOrEmpty(kind))
                {
                    if (Enum.TryParse<PropertyContextType>(kind, out var parsedKind))
                    {
                        enumKind = parsedKind;
                    }
                }

                var item = new ZumenSearch.Application.SearchResults.PropertySearchResultItem(s, enumKind);

                item.SetName(reader.GetString(reader.GetOrdinal("name")) ?? string.Empty);

                item.ThumbnailFilename = reader.GetString(reader.GetOrdinal("thumbnail_filename")) ?? string.Empty;
                item.CreatedAt = reader.GetString(reader.GetOrdinal("created_at")) ?? string.Empty;//Convert.ToString(reader["created_at"]) ?? string.Empty;
                item.UpdatedAt = reader.GetString(reader.GetOrdinal("updated_at")) ?? string.Empty;//Convert.ToString(reader["updated_at"]) ?? string.Empty;

                // TODO: More properties should be added here as needed.


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

    ResultWrapper IRentResidentialPropertyRepository.Upsert(ZumenSearch.Domain.Entities.Rent.Residentials.Property residentialProperty) => UpsertRentResidential(residentialProperty);

    RentResidentialPropertyResultWrapper IRentResidentialPropertyRepository.SelectById(string propertyId, bool duplicate) => SelectRentResidentialById(propertyId, duplicate);

    ResultWrapper IRentResidentialPropertyRepository.Delete(string propertyId) => DeleteRentResidential(propertyId);

    public ZumenSearch.Application.Results.ResultWrapper UpsertRentResidential(ZumenSearch.Domain.Entities.Rent.Residentials.Property property)
    {
        var res = new ResultWrapper();
        var savedPictures = new List<ZumenSearch.Domain.Entities.Rent.Residentials.PropertyPicture>();
        var savedPdfs = new List<ZumenSearch.Domain.Entities.Rent.Residentials.PropertyPdf>();

        if (string.IsNullOrEmpty(property.Id))
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
                cmd.CommandText = "SELECT 1 FROM properties WHERE property_id = @propertyId";
                cmd.Parameters.AddWithValue("@propertyId", property.Id);
                var propertyExists = cmd.ExecuteScalar() is not null;
                if ((property.Status == EntityStatus.New) == propertyExists)
                    throw new InvalidOperationException("The residential property was already created or no longer exists. Reload before saving.");
                cmd.Parameters.Clear();

                // Main property table
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Clear();

                var sqlUpsert = "INSERT INTO properties (property_id, name, property_context_type, thumbnail_filename, fudousan_id, fudousan_id_additional_code, location_prefecture_code, location_prefecture, location_machiaza_id, location_county, location_city, location_ward, location_oaza_cho, location_choume, location_edaban, location_postal_code, location_full, location_latitude, location_longitude, " +
                    "train1_line_code, train1_line_name, train1_station_code, train1_station_name, train1_ekitoho, buss1_stop_name, buss1_Jyousya, buss1_Toho, " +
                    "train2_line_code, train2_line_name, train2_station_code, train2_station_name, train2_ekitoho, buss2_stop_name, buss2_Jyousya, buss2_Toho, " +
                    "train3_line_code, train3_line_name, train3_station_code, train3_station_name, train3_ekitoho, buss3_stop_name, buss3_Jyousya, buss3_Toho, " +
                    "train4_line_code, train4_line_name, train4_station_code, train4_station_name, train4_ekitoho, buss4_stop_name, buss4_Jyousya, buss4_Toho, updated_at) ";

                sqlUpsert += "VALUES (@propertyId, @name, @propertyContextType, @thumbnailPath, @fudousanId, @fudousanIdAdditionalCode, @locPrefId, @locPrefecture, @locMachiazaId, @locCounty, @locCity, @locWard, @locOazaCho, @locChoume, @locEdaban, @locPostalCode, @locLocationFull, @locationLatitude, @locationLongitude, " +
                    "@train1_line_code, @train1_line_name, @train1_station_code, @train1_station_name, @train1_ekitoho, @buss1_stop_name, @buss1_Jyousya, @buss1_Toho, " +
                    "@train2_line_code, @train2_line_name, @train2_station_code, @train2_station_name, @train2_ekitoho, @buss2_stop_name, @buss2_Jyousya, @buss2_Toho, " +
                    "@train3_line_code, @train3_line_name, @train3_station_code, @train3_station_name, @train3_ekitoho, @buss3_stop_name, @buss3_Jyousya, @buss3_Toho, " +
                    "@train4_line_code, @train4_line_name, @train4_station_code, @train4_station_name, @train4_ekitoho, @buss4_stop_name, @buss4_Jyousya, @buss4_Toho, @updated_at) ";

                sqlUpsert += "ON CONFLICT (property_id) DO UPDATE SET " +
                    "name = @name, property_context_type = @propertyContextType, thumbnail_filename = @thumbnailPath, " +
                    "fudousan_id = @fudousanId, fudousan_id_additional_code = @fudousanIdAdditionalCode, " +
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

                cmd.Parameters.AddWithValue("@propertyId", property.Id);
                cmd.Parameters.AddWithValue("@name", property.Name);
                cmd.Parameters.AddWithValue("@propertyContextType", property.PropertyContextType.ToString());
                cmd.Parameters.AddWithValue("@thumbnailPath", property.ThumbnailFilename);

                cmd.Parameters.AddWithValue("@fudousanId", property.FudousanId);
                cmd.Parameters.AddWithValue("@fudousanIdAdditionalCode", property.FudousanIdAdditionalCode);

                // Address/Location
                cmd.Parameters.AddWithValue("@locPrefId", property.Address.Prefecture?.MunicipalityCode ?? string.Empty);
                cmd.Parameters.AddWithValue("@locPrefecture", property.Address.Prefecture?.Name ?? string.Empty);
                cmd.Parameters.AddWithValue("@locMachiazaId", property.Address.MachiazaId ?? string.Empty);
                cmd.Parameters.AddWithValue("@locCounty", property.Address.CountyAndCity?.County ?? string.Empty);
                cmd.Parameters.AddWithValue("@locCity", property.Address.CountyAndCity?.City ?? string.Empty);
                cmd.Parameters.AddWithValue("@locWard", property.Address.WardAndOaza?.Ward ?? string.Empty);
                cmd.Parameters.AddWithValue("@locOazaCho", property.Address.WardAndOaza?.Oaza ?? string.Empty);
                cmd.Parameters.AddWithValue("@locChoume", property.Address.Choume?.Chou ?? string.Empty);
                cmd.Parameters.AddWithValue("@locEdaban", property.Address.Edaban ?? string.Empty);
                cmd.Parameters.AddWithValue("@locPostalCode", property.Address.PostalCode ?? string.Empty);
                cmd.Parameters.AddWithValue("@locLocationFull", property.Address.AddressFull ?? string.Empty);
                cmd.Parameters.AddWithValue("@locationLatitude", property.Address.LocationLatitude);
                cmd.Parameters.AddWithValue("@locationLongitude", property.Address.LocationLongitude);
                // Train1
                cmd.Parameters.AddWithValue("@train1_line_code", property.Train1.RailLine.LineCode);
                cmd.Parameters.AddWithValue("@train1_line_name", property.Train1.RailLine.LineName);
                cmd.Parameters.AddWithValue("@train1_station_code", property.Train1.RailStation.StationCode);
                cmd.Parameters.AddWithValue("@train1_station_name", property.Train1.RailStation.StationName);
                cmd.Parameters.AddWithValue("@train1_ekitoho", property.Train1.EkiToho); //int
                // Buss1
                cmd.Parameters.AddWithValue("@buss1_stop_name", property.BusStop1);
                cmd.Parameters.AddWithValue("@buss1_Jyousya", property.BusJyousya1);
                cmd.Parameters.AddWithValue("@buss1_Toho", property.BusStopToho1);

                // Train2
                cmd.Parameters.AddWithValue("@train2_line_code", property.Train2.RailLine.LineCode);
                cmd.Parameters.AddWithValue("@train2_line_name", property.Train2.RailLine.LineName);
                cmd.Parameters.AddWithValue("@train2_station_code", property.Train2.RailStation.StationCode);
                cmd.Parameters.AddWithValue("@train2_station_name", property.Train2.RailStation.StationName);
                cmd.Parameters.AddWithValue("@train2_ekitoho", property.Train2.EkiToho); //int
                // Buss2
                cmd.Parameters.AddWithValue("@buss2_stop_name", property.BusStop2);
                cmd.Parameters.AddWithValue("@buss2_Jyousya", property.BusJyousya2);
                cmd.Parameters.AddWithValue("@buss2_Toho", property.BusStopToho2);

                // Train3
                cmd.Parameters.AddWithValue("@train3_line_code", property.Train3.RailLine.LineCode);
                cmd.Parameters.AddWithValue("@train3_line_name", property.Train3.RailLine.LineName);
                cmd.Parameters.AddWithValue("@train3_station_code", property.Train3.RailStation.StationCode);
                cmd.Parameters.AddWithValue("@train3_station_name", property.Train3.RailStation.StationName);
                cmd.Parameters.AddWithValue("@train3_ekitoho", property.Train3.EkiToho); //int
                // Buss3
                cmd.Parameters.AddWithValue("@buss3_stop_name", property.BusStop3);
                cmd.Parameters.AddWithValue("@buss3_Jyousya", property.BusJyousya3);
                cmd.Parameters.AddWithValue("@buss3_Toho", property.BusStopToho3);

                // Train4
                cmd.Parameters.AddWithValue("@train4_line_code", property.Train4.RailLine.LineCode);
                cmd.Parameters.AddWithValue("@train4_line_name", property.Train4.RailLine.LineName);
                cmd.Parameters.AddWithValue("@train4_station_code", property.Train4.RailStation.StationCode);
                cmd.Parameters.AddWithValue("@train4_station_name", property.Train4.RailStation.StationName);
                cmd.Parameters.AddWithValue("@train4_ekitoho", property.Train4.EkiToho); //int
                // Buss4
                cmd.Parameters.AddWithValue("@buss4_stop_name", property.BusStop4);
                cmd.Parameters.AddWithValue("@buss4_Jyousya", property.BusJyousya4);
                cmd.Parameters.AddWithValue("@buss4_Toho", property.BusStopToho4);

                cmd.Parameters.AddWithValue("@updated_at", DateTimeOffset.UtcNow.ToString("s"));
                res.AffectedCount = cmd.ExecuteNonQuery();

                // rent_residentials
                cmd.Parameters.Clear();

                sqlUpsert = "INSERT INTO rent_residentials (property_id, property_kind, is_unit_ownership, property_structure, floor_count_above_ground, floor_count_basement, total_unit_count, built_year_month, remarks, " +
                    "kanri_shutai, kanri_is_maintenance_management, kanri_is_tenant_management, kanri_is_payment_management, kanri_is_other_management, kanri_remark_jisya, kanri_name_of_company_tasya, kanri_contact_info_of_company_tasya, kanri_remark_tasya, kanri_remark_kashinushi, " +
                    "has_autolock, has_elevator, has_security_camera, has_parcel_locker, has_electric, " +
                    "property_electric_kind, electric_detail, has_gas, gas_type, gas_detail, " +
                    "tap_water_type, tap_water_detail, sewerage_type, sewerage_detail, ground_digital, " +
                    "bs_antenna, cs_antenna, catv, usen, internet, " +
                    "fiber, internet_free, tv_antenna_detail, internet_detail, has_parking, " +
                    "parking_contract_type, parking_availability, is_parking_free, parking_fee, parking_fee_tax_type, " +
                    "parking_spaces, parking_distance, parking_notes, has_bicycle_parking, bicycle_parking_availability, " +
                    "is_bicycle_parking_free, bicycle_parking_fee, bicycle_parking_fee_tax_type, bicycle_parking_notes, has_motorcycle_parking, " +
                    "motorcycle_parking_availability, is_motorcycle_parking_free, motorcycle_parking_fee, motorcycle_parking_fee_tax_type, motorcycle_parking_notes) ";
                sqlUpsert += "VALUES (@propertyId, @propertyKind, @isUnitOwnership, @propertyStructure, @floorCountAboveGround, @floorCountBasement, @totalUnitCount, @builtYearMonth, @remarks, " +
                    "@kanri_shutai, @kanri_is_maintenance_management, @kanri_is_tenant_management, @kanri_is_payment_management, @kanri_is_other_management, @kanri_remark_jisya, @kanri_name_of_company_tasya, @kanri_contact_info_of_company_tasya, @kanri_remark_tasya, @kanri_remark_kashinushi, " +
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
                sqlUpsert += "DO UPDATE SET property_kind = @propertyKind, is_unit_ownership = @isUnitOwnership, property_structure = @propertyStructure, floor_count_above_ground = @floorCountAboveGround, floor_count_basement = @floorCountBasement, total_unit_count = @totalUnitCount, built_year_month = @builtYearMonth, remarks = @remarks, " +
                    "kanri_shutai = excluded.kanri_shutai, kanri_is_maintenance_management = excluded.kanri_is_maintenance_management, kanri_is_tenant_management = excluded.kanri_is_tenant_management, kanri_is_payment_management = excluded.kanri_is_payment_management, kanri_is_other_management = excluded.kanri_is_other_management, " +
                    "kanri_remark_jisya = excluded.kanri_remark_jisya, kanri_name_of_company_tasya = excluded.kanri_name_of_company_tasya, kanri_contact_info_of_company_tasya = excluded.kanri_contact_info_of_company_tasya, kanri_remark_tasya = excluded.kanri_remark_tasya, kanri_remark_kashinushi = excluded.kanri_remark_kashinushi, " +
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

                cmd.Parameters.AddWithValue("@propertyId", property.Id);
                cmd.Parameters.AddWithValue("@propertyKind", property.PropertyKind.ToString());
                cmd.Parameters.AddWithValue("@isUnitOwnership", property.IsUnitLevelOwnership ? 1 : 0); // bool to int
                cmd.Parameters.AddWithValue("@propertyStructure", property.PropertyStructure.ToString());
                cmd.Parameters.AddWithValue("@floorCountAboveGround", property.FloorCountAboveGround);// int
                cmd.Parameters.AddWithValue("@floorCountBasement", property.FloorCountBasement);// int
                cmd.Parameters.AddWithValue("@totalUnitCount", property.TotalUnitCount);// int
                cmd.Parameters.AddWithValue("@builtYearMonth", property.BuiltYearAndMonth.ToString("s"));
                cmd.Parameters.AddWithValue("@remarks", property.Remarks);
                cmd.Parameters.AddWithValue("@kanri_shutai", property.KanriShutai.ToString());
                cmd.Parameters.AddWithValue("@kanri_is_maintenance_management", property.KanriIsMaintenanceManagementIfIsKanriJisya ? 1 : 0);
                cmd.Parameters.AddWithValue("@kanri_is_tenant_management", property.KanriIsTenantManagementIfIsKanriJisya ? 1 : 0);
                cmd.Parameters.AddWithValue("@kanri_is_payment_management", property.KanriIsPaymentManagementIfIsKanriJisya ? 1 : 0);
                cmd.Parameters.AddWithValue("@kanri_is_other_management", property.KanriIsOtherManagementIfIsKanriJisya ? 1 : 0);
                cmd.Parameters.AddWithValue("@kanri_remark_jisya", property.KanriRemarkIfIsKanriJisya);
                cmd.Parameters.AddWithValue("@kanri_name_of_company_tasya", property.KanriNameOfCompanyIfIsKanriTasya);
                cmd.Parameters.AddWithValue("@kanri_contact_info_of_company_tasya", property.KanriContactInfoOfCompanyIfIsKanriTasya);
                cmd.Parameters.AddWithValue("@kanri_remark_tasya", property.KanriRemarkIfIsKanriTasya);
                cmd.Parameters.AddWithValue("@kanri_remark_kashinushi", property.KanriRemarkIfIsKanriKashinushi);
                cmd.Parameters.AddWithValue("@facility_has_autolock", property.HasAutolock ? 1 : 0);
                cmd.Parameters.AddWithValue("@facility_has_elevator", property.HasElevator ? 1 : 0);
                cmd.Parameters.AddWithValue("@facility_has_security_camera", property.HasSecurityCamera ? 1 : 0);
                cmd.Parameters.AddWithValue("@facility_has_parcel_locker", property.HasParcelLocker ? 1 : 0);
                cmd.Parameters.AddWithValue("@facility_has_electric", property.HasElectric ? 1 : 0);
                cmd.Parameters.AddWithValue("@facility_property_electric_kind", property.PropertyElectricKind.ToString());
                cmd.Parameters.AddWithValue("@facility_electric_detail", property.ElectricDetail ?? string.Empty);
                cmd.Parameters.AddWithValue("@facility_has_gas", property.HasGas ? 1 : 0);
                cmd.Parameters.AddWithValue("@facility_gas_type", property.GasType.ToString());
                cmd.Parameters.AddWithValue("@facility_gas_detail", property.GasDetail ?? string.Empty);
                cmd.Parameters.AddWithValue("@facility_tap_water_type", property.TapWaterType.ToString());
                cmd.Parameters.AddWithValue("@facility_tap_water_detail", property.TapWaterDetail ?? string.Empty);
                cmd.Parameters.AddWithValue("@facility_sewerage_type", property.SewerageType.ToString());
                cmd.Parameters.AddWithValue("@facility_sewerage_detail", property.SewerageDetail ?? string.Empty);
                cmd.Parameters.AddWithValue("@facility_ground_digital", property.GroundDigital ? 1 : 0);
                cmd.Parameters.AddWithValue("@facility_bs_antenna", property.BsAntenna ? 1 : 0);
                cmd.Parameters.AddWithValue("@facility_cs_antenna", property.CsAntenna ? 1 : 0);
                cmd.Parameters.AddWithValue("@facility_catv", property.Catv ? 1 : 0);
                cmd.Parameters.AddWithValue("@facility_usen", property.Usen ? 1 : 0);
                cmd.Parameters.AddWithValue("@facility_internet", property.Internet ? 1 : 0);
                cmd.Parameters.AddWithValue("@facility_fiber", property.Fiber ? 1 : 0);
                cmd.Parameters.AddWithValue("@facility_internet_free", property.InternetFree ? 1 : 0);
                cmd.Parameters.AddWithValue("@facility_tv_antenna_detail", property.TvAntennaDetail ?? string.Empty);
                cmd.Parameters.AddWithValue("@facility_internet_detail", property.InternetDetail ?? string.Empty);
                cmd.Parameters.AddWithValue("@facility_has_parking", property.HasParking ? 1 : 0);
                cmd.Parameters.AddWithValue("@facility_parking_contract_type", property.ParkingContractType.ToString());
                cmd.Parameters.AddWithValue("@facility_parking_availability", property.ParkingAvailability.ToString());
                cmd.Parameters.AddWithValue("@facility_is_parking_free", property.IsParkingFree ? 1 : 0);
                cmd.Parameters.AddWithValue("@facility_parking_fee", property.ParkingFee ?? string.Empty);
                cmd.Parameters.AddWithValue("@facility_parking_fee_tax_type", property.ParkingFeeTaxType.ToString());
                cmd.Parameters.AddWithValue("@facility_parking_spaces", property.ParkingSpaces ?? string.Empty);
                cmd.Parameters.AddWithValue("@facility_parking_distance", property.ParkingDistance ?? string.Empty);
                cmd.Parameters.AddWithValue("@facility_parking_notes", property.ParkingNotes ?? string.Empty);
                cmd.Parameters.AddWithValue("@facility_has_bicycle_parking", property.HasBicycleParking ? 1 : 0);
                cmd.Parameters.AddWithValue("@facility_bicycle_parking_availability", property.BicycleParkingAvailability.ToString());
                cmd.Parameters.AddWithValue("@facility_is_bicycle_parking_free", property.IsBicycleParkingFree ? 1 : 0);
                cmd.Parameters.AddWithValue("@facility_bicycle_parking_fee", property.BicycleParkingFee ?? string.Empty);
                cmd.Parameters.AddWithValue("@facility_bicycle_parking_fee_tax_type", property.BicycleParkingFeeTaxType.ToString());
                cmd.Parameters.AddWithValue("@facility_bicycle_parking_notes", property.BicycleParkingNotes ?? string.Empty);
                cmd.Parameters.AddWithValue("@facility_has_motorcycle_parking", property.HasMotorcycleParking ? 1 : 0);
                cmd.Parameters.AddWithValue("@facility_motorcycle_parking_availability", property.MotorcycleParkingAvailability.ToString());
                cmd.Parameters.AddWithValue("@facility_is_motorcycle_parking_free", property.IsMotorcycleParkingFree ? 1 : 0);
                cmd.Parameters.AddWithValue("@facility_motorcycle_parking_fee", property.MotorcycleParkingFee ?? string.Empty);
                cmd.Parameters.AddWithValue("@facility_motorcycle_parking_fee_tax_type", property.MotorcycleParkingFeeTaxType.ToString());
                cmd.Parameters.AddWithValue("@facility_motorcycle_parking_notes", property.MotorcycleParkingNotes ?? string.Empty);

                cmd.ExecuteNonQuery();

                cmd.Parameters.Clear();

                // 写真（建物）rent_residential_pictures
                if (property.Pictures.Count > 0)
                {
                    foreach (var pic in property.Pictures)
                    {
                        var sqlUpsertPicture = "INSERT INTO rent_residential_pictures (picture_id, property_id, filename, type, description, is_main) ";
                        sqlUpsertPicture += "VALUES (@pictureId, @propertyId, @filePath, @type, @description, @isMain) ";
                        sqlUpsertPicture += "ON CONFLICT (picture_id) ";
                        sqlUpsertPicture += "DO UPDATE SET filename = @filePath, type = @type, description = @description, is_main = @isMain";

                        cmd.CommandText = sqlUpsertPicture;

                        // ループなので、前のパラメーターをクリアする。
                        cmd.Parameters.Clear();

                        cmd.Parameters.AddWithValue("@pictureId", pic.Id);
                        cmd.Parameters.AddWithValue("@propertyId", property.Id);
                        cmd.Parameters.AddWithValue("@filePath", pic.ImageFilename);
                        cmd.Parameters.AddWithValue("@type", pic.PictureType.ToString());
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
                            savedPictures.Add(pic);
                        }
                    }
                }

                cmd.Parameters.Clear();

                // 物件写真の削除リストを処理
                if (property.PicturesToBeDeleted.Count > 0)
                {
                    foreach (var delp in property.PicturesToBeDeleted)
                    {
                        // 削除
                        cmd.Parameters.Clear();
                        cmd.CommandText = "DELETE FROM rent_residential_pictures WHERE picture_id = @pictureId";
                        cmd.Parameters.AddWithValue("@pictureId", delp.Id);
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
                if (property.Pdfs.Count > 0)
                {
                    foreach (var pic in property.Pdfs)
                    {
                        var sqlUpsertPdf = "INSERT INTO rent_residential_pdfs (pdf_id, property_id, filename, thumbnail_filename, type, description, is_main) ";
                        sqlUpsertPdf += "VALUES (@pdfId, @propertyId, @filePath, @thumbnailPath, @type, @description, @isMain) ";
                        sqlUpsertPdf += "ON CONFLICT (pdf_id) ";
                        sqlUpsertPdf += "DO UPDATE SET filename = @filePath, thumbnail_filename = @thumbnailPath, type = @type, description = @description, is_main = @isMain";

                        cmd.CommandText = sqlUpsertPdf;

                        // ループなので、前のパラメーターをクリアする。
                        cmd.Parameters.Clear();

                        cmd.Parameters.AddWithValue("@pdfId", pic.Id);
                        cmd.Parameters.AddWithValue("@propertyId", property.Id);
                        cmd.Parameters.AddWithValue("@filePath", pic.PdfFilename);
                        cmd.Parameters.AddWithValue("@thumbnailPath", pic.ThumbnailFilename);
                        cmd.Parameters.AddWithValue("@type", pic.PdfType.ToString());
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
                            savedPdfs.Add(pic);
                        }
                    }
                }

                cmd.Parameters.Clear();

                // PDF（建物）の削除リストを処理
                if (property.PdfsToBeDeleted.Count > 0)
                {
                    foreach (var delp in property.PdfsToBeDeleted)
                    {
                        // 削除
                        cmd.Parameters.Clear();
                        cmd.CommandText = "DELETE FROM rent_residential_pdfs WHERE pdf_id = @pdfId";
                        cmd.Parameters.AddWithValue("@pdfId", delp.Id);
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

                // 貸主（建物）rent_lessors_properties
                if (property.Lessors.Count > 0)
                {
                    foreach (var psn in property.Lessors)
                    {
                        var sqlUpsertLessor = "INSERT INTO rent_lessors_properties (lessor_id, property_id, property_context_type) ";
                        sqlUpsertLessor += "VALUES (@lessor_id, @property_id, @property_context_type) ";
                        sqlUpsertLessor += "ON CONFLICT (lessor_id, property_id) ";
                        sqlUpsertLessor += "DO NOTHING";

                        cmd.CommandText = sqlUpsertLessor;
                        cmd.Parameters.Clear();
                        cmd.Parameters.AddWithValue("@lessor_id", psn.Id);
                        cmd.Parameters.AddWithValue("@property_id", property.Id);
                        cmd.Parameters.AddWithValue("@property_context_type", property.PropertyContextType.ToString());
                        cmd.ExecuteNonQuery();
                    }
                }

                cmd.Parameters.Clear();

                // 貸主（建物）の削除リストを処理
                if (property.LessorsToBeDeleted.Count > 0)
                {
                    foreach (var psn in property.LessorsToBeDeleted)
                    {
                        cmd.Parameters.Clear();
                        cmd.CommandText = "DELETE FROM rent_lessors_properties WHERE lessor_id = @lessorId AND property_id = @propertyId";
                        cmd.Parameters.AddWithValue("@lessorId", psn.Id);
                        cmd.Parameters.AddWithValue("@propertyId", property.Id);
                        var sqlResult = cmd.ExecuteNonQuery();
                        if (sqlResult > 0)
                        {
                            Debug.WriteLine("Lessor deleted");
                        }
                    }
                }

                cmd.Parameters.Clear();

                // 宅建業者（建物）brokers_properties
                if (property.Brokers.Count > 0)
                {
                    foreach (var psn in property.Brokers)
                    {
                        var sqlUpsertBroker = "INSERT INTO brokers_properties (broker_id, property_id, property_context_type) ";
                        sqlUpsertBroker += "VALUES (@broker_id, @property_id, @property_context_type) ";
                        sqlUpsertBroker += "ON CONFLICT (broker_id, property_id) ";
                        sqlUpsertBroker += "DO NOTHING";

                        cmd.CommandText = sqlUpsertBroker;
                        cmd.Parameters.Clear();
                        cmd.Parameters.AddWithValue("@broker_id", psn.Id);
                        cmd.Parameters.AddWithValue("@property_id", property.Id);
                        cmd.Parameters.AddWithValue("@property_context_type", property.PropertyContextType.ToString());
                        cmd.ExecuteNonQuery();
                    }
                }

                cmd.Parameters.Clear();

                // 宅建業者（建物）の削除リストを処理
                if (property.BrokersToBeDeleted.Count > 0)
                {
                    foreach (var psn in property.BrokersToBeDeleted)
                    {
                        cmd.Parameters.Clear();
                        cmd.CommandText = "DELETE FROM brokers_properties WHERE broker_id = @brokerId AND property_id = @propertyId";
                        cmd.Parameters.AddWithValue("@brokerId", psn.Id);
                        cmd.Parameters.AddWithValue("@propertyId", property.Id);
                        var sqlResult = cmd.ExecuteNonQuery();
                        if (sqlResult > 0)
                        {
                            Debug.WriteLine("Broker deleted");
                        }
                    }
                }

                cmd.Parameters.Clear();

                // 部屋 rent_residential_listings
                if (property.ListingsToBeSaved.Count > 0)
                {
                    foreach (var unit in property.ListingsToBeSaved)
                    {
                        // Reuse the code for upsert listing.
                        _ = DoUpsertRentResidentialListing(cmd, property.Id, unit);
                    }
                }

                cmd.Parameters.Clear();

                // 部屋の削除リストを処理
                if (property.ListingsToBeDeleted.Count > 0)
                {
                    foreach (var delr in property.ListingsToBeDeleted)
                    {
                        // 削除
                        cmd.Parameters.Clear();
                        cmd.CommandText = "DELETE FROM rent_residential_listings WHERE listing_id = @listingId";
                        cmd.Parameters.AddWithValue("@listingId", delr);
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

                property.LessorsToBeDeleted.Clear();
                property.BrokersToBeDeleted.Clear();
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

        if (res.IsError) return res;

        foreach (var picture in savedPictures)
        {
            picture.SetStatus(EntityStatus.Saved);
            picture.SetIsModified(false);
        }
        foreach (var pdf in savedPdfs)
        {
            pdf.SetStatus(EntityStatus.Saved);
            pdf.SetIsModified(false);
        }

        //Debug.WriteLine(string.Format("{0} Entries Inserted to DB", res.AffectedCount.ToString()));

        property.SetIsModified(false);
        property.SetStatus(EntityStatus.Saved);

        return res;
    }

    public RentResidentialPropertyResultWrapper SelectRentResidentialById(string propertyId, bool duplicate = false)
    {
        var res = new RentResidentialPropertyResultWrapper();

        if (string.IsNullOrEmpty(propertyId))
        {
            res.IsError = true;
            // TODO:
            return res;
        }

        ZumenSearch.Domain.Entities.Rent.Residentials.Property property;

        if (duplicate)
        {
            property = new ZumenSearch.Domain.Entities.Rent.Residentials.Property(Guid.CreateVersion7().ToString("N"), EntityStatus.New);
        }
        else
        {
            property = new ZumenSearch.Domain.Entities.Rent.Residentials.Property(propertyId, EntityStatus.Saved);
        }

        _readerWriterLock.EnterReadLock();
        try
        {
            using var connection = new SqliteConnection(_connectionStringBuilder.ConnectionString);
            connection.Open();

            using var cmd = connection.CreateCommand();
            cmd.CommandText = "SELECT properties.name as propertyName, " +
                "properties.property_context_type as propertyContextType, " +
                "properties.fudousan_id as fudousanId, " +
                "properties.fudousan_id_additional_code as fudousanIdAdditionalCode, " +
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

                "rent_residentials.property_kind as resiPropertyKind, " +
                "rent_residentials.is_unit_ownership as resiUnitOwnership, " +
                "rent_residentials.property_structure as resiPropertyStructure, " +
                "rent_residentials.floor_count_above_ground as resiAboveGroundFloorCount, " +
                "rent_residentials.floor_count_basement as resiBasementFloorCount, " +
                "rent_residentials.total_unit_count as resiTotalUnitCount, " +
                "rent_residentials.built_year_month as resiBuiltYearMonth, " +
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
                "rent_residentials.kanri_shutai as kanri_shutai, " +
                "rent_residentials.kanri_is_maintenance_management as kanri_is_maintenance_management, " +
                "rent_residentials.kanri_is_tenant_management as kanri_is_tenant_management, " +
                "rent_residentials.kanri_is_payment_management as kanri_is_payment_management, " +
                "rent_residentials.kanri_is_other_management as kanri_is_other_management, " +
                "rent_residentials.kanri_remark_jisya as kanri_remark_jisya, " +
                "rent_residentials.kanri_name_of_company_tasya as kanri_name_of_company_tasya, " +
                "rent_residentials.kanri_contact_info_of_company_tasya as kanri_contact_info_of_company_tasya, " +
                "rent_residentials.kanri_remark_tasya as kanri_remark_tasya, " +
                "rent_residentials.kanri_remark_kashinushi as kanri_remark_kashinushi, " +

                //"rent_residentials.updated_at as UpdatedAt, " +
                "properties.property_id as propertyId " +
                "FROM rent_residentials INNER JOIN properties USING (property_id) WHERE properties.property_id = @propertyId";
            cmd.Parameters.Clear();
            cmd.Parameters.AddWithValue("@propertyId", propertyId);

            bool isFound = false;

            using (var reader = cmd.ExecuteReader())
            {
                if (reader.Read())
                {
                    var pId = Convert.ToString(reader["propertyId"], CultureInfo.InvariantCulture);

                    if (!propertyId.Equals(pId, StringComparison.Ordinal))
                    {
                        Debug.WriteLine("DataAccess::SelectRentResidentialsById: propertyId is null or empty for a rent residential entry.");
                        //continue;
                    }

                    isFound = true;

                    property.SetName(Convert.ToString(reader["propertyName"], CultureInfo.InvariantCulture) ?? "");

                    // Address/Location
                    property.Address.SetMachiazaId(Convert.ToString(reader["locMachiazaId"], CultureInfo.InvariantCulture) ?? "");
                    property.Address.SetPrefectureByMunicipalityCode(Convert.ToString(reader["locPrefId"], CultureInfo.InvariantCulture) ?? "", Convert.ToString(reader["locPrefecture"], CultureInfo.InvariantCulture) ?? "");
                    property.Address.SetCountyAndCity(property.Address.MachiazaId, Convert.ToString(reader["locCounty"], CultureInfo.InvariantCulture) ?? "", Convert.ToString(reader["locCity"], CultureInfo.InvariantCulture) ?? "");
                    property.Address.SetWardAndOaza(property.Address.MachiazaId, Convert.ToString(reader["locWard"], CultureInfo.InvariantCulture) ?? "", Convert.ToString(reader["locOazaCho"], CultureInfo.InvariantCulture) ?? "");
                    property.Address.SetChoume(property.Address.MachiazaId, Convert.ToString(reader["locChoume"], CultureInfo.InvariantCulture) ?? "");
                    property.Address.SetEdaban(Convert.ToString(reader["locEdaban"], CultureInfo.InvariantCulture) ?? "");
                    property.Address.SetPostalCode(Convert.ToString(reader["locPostalCode"], CultureInfo.InvariantCulture) ?? string.Empty);
                    property.Address.SetLocationLatitude(Convert.ToString(reader["locationLatitude"], CultureInfo.InvariantCulture) ?? string.Empty);
                    property.Address.SetLocationLongitude(Convert.ToString(reader["locationLongitude"], CultureInfo.InvariantCulture) ?? string.Empty);

                    property.Train1.SetRailLine(Convert.ToString(reader["train1LineCode"], CultureInfo.InvariantCulture) ?? "", Convert.ToString(reader["train1LineName"], CultureInfo.InvariantCulture) ?? "");
                    property.Train1.SetRailStation(property.Train1.RailLine.LineCode, Convert.ToString(reader["train1StationCode"], CultureInfo.InvariantCulture) ?? "", Convert.ToString(reader["train1StationName"], CultureInfo.InvariantCulture) ?? "");
                    property.SetBusStop1(Convert.ToString(reader["buss1StopName"], CultureInfo.InvariantCulture) ?? "");
                    property.SetBusJyousya1(Convert.ToString(reader["buss1Jyousya"], CultureInfo.InvariantCulture) ?? "");
                    property.SetBusStopToho1(Convert.ToString(reader["buss1Toho"], CultureInfo.InvariantCulture) ?? "");
                    property.Train1.SetEkiToho(Convert.ToInt32(reader["train1EkiToho"], CultureInfo.InvariantCulture));

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

                    property.SetPropertyKindFromString(Convert.ToString(reader["resiPropertyKind"], CultureInfo.InvariantCulture) ?? "");
                    property.SetIsUnitLevelOwnership(Convert.ToInt32(reader["resiUnitOwnership"], CultureInfo.InvariantCulture) != 0); // int to bool
                    property.SetStructureTypeFromString(Convert.ToString(reader["resiPropertyStructure"], CultureInfo.InvariantCulture) ?? "");
                    property.SetFloorCountAboveGround(Convert.ToInt32(reader["resiAboveGroundFloorCount"], CultureInfo.InvariantCulture));
                    property.SetFloorCountBasement(Convert.ToInt32(reader["resiBasementFloorCount"], CultureInfo.InvariantCulture));
                    property.SetTotalUnitCount(Convert.ToInt32(reader["resiTotalUnitCount"], CultureInfo.InvariantCulture));
                    property.SetBuiltYearAndMonthFromString(Convert.ToString(reader["resiBuiltYearMonth"], CultureInfo.InvariantCulture) ?? "");
                    property.SetFudousanId(Convert.ToString(reader["fudousanId"], CultureInfo.InvariantCulture) ?? "");
                    property.SetFudousanIdAdditionalCode(Convert.ToString(reader["fudousanIdAdditionalCode"], CultureInfo.InvariantCulture) ?? "");
                    property.SetRemarks(Convert.ToString(reader["resiRemarks"], CultureInfo.InvariantCulture) ?? "");
                    property.SetKanriShutaiFromString(reader.GetString(reader.GetOrdinal("kanri_shutai")));
                    property.SetKanriIsMaintenanceManagementIfIsKanriJisya(Convert.ToInt32(reader["kanri_is_maintenance_management"], CultureInfo.InvariantCulture) != 0);
                    property.SetKanriIsTenantManagementIfIsKanriJisya(Convert.ToInt32(reader["kanri_is_tenant_management"], CultureInfo.InvariantCulture) != 0);
                    property.SetKanriIsPaymentManagementIfIsKanriJisya(Convert.ToInt32(reader["kanri_is_payment_management"], CultureInfo.InvariantCulture) != 0);
                    property.SetKanriIsOtherManagementIfIsKanriJisya(Convert.ToInt32(reader["kanri_is_other_management"], CultureInfo.InvariantCulture) != 0);
                    property.SetKanriRemarkIfIsKanriJisya(reader.GetString(reader.GetOrdinal("kanri_remark_jisya")));
                    property.SetKanriNameOfCompanyIfIsKanriTasya(reader.GetString(reader.GetOrdinal("kanri_name_of_company_tasya")));
                    property.SetKanriContactInfoOfCompanyIfIsKanriTasya(reader.GetString(reader.GetOrdinal("kanri_contact_info_of_company_tasya")));
                    property.SetKanriRemarkIfIsKanriTasya(reader.GetString(reader.GetOrdinal("kanri_remark_tasya")));
                    property.SetKanriRemarkIfIsKanriKashinushi(reader.GetString(reader.GetOrdinal("kanri_remark_kashinushi")));

                    property.HasAutolock = Convert.ToInt32(reader["has_autolock"], CultureInfo.InvariantCulture) != 0;
                    property.HasElevator = Convert.ToInt32(reader["has_elevator"], CultureInfo.InvariantCulture) != 0;
                    property.HasSecurityCamera = Convert.ToInt32(reader["has_security_camera"], CultureInfo.InvariantCulture) != 0;
                    property.HasParcelLocker = Convert.ToInt32(reader["has_parcel_locker"], CultureInfo.InvariantCulture) != 0;
                    property.HasElectric = Convert.ToInt32(reader["has_electric"], CultureInfo.InvariantCulture) != 0;
                    var electricKindValue = reader["property_electric_kind"];
                    if (electricKindValue is string electricKindText && Enum.TryParse<ZumenSearch.Domain.Entities.Rent.Residentials.Property.EnumElectricType>(electricKindText, out var electricKind) && Enum.IsDefined(electricKind))
                    {
                        property.PropertyElectricKind = electricKind;
                    }
                    else
                    {
                        Debug.WriteLine($"Invalid property_electric_kind value: {(electricKindValue is DBNull ? "<NULL>" : electricKindValue)}");
                    }
                    property.ElectricDetail = reader.GetString(reader.GetOrdinal("electric_detail"));
                    property.HasGas = Convert.ToInt32(reader["has_gas"], CultureInfo.InvariantCulture) != 0;
                    var gasTypeValue = reader["gas_type"];
                    if (gasTypeValue is string gasTypeText && Enum.TryParse<ZumenSearch.Domain.Entities.Rent.Residentials.FacilityGasType>(gasTypeText, out var gasType) && Enum.IsDefined(gasType))
                    {
                        property.GasType = gasType;
                    }
                    else
                    {
                        Debug.WriteLine($"Invalid gas_type value: {(gasTypeValue is DBNull ? "<NULL>" : gasTypeValue)}");
                    }
                    property.GasDetail = reader.GetString(reader.GetOrdinal("gas_detail"));
                    var tapWaterTypeValue = reader["tap_water_type"];
                    if (tapWaterTypeValue is string tapWaterTypeText && Enum.TryParse<ZumenSearch.Domain.Entities.Rent.Residentials.FacilityTapWaterType>(tapWaterTypeText, out var tapWaterType) && Enum.IsDefined(tapWaterType))
                    {
                        property.TapWaterType = tapWaterType;
                    }
                    else
                    {
                        Debug.WriteLine($"Invalid tap_water_type value: {(tapWaterTypeValue is DBNull ? "<NULL>" : tapWaterTypeValue)}");
                    }
                    property.TapWaterDetail = reader.GetString(reader.GetOrdinal("tap_water_detail"));
                    var sewerageTypeValue = reader["sewerage_type"];
                    if (sewerageTypeValue is string sewerageTypeText && Enum.TryParse<ZumenSearch.Domain.Entities.Rent.Residentials.FacilitySewerageType>(sewerageTypeText, out var sewerageType) && Enum.IsDefined(sewerageType))
                    {
                        property.SewerageType = sewerageType;
                    }
                    else
                    {
                        Debug.WriteLine($"Invalid sewerage_type value: {(sewerageTypeValue is DBNull ? "<NULL>" : sewerageTypeValue)}");
                    }
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
                    var parkingContractValue = reader["parking_contract_type"];
                    if (parkingContractValue is string parkingContractText && Enum.TryParse<ZumenSearch.Domain.Entities.Rent.Residentials.FacilityParkingContractType>(parkingContractText, out var parkingContractType) && Enum.IsDefined(parkingContractType))
                    {
                        property.ParkingContractType = parkingContractType;
                    }
                    else
                    {
                        Debug.WriteLine($"Invalid parking_contract_type value: {(parkingContractValue is DBNull ? "<NULL>" : parkingContractValue)}");
                    }
                    var parkingAvailabilityValue = reader["parking_availability"];
                    if (parkingAvailabilityValue is string parkingAvailabilityText && Enum.TryParse<ZumenSearch.Domain.Entities.Rent.Residentials.FacilityAvailabilityType>(parkingAvailabilityText, out var parkingAvailability) && Enum.IsDefined(parkingAvailability))
                    {
                        property.ParkingAvailability = parkingAvailability;
                    }
                    else
                    {
                        Debug.WriteLine($"Invalid parking_availability value: {(parkingAvailabilityValue is DBNull ? "<NULL>" : parkingAvailabilityValue)}");
                    }
                    property.IsParkingFree = Convert.ToInt32(reader["is_parking_free"], CultureInfo.InvariantCulture) != 0;
                    property.ParkingFee = reader.GetString(reader.GetOrdinal("parking_fee"));
                    var parkingFeeTaxValue = reader["parking_fee_tax_type"];
                    if (parkingFeeTaxValue is string parkingFeeTaxText && Enum.TryParse<ZumenSearch.Domain.Entities.Rent.Residentials.FacilityTaxType>(parkingFeeTaxText, out var parkingFeeTaxType) && Enum.IsDefined(parkingFeeTaxType))
                    {
                        property.ParkingFeeTaxType = parkingFeeTaxType;
                    }
                    else
                    {
                        Debug.WriteLine($"Invalid parking_fee_tax_type value: {(parkingFeeTaxValue is DBNull ? "<NULL>" : parkingFeeTaxValue)}");
                    }
                    property.ParkingSpaces = reader.GetString(reader.GetOrdinal("parking_spaces"));
                    property.ParkingDistance = reader.GetString(reader.GetOrdinal("parking_distance"));
                    property.ParkingNotes = reader.GetString(reader.GetOrdinal("parking_notes"));
                    property.HasBicycleParking = Convert.ToInt32(reader["has_bicycle_parking"], CultureInfo.InvariantCulture) != 0;
                    var bicycleParkingAvailabilityValue = reader["bicycle_parking_availability"];
                    if (bicycleParkingAvailabilityValue is string bicycleParkingAvailabilityText && Enum.TryParse<ZumenSearch.Domain.Entities.Rent.Residentials.FacilityAvailabilityType>(bicycleParkingAvailabilityText, out var bicycleParkingAvailability) && Enum.IsDefined(bicycleParkingAvailability))
                    {
                        property.BicycleParkingAvailability = bicycleParkingAvailability;
                    }
                    else
                    {
                        Debug.WriteLine($"Invalid bicycle_parking_availability value: {(bicycleParkingAvailabilityValue is DBNull ? "<NULL>" : bicycleParkingAvailabilityValue)}");
                    }
                    property.IsBicycleParkingFree = Convert.ToInt32(reader["is_bicycle_parking_free"], CultureInfo.InvariantCulture) != 0;
                    property.BicycleParkingFee = reader.GetString(reader.GetOrdinal("bicycle_parking_fee"));
                    var bicycleParkingFeeTaxValue = reader["bicycle_parking_fee_tax_type"];
                    if (bicycleParkingFeeTaxValue is string bicycleParkingFeeTaxText && Enum.TryParse<ZumenSearch.Domain.Entities.Rent.Residentials.FacilityTaxType>(bicycleParkingFeeTaxText, out var bicycleParkingFeeTaxType) && Enum.IsDefined(bicycleParkingFeeTaxType))
                    {
                        property.BicycleParkingFeeTaxType = bicycleParkingFeeTaxType;
                    }
                    else
                    {
                        Debug.WriteLine($"Invalid bicycle_parking_fee_tax_type value: {(bicycleParkingFeeTaxValue is DBNull ? "<NULL>" : bicycleParkingFeeTaxValue)}");
                    }
                    property.BicycleParkingNotes = reader.GetString(reader.GetOrdinal("bicycle_parking_notes"));
                    property.HasMotorcycleParking = Convert.ToInt32(reader["has_motorcycle_parking"], CultureInfo.InvariantCulture) != 0;
                    var motorcycleParkingAvailabilityValue = reader["motorcycle_parking_availability"];
                    if (motorcycleParkingAvailabilityValue is string motorcycleParkingAvailabilityText && Enum.TryParse<ZumenSearch.Domain.Entities.Rent.Residentials.FacilityAvailabilityType>(motorcycleParkingAvailabilityText, out var motorcycleParkingAvailability) && Enum.IsDefined(motorcycleParkingAvailability))
                    {
                        property.MotorcycleParkingAvailability = motorcycleParkingAvailability;
                    }
                    else
                    {
                        Debug.WriteLine($"Invalid motorcycle_parking_availability value: {(motorcycleParkingAvailabilityValue is DBNull ? "<NULL>" : motorcycleParkingAvailabilityValue)}");
                    }
                    property.IsMotorcycleParkingFree = Convert.ToInt32(reader["is_motorcycle_parking_free"], CultureInfo.InvariantCulture) != 0;
                    property.MotorcycleParkingFee = reader.GetString(reader.GetOrdinal("motorcycle_parking_fee"));
                    var motorcycleParkingFeeTaxValue = reader["motorcycle_parking_fee_tax_type"];
                    if (motorcycleParkingFeeTaxValue is string motorcycleParkingFeeTaxText && Enum.TryParse<ZumenSearch.Domain.Entities.Rent.Residentials.FacilityTaxType>(motorcycleParkingFeeTaxText, out var motorcycleParkingFeeTaxType) && Enum.IsDefined(motorcycleParkingFeeTaxType))
                    {
                        property.MotorcycleParkingFeeTaxType = motorcycleParkingFeeTaxType;
                    }
                    else
                    {
                        Debug.WriteLine($"Invalid motorcycle_parking_fee_tax_type value: {(motorcycleParkingFeeTaxValue is DBNull ? "<NULL>" : motorcycleParkingFeeTaxValue)}");
                    }
                    property.MotorcycleParkingNotes = reader.GetString(reader.GetOrdinal("motorcycle_parking_notes"));

                    property.SetIsModified(false);

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
            cmd.Parameters.AddWithValue("@propertyId", propertyId);
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    var picid = Convert.ToString(reader["picture_id"], CultureInfo.InvariantCulture) ?? string.Empty;

                    // If duplicate is true, generate a new picture ID; otherwise, use the existing one.
                    picid = duplicate ? Guid.CreateVersion7().ToString("N") : picid;

                    var picpath = Convert.ToString(reader["filename"], CultureInfo.InvariantCulture) ?? string.Empty;
                    if (!string.IsNullOrEmpty(picid) && !string.IsNullOrEmpty(picpath))
                    {
                        var rlpic = new ZumenSearch.Domain.Entities.Rent.Residentials.PropertyPicture(picid, picpath, EntityStatus.Saved)
                        {
                            Description = Convert.ToString(reader["description"], CultureInfo.InvariantCulture) ?? string.Empty,
                        };

                        var strType = Convert.ToString(reader["type"], CultureInfo.InvariantCulture) ?? string.Empty;
                        if (!Enum.TryParse<ZumenSearch.Domain.Enums.Rent.Residentials.PropertyPictureType>(strType, out var pictureType) || !Enum.IsDefined(pictureType))
                        {
                            Debug.WriteLine($"Invalid residential property Picture type: {strType}. Defaulting to Unspecified.");
                            pictureType = ZumenSearch.Domain.Enums.Rent.Residentials.PropertyPictureType.Unspecified;
                        }
                        rlpic.PictureType = pictureType;
                        rlpic.IsMain = Convert.ToInt32(reader["is_main"], CultureInfo.InvariantCulture) != 0; // int to bool
                        rlpic.SetIsModified(false);

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
            cmd.Parameters.AddWithValue("@propertyId", propertyId);
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    var pdfid = Convert.ToString(reader["pdf_id"], CultureInfo.InvariantCulture) ?? string.Empty;

                    // If duplicate is true, generate a new PDF ID; otherwise, use the existing one.
                    pdfid = duplicate ? Guid.CreateVersion7().ToString("N") : pdfid;

                    var pdfpath = Convert.ToString(reader["filename"], CultureInfo.InvariantCulture) ?? string.Empty;
                    var thumbpath = Convert.ToString(reader["thumbnail_filename"], CultureInfo.InvariantCulture) ?? string.Empty;
                    if (!string.IsNullOrWhiteSpace(pdfid) && !string.IsNullOrWhiteSpace(pdfpath) && !string.IsNullOrWhiteSpace(thumbpath))
                    {
                        var rlpdf = new ZumenSearch.Domain.Entities.Rent.Residentials.PropertyPdf(pdfid, pdfpath, thumbpath, EntityStatus.Saved)
                        {
                            Description = Convert.ToString(reader["description"], CultureInfo.InvariantCulture) ?? string.Empty
                        };

                        var strType = Convert.ToString(reader["type"], CultureInfo.InvariantCulture) ?? string.Empty;
                        if (!Enum.TryParse<ZumenSearch.Domain.Enums.Rent.Residentials.PropertyPdfType>(strType, out var pdfType) || !Enum.IsDefined(pdfType))
                        {
                            Debug.WriteLine($"Invalid residential property PDF type: {strType}. Defaulting to Unspecified.");
                            pdfType = ZumenSearch.Domain.Enums.Rent.Residentials.PropertyPdfType.Unspecified;
                        }
                        rlpdf.PdfType = pdfType;
                        rlpdf.IsMain = Convert.ToInt32(reader["is_main"], CultureInfo.InvariantCulture) != 0; // int to bool
                        rlpdf.SetIsModified(false);

                        property.Pdfs.Add(rlpdf);
                    }
                    else
                    {
                        throw new InvalidDataException("Residential property PDF has an empty ID, filename or thumbnail filename.");
                    }
                }
            }

            // 貸主（建物）
            var lessorIdList = new List<string>();
            cmd.CommandText = "SELECT * FROM rent_lessors_properties WHERE property_id = @propertyId";
            cmd.Parameters.Clear();
            cmd.Parameters.AddWithValue("@propertyId", propertyId);
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
                    cmd.CommandText = "SELECT lessor_id, name, person_kind, name_last, name_first, name_company, name_company_type, name_company_type_position, phone_number, mobile_phone_number, contact_name, location_postal_code, location_full, location_machiaza_id, location_prefecture, location_county, location_city, location_ward, location_oaza, location_choume, location_edaban, remarks FROM rent_lessors WHERE lessor_id = @lessorId";
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
            cmd.CommandText = "SELECT * FROM brokers_properties WHERE property_id = @propertyId";
            cmd.Parameters.Clear();
            cmd.Parameters.AddWithValue("@propertyId", propertyId);
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
                    cmd.CommandText = "SELECT broker_id, name, person_kind, name_last, name_first, name_company, name_company_type, name_company_type_position, phone_number, mobile_phone_number, contact_name, location_postal_code, location_full, location_machiaza_id, location_prefecture, location_county, location_city, location_ward, location_oaza, location_choume, location_edaban, remarks FROM brokers WHERE broker_id = @brokerId";
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

            // Do not duplicate the listings if this is a duplicate operation; otherwise, retrieve the listings for the property.
            if (!duplicate)
            {
                // 部屋
                cmd.CommandText = "SELECT listing_id, name FROM rent_residential_listings WHERE property_id = @propertyId";
                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("@propertyId", propertyId);
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var listingId = reader.GetString(reader.GetOrdinal("listing_id"));
                        var listingName = reader.GetString(reader.GetOrdinal("name"));
                        if (string.IsNullOrWhiteSpace(listingId)) throw new InvalidOperationException("Residential listing ID is empty.");
                        property.ListingSummaries.Add(new ZumenSearch.Domain.Entities.Rent.Residentials.ListingSummary(listingId, listingName));
                    }
                }
            }

            // Reset property Isdirty flag.
            if (duplicate)
            {
                property.SetStatus(EntityStatus.New);
            }
            else
            {
                property.SetStatus(EntityStatus.Saved);
            }
            property.SetIsModified(duplicate);

            res.Property = property;
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

    public ZumenSearch.Application.Results.ResultWrapper DeleteRentResidential(string propertyId)
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
                cmd.CommandText = "DELETE FROM properties WHERE property_id = @propertyId;";
                cmd.Parameters.AddWithValue("@propertyId", propertyId);
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

    ResultWrapper IRentResidentialListingRepository.Upsert(string propertyId, ZumenSearch.Domain.Entities.Rent.Residentials.Listing listing) => UpsertRentResidentialListing(propertyId, listing);

    ZumenSearch.Application.Results.ListingSearchResultWrapper IRentResidentialListingRepository.Search(ZumenSearch.Application.SearchResults.RentResidentialListingSearchCriteria criteria) => SelectRentResidentialListings(criteria);

    ResultWrapper IRentResidentialListingRepository.Delete(string listingId) => DeleteRentResidentialListing(listingId);

    RentResidentialListingResultWrapper IRentResidentialListingRepository.SelectById(string propertyId, string listingId, bool duplicate) => SelectRentResidentialListingById(propertyId, listingId, duplicate);

    private static int DoUpsertRentResidentialListing(SqliteCommand cmd, string propertyId, ZumenSearch.Domain.Entities.Rent.Residentials.Listing listing)
    {
        if (string.IsNullOrWhiteSpace(propertyId) || string.IsNullOrWhiteSpace(listing.Id) || !string.Equals(listing.PropertyId, propertyId, StringComparison.Ordinal))
        {
            throw new InvalidOperationException("Residential listing property ID does not match the supplied property ID.");
        }

        cmd.CommandText = """
            INSERT INTO rent_residential_listings (
                listing_id, property_id, is_property_unit_ownership, name, fudousan_id_additional_code,
                floor_plan_type, floor_plan_extra_type, floor_area, floor_type, floor_number,
                is_corner_room, main_exposure_direction, current_status,
                is_available_for_rent, available_from_month, available_from_period,
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
                @listing_id, @property_id, @is_property_unit_ownership, @name, @fudousan_id_additional_code,
                @floor_plan_type, @floor_plan_extra_type, @floor_area, @floor_type, @floor_number,
                @is_corner_room, @main_exposure_direction, @current_status,
                @is_available_for_rent, @available_from_month, @available_from_period,
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
                fudousan_id_additional_code = @fudousan_id_additional_code,
                floor_plan_type = @floor_plan_type,
                floor_plan_extra_type = @floor_plan_extra_type,
                floor_area = @floor_area,
                floor_type = @floor_type,
                floor_number = @floor_number,
                is_corner_room = @is_corner_room,
                main_exposure_direction = @main_exposure_direction,
                current_status = @current_status,
                is_available_for_rent = @is_available_for_rent,
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
                updated_at = @updated_at
            WHERE property_id = excluded.property_id;
            """;

        // Clear parameters before setting new ones because this method can be called multiple times in a loop.
        cmd.Parameters.Clear();

        cmd.Parameters.AddWithValue("@listing_id", listing.Id);
        cmd.Parameters.AddWithValue("@property_id", propertyId);
        cmd.Parameters.AddWithValue("@is_property_unit_ownership", listing.IsPropertyUnitLevelOwnership ? 1 : 0); // bool to int
        cmd.Parameters.AddWithValue("@name", listing.Name);

        cmd.Parameters.AddWithValue("@floor_plan_type", listing.FloorPlan.ToString());
        cmd.Parameters.AddWithValue("@floor_plan_extra_type", listing.FloorPlanExtra.ToString());
        cmd.Parameters.AddWithValue("@floor_area", listing.FloorExclusiveAreaSqm);
        cmd.Parameters.AddWithValue("@floor_type", listing.FloorGround.ToString());
        cmd.Parameters.AddWithValue("@floor_number", listing.FloorNumber);
        cmd.Parameters.AddWithValue("@is_corner_room", listing.IsCornerRoom ? 1 : 0);
        cmd.Parameters.AddWithValue("@main_exposure_direction", listing.SelectedExposureDirectionType.ToString());//listing.ExposureDirection.ToString());
        cmd.Parameters.AddWithValue("@current_status", listing.SelectedCurrentStatusType.ToString());
        cmd.Parameters.AddWithValue("@is_available_for_rent", listing.IsAvailableForRentNow ? 1 : 0);
        cmd.Parameters.AddWithValue("@available_from_month", listing.SelectedAvailabilityMonthType.ToString());
        cmd.Parameters.AddWithValue("@available_from_period", listing.SelectedAvailabilityPeriodType.ToString());
        cmd.Parameters.AddWithValue("@is_immediate_occupancy", listing.IsAvailableForImmediateMoveIn ? 1 : 0);
        cmd.Parameters.AddWithValue("@current_status_checked_at", listing.OccupancyStatusCheckedAt?.ToString("O", CultureInfo.InvariantCulture) ?? "");

        cmd.Parameters.AddWithValue("@fudousan_id_additional_code", listing.FudousanIdAdditionalCode ?? string.Empty);
        cmd.Parameters.AddWithValue("@remarks", listing.Remarks ?? string.Empty);

        cmd.Parameters.AddWithValue("@chinryou", listing.Chinryou);

        cmd.Parameters.AddWithValue("@management_fee", listing.ManagementFee);
        cmd.Parameters.AddWithValue("@management_fee_unit", listing.ManagementFeeUnit.ToString());
        cmd.Parameters.AddWithValue("@security_deposit", listing.SecurityDeposit);
        cmd.Parameters.AddWithValue("@security_deposit_unit", listing.SecurityDepositUnit.ToString());
        cmd.Parameters.AddWithValue("@key_money", listing.KeyMoney);
        cmd.Parameters.AddWithValue("@key_money_unit", listing.KeyMoneyUnit.ToString());
        cmd.Parameters.AddWithValue("@contract_type", listing.ContractType.ToString());
        cmd.Parameters.AddWithValue("@contract_period_years", listing.ContractPeriodYears);
        cmd.Parameters.AddWithValue("@renewal_fee", listing.RenewalFee);
        cmd.Parameters.AddWithValue("@renewal_fee_unit", listing.RenewalFeeUnit.ToString());
        cmd.Parameters.AddWithValue("@recontract_fee", listing.RecontractFee);
        cmd.Parameters.AddWithValue("@recontract_fee_unit", listing.RecontractFeeUnit.ToString());
        cmd.Parameters.AddWithValue("@guarantor_company", listing.GuarantorCompany);
        cmd.Parameters.AddWithValue("@housing_insurance", listing.HousingInsurance);
        cmd.Parameters.AddWithValue("@other_fees", listing.OtherFees);
        cmd.Parameters.AddWithValue("@pet_friendly", listing.PetFriendly ? 1 : 0);
        cmd.Parameters.AddWithValue("@two_person_occupancy", listing.TwoPersonOccupancy ? 1 : 0);
        cmd.Parameters.AddWithValue("@no_guarantor_needed", listing.NoGuarantorNeeded ? 1 : 0);
        cmd.Parameters.AddWithValue("@office_use_allowed", listing.OfficeUseAllowed ? 1 : 0);
        cmd.Parameters.AddWithValue("@other_conditions", listing.OtherConditions);
        cmd.Parameters.AddWithValue("@contract_remarks", listing.ContractRemarks);
        cmd.Parameters.AddWithValue("@handling_type", listing.HandlingType.ToString());
        cmd.Parameters.AddWithValue("@transaction_type", listing.TransactionType.ToString());
        cmd.Parameters.AddWithValue("@lessor_commission", listing.LessorCommission);
        cmd.Parameters.AddWithValue("@lessor_commission_unit", listing.LessorCommissionUnit.ToString());
        cmd.Parameters.AddWithValue("@lessee_commission", listing.LesseeCommission);
        cmd.Parameters.AddWithValue("@lessee_commission_unit", listing.LesseeCommissionUnit.ToString());
        cmd.Parameters.AddWithValue("@listing_broker_commission", listing.ListingBrokerCommission);
        cmd.Parameters.AddWithValue("@listing_broker_commission_unit", listing.ListingBrokerCommissionUnit.ToString());
        cmd.Parameters.AddWithValue("@tenant_broker_commission", listing.TenantBrokerCommission);
        cmd.Parameters.AddWithValue("@tenant_broker_commission_unit", listing.TenantBrokerCommissionUnit.ToString());
        cmd.Parameters.AddWithValue("@transaction_remarks", listing.TransactionRemarks);
        cmd.Parameters.AddWithValue("@has_air_conditioner", listing.HasAirConditioner ? 1 : 0);
        cmd.Parameters.AddWithValue("@has_wood_flooring", listing.HasWoodFlooring ? 1 : 0);
        cmd.Parameters.AddWithValue("@has_video_intercom", listing.HasVideoIntercom ? 1 : 0);
        cmd.Parameters.AddWithValue("@has_bath_reheating", listing.HasBathReheating ? 1 : 0);
        cmd.Parameters.AddWithValue("@has_gas_stove", listing.HasGasStove ? 1 : 0);
        cmd.Parameters.AddWithValue("@has_induction_cooktop", listing.HasInductionCooktop ? 1 : 0);
        cmd.Parameters.AddWithValue("@has_multiple_burners", listing.HasMultipleBurners ? 1 : 0);
        cmd.Parameters.AddWithValue("@has_system_kitchen", listing.HasSystemKitchen ? 1 : 0);
        cmd.Parameters.AddWithValue("@has_loft", listing.HasLoft ? 1 : 0);
        cmd.Parameters.AddWithValue("@has_indoor_washing_machine_space", listing.HasIndoorWashingMachineSpace ? 1 : 0);
        cmd.Parameters.AddWithValue("@has_bathroom_dryer", listing.HasBathroomDryer ? 1 : 0);
        cmd.Parameters.AddWithValue("@has_separate_bath_and_toilet", listing.HasSeparateBathAndToilet ? 1 : 0);
        cmd.Parameters.AddWithValue("@has_separate_washbasin", listing.HasSeparateWashbasin ? 1 : 0);
        cmd.Parameters.AddWithValue("@has_warm_water_toilet_seat", listing.HasWarmWaterToiletSeat ? 1 : 0);
        cmd.Parameters.AddWithValue("@appliance_remarks", listing.ApplianceRemarks);

        cmd.Parameters.AddWithValue("@updated_at", DateTimeOffset.UtcNow.ToString("s"));

        var result = cmd.ExecuteNonQuery();
        if (result != 1)
        {
            throw new InvalidOperationException("Residential listing ID is already associated with a different property.");
        }

        cmd.Parameters.Clear();

        // 部屋写真 rent_residential_listing_pictures table - Insert or Update
        if (listing.Pictures.Count > 0)
        {
            foreach (var pic in listing.Pictures)
            {
                // Upsert
                var sqlUpsertRoom = "INSERT INTO rent_residential_listing_pictures (picture_id, listing_id, property_id, filename, type, description, is_main) VALUES (@picture_id, @listing_id, @property_id, @filename, @type, @description, @is_main) ";
                sqlUpsertRoom += "ON CONFLICT(picture_id) ";
                sqlUpsertRoom += "DO UPDATE SET filename = @filename, type = @type, description = @description, is_main = @is_main WHERE listing_id = @listing_id AND property_id = @property_id";
                var exec = true;

                cmd.CommandText = sqlUpsertRoom;

                if (exec)
                {
                    // ループなので、前のパラメーターをクリアする。
                    cmd.Parameters.Clear();

                    cmd.Parameters.AddWithValue("@picture_id", pic.Id);
                    cmd.Parameters.AddWithValue("@listing_id", listing.Id);
                    cmd.Parameters.AddWithValue("@property_id", propertyId);
                    cmd.Parameters.AddWithValue("@filename", pic.ImageFilename);
                    cmd.Parameters.AddWithValue("@type", pic.PictureType.ToString());
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

                    if (cmd.ExecuteNonQuery() != 1) throw new InvalidOperationException("Residential listing picture ID belongs to another listing.");
                }

            }
        }

        // 部屋写真の削除リストを処理
        if (listing.PicturesToBeDeleted.Count > 0)
        {
            foreach (var delr in listing.PicturesToBeDeleted)
            {
                cmd.Parameters.Clear();
                cmd.CommandText = "DELETE FROM rent_residential_listing_pictures WHERE picture_id = @id AND listing_id = @listingId AND property_id = @propertyId";
                cmd.Parameters.AddWithValue("@id", delr.Id);
                cmd.Parameters.AddWithValue("@listingId", listing.Id);
                cmd.Parameters.AddWithValue("@propertyId", propertyId);
                cmd.ExecuteNonQuery();
            }
            // let's not do this.
            //listing.UnitPicturesToBeDeleted.Clear();
        }

        // 部屋図面 rent_residential_listing_pdfs table - Insert or Update
        if (listing.Pdfs.Count > 0)
        {
            foreach (var pdf in listing.Pdfs)
            {
                // Upsert
                var sqlUpsertRoom = "INSERT INTO rent_residential_listing_pdfs (pdf_id, listing_id, property_id, filename, thumbnail_filename, type, description, is_main) VALUES (@pdf_id, @listing_id, @property_id, @filename, @thumbnail_filename, @type, @description, @is_main) ";
                sqlUpsertRoom += "ON CONFLICT(pdf_id) ";
                sqlUpsertRoom += "DO UPDATE SET filename = @filename, thumbnail_filename = @thumbnail_filename, type = @type, description = @description, is_main = @is_main WHERE listing_id = @listing_id AND property_id = @property_id";
                var exec = true;

                cmd.CommandText = sqlUpsertRoom;

                if (exec)
                {
                    // ループなので、前のパラメーターをクリアする。
                    cmd.Parameters.Clear();

                    cmd.Parameters.AddWithValue("@pdf_id", pdf.Id);
                    cmd.Parameters.AddWithValue("@listing_id", listing.Id);
                    cmd.Parameters.AddWithValue("@property_id", propertyId);
                    cmd.Parameters.AddWithValue("@filename", pdf.PdfFilename);
                    cmd.Parameters.AddWithValue("@thumbnail_filename", pdf.ThumbnailFilename);
                    cmd.Parameters.AddWithValue("@type", pdf.PdfType.ToString());
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

                    if (cmd.ExecuteNonQuery() != 1) throw new InvalidOperationException("Residential listing PDF ID belongs to another listing.");
                }

            }
        }

        // 部屋図面の削除リストを処理
        if (listing.PdfsToBeDeleted.Count > 0)
        {
            foreach (var delr in listing.PdfsToBeDeleted)
            {
                cmd.Parameters.Clear();
                cmd.CommandText = "DELETE FROM rent_residential_listing_pdfs WHERE pdf_id = @id AND listing_id = @listingId AND property_id = @propertyId";
                cmd.Parameters.AddWithValue("@id", delr.Id);
                cmd.Parameters.AddWithValue("@listingId", listing.Id);
                cmd.Parameters.AddWithValue("@propertyId", propertyId);
                cmd.ExecuteNonQuery();
            }
            // let's not do this.
            //listing.UnitPicturesToBeDeleted.Clear();
        }

        // 部屋貸主 rent_lessors_residential_listings - Insert or Update
        if (listing.Lessors.Count > 0)
        {
            foreach (var psn in listing.Lessors)
            {
                var sqlUpsert = "INSERT INTO rent_lessors_residential_listings (lessor_id, property_id, property_context_type, listing_id) VALUES (@lessor_id, @property_id, @property_context_type, @listing_id) ";
                sqlUpsert += "ON CONFLICT(lessor_id, property_id, listing_id) DO NOTHING";
                cmd.CommandText = sqlUpsert;
                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("@lessor_id", psn.Id);
                cmd.Parameters.AddWithValue("@property_id", propertyId);
                cmd.Parameters.AddWithValue("@property_context_type", PropertyContextType.RentResidential.ToString());
                cmd.Parameters.AddWithValue("@listing_id", listing.Id);
                cmd.ExecuteNonQuery();
            }
        }

        // 部屋貸主の削除リストを処理
        if (listing.LessorsToBeDeleted.Count > 0)
        {
            foreach (var psn in listing.LessorsToBeDeleted)
            {
                cmd.Parameters.Clear();
                cmd.CommandText = "DELETE FROM rent_lessors_residential_listings WHERE lessor_id = @id AND property_id = @propertyId AND listing_id = @listingId";
                cmd.Parameters.AddWithValue("@id", psn.Id);
                cmd.Parameters.AddWithValue("@propertyId", propertyId);
                cmd.Parameters.AddWithValue("@listingId", listing.Id);
                cmd.ExecuteNonQuery();
            }
        }

        // 部屋宅建業者 brokers_residential_listings - Insert or Update
        if (listing.Brokers.Count > 0)
        {
            foreach (var broker in listing.Brokers)
            {
                cmd.Parameters.Clear();
                cmd.CommandText = """
                    INSERT INTO brokers_residential_listings
                        (broker_id, property_id, property_context_type, listing_id)
                    VALUES
                        (@brokerId, @propertyId, @propertyContextType, @listingId)
                    ON CONFLICT (broker_id, property_id, listing_id) DO NOTHING;
                    """;
                cmd.Parameters.AddWithValue("@brokerId", broker.Id);
                cmd.Parameters.AddWithValue("@propertyId", propertyId);
                cmd.Parameters.AddWithValue("@propertyContextType", PropertyContextType.RentResidential.ToString());
                cmd.Parameters.AddWithValue("@listingId", listing.Id);
                cmd.ExecuteNonQuery();
            }
        }

        // 部屋宅建業者の削除リストを処理
        if (listing.BrokersToBeDeleted.Count > 0)
        {
            foreach (var broker in listing.BrokersToBeDeleted)
            {
                cmd.Parameters.Clear();
                cmd.CommandText = """
                    DELETE FROM brokers_residential_listings
                    WHERE broker_id = @brokerId AND property_id = @propertyId AND listing_id = @listingId;
                    """;
                cmd.Parameters.AddWithValue("@brokerId", broker.Id);
                cmd.Parameters.AddWithValue("@propertyId", propertyId);
                cmd.Parameters.AddWithValue("@listingId", listing.Id);
                cmd.ExecuteNonQuery();
            }
        }

        return result;
    }

    private static void MarkRentResidentialListingSaved(ZumenSearch.Domain.Entities.Rent.Residentials.Listing listing)
    {
        listing.SetIsModified(false);
        listing.SetStatus(EntityStatus.Saved);
        listing.SetPropertyStatus(EntityStatus.Saved);
        foreach (var picture in listing.Pictures)
        {
            picture.SetStatus(EntityStatus.Saved);
            picture.SetIsModified(false);
        }
        foreach (var pdf in listing.Pdfs)
        {
            pdf.SetStatus(EntityStatus.Saved);
            pdf.SetIsModified(false);
        }
    }

    public ZumenSearch.Application.Results.ResultWrapper UpsertRentResidentialListing(string propertyId, ZumenSearch.Domain.Entities.Rent.Residentials.Listing listing)
    {
        var res = new ResultWrapper();

        if (string.IsNullOrWhiteSpace(propertyId) || string.IsNullOrWhiteSpace(listing.Id) || !string.Equals(listing.PropertyId, propertyId, StringComparison.Ordinal))
        {
            res.IsError = true;
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
                        is_corner_room, main_exposure_direction, current_status, is_available_for_rent,
                        available_from_month, available_from_period, is_immediate_occupancy,
                        current_status_checked_at, remarks)
                    VALUES (
                        @listing_id, @property_id, @is_property_unit_ownership, @name, @chinryou,
                        @room_count, @floor_plan_type, @floor_plan_extra_type, @floor_area, @floor_type, @floor_number,
                        @is_corner_room, @main_exposure_direction, @current_status, @is_available_for_rent,
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
                        is_available_for_rent = @is_available_for_rent,
                        available_from_month = @available_from_month,
                        available_from_period = @available_from_period,
                        is_immediate_occupancy = @is_immediate_occupancy,
                        current_status_checked_at = @current_status_checked_at,
                        remarks = @remarks,
                        updated_at = @updated_at;
                    """;
                cmd.Parameters.Clear();

                */
                cmd.Parameters.Clear();
                res.AffectedCount = DoUpsertRentResidentialListing(cmd, propertyId, listing);

                // Commit
                cmd.Transaction.Commit();
                MarkRentResidentialListingSaved(listing);

                // Clear deletion queues only after the commit succeeds.
                listing.LessorsToBeDeleted.Clear();
                listing.BrokersToBeDeleted.Clear();
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

    public ZumenSearch.Application.Results.ListingSearchResultWrapper SelectRentResidentialListings(RentResidentialListingSearchCriteria criteria)
    {
        var res = new ListingSearchResultWrapper();

        _readerWriterLock.EnterReadLock();
        try
        {
            using var connection = new SqliteConnection(_connectionStringBuilder.ConnectionString);
            connection.Open();

            using var cmd = connection.CreateCommand();
            var conditions = new List<string>();

            // Text search
            AddTextFilter(criteria.PropertyName, "p.name LIKE @propertyName ESCAPE '\\'", "propertyName");
            AddTextFilter(criteria.Address, "p.location_full LIKE @address ESCAPE '\\'", "address");
            AddTextFilter(
                criteria.LessorName,
                "((r.is_unit_ownership = 0 AND EXISTS (SELECT 1 FROM rent_lessors_properties AS relation INNER JOIN rent_lessors AS lessor ON lessor.lessor_id = relation.lessor_id WHERE relation.property_id = p.property_id AND lessor.name LIKE @lessorName ESCAPE '\\')) " +
                "OR (r.is_unit_ownership = 1 AND EXISTS (SELECT 1 FROM rent_lessors_residential_listings AS relation INNER JOIN rent_lessors AS lessor ON lessor.lessor_id = relation.lessor_id WHERE relation.property_id = p.property_id AND relation.listing_id = l.listing_id AND lessor.name LIKE @lessorName ESCAPE '\\')))",
                "lessorName");
            AddTextFilter(criteria.FudousanId, "p.fudousan_id LIKE @fudousanId ESCAPE '\\'", "fudousanId");

            if (!string.IsNullOrWhiteSpace(criteria.Keyword) && criteria.Keyword.Trim() != "*")
            {
                cmd.Parameters.AddWithValue("@keywordContextType", PropertyContextType.RentResidential.ToString());
                AddTextFilter(criteria.Keyword, "(p.name LIKE @keyword ESCAPE '\\' OR l.name LIKE @keyword ESCAPE '\\' OR p.location_full LIKE @keyword ESCAPE '\\' OR p.fudousan_id LIKE @keyword ESCAPE '\\' OR " +
                    "p.train1_station_name LIKE @keyword ESCAPE '\\' OR p.train2_station_name LIKE @keyword ESCAPE '\\' OR p.train3_station_name LIKE @keyword ESCAPE '\\' OR p.train4_station_name LIKE @keyword ESCAPE '\\' OR " +
                    "EXISTS (SELECT 1 FROM rent_lessors AS lessor WHERE lessor.name LIKE @keyword ESCAPE '\\' AND ((r.is_unit_ownership = 0 AND EXISTS (SELECT 1 FROM rent_lessors_properties AS relation WHERE relation.property_id = p.property_id AND relation.property_context_type = @keywordContextType AND relation.lessor_id = lessor.lessor_id)) OR (r.is_unit_ownership = 1 AND EXISTS (SELECT 1 FROM rent_lessors_residential_listings AS relation WHERE relation.property_id = p.property_id AND relation.listing_id = l.listing_id AND relation.property_context_type = @keywordContextType AND relation.lessor_id = lessor.lessor_id)))))", "keyword");
            }

            // Rent and other numeric limits
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

            // Station, line, and walking distance must match the same route.
            var hasStationFilter = !string.IsNullOrWhiteSpace(criteria.Station);
            var hasLineFilter = !string.IsNullOrWhiteSpace(criteria.Line);
            var hasWalkFilter = double.IsFinite(criteria.MaximumStationWalkMinutes);

            if (hasStationFilter || hasLineFilter || hasWalkFilter)
            {
                if (hasStationFilter)
                {
                    cmd.Parameters.AddWithValue("@station", "%" + EscapeLikeValue(criteria.Station) + "%");
                }

                if (hasLineFilter)
                {
                    cmd.Parameters.AddWithValue("@line", "%" + EscapeLikeValue(criteria.Line) + "%");
                }

                if (hasWalkFilter)
                {
                    cmd.Parameters.AddWithValue("@maximumWalk", criteria.MaximumStationWalkMinutes);
                }

                var matchingRoutes = new List<string>();

                for (var routeNumber = 1; routeNumber <= 4; routeNumber++)
                {
                    var routeConditions = new List<string>();

                    if (hasStationFilter)
                    {
                        routeConditions.Add($"p.train{routeNumber}_station_name LIKE @station ESCAPE '\\'");
                    }

                    if (hasLineFilter)
                    {
                        routeConditions.Add($"p.train{routeNumber}_line_name LIKE @line ESCAPE '\\'");
                    }

                    if (hasWalkFilter)
                    {
                        routeConditions.Add($"p.train{routeNumber}_ekitoho > 0 AND p.train{routeNumber}_ekitoho <= @maximumWalk");
                    }

                    matchingRoutes.Add("(" + string.Join(" AND ", routeConditions) + ")");
                }

                conditions.Add("(" + string.Join(" OR ", matchingRoutes) + ")");
            }

            // Property and room types, building age, and floor area
            AddEnumFilter("r.property_kind", criteria.PropertyKinds.Select(value => value.ToString()), "propertyKind");
            AddEnumFilter("r.property_structure", criteria.PropertyStructures.Select(value => value.ToString()), "propertyStructure");
            AddNumberFilter(DateTime.Now.Year - criteria.MaximumBuildingAgeYears, "CAST(substr(r.built_year_month, 1, 4) AS REAL)", "minimumBuiltYear", ">=");
            AddEnumFilter("l.floor_plan_type", criteria.FloorPlans.Select(value => value.ToString()), "floorPlan");
            AddNumberFilter(criteria.MinimumFloorArea, "CAST(REPLACE(l.floor_area, ',', '') AS REAL)", "minimumFloorArea", ">=");
            AddNumberFilter(criteria.MaximumFloorArea, "CAST(REPLACE(l.floor_area, ',', '') AS REAL)", "maximumFloorArea", "<=");

            // Room features
            AddFlag(criteria.PetFriendly, "l.pet_friendly");
            AddFlag(criteria.TwoPersonOccupancy, "l.two_person_occupancy");
            AddFlag(criteria.NoGuarantorNeeded, "l.no_guarantor_needed");
            AddFlag(criteria.OfficeUseAllowed, "l.office_use_allowed");

            if (criteria.HandlingType is { } handlingType)
            {
                conditions.Add("l.handling_type = @handlingType");
                cmd.Parameters.AddWithValue("@handlingType", handlingType.ToString());
            }

            if (criteria.TransactionType is { } transactionType)
            {
                conditions.Add("l.transaction_type = @transactionType");
                cmd.Parameters.AddWithValue("@transactionType", transactionType.ToString());
            }

            // Commission limits and units
            AddCommissionFilter(criteria.MaximumLessorCommission, criteria.LessorCommissionUnit, "l.lessor_commission", "l.lessor_commission_unit", "lessorCommission");
            AddCommissionFilter(criteria.MaximumLesseeCommission, criteria.LesseeCommissionUnit, "l.lessee_commission", "l.lessee_commission_unit", "lesseeCommission");
            AddCommissionFilter(criteria.MaximumListingBrokerCommission, criteria.ListingBrokerCommissionUnit, "l.listing_broker_commission", "l.listing_broker_commission_unit", "listingBrokerCommission");
            AddCommissionFilter(criteria.MaximumTenantBrokerCommission, criteria.TenantBrokerCommissionUnit, "l.tenant_broker_commission", "l.tenant_broker_commission_unit", "tenantBrokerCommission");

            // Listing and property features
            AddFlag(criteria.IsRecruiting, "l.is_available_for_rent");
            AddFlag(criteria.ImmediateOccupancy, "l.is_immediate_occupancy");
            AddFlag(criteria.CornerRoom, "l.is_corner_room");
            AddFlag(criteria.HasAutolock, "r.has_autolock");
            AddFlag(criteria.HasElevator, "r.has_elevator");
            AddFlag(criteria.HasParcelLocker, "r.has_parcel_locker");
            AddFlag(criteria.HasFreeInternet, "r.internet_free");
            AddFlag(criteria.HasAirConditioner, "l.has_air_conditioner");
            AddFlag(criteria.HasSeparateBathAndToilet, "l.has_separate_bath_and_toilet");
            AddFlag(criteria.HasParking, "r.has_parking");

            // Build and execute the search query
            var whereClause = conditions.Count == 0
                ? string.Empty
                : " WHERE " + string.Join(" AND ", conditions);

            cmd.CommandText =
                "SELECT p.name AS propertyName, l.name AS roomName, " +
                "l.listing_id AS roomId, l.property_id AS propertyId, p.fudousan_id " +
                "FROM rent_residential_listings AS l " +
                "LEFT JOIN properties AS p USING (property_id) " +
                "LEFT JOIN rent_residentials AS r USING (property_id)" +
                whereClause +
                " ORDER BY p.name, l.name";

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                var propertyId = Convert.ToString(reader["propertyId"], CultureInfo.InvariantCulture);
                if (string.IsNullOrEmpty(propertyId))
                {
                    Debug.WriteLine("DataAccess::SelectRentResidentialListings: propertyId is null or empty for a rent residential.");
                    continue;
                }

                var listingId = Convert.ToString(reader["roomId"], CultureInfo.InvariantCulture);
                if (string.IsNullOrEmpty(listingId))
                {
                    Debug.WriteLine("DataAccess::SelectRentResidentialListings: roomId is null or empty for a rent residential room.");
                    continue;
                }

                var resultItem = new ZumenSearch.Application.SearchResults.ListingSearchResultItem(
                    listingId,
                    propertyId,
                    PropertyContextType.RentResidential,
                    Convert.ToString(reader["fudousan_id"], CultureInfo.InvariantCulture) ?? string.Empty);

                resultItem.SetName(Convert.ToString(reader["roomName"], CultureInfo.InvariantCulture) ?? string.Empty);

                var propertyName = Convert.ToString(reader["propertyName"], CultureInfo.InvariantCulture);
                if (!string.IsNullOrEmpty(propertyName))
                {
                    resultItem.PropertyName = propertyName;
                }

                resultItem.SetIsModified(false);
                res.ListingSearchResult.Add(resultItem);
            }

            // Local filter builders keep parameter handling consistent without splitting the method.
            void AddTextFilter(string value, string condition, string parameterName)
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    return;
                }

                conditions.Add(condition);
                cmd.Parameters.AddWithValue("@" + parameterName, "%" + EscapeLikeValue(value) + "%");
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

                var parameterNames = selectedValues
                    .Select((_, index) => "@" + parameterName + index.ToString(CultureInfo.InvariantCulture))
                    .ToArray();

                conditions.Add(column + " IN (" + string.Join(", ", parameterNames) + ")");

                for (var index = 0; index < selectedValues.Length; index++)
                {
                    cmd.Parameters.AddWithValue(parameterNames[index], selectedValues[index]);
                }
            }

            void AddFlag(bool selected, string column)
            {
                if (selected)
                {
                    conditions.Add(column + " = 1");
                }
            }

            void AddCommissionFilter(double maximum, ZumenSearch.Domain.Entities.Rent.Residentials.ListingCommissionUnitType? unit, string amountColumn, string unitColumn, string parameterName)
            {
                if (unit is { } commissionUnit)
                {
                    conditions.Add(unitColumn + " = @" + parameterName + "Unit");
                    cmd.Parameters.AddWithValue("@" + parameterName + "Unit", commissionUnit.ToString());
                }

                if (double.IsFinite(maximum))
                {
                    conditions.Add("CAST(REPLACE(" + amountColumn + ", ',', '') AS REAL) <= @" + parameterName);
                    cmd.Parameters.AddWithValue("@" + parameterName, maximum);
                }
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

    public RentResidentialListingResultWrapper SelectRentResidentialListingById(string propertyId, string listingId, bool duplicate = false)
    {
        var res = new RentResidentialListingResultWrapper();

        if (string.IsNullOrEmpty(listingId))
        {
            res.IsError = true;
            Debug.WriteLine("DataAccess::SelectRentResidentialListingById: listingId is null or empty.");
            // TODO:
            return res;
        }

        if (string.IsNullOrEmpty(propertyId))
        {
            res.IsError = true;
            Debug.WriteLine("DataAccess::SelectRentResidentialListingById: propertyId is null or empty.");
            // TODO:
            return res;
        }

        _readerWriterLock.EnterReadLock();
        try
        {
            using var connection = new SqliteConnection(_connectionStringBuilder.ConnectionString);
            connection.Open();

            using var cmd = connection.CreateCommand();

            var propertyName = string.Empty;
            var isFound = false;
            var isUnitOwnership = false;
            var fudousanId = string.Empty;

            cmd.CommandText = "SELECT p.name, p.fudousan_id, r.is_unit_ownership FROM properties AS p INNER JOIN rent_residentials AS r ON r.property_id = p.property_id WHERE p.property_id = @propertyId";
            cmd.Parameters.AddWithValue("@propertyId", propertyId);
            using (var reader = cmd.ExecuteReader())
            {
                if (reader.Read())
                {
                    propertyName = reader.GetString(reader.GetOrdinal("name"));
                    isUnitOwnership = Convert.ToInt32(reader["is_unit_ownership"], CultureInfo.InvariantCulture) != 0;
                    fudousanId = reader.GetString(reader.GetOrdinal("fudousan_id"));
                    isFound = true;
                }
            }

            if (isFound)
            {
                cmd.CommandText = "SELECT * FROM rent_residential_listings WHERE listing_id = @listingId AND property_id = @propertyId";
                cmd.Parameters.AddWithValue("@listingId", listingId);
                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        res.PropertyName = propertyName;
                        res.Listing = GetRentResidentialListing(reader, propertyId, propertyName, isUnitOwnership, fudousanId, duplicate);
                    }
                }

                if (res.Listing is not null)
                {
                    SetRentResidentialListingChildValues(cmd, res.Listing, listingId, duplicate);
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

    private static ZumenSearch.Domain.Entities.Rent.Residentials.Listing? GetRentResidentialListing(SqliteDataReader reader, string propertyId, string propertyName, bool isUnitOwnership, string fudousanId, bool duplicate)
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

        // If duplicate is true, generate a new listing ID; otherwise, use the existing one.
        string id = duplicate ? Guid.CreateVersion7().ToString("N") : listingId;
        // Set the status based on whether it's a duplicate or not.
        EntityStatus status = duplicate ? EntityStatus.New : EntityStatus.Saved;

        // Create listing.
        var room = new ZumenSearch.Domain.Entities.Rent.Residentials.Listing(id, status, propertyId, EntityStatus.Saved, isUnitOwnership, propertyName, fudousanId);

        // try?
        room.SetName(reader.GetString(reader.GetOrdinal("name")) ?? string.Empty);
        room.SetFloorPlanTypeLabelFromString(Convert.ToString(reader["floor_plan_type"], CultureInfo.InvariantCulture) ?? string.Empty);
        room.SetFloorPlanExtraTypeLabelFromString(Convert.ToString(reader["floor_plan_extra_type"], CultureInfo.InvariantCulture) ?? string.Empty);
        room.SetFloorGroundTypeLabelFromString(Convert.ToString(reader["floor_type"], CultureInfo.InvariantCulture) ?? string.Empty);
        room.SetFloorExclusiveAreaSqm(Convert.ToString(reader["floor_area"], CultureInfo.InvariantCulture) ?? string.Empty);
        room.SetFloorNumber(Convert.ToString(reader["floor_number"], CultureInfo.InvariantCulture) ?? string.Empty);
        room.SetIsCornerRoom(Convert.ToInt32(reader["is_corner_room"], CultureInfo.InvariantCulture) != 0);
        room.SetExposureDirectionTypeLabelFromString(Convert.ToString(reader["main_exposure_direction"], CultureInfo.InvariantCulture) ?? string.Empty);
        room.SetAvailabilityStatusTypeLabelFromString(Convert.ToString(reader["current_status"], CultureInfo.InvariantCulture) ?? string.Empty);
        room.SetIsAvailableForRentNow(Convert.ToInt32(reader["is_available_for_rent"], CultureInfo.InvariantCulture) != 0);
        room.SetAvailabilityMonthTypeLabelFromString(Convert.ToString(reader["available_from_month"], CultureInfo.InvariantCulture) ?? string.Empty);
        room.SetAvailabilityPeriodTypeLabelFromString(Convert.ToString(reader["available_from_period"], CultureInfo.InvariantCulture) ?? string.Empty);
        room.SetIsAvailableForImmediateMoveIn(Convert.ToInt32(reader["is_immediate_occupancy"], CultureInfo.InvariantCulture) != 0);
        room.SetOccupancyStatusCheckedAtFromString(Convert.ToString(reader["current_status_checked_at"], CultureInfo.InvariantCulture) ?? string.Empty);

        room.SetFudousanIdAdditionalCode(Convert.ToString(reader["fudousan_id_additional_code"], CultureInfo.InvariantCulture) ?? string.Empty);
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

        // If duplicate is true, mark the room as modified to indicate that it has unsaved changes.
        room.SetIsModified(duplicate);

        //Debug.WriteLine($"Room ID: {room.Id}, Room Name: {room.RoomName}");

        return room;
    }

    private static void SetRentResidentialListingChildValues(SqliteCommand cmd, ZumenSearch.Domain.Entities.Rent.Residentials.Listing listing, string listingId, bool duplicate)
    {
        // Use listingId, not listing.Id, because listing.Id may be a new ID if duplicate is true.

        // 部屋写真
        cmd.CommandText = "SELECT * FROM rent_residential_listing_pictures WHERE listing_id = @listingId";
        cmd.Parameters.Clear();
        cmd.Parameters.AddWithValue("@listingId", listingId);
        using (var reader = cmd.ExecuteReader())
        {
            while (reader.Read())
            {
                var picid = Convert.ToString(reader["picture_id"], CultureInfo.InvariantCulture) ?? string.Empty;
                
                // If duplicate is true, generate a new picture ID; otherwise, use the existing one.
                picid = duplicate ? Guid.CreateVersion7().ToString("N") : picid;

                var picpath = Convert.ToString(reader["filename"], CultureInfo.InvariantCulture) ?? string.Empty;
                if (!string.IsNullOrEmpty(picid) && !string.IsNullOrEmpty(picpath))
                {
                    var rlpic = new ZumenSearch.Domain.Entities.Rent.Residentials.ListingPicture(picid, picpath, EntityStatus.Saved)
                    {
                        Description = Convert.ToString(reader["description"], CultureInfo.InvariantCulture) ?? string.Empty
                    };

                    var strType = Convert.ToString(reader["type"], CultureInfo.InvariantCulture) ?? string.Empty;
                    if (!Enum.TryParse<ZumenSearch.Domain.Enums.Rent.Residentials.ListingPictureType>(strType, out var pictureType) || !Enum.IsDefined(pictureType))
                    {
                        Debug.WriteLine($"Invalid residential listing picture type: {strType}. Defaulting to Unspecified.");
                        pictureType = ZumenSearch.Domain.Enums.Rent.Residentials.ListingPictureType.Unspecified;
                    }
                    rlpic.PictureType = pictureType;
                    rlpic.IsMain = Convert.ToInt32(reader["is_main"], CultureInfo.InvariantCulture) != 0; // int to bool
                    rlpic.SetIsModified(false);

                    listing.Pictures.Add(rlpic);
                }
                else
                {
                    Debug.WriteLine("Residential listing picture has an empty ID or filename.");
                }
            }
        }

        // 部屋PDF
        cmd.CommandText = "SELECT * FROM rent_residential_listing_pdfs WHERE listing_id = @listingId";
        cmd.Parameters.Clear();
        cmd.Parameters.AddWithValue("@listingId", listingId);
        using (var reader = cmd.ExecuteReader())
        {
            while (reader.Read())
            {
                var pdfid = Convert.ToString(reader["pdf_id"], CultureInfo.InvariantCulture) ?? string.Empty;

                // If duplicate is true, generate a new picture ID; otherwise, use the existing one.
                pdfid = duplicate ? Guid.CreateVersion7().ToString("N") : pdfid;

                var pdfpath = Convert.ToString(reader["filename"], CultureInfo.InvariantCulture) ?? string.Empty;
                var thumbpath = Convert.ToString(reader["thumbnail_filename"], CultureInfo.InvariantCulture) ?? string.Empty;
                if (!string.IsNullOrWhiteSpace(pdfid) && !string.IsNullOrWhiteSpace(pdfpath) && !string.IsNullOrWhiteSpace(thumbpath))
                {
                    var rlpdf = new ZumenSearch.Domain.Entities.Rent.Residentials.ListingPdf(pdfid, pdfpath, thumbpath, EntityStatus.Saved)
                    {
                        Description = Convert.ToString(reader["description"], CultureInfo.InvariantCulture) ?? string.Empty
                    };

                    var strType = Convert.ToString(reader["type"], CultureInfo.InvariantCulture) ?? string.Empty;
                    if (!Enum.TryParse<ZumenSearch.Domain.Enums.Rent.Residentials.ListingPdfType>(strType, out var pdfType) || !Enum.IsDefined(pdfType))
                    {
                        Debug.WriteLine($"Invalid residential listing PDF type: {strType}. Defaulting to Unspecified.");
                        pdfType = ZumenSearch.Domain.Enums.Rent.Residentials.ListingPdfType.Unspecified;
                    }
                    rlpdf.PdfType = pdfType;
                    rlpdf.IsMain = Convert.ToInt32(reader["is_main"], CultureInfo.InvariantCulture) != 0; // int to bool
                    rlpdf.SetIsModified(false);

                    listing.Pdfs.Add(rlpdf);
                }
                else
                {
                    //throw new InvalidDataException("Residential listing PDF has an empty ID, filename or thumbnail filename.");
                    Debug.WriteLine("Residential listing PDF has an empty ID, filename or thumbnail filename.");
                }
            }
        }

        // 部屋貸主
        var lessorIds = new List<string>();
        cmd.CommandText = "SELECT * FROM rent_lessors_residential_listings WHERE property_id = @propertyId AND listing_id = @listingId";
        cmd.Parameters.Clear();
        cmd.Parameters.AddWithValue("@propertyId", listing.PropertyId);
        cmd.Parameters.AddWithValue("@listingId", listingId);
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
                cmd.CommandText = "SELECT lessor_id, name, person_kind, name_last, name_first, name_company, name_company_type, name_company_type_position, phone_number, mobile_phone_number, contact_name, location_postal_code, location_full, location_machiaza_id, location_prefecture, location_county, location_city, location_ward, location_oaza, location_choume, location_edaban, remarks FROM rent_lessors WHERE lessor_id = @lessorId";
                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("@lessorId", lessId);
#pragma warning disable IDE0063 // Use simple 'using' statement
                using (var reader2 = cmd.ExecuteReader())
                {
                    while (reader2.Read())
                    {
                        var lessor = GetPerson(reader2, lessId);
                        if (lessor is not null) listing.Lessors.Add(lessor);
                    }
                }
#pragma warning restore IDE0063 // Use simple 'using' statement
            }
        }

        // 宅建業者（部屋）
        cmd.CommandText = """
            SELECT b.broker_id, b.name, b.person_kind, b.name_last, b.name_first, b.name_company, b.name_company_type, b.name_company_type_position, b.phone_number, b.mobile_phone_number, b.contact_name, b.location_postal_code, b.location_full, b.location_machiaza_id, b.location_prefecture, b.location_county, b.location_city, b.location_ward, b.location_oaza, b.location_choume, b.location_edaban, b.remarks
            FROM brokers_residential_listings AS association
            INNER JOIN brokers AS b ON b.broker_id = association.broker_id
            WHERE association.property_id = @propertyId AND association.listing_id = @listingId;
            """;
        cmd.Parameters.Clear();
        cmd.Parameters.AddWithValue("@propertyId", listing.PropertyId);
        cmd.Parameters.AddWithValue("@listingId", listingId);

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
                    listing.Brokers.Add(broker);
                }
            }
        }

        cmd.Parameters.Clear();
    }

    public ZumenSearch.Application.Results.ResultWrapper DeleteRentResidentialListing(string listingId)
    {
        var res = new ResultWrapper();

        if (string.IsNullOrEmpty(listingId))
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
                cmd.CommandText = "DELETE FROM rent_residential_listings WHERE listing_id = @listingId;";
                cmd.Parameters.AddWithValue("@listingId", listingId);
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

    ResultWrapper IRentCommercialPropertyRepository.Upsert(ZumenSearch.Domain.Entities.Rent.Commercials.Property commercialProperty) => UpsertRentCommercial(commercialProperty);

    RentCommercialPropertyResultWrapper IRentCommercialPropertyRepository.SelectById(string propertyId, bool duplicate) => SelectRentCommercialById(propertyId, duplicate);

    ResultWrapper IRentCommercialPropertyRepository.Delete(string propertyId) => DeleteRentCommercial(propertyId);

    public ZumenSearch.Application.Results.ResultWrapper UpsertRentCommercial(ZumenSearch.Domain.Entities.Rent.Commercials.Property property)
    {
        var res = new ResultWrapper();
        var savedPictures = new List<ZumenSearch.Domain.Entities.Rent.Commercials.PropertyPicture>();
        var savedPdfs = new List<ZumenSearch.Domain.Entities.Rent.Commercials.PropertyPdf>();

        if (string.IsNullOrEmpty(property.Id))
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
                cmd.CommandText = "SELECT 1 FROM properties WHERE property_id = @propertyId";
                cmd.Parameters.AddWithValue("@propertyId", property.Id);
                var propertyExists = cmd.ExecuteScalar() is not null;
                if ((property.Status == EntityStatus.New) == propertyExists)
                    throw new InvalidOperationException("The commercial property was already created or no longer exists. Reload before saving.");
                cmd.Parameters.Clear();

                // Main property table
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Clear();

                var sqlUpsert = "INSERT INTO properties (property_id, name, property_context_type, thumbnail_filename, fudousan_id, fudousan_id_additional_code, location_prefecture_code, location_prefecture, location_machiaza_id, location_county, location_city, location_ward, location_oaza_cho, location_choume, location_edaban, location_postal_code, location_full, location_latitude, location_longitude, " +
                    "train1_line_code, train1_line_name, train1_station_code, train1_station_name, train1_ekitoho, buss1_stop_name, buss1_Jyousya, buss1_Toho, " +
                    "train2_line_code, train2_line_name, train2_station_code, train2_station_name, train2_ekitoho, buss2_stop_name, buss2_Jyousya, buss2_Toho, " +
                    "train3_line_code, train3_line_name, train3_station_code, train3_station_name, train3_ekitoho, buss3_stop_name, buss3_Jyousya, buss3_Toho, " +
                    "train4_line_code, train4_line_name, train4_station_code, train4_station_name, train4_ekitoho, buss4_stop_name, buss4_Jyousya, buss4_Toho, updated_at) ";

                sqlUpsert += "VALUES (@propertyId, @name, @propertyContextType, @thumbnailPath, @fudousanId, @fudousanIdAdditionalCode, @locPrefId, @locPrefecture, @locMachiazaId, @locCounty, @locCity, @locWard, @locOazaCho, @locChoume, @locEdaban, @locPostalCode, @locLocationFull, @locationLatitude, @locationLongitude, " +
                    "@train1_line_code, @train1_line_name, @train1_station_code, @train1_station_name, @train1_ekitoho, @buss1_stop_name, @buss1_Jyousya, @buss1_Toho, " +
                    "@train2_line_code, @train2_line_name, @train2_station_code, @train2_station_name, @train2_ekitoho, @buss2_stop_name, @buss2_Jyousya, @buss2_Toho, " +
                    "@train3_line_code, @train3_line_name, @train3_station_code, @train3_station_name, @train3_ekitoho, @buss3_stop_name, @buss3_Jyousya, @buss3_Toho, " +
                    "@train4_line_code, @train4_line_name, @train4_station_code, @train4_station_name, @train4_ekitoho, @buss4_stop_name, @buss4_Jyousya, @buss4_Toho, @updated_at) ";

                sqlUpsert += "ON CONFLICT (property_id) DO UPDATE SET " +
                    "name = @name, property_context_type = @propertyContextType, thumbnail_filename = @thumbnailPath, " +
                    "fudousan_id = @fudousanId, fudousan_id_additional_code = @fudousanIdAdditionalCode, " +
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

                cmd.Parameters.AddWithValue("@propertyId", property.Id);
                cmd.Parameters.AddWithValue("@name", property.Name);
                cmd.Parameters.AddWithValue("@propertyContextType", property.PropertyContextType.ToString());
                cmd.Parameters.AddWithValue("@thumbnailPath", property.ThumbnailFilename);

                cmd.Parameters.AddWithValue("@fudousanId", property.FudousanId);
                cmd.Parameters.AddWithValue("@fudousanIdAdditionalCode", property.FudousanIdAdditionalCode);

                // Address/Location
                cmd.Parameters.AddWithValue("@locPrefId", property.Address.Prefecture?.MunicipalityCode ?? string.Empty);
                cmd.Parameters.AddWithValue("@locPrefecture", property.Address.Prefecture?.Name ?? string.Empty);
                cmd.Parameters.AddWithValue("@locMachiazaId", property.Address.MachiazaId ?? string.Empty);
                cmd.Parameters.AddWithValue("@locCounty", property.Address.CountyAndCity?.County ?? string.Empty);
                cmd.Parameters.AddWithValue("@locCity", property.Address.CountyAndCity?.City ?? string.Empty);
                cmd.Parameters.AddWithValue("@locWard", property.Address.WardAndOaza?.Ward ?? string.Empty);
                cmd.Parameters.AddWithValue("@locOazaCho", property.Address.WardAndOaza?.Oaza ?? string.Empty);
                cmd.Parameters.AddWithValue("@locChoume", property.Address.Choume?.Chou ?? string.Empty);
                cmd.Parameters.AddWithValue("@locEdaban", property.Address.Edaban ?? string.Empty);
                cmd.Parameters.AddWithValue("@locPostalCode", property.Address.PostalCode ?? string.Empty);
                cmd.Parameters.AddWithValue("@locLocationFull", property.Address.AddressFull ?? string.Empty);
                cmd.Parameters.AddWithValue("@locationLatitude", property.Address.LocationLatitude);
                cmd.Parameters.AddWithValue("@locationLongitude", property.Address.LocationLongitude);
                // Train1
                cmd.Parameters.AddWithValue("@train1_line_code", property.Train1.RailLine.LineCode);
                cmd.Parameters.AddWithValue("@train1_line_name", property.Train1.RailLine.LineName);
                cmd.Parameters.AddWithValue("@train1_station_code", property.Train1.RailStation.StationCode);
                cmd.Parameters.AddWithValue("@train1_station_name", property.Train1.RailStation.StationName);
                cmd.Parameters.AddWithValue("@train1_ekitoho", property.Train1.EkiToho); //int
                // Buss1
                cmd.Parameters.AddWithValue("@buss1_stop_name", property.BusStop1);
                cmd.Parameters.AddWithValue("@buss1_Jyousya", property.BusJyousya1);
                cmd.Parameters.AddWithValue("@buss1_Toho", property.BusStopToho1);

                // Train2
                cmd.Parameters.AddWithValue("@train2_line_code", property.Train2.RailLine.LineCode);
                cmd.Parameters.AddWithValue("@train2_line_name", property.Train2.RailLine.LineName);
                cmd.Parameters.AddWithValue("@train2_station_code", property.Train2.RailStation.StationCode);
                cmd.Parameters.AddWithValue("@train2_station_name", property.Train2.RailStation.StationName);
                cmd.Parameters.AddWithValue("@train2_ekitoho", property.Train2.EkiToho); //int
                // Buss2
                cmd.Parameters.AddWithValue("@buss2_stop_name", property.BusStop2);
                cmd.Parameters.AddWithValue("@buss2_Jyousya", property.BusJyousya2);
                cmd.Parameters.AddWithValue("@buss2_Toho", property.BusStopToho2);

                // Train3
                cmd.Parameters.AddWithValue("@train3_line_code", property.Train3.RailLine.LineCode);
                cmd.Parameters.AddWithValue("@train3_line_name", property.Train3.RailLine.LineName);
                cmd.Parameters.AddWithValue("@train3_station_code", property.Train3.RailStation.StationCode);
                cmd.Parameters.AddWithValue("@train3_station_name", property.Train3.RailStation.StationName);
                cmd.Parameters.AddWithValue("@train3_ekitoho", property.Train3.EkiToho); //int
                // Buss3
                cmd.Parameters.AddWithValue("@buss3_stop_name", property.BusStop3);
                cmd.Parameters.AddWithValue("@buss3_Jyousya", property.BusJyousya3);
                cmd.Parameters.AddWithValue("@buss3_Toho", property.BusStopToho3);

                // Train4
                cmd.Parameters.AddWithValue("@train4_line_code", property.Train4.RailLine.LineCode);
                cmd.Parameters.AddWithValue("@train4_line_name", property.Train4.RailLine.LineName);
                cmd.Parameters.AddWithValue("@train4_station_code", property.Train4.RailStation.StationCode);
                cmd.Parameters.AddWithValue("@train4_station_name", property.Train4.RailStation.StationName);
                cmd.Parameters.AddWithValue("@train4_ekitoho", property.Train4.EkiToho); //int
                // Buss4
                cmd.Parameters.AddWithValue("@buss4_stop_name", property.BusStop4);
                cmd.Parameters.AddWithValue("@buss4_Jyousya", property.BusJyousya4);
                cmd.Parameters.AddWithValue("@buss4_Toho", property.BusStopToho4);

                cmd.Parameters.AddWithValue("@updated_at", DateTimeOffset.UtcNow.ToString("s"));
                res.AffectedCount = cmd.ExecuteNonQuery();

                // rent_commercials
                cmd.Parameters.Clear();

                sqlUpsert = "INSERT INTO rent_commercials (property_id, commercial_kind, is_unit_ownership, property_structure, floor_count_above_ground, floor_count_basement, total_floor_area, built_year_month, remarks, " +
                    "passenger_elevator_availability, passenger_elevator_count, freight_elevator_availability, freight_elevator_details, loading_area_availability, loading_area_details, " +
                    "security_availability, security_mechanical, security_staffed, security_autolock, security_camera, security_other, security_details, " +
                    "electricity_availability, electricity_details, gas_availability, gas_type, gas_details, water_availability, water_type, water_details, sewerage_type, sewerage_details, " +
                    "internet_availability, fiber_availability, internet_details, fire_safety_availability, fire_safety_details, " +
                    "parking_availability, parking_vacancy, parking_spaces, parking_monthly_fee, parking_tax, parking_free, parking_details, " +
                    "management_type, management_maintenance, management_tenant, management_payment, management_other, management_company_remarks, management_external_name, management_external_contact, management_external_remarks, management_lessor_remarks, updated_at) ";
                sqlUpsert += "VALUES (@propertyId, @commercialKind, @isUnitOwnership, @buildingStructure, @floorCountAboveGround, @floorCountBasement, @totalFloorArea, @builtYearMonth, @remarks, " +
                    "@passengerElevatorAvailability, @passengerElevatorCount, @freightElevatorAvailability, @freightElevatorDetails, @loadingAreaAvailability, @loadingAreaDetails, " +
                    "@securityAvailability, @securityMechanical, @securityStaffed, @securityAutolock, @securityCamera, @securityOther, @securityDetails, " +
                    "@electricityAvailability, @electricityDetails, @gasAvailability, @gasType, @gasDetails, @waterAvailability, @waterType, @waterDetails, @sewerageType, @sewerageDetails, " +
                    "@internetAvailability, @fiberAvailability, @internetDetails, @fireSafetyAvailability, @fireSafetyDetails, " +
                    "@parkingAvailability, @parkingVacancy, @parkingSpaces, @parkingMonthlyFee, @parkingTax, @parkingFree, @parkingDetails, " +
                    "@managementType, @managementMaintenance, @managementTenant, @managementPayment, @managementOther, @managementCompanyRemarks, @managementExternalName, @managementExternalContact, @managementExternalRemarks, @managementLessorRemarks, @updatedAt) ";
                sqlUpsert += "ON CONFLICT(property_id) ";
                sqlUpsert += "DO UPDATE SET commercial_kind = excluded.commercial_kind, is_unit_ownership = excluded.is_unit_ownership, property_structure = excluded.property_structure, floor_count_above_ground = excluded.floor_count_above_ground, floor_count_basement = excluded.floor_count_basement, total_floor_area = excluded.total_floor_area, built_year_month = excluded.built_year_month, remarks = excluded.remarks, " +
                    "passenger_elevator_availability = excluded.passenger_elevator_availability, passenger_elevator_count = excluded.passenger_elevator_count, freight_elevator_availability = excluded.freight_elevator_availability, freight_elevator_details = excluded.freight_elevator_details, loading_area_availability = excluded.loading_area_availability, loading_area_details = excluded.loading_area_details, " +
                    "security_availability = excluded.security_availability, security_mechanical = excluded.security_mechanical, security_staffed = excluded.security_staffed, security_autolock = excluded.security_autolock, security_camera = excluded.security_camera, security_other = excluded.security_other, security_details = excluded.security_details, " +
                    "electricity_availability = excluded.electricity_availability, electricity_details = excluded.electricity_details, gas_availability = excluded.gas_availability, gas_type = excluded.gas_type, gas_details = excluded.gas_details, water_availability = excluded.water_availability, water_type = excluded.water_type, water_details = excluded.water_details, sewerage_type = excluded.sewerage_type, sewerage_details = excluded.sewerage_details, " +
                    "internet_availability = excluded.internet_availability, fiber_availability = excluded.fiber_availability, internet_details = excluded.internet_details, fire_safety_availability = excluded.fire_safety_availability, fire_safety_details = excluded.fire_safety_details, " +
                    "parking_availability = excluded.parking_availability, parking_vacancy = excluded.parking_vacancy, parking_spaces = excluded.parking_spaces, parking_monthly_fee = excluded.parking_monthly_fee, parking_tax = excluded.parking_tax, parking_free = excluded.parking_free, parking_details = excluded.parking_details, " +
                    "management_type = excluded.management_type, management_maintenance = excluded.management_maintenance, management_tenant = excluded.management_tenant, management_payment = excluded.management_payment, management_other = excluded.management_other, management_company_remarks = excluded.management_company_remarks, management_external_name = excluded.management_external_name, management_external_contact = excluded.management_external_contact, management_external_remarks = excluded.management_external_remarks, management_lessor_remarks = excluded.management_lessor_remarks, updated_at = excluded.updated_at";

                cmd.CommandText = sqlUpsert;

                cmd.Parameters.AddWithValue("@propertyId", property.Id);
                cmd.Parameters.AddWithValue("@commercialKind", property.CommercialKind.ToString());
                cmd.Parameters.AddWithValue("@isUnitOwnership", property.IsUnitOwnership ? 1 : 0);
                cmd.Parameters.AddWithValue("@buildingStructure", property.PropertyStructure.ToString());
                cmd.Parameters.AddWithValue("@floorCountAboveGround", property.FloorCountAboveGround);
                cmd.Parameters.AddWithValue("@floorCountBasement", property.FloorCountBasement);
                cmd.Parameters.AddWithValue("@totalFloorArea", property.TotalFloorArea.ToString(CultureInfo.InvariantCulture));
                cmd.Parameters.AddWithValue("@builtYearMonth", property.BuiltYearAndMonth.ToString("s"));
                cmd.Parameters.AddWithValue("@remarks", property.Remarks);
                cmd.Parameters.AddWithValue("@managementType", (int)property.KanriShutai);
                cmd.Parameters.AddWithValue("@managementMaintenance", property.KanriIsMaintenanceManagementIfIsKanriJisya ? 1 : 0);
                cmd.Parameters.AddWithValue("@managementTenant", property.KanriIsTenantManagementIfIsKanriJisya ? 1 : 0);
                cmd.Parameters.AddWithValue("@managementPayment", property.KanriIsPaymentManagementIfIsKanriJisya ? 1 : 0);
                cmd.Parameters.AddWithValue("@managementOther", property.KanriIsOtherManagementIfIsKanriJisya ? 1 : 0);
                cmd.Parameters.AddWithValue("@managementCompanyRemarks", property.KanriRemarkIfIsKanriJisya);
                cmd.Parameters.AddWithValue("@managementExternalName", property.KanriNameOfCompanyIfIsKanriTasya);
                cmd.Parameters.AddWithValue("@managementExternalContact", property.KanriContactInfoOfCompanyIfIsKanriTasya);
                cmd.Parameters.AddWithValue("@managementExternalRemarks", property.KanriRemarkIfIsKanriTasya);
                cmd.Parameters.AddWithValue("@managementLessorRemarks", property.KanriRemarkIfIsKanriKashinushi);
                cmd.Parameters.AddWithValue("@passengerElevatorAvailability", property.HasPassengerElevator ? 1 : 0);
                cmd.Parameters.AddWithValue("@passengerElevatorCount", property.PassengerElevatorCount is int count ? count : DBNull.Value);
                cmd.Parameters.AddWithValue("@freightElevatorAvailability", property.HasFreightElevator ? 1 : 0);
                cmd.Parameters.AddWithValue("@freightElevatorDetails", property.FreightElevatorDetails);
                cmd.Parameters.AddWithValue("@loadingAreaAvailability", property.HasLoadingArea ? 1 : 0);
                cmd.Parameters.AddWithValue("@loadingAreaDetails", property.LoadingAreaDetails);
                cmd.Parameters.AddWithValue("@securityAvailability", property.HasSecurity ? 1 : 0);
                cmd.Parameters.AddWithValue("@securityMechanical", property.HasMechanicalSecurity ? 1 : 0);
                cmd.Parameters.AddWithValue("@securityStaffed", property.HasStaffedSecurity ? 1 : 0);
                cmd.Parameters.AddWithValue("@securityAutolock", property.HasAutolock ? 1 : 0);
                cmd.Parameters.AddWithValue("@securityCamera", property.HasSecurityCamera ? 1 : 0);
                cmd.Parameters.AddWithValue("@securityOther", property.HasOtherSecurity ? 1 : 0);
                cmd.Parameters.AddWithValue("@securityDetails", property.SecurityDetails);
                cmd.Parameters.AddWithValue("@electricityAvailability", property.HasElectricity ? 1 : 0);
                cmd.Parameters.AddWithValue("@electricityDetails", property.ElectricityDetails);
                cmd.Parameters.AddWithValue("@gasAvailability", property.HasGas ? 1 : 0);
                cmd.Parameters.AddWithValue("@gasType", (int)property.GasType);
                cmd.Parameters.AddWithValue("@gasDetails", property.GasDetails);
                cmd.Parameters.AddWithValue("@waterAvailability", property.HasWater ? 1 : 0);
                cmd.Parameters.AddWithValue("@waterType", (int)property.WaterType);
                cmd.Parameters.AddWithValue("@waterDetails", property.WaterDetails);
                cmd.Parameters.AddWithValue("@sewerageType", (int)property.SewerageType);
                cmd.Parameters.AddWithValue("@sewerageDetails", property.SewerageDetails);
                cmd.Parameters.AddWithValue("@internetAvailability", property.HasInternet ? 1 : 0);
                cmd.Parameters.AddWithValue("@fiberAvailability", property.HasFiber ? 1 : 0);
                cmd.Parameters.AddWithValue("@internetDetails", property.InternetDetails);
                cmd.Parameters.AddWithValue("@fireSafetyAvailability", property.HasFireSafety ? 1 : 0);
                cmd.Parameters.AddWithValue("@fireSafetyDetails", property.FireSafetyDetails);
                cmd.Parameters.AddWithValue("@parkingAvailability", property.HasParking ? 1 : 0);
                cmd.Parameters.AddWithValue("@parkingVacancy", (int)property.ParkingVacancy);
                cmd.Parameters.AddWithValue("@parkingSpaces", property.ParkingSpaces is int spaces ? spaces : DBNull.Value);
                cmd.Parameters.AddWithValue("@parkingMonthlyFee", property.ParkingMonthlyFee is decimal fee ? fee.ToString(CultureInfo.InvariantCulture) : DBNull.Value);
                cmd.Parameters.AddWithValue("@parkingTax", (int)property.ParkingTax);
                cmd.Parameters.AddWithValue("@parkingFree", property.IsParkingFree ? 1 : 0);
                cmd.Parameters.AddWithValue("@parkingDetails", property.ParkingDetails);
                cmd.Parameters.AddWithValue("@updatedAt", DateTimeOffset.UtcNow.ToString("s"));

                res.AffectedCount = cmd.ExecuteNonQuery();

                cmd.Parameters.Clear();

                // 写真（建物）rent_commercial_pictures
                if (property.Pictures.Count > 0)
                {
                    foreach (var pic in property.Pictures)
                    {
                        var sqlUpsertPicture = "INSERT INTO rent_commercial_pictures (picture_id, property_id, filename, type, description, is_main) ";
                        sqlUpsertPicture += "VALUES (@pictureId, @propertyId, @filePath, @type, @description, @isMain) ";
                        sqlUpsertPicture += "ON CONFLICT (picture_id) ";
                        sqlUpsertPicture += "DO UPDATE SET filename = @filePath, type = @type, description = @description, is_main = @isMain";

                        cmd.CommandText = sqlUpsertPicture;

                        // ループなので、前のパラメーターをクリアする。
                        cmd.Parameters.Clear();

                        cmd.Parameters.AddWithValue("@pictureId", pic.Id);
                        cmd.Parameters.AddWithValue("@propertyId", property.Id);
                        cmd.Parameters.AddWithValue("@filePath", pic.ImageFilename);
                        cmd.Parameters.AddWithValue("@type", pic.PictureType.ToString());
                        cmd.Parameters.AddWithValue("@description", pic.Description);

                        //Debug.WriteLine($"Inserting picture: {pic.ImageLocation}, {pic.Id}, isMain: {pic.IsMain} @DataAccess::InsertRentCommercial");

                        var paramIsMain = new SqliteParameter("@isMain", System.Data.DbType.Int32)
                        {
                            Value = pic.IsMain ? 1 : 0
                        };
                        cmd.Parameters.Add(paramIsMain);

                        var r = cmd.ExecuteNonQuery();
                        if (r > 0)
                        {
                            savedPictures.Add(pic);
                        }
                    }
                }

                cmd.Parameters.Clear();

                // 物件写真の削除リストを処理
                if (property.PicturesToBeDeleted.Count > 0)
                {
                    foreach (var delp in property.PicturesToBeDeleted)
                    {
                        // 削除
                        cmd.Parameters.Clear();
                        cmd.CommandText = "DELETE FROM rent_commercial_pictures WHERE picture_id = @pictureId";
                        cmd.Parameters.AddWithValue("@pictureId", delp.Id);
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

                // PDF（建物）rent_commercial_pdfs
                if (property.Pdfs.Count > 0)
                {
                    foreach (var pic in property.Pdfs)
                    {
                        var sqlUpsertPdf = "INSERT INTO rent_commercial_pdfs (pdf_id, property_id, filename, thumbnail_filename, type, description, is_main) ";
                        sqlUpsertPdf += "VALUES (@pdfId, @propertyId, @filePath, @thumbnailPath, @type, @description, @isMain) ";
                        sqlUpsertPdf += "ON CONFLICT (pdf_id) ";
                        sqlUpsertPdf += "DO UPDATE SET filename = @filePath, thumbnail_filename = @thumbnailPath, type = @type, description = @description, is_main = @isMain";

                        cmd.CommandText = sqlUpsertPdf;

                        // ループなので、前のパラメーターをクリアする。
                        cmd.Parameters.Clear();

                        cmd.Parameters.AddWithValue("@pdfId", pic.Id);
                        cmd.Parameters.AddWithValue("@propertyId", property.Id);
                        cmd.Parameters.AddWithValue("@filePath", pic.PdfFilename);
                        cmd.Parameters.AddWithValue("@thumbnailPath", pic.ThumbnailFilename);
                        cmd.Parameters.AddWithValue("@type", pic.PdfType.ToString());
                        cmd.Parameters.AddWithValue("@description", pic.Description);

                        //Debug.WriteLine($"Inserting picture: {pic.ImageLocation}, {pic.Id}, isMain: {pic.IsMain} @DataAccess::InsertRentCommercial");

                        var paramIsMain = new SqliteParameter("@isMain", System.Data.DbType.Int32)
                        {
                            Value = pic.IsMain ? 1 : 0
                        };
                        cmd.Parameters.Add(paramIsMain);

                        var r = cmd.ExecuteNonQuery();
                        if (r > 0)
                        {
                            savedPdfs.Add(pic);
                        }
                    }
                }

                cmd.Parameters.Clear();

                // PDF（建物）の削除リストを処理
                if (property.PdfsToBeDeleted.Count > 0)
                {
                    foreach (var delp in property.PdfsToBeDeleted)
                    {
                        // 削除
                        cmd.Parameters.Clear();
                        cmd.CommandText = "DELETE FROM rent_commercial_pdfs WHERE pdf_id = @pdfId";
                        cmd.Parameters.AddWithValue("@pdfId", delp.Id);
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

                // 貸主（建物）rent_lessors_properties
                if (property.Lessors.Count > 0)
                {
                    foreach (var psn in property.Lessors)
                    {
                        var sqlUpsertLessor = "INSERT INTO rent_lessors_properties (lessor_id, property_id, property_context_type) ";
                        sqlUpsertLessor += "VALUES (@lessor_id, @property_id, @property_context_type) ";
                        sqlUpsertLessor += "ON CONFLICT (lessor_id, property_id) ";
                        sqlUpsertLessor += "DO NOTHING";

                        cmd.CommandText = sqlUpsertLessor;
                        cmd.Parameters.Clear();
                        cmd.Parameters.AddWithValue("@lessor_id", psn.Id);
                        cmd.Parameters.AddWithValue("@property_id", property.Id);
                        cmd.Parameters.AddWithValue("@property_context_type", property.PropertyContextType.ToString());
                        cmd.ExecuteNonQuery();
                    }
                }

                cmd.Parameters.Clear();

                // 貸主（建物）の削除リストを処理
                if (property.LessorsToBeDeleted.Count > 0)
                {
                    foreach (var psn in property.LessorsToBeDeleted)
                    {
                        cmd.Parameters.Clear();
                        cmd.CommandText = "DELETE FROM rent_lessors_properties WHERE lessor_id = @lessorId AND property_id = @propertyId";
                        cmd.Parameters.AddWithValue("@lessorId", psn.Id);
                        cmd.Parameters.AddWithValue("@propertyId", property.Id);
                        var sqlResult = cmd.ExecuteNonQuery();
                        if (sqlResult > 0)
                        {
                            Debug.WriteLine("Lessor deleted");
                        }
                    }
                }

                cmd.Parameters.Clear();

                // 宅建業者（建物）brokers_properties
                if (property.Brokers.Count > 0)
                {
                    foreach (var psn in property.Brokers)
                    {
                        var sqlUpsertBroker = "INSERT INTO brokers_properties (broker_id, property_id, property_context_type) ";
                        sqlUpsertBroker += "VALUES (@broker_id, @property_id, @property_context_type) ";
                        sqlUpsertBroker += "ON CONFLICT (broker_id, property_id) ";
                        sqlUpsertBroker += "DO NOTHING";

                        cmd.CommandText = sqlUpsertBroker;
                        cmd.Parameters.Clear();
                        cmd.Parameters.AddWithValue("@broker_id", psn.Id);
                        cmd.Parameters.AddWithValue("@property_id", property.Id);
                        cmd.Parameters.AddWithValue("@property_context_type", property.PropertyContextType.ToString());
                        cmd.ExecuteNonQuery();
                    }
                }

                cmd.Parameters.Clear();

                // 宅建業者（建物）の削除リストを処理
                if (property.BrokersToBeDeleted.Count > 0)
                {
                    foreach (var psn in property.BrokersToBeDeleted)
                    {
                        cmd.Parameters.Clear();
                        cmd.CommandText = "DELETE FROM brokers_properties WHERE broker_id = @brokerId AND property_id = @propertyId";
                        cmd.Parameters.AddWithValue("@brokerId", psn.Id);
                        cmd.Parameters.AddWithValue("@propertyId", property.Id);
                        var sqlResult = cmd.ExecuteNonQuery();
                        if (sqlResult > 0)
                        {
                            Debug.WriteLine("Broker deleted");
                        }
                    }
                }

                cmd.Parameters.Clear();

                // 募集物件 rent_commercial_units
                if (property.ListingsToBeSaved.Count > 0)
                {
                    foreach (var listing in property.ListingsToBeSaved)
                    {
                        // Reuse the code for upsert listing.
                        _ = DoUpsertRentCommercialListing(cmd, property.Id, listing);
                    }
                }

                cmd.Parameters.Clear();

                // 募集物件の削除リストを処理
                if (property.ListingsToBeDeleted.Count > 0)
                {
                    foreach (var delr in property.ListingsToBeDeleted)
                    {
                        // 削除
                        cmd.Parameters.Clear();
                        cmd.CommandText = "DELETE FROM rent_commercial_units WHERE listing_id = @listingId";
                        cmd.Parameters.AddWithValue("@listingId", delr);
                        var deletedListingResult = cmd.ExecuteNonQuery();
                        if (deletedListingResult > 0)
                        {
                            // TODO:
                            Debug.WriteLine("Listing deleted @UpdateRentCommercial in DataAccessService");
                        }
                    }
                    // Let's not
                    //property.ListingsToBeDeleted.Clear();
                }

                // commit
                cmd.Transaction.Commit();

                property.LessorsToBeDeleted.Clear();
                property.BrokersToBeDeleted.Clear();
            }
            catch (Exception ex)
            {
                cmd.Transaction.Rollback();

                SetDatabaseError(res, ex, "cmd.ExecuteNonQuery(), cmd.Transaction.Commit", "Failed to update database tables. Transaction.Rollback()", nameof(UpsertRentCommercial));
                return res;
            }
        }
        catch (Exception ex)
        {
            SetDatabaseError(res, ex, "connection.Open()", "Failed to connect to a SQLite database file", nameof(UpsertRentCommercial));
        }
        finally
        {
            _readerWriterLock.ExitWriteLock();
        }

        if (res.IsError) return res;

        foreach (var picture in savedPictures)
        {
            picture.SetStatus(EntityStatus.Saved);
            picture.SetIsModified(false);
        }
        foreach (var pdf in savedPdfs)
        {
            pdf.SetStatus(EntityStatus.Saved);
            pdf.SetIsModified(false);
        }

        //Debug.WriteLine(string.Format("{0} Entries Inserted to DB", res.AffectedCount.ToString()));

        property.SetIsModified(false);
        property.SetStatus(EntityStatus.Saved);

        return res;
    }

    public RentCommercialPropertyResultWrapper SelectRentCommercialById(string propertyId, bool duplicate = false)
    {
        var res = new RentCommercialPropertyResultWrapper();

        if (string.IsNullOrEmpty(propertyId))
        {
            res.IsError = true;
            // TODO:
            return res;
        }

        ZumenSearch.Domain.Entities.Rent.Commercials.Property property;

        if (duplicate)
        {
            property = new ZumenSearch.Domain.Entities.Rent.Commercials.Property(Guid.CreateVersion7().ToString("N"), EntityStatus.New);
        }
        else
        {
            property = new ZumenSearch.Domain.Entities.Rent.Commercials.Property(propertyId, EntityStatus.Saved);
        }

        _readerWriterLock.EnterReadLock();
        try
        {
            using var connection = new SqliteConnection(_connectionStringBuilder.ConnectionString);
            connection.Open();

            using var cmd = connection.CreateCommand();
            cmd.CommandText = "SELECT properties.name as propertyName, " +
                "properties.property_context_type as propertyContextType, " +
                "properties.fudousan_id as fudousanId, " +
                "properties.fudousan_id_additional_code as fudousanIdAdditionalCode, " +
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
                "properties.thumbnail_filename as thumbnail_filename, " +

                "rent_commercials.commercial_kind as commercial_kind, " +
                "rent_commercials.is_unit_ownership as is_unit_ownership, " +
                "rent_commercials.property_structure as property_structure, " +
                "rent_commercials.floor_count_above_ground as floor_count_above_ground, " +
                "rent_commercials.floor_count_basement as floor_count_basement, " +
                "rent_commercials.total_floor_area as total_floor_area, " +
                "rent_commercials.built_year_month as built_year_month, " +
                "rent_commercials.remarks as remarks, " +
                "rent_commercials.passenger_elevator_availability as passenger_elevator_availability, " +
                "rent_commercials.passenger_elevator_count as passenger_elevator_count, " +
                "rent_commercials.freight_elevator_availability as freight_elevator_availability, " +
                "rent_commercials.freight_elevator_details as freight_elevator_details, " +
                "rent_commercials.loading_area_availability as loading_area_availability, " +
                "rent_commercials.loading_area_details as loading_area_details, " +
                "rent_commercials.security_availability as security_availability, " +
                "rent_commercials.security_mechanical as security_mechanical, " +
                "rent_commercials.security_staffed as security_staffed, " +
                "rent_commercials.security_autolock as security_autolock, " +
                "rent_commercials.security_camera as security_camera, " +
                "rent_commercials.security_other as security_other, " +
                "rent_commercials.security_details as security_details, " +
                "rent_commercials.electricity_availability as electricity_availability, " +
                "rent_commercials.electricity_details as electricity_details, " +
                "rent_commercials.gas_availability as gas_availability, " +
                "rent_commercials.gas_type as gas_type, " +
                "rent_commercials.gas_details as gas_details, " +
                "rent_commercials.water_availability as water_availability, " +
                "rent_commercials.water_type as water_type, " +
                "rent_commercials.water_details as water_details, " +
                "rent_commercials.sewerage_type as sewerage_type, " +
                "rent_commercials.sewerage_details as sewerage_details, " +
                "rent_commercials.internet_availability as internet_availability, " +
                "rent_commercials.fiber_availability as fiber_availability, " +
                "rent_commercials.internet_details as internet_details, " +
                "rent_commercials.fire_safety_availability as fire_safety_availability, " +
                "rent_commercials.fire_safety_details as fire_safety_details, " +
                "rent_commercials.parking_availability as parking_availability, " +
                "rent_commercials.parking_vacancy as parking_vacancy, " +
                "rent_commercials.parking_spaces as parking_spaces, " +
                "rent_commercials.parking_monthly_fee as parking_monthly_fee, " +
                "rent_commercials.parking_tax as parking_tax, " +
                "rent_commercials.parking_free as parking_free, " +
                "rent_commercials.parking_details as parking_details, " +
                "rent_commercials.management_type as management_type, " +
                "rent_commercials.management_maintenance as management_maintenance, " +
                "rent_commercials.management_tenant as management_tenant, " +
                "rent_commercials.management_payment as management_payment, " +
                "rent_commercials.management_other as management_other, " +
                "rent_commercials.management_company_remarks as management_company_remarks, " +
                "rent_commercials.management_external_name as management_external_name, " +
                "rent_commercials.management_external_contact as management_external_contact, " +
                "rent_commercials.management_external_remarks as management_external_remarks, " +
                "rent_commercials.management_lessor_remarks as management_lessor_remarks, " +

                "properties.property_id as propertyId " +
                "FROM rent_commercials INNER JOIN properties USING (property_id) WHERE properties.property_id = @propertyId";
            cmd.Parameters.Clear();
            cmd.Parameters.AddWithValue("@propertyId", propertyId);

            bool isFound = false;

            using (var reader = cmd.ExecuteReader())
            {
                if (reader.Read())
                {
                    var pId = Convert.ToString(reader["propertyId"], CultureInfo.InvariantCulture);

                    if (!propertyId.Equals(pId, StringComparison.Ordinal))
                    {
                        Debug.WriteLine("DataAccess::SelectRentCommercialsById: propertyId is null or empty for a rent commercial entry.");
                        //continue;
                    }

                    isFound = true;

                    property.SetName(Convert.ToString(reader["propertyName"], CultureInfo.InvariantCulture) ?? "");

                    // Address/Location
                    property.Address.SetMachiazaId(Convert.ToString(reader["locMachiazaId"], CultureInfo.InvariantCulture) ?? "");
                    property.Address.SetPrefectureByMunicipalityCode(Convert.ToString(reader["locPrefId"], CultureInfo.InvariantCulture) ?? "", Convert.ToString(reader["locPrefecture"], CultureInfo.InvariantCulture) ?? "");
                    property.Address.SetCountyAndCity(property.Address.MachiazaId, Convert.ToString(reader["locCounty"], CultureInfo.InvariantCulture) ?? "", Convert.ToString(reader["locCity"], CultureInfo.InvariantCulture) ?? "");
                    property.Address.SetWardAndOaza(property.Address.MachiazaId, Convert.ToString(reader["locWard"], CultureInfo.InvariantCulture) ?? "", Convert.ToString(reader["locOazaCho"], CultureInfo.InvariantCulture) ?? "");
                    property.Address.SetChoume(property.Address.MachiazaId, Convert.ToString(reader["locChoume"], CultureInfo.InvariantCulture) ?? "");
                    property.Address.SetEdaban(Convert.ToString(reader["locEdaban"], CultureInfo.InvariantCulture) ?? "");
                    property.Address.SetPostalCode(Convert.ToString(reader["locPostalCode"], CultureInfo.InvariantCulture) ?? string.Empty);
                    property.Address.SetLocationLatitude(Convert.ToString(reader["locationLatitude"], CultureInfo.InvariantCulture) ?? string.Empty);
                    property.Address.SetLocationLongitude(Convert.ToString(reader["locationLongitude"], CultureInfo.InvariantCulture) ?? string.Empty);

                    property.Train1.SetRailLine(Convert.ToString(reader["train1LineCode"], CultureInfo.InvariantCulture) ?? "", Convert.ToString(reader["train1LineName"], CultureInfo.InvariantCulture) ?? "");
                    property.Train1.SetRailStation(property.Train1.RailLine.LineCode, Convert.ToString(reader["train1StationCode"], CultureInfo.InvariantCulture) ?? "", Convert.ToString(reader["train1StationName"], CultureInfo.InvariantCulture) ?? "");
                    property.SetBusStop1(Convert.ToString(reader["buss1StopName"], CultureInfo.InvariantCulture) ?? "");
                    property.SetBusJyousya1(Convert.ToString(reader["buss1Jyousya"], CultureInfo.InvariantCulture) ?? "");
                    property.SetBusStopToho1(Convert.ToString(reader["buss1Toho"], CultureInfo.InvariantCulture) ?? "");
                    property.Train1.SetEkiToho(Convert.ToInt32(reader["train1EkiToho"], CultureInfo.InvariantCulture));

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

                    property.SetCommercialKindTypeLabelFromString(Convert.ToString(reader["commercial_kind"], CultureInfo.InvariantCulture) ?? "");
                    property.SetIsUnitOwnership(Convert.ToInt32(reader["is_unit_ownership"], CultureInfo.InvariantCulture) != 0); // int to bool
                    property.SetPropertyStructureTypeLabelFromString(Convert.ToString(reader["property_structure"], CultureInfo.InvariantCulture) ?? string.Empty);
                    property.SetFloorCountAboveGround(Convert.ToInt32(reader["floor_count_above_ground"], CultureInfo.InvariantCulture));
                    property.SetFloorCountBasement(Convert.ToInt32(reader["floor_count_basement"], CultureInfo.InvariantCulture));
                    property.SetTotalFloorArea(decimal.Parse(reader.GetString(reader.GetOrdinal("total_floor_area")), CultureInfo.InvariantCulture));
                    property.SetBuiltYearAndMonthFromString(Convert.ToString(reader["built_year_month"], CultureInfo.InvariantCulture) ?? string.Empty);
                    property.ThumbnailFilename = Convert.ToString(reader["thumbnail_filename"], CultureInfo.InvariantCulture) ?? string.Empty;
                    property.SetFudousanId(Convert.ToString(reader["fudousanId"], CultureInfo.InvariantCulture) ?? string.Empty);
                    property.SetFudousanIdAdditionalCode(Convert.ToString(reader["fudousanIdAdditionalCode"], CultureInfo.InvariantCulture) ?? string.Empty);
                    property.SetRemarks(Convert.ToString(reader["remarks"], CultureInfo.InvariantCulture) ?? string.Empty);
                    property.SetKanriShutai((ZumenSearch.Domain.Entities.Rent.Commercials.Property.EnumKanriShutai)Convert.ToInt32(reader["management_type"], CultureInfo.InvariantCulture));
                    property.SetKanriIsMaintenanceManagementIfIsKanriJisya(Convert.ToInt32(reader["management_maintenance"], CultureInfo.InvariantCulture) != 0);
                    property.SetKanriIsTenantManagementIfIsKanriJisya(Convert.ToInt32(reader["management_tenant"], CultureInfo.InvariantCulture) != 0);
                    property.SetKanriIsPaymentManagementIfIsKanriJisya(Convert.ToInt32(reader["management_payment"], CultureInfo.InvariantCulture) != 0);
                    property.SetKanriIsOtherManagementIfIsKanriJisya(Convert.ToInt32(reader["management_other"], CultureInfo.InvariantCulture) != 0);
                    property.SetKanriRemarkIfIsKanriJisya(Convert.ToString(reader["management_company_remarks"], CultureInfo.InvariantCulture) ?? string.Empty);
                    property.SetKanriNameOfCompanyIfIsKanriTasya(Convert.ToString(reader["management_external_name"], CultureInfo.InvariantCulture) ?? string.Empty);
                    property.SetKanriContactInfoOfCompanyIfIsKanriTasya(Convert.ToString(reader["management_external_contact"], CultureInfo.InvariantCulture) ?? string.Empty);
                    property.SetKanriRemarkIfIsKanriTasya(Convert.ToString(reader["management_external_remarks"], CultureInfo.InvariantCulture) ?? string.Empty);
                    property.SetKanriRemarkIfIsKanriKashinushi(Convert.ToString(reader["management_lessor_remarks"], CultureInfo.InvariantCulture) ?? string.Empty);

                    property.HasPassengerElevator = Convert.ToInt32(reader["passenger_elevator_availability"], CultureInfo.InvariantCulture) != 0;
                    property.PassengerElevatorCount = reader.IsDBNull(reader.GetOrdinal("passenger_elevator_count")) ? null : Convert.ToInt32(reader["passenger_elevator_count"], CultureInfo.InvariantCulture);
                    property.HasFreightElevator = Convert.ToInt32(reader["freight_elevator_availability"], CultureInfo.InvariantCulture) != 0;
                    property.FreightElevatorDetails = Convert.ToString(reader["freight_elevator_details"], CultureInfo.InvariantCulture) ?? string.Empty;
                    property.HasLoadingArea = Convert.ToInt32(reader["loading_area_availability"], CultureInfo.InvariantCulture) != 0;
                    property.LoadingAreaDetails = Convert.ToString(reader["loading_area_details"], CultureInfo.InvariantCulture) ?? string.Empty;
                    property.HasSecurity = Convert.ToInt32(reader["security_availability"], CultureInfo.InvariantCulture) != 0;
                    property.HasMechanicalSecurity = Convert.ToInt32(reader["security_mechanical"], CultureInfo.InvariantCulture) != 0;
                    property.HasStaffedSecurity = Convert.ToInt32(reader["security_staffed"], CultureInfo.InvariantCulture) != 0;
                    property.HasAutolock = Convert.ToInt32(reader["security_autolock"], CultureInfo.InvariantCulture) != 0;
                    property.HasSecurityCamera = Convert.ToInt32(reader["security_camera"], CultureInfo.InvariantCulture) != 0;
                    property.HasOtherSecurity = Convert.ToInt32(reader["security_other"], CultureInfo.InvariantCulture) != 0;
                    property.SecurityDetails = Convert.ToString(reader["security_details"], CultureInfo.InvariantCulture) ?? string.Empty;
                    property.HasElectricity = Convert.ToInt32(reader["electricity_availability"], CultureInfo.InvariantCulture) != 0;
                    property.ElectricityDetails = Convert.ToString(reader["electricity_details"], CultureInfo.InvariantCulture) ?? string.Empty;
                    property.HasGas = Convert.ToInt32(reader["gas_availability"], CultureInfo.InvariantCulture) != 0;
                    property.GasType = (ZumenSearch.Domain.Entities.Rent.Commercials.GasSupplyType)Convert.ToInt32(reader["gas_type"], CultureInfo.InvariantCulture);
                    property.GasDetails = Convert.ToString(reader["gas_details"], CultureInfo.InvariantCulture) ?? string.Empty;
                    property.HasWater = Convert.ToInt32(reader["water_availability"], CultureInfo.InvariantCulture) != 0;
                    property.WaterType = (ZumenSearch.Domain.Entities.Rent.Commercials.WaterSupplyType)Convert.ToInt32(reader["water_type"], CultureInfo.InvariantCulture);
                    property.WaterDetails = Convert.ToString(reader["water_details"], CultureInfo.InvariantCulture) ?? string.Empty;
                    property.SewerageType = (ZumenSearch.Domain.Entities.Rent.Commercials.SewerageType)Convert.ToInt32(reader["sewerage_type"], CultureInfo.InvariantCulture);
                    property.SewerageDetails = Convert.ToString(reader["sewerage_details"], CultureInfo.InvariantCulture) ?? string.Empty;
                    property.HasInternet = Convert.ToInt32(reader["internet_availability"], CultureInfo.InvariantCulture) != 0;
                    property.HasFiber = Convert.ToInt32(reader["fiber_availability"], CultureInfo.InvariantCulture) != 0;
                    property.InternetDetails = Convert.ToString(reader["internet_details"], CultureInfo.InvariantCulture) ?? string.Empty;
                    property.HasFireSafety = Convert.ToInt32(reader["fire_safety_availability"], CultureInfo.InvariantCulture) != 0;
                    property.FireSafetyDetails = Convert.ToString(reader["fire_safety_details"], CultureInfo.InvariantCulture) ?? string.Empty;
                    property.HasParking = Convert.ToInt32(reader["parking_availability"], CultureInfo.InvariantCulture) != 0;
                    property.ParkingVacancy = (ZumenSearch.Domain.Entities.Rent.Commercials.ParkingVacancyType)Convert.ToInt32(reader["parking_vacancy"], CultureInfo.InvariantCulture);
                    property.ParkingSpaces = reader.IsDBNull(reader.GetOrdinal("parking_spaces")) ? null : Convert.ToInt32(reader["parking_spaces"], CultureInfo.InvariantCulture);
                    property.ParkingMonthlyFee = reader.IsDBNull(reader.GetOrdinal("parking_monthly_fee")) ? null : decimal.Parse(reader.GetString(reader.GetOrdinal("parking_monthly_fee")), CultureInfo.InvariantCulture);
                    property.ParkingTax = (ZumenSearch.Domain.Entities.Rent.Commercials.ParkingTaxType)Convert.ToInt32(reader["parking_tax"], CultureInfo.InvariantCulture);
                    property.IsParkingFree = Convert.ToInt32(reader["parking_free"], CultureInfo.InvariantCulture) != 0;
                    property.ParkingDetails = Convert.ToString(reader["parking_details"], CultureInfo.InvariantCulture) ?? string.Empty;

                    property.SetIsModified(false);

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
            cmd.CommandText = "SELECT * FROM rent_commercial_pictures WHERE property_id = @propertyId";
            cmd.Parameters.Clear();
            cmd.Parameters.AddWithValue("@propertyId", propertyId);
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    var picid = Convert.ToString(reader["picture_id"], CultureInfo.InvariantCulture) ?? string.Empty;

                    // If duplicate is true, generate a new picture ID; otherwise, use the existing one.
                    picid = duplicate ? Guid.CreateVersion7().ToString("N") : picid;

                    var picpath = Convert.ToString(reader["filename"], CultureInfo.InvariantCulture) ?? string.Empty;
                    if (!string.IsNullOrEmpty(picid) && !string.IsNullOrEmpty(picpath))
                    {
                        var rlpic = new ZumenSearch.Domain.Entities.Rent.Commercials.PropertyPicture(picid, picpath, EntityStatus.Saved)
                        {
                            Description = Convert.ToString(reader["description"], CultureInfo.InvariantCulture) ?? string.Empty,
                        };

                        var strType = Convert.ToString(reader["type"], CultureInfo.InvariantCulture) ?? string.Empty;
                        if (!Enum.TryParse<ZumenSearch.Domain.Enums.Rent.Commercials.PropertyPictureType>(strType, out var pictureType) || !Enum.IsDefined(pictureType))
                        {
                            Debug.WriteLine($"Invalid commercial property picture type: {strType}. Defaulting to Unspecified.");
                            pictureType = ZumenSearch.Domain.Enums.Rent.Commercials.PropertyPictureType.Unspecified;
                        }
                        rlpic.PictureType = pictureType;
                        rlpic.IsMain = Convert.ToInt32(reader["is_main"], CultureInfo.InvariantCulture) != 0; // int to bool
                        rlpic.SetIsModified(false);

                        property.Pictures.Add(rlpic);
                    }
                    else
                    {
                        Debug.WriteLine("picture_id or filename is null/empty.");
                    }
                }
            }

            // PDF（建物）
            cmd.CommandText = "SELECT * FROM rent_commercial_pdfs WHERE property_id = @propertyId";
            cmd.Parameters.Clear();
            cmd.Parameters.AddWithValue("@propertyId", propertyId);
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    var pdfid = Convert.ToString(reader["pdf_id"], CultureInfo.InvariantCulture) ?? string.Empty;

                    // If duplicate is true, generate a new PDF ID; otherwise, use the existing one.
                    pdfid = duplicate ? Guid.CreateVersion7().ToString("N") : pdfid;

                    var pdfpath = Convert.ToString(reader["filename"], CultureInfo.InvariantCulture) ?? string.Empty;
                    var thumbpath = Convert.ToString(reader["thumbnail_filename"], CultureInfo.InvariantCulture) ?? string.Empty;
                    if (!string.IsNullOrWhiteSpace(pdfid) && !string.IsNullOrWhiteSpace(pdfpath) && !string.IsNullOrWhiteSpace(thumbpath))
                    {
                        var rlpdf = new ZumenSearch.Domain.Entities.Rent.Commercials.PropertyPdf(pdfid, pdfpath, thumbpath, EntityStatus.Saved)
                        {
                            Description = Convert.ToString(reader["description"], CultureInfo.InvariantCulture) ?? string.Empty
                        };

                        var strType = Convert.ToString(reader["type"], CultureInfo.InvariantCulture) ?? string.Empty;
                        if (!Enum.TryParse<ZumenSearch.Domain.Enums.Rent.Commercials.PropertyPdfType>(strType, out var pdfType) || !Enum.IsDefined(pdfType))
                        {
                            Debug.WriteLine($"Invalid commercial property PDF type: {strType}. Defaulting to Unspecified.");
                            pdfType = ZumenSearch.Domain.Enums.Rent.Commercials.PropertyPdfType.Unspecified;
                        }
                        rlpdf.PdfType = pdfType;
                        rlpdf.IsMain = Convert.ToInt32(reader["is_main"], CultureInfo.InvariantCulture) != 0; // int to bool
                        rlpdf.SetIsModified(false);

                        property.Pdfs.Add(rlpdf);
                    }
                    else
                    {
                        throw new InvalidDataException("Commercial property PDF has an empty ID, filename or thumbnail filename.");
                    }
                }
            }

            // 貸主（建物）
            var lessorIdList = new List<string>();
            cmd.CommandText = "SELECT * FROM rent_lessors_properties WHERE property_id = @propertyId";
            cmd.Parameters.Clear();
            cmd.Parameters.AddWithValue("@propertyId", propertyId);
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
                    cmd.CommandText = "SELECT lessor_id, name, person_kind, name_last, name_first, name_company, name_company_type, name_company_type_position, phone_number, mobile_phone_number, contact_name, location_postal_code, location_full, location_machiaza_id, location_prefecture, location_county, location_city, location_ward, location_oaza, location_choume, location_edaban, remarks FROM rent_lessors WHERE lessor_id = @lessorId";
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
                                Debug.WriteLine("DataAccess::SelectRentCommercialById: lessor_id is null or empty.");
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
            cmd.CommandText = "SELECT * FROM brokers_properties WHERE property_id = @propertyId";
            cmd.Parameters.Clear();
            cmd.Parameters.AddWithValue("@propertyId", propertyId);
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
                    cmd.CommandText = "SELECT broker_id, name, person_kind, name_last, name_first, name_company, name_company_type, name_company_type_position, phone_number, mobile_phone_number, contact_name, location_postal_code, location_full, location_machiaza_id, location_prefecture, location_county, location_city, location_ward, location_oaza, location_choume, location_edaban, remarks FROM brokers WHERE broker_id = @brokerId";
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
                                Debug.WriteLine("DataAccess::SelectRentCommercialById: broker_id is null or empty.");
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

            // Do not duplicate the listings if this is a duplicate operation; otherwise, retrieve the listings for the property.
            if (!duplicate)
            {
                // 募集物件
                cmd.CommandText = "SELECT listing_id, name FROM rent_commercial_units WHERE property_id = @propertyId";
                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("@propertyId", propertyId);
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var listingId = reader.GetString(reader.GetOrdinal("listing_id"));
                        var listingName = reader.GetString(reader.GetOrdinal("name"));
                        if (string.IsNullOrWhiteSpace(listingId)) throw new InvalidOperationException("Commercial listing ID is empty.");
                        property.ListingSummaries.Add(new ZumenSearch.Domain.Entities.Rent.Commercials.ListingSummary(listingId, listingName));
                    }
                }
            }

            // Reset property Isdirty flag.
            if (duplicate)
            {
                property.SetStatus(EntityStatus.New);
            }
            else
            {
                property.SetStatus(EntityStatus.Saved);
            }
            property.SetIsModified(duplicate);

            res.Property = property;
        }
        catch (Exception ex)
        {
            SetDatabaseError(res, ex, "connection.Open(), reader.Read()", "Failed to connect to or read a SQLite database file", nameof(SelectRentCommercialById));
        }
        finally
        {
            _readerWriterLock.ExitReadLock();
        }

        return res;
    }

    public ZumenSearch.Application.Results.ResultWrapper DeleteRentCommercial(string propertyId)
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
                cmd.CommandText = "DELETE FROM properties WHERE property_id = @propertyId;";
                cmd.Parameters.AddWithValue("@propertyId", propertyId);
                res.AffectedCount = cmd.ExecuteNonQuery();

                cmd.Transaction.Commit();
            }
            catch (Exception ex)
            {
                cmd.Transaction.Rollback();

                SetDatabaseError(res, ex, "cmd.ExecuteNonQuery(), cmd.Transaction.Commit", "Failed to delete database recode. Transaction.Rollback()", nameof(DeleteRentCommercial));
                return res;
            }
        }
        catch (Exception ex)
        {
            SetDatabaseError(res, ex, "Connection.Open", "Failed to Connect to a SQLite database file", nameof(DeleteRentCommercial));

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

    #region == Rent Commercial Listing ==

    ResultWrapper IRentCommercialListingRepository.Upsert(string propertyId, ZumenSearch.Domain.Entities.Rent.Commercials.Listing.Listing listing) => UpsertRentCommercialListing(propertyId, listing);

    ListingSearchResultWrapper IRentCommercialListingRepository.Search(RentCommercialListingSearchCriteria criteria) => SelectRentCommercialListings(criteria);

    RentCommercialListingResultWrapper IRentCommercialListingRepository.SelectById(string propertyId, string listingId, bool duplicate) => SelectRentCommercialListingById(propertyId, listingId, duplicate);

    ResultWrapper IRentCommercialListingRepository.Delete(string listingId) => DeleteRentCommercialListing(listingId);

    private static int DoUpsertRentCommercialListing(SqliteCommand cmd, string propertyId, ZumenSearch.Domain.Entities.Rent.Commercials.Listing.Listing listing)
    {
        if (string.IsNullOrWhiteSpace(propertyId) || string.IsNullOrWhiteSpace(listing.Id) || !string.Equals(listing.PropertyId, propertyId, StringComparison.Ordinal))
        {
            throw new InvalidOperationException("Commercial listing property ID does not match the supplied property ID.");
        }

        cmd.CommandText = """
            INSERT INTO rent_commercial_units (
                listing_id, property_id, is_property_unit_ownership, name, fudousan_id_additional_code,
                floor_area, floor_ground, floor_number, usage, use_details, availability_status,
                is_available_for_rent, availability_month, availability_period,
                is_available_immediately, current_status_checked_at, remarks, chinryou, rent_tax,
                kyoueki_fee, management_fee_tax, shikikin, shikikin_unit, guarantee_deposit, guarantee_deposit_unit,
                reikin, reikin_unit, rights_money, rights_money_unit, deposit_deduction, deposit_deduction_unit,
                contract_type, contract_period_years, renewal_fee, renewal_fee_unit, recontract_fee, recontract_fee_unit,
                guarantor_company, insurance, other_fees, contract_conditions, contract_remarks,
                handling_type, transaction_type, lessor_commission, lessor_commission_unit,
                lessee_commission, lessee_commission_unit, listing_broker_commission, listing_broker_commission_unit,
                tenant_broker_commission, tenant_broker_commission_unit, transaction_remarks,
                air_conditioning, private_toilet, hot_water, kitchen, ventilation, unit_internet, equipment_details,
                unit_condition, fixture_transfer, business_hours, other_conditions,
                permits_heavy_food, permits_light_food, permits_retail, permits_services, permits_office, permits_other,
                permitted_business_details, facilities_remarks, updated_at)
            VALUES (
                @listingId, @propertyId, @isUnitOwnership, @name, @fudousanIdAdditionalCode,
                @floorArea, @floorGround, @floorNumber, @usage, @useDetails, @availabilityStatus,
                @isAvailableForRent, @availabilityMonth, @availabilityPeriod,
                @isAvailableImmediately, @currentStatusCheckedAt, @remarks, @chinryou, @rentTax,
                @managementFee, @managementFeeTax, @securityDeposit, @securityDepositUnit, @guaranteeDeposit, @guaranteeDepositUnit,
                @keyMoney, @keyMoneyUnit, @rightsMoney, @rightsMoneyUnit, @depositDeduction, @depositDeductionUnit,
                @contractType, @contractPeriodYears, @renewalFee, @renewalFeeUnit, @recontractFee, @recontractFeeUnit,
                @guarantorCompany, @insurance, @otherFees, @contractConditions, @contractRemarks,
                @handlingType, @transactionType, @lessorCommission, @lessorCommissionUnit,
                @lesseeCommission, @lesseeCommissionUnit, @listingBrokerCommission, @listingBrokerCommissionUnit,
                @tenantBrokerCommission, @tenantBrokerCommissionUnit, @transactionRemarks,
                @airConditioning, @privateToilet, @hotWater, @kitchen, @ventilation, @unitInternet, @equipmentDetails,
                @unitCondition, @fixtureTransfer, @businessHoursRestrictions, @useConditions,
                @permitsHeavyFood, @permitsLightFood, @permitsRetail, @permitsServices, @permitsOffice, @permitsOther,
                @permittedBusinessDetails, @facilitiesRemarks, @updatedAt)
            ON CONFLICT(listing_id) DO UPDATE SET
                is_property_unit_ownership = @isUnitOwnership,
                name = @name,
                fudousan_id_additional_code = @fudousanIdAdditionalCode,
                floor_area = @floorArea,
                floor_ground = @floorGround,
                floor_number = @floorNumber,
                usage = @usage,
                use_details = @useDetails,
                availability_status = @availabilityStatus,
                is_available_for_rent = @isAvailableForRent,
                availability_month = @availabilityMonth,
                availability_period = @availabilityPeriod,
                is_available_immediately = @isAvailableImmediately,
                current_status_checked_at = @currentStatusCheckedAt,
                remarks = @remarks,
                chinryou = @chinryou,
                rent_tax = @rentTax,
                kyoueki_fee = @managementFee,
                management_fee_tax = @managementFeeTax,
                shikikin = @securityDeposit,
                shikikin_unit = @securityDepositUnit,
                guarantee_deposit = @guaranteeDeposit,
                guarantee_deposit_unit = @guaranteeDepositUnit,
                reikin = @keyMoney,
                reikin_unit = @keyMoneyUnit,
                rights_money = @rightsMoney,
                rights_money_unit = @rightsMoneyUnit,
                deposit_deduction = @depositDeduction,
                deposit_deduction_unit = @depositDeductionUnit,
                contract_type = @contractType,
                contract_period_years = @contractPeriodYears,
                renewal_fee = @renewalFee,
                renewal_fee_unit = @renewalFeeUnit,
                recontract_fee = @recontractFee,
                recontract_fee_unit = @recontractFeeUnit,
                guarantor_company = @guarantorCompany,
                insurance = @insurance,
                other_fees = @otherFees,
                contract_conditions = @contractConditions,
                contract_remarks = @contractRemarks,
                handling_type = @handlingType,
                transaction_type = @transactionType,
                lessor_commission = @lessorCommission,
                lessor_commission_unit = @lessorCommissionUnit,
                lessee_commission = @lesseeCommission,
                lessee_commission_unit = @lesseeCommissionUnit,
                listing_broker_commission = @listingBrokerCommission,
                listing_broker_commission_unit = @listingBrokerCommissionUnit,
                tenant_broker_commission = @tenantBrokerCommission,
                tenant_broker_commission_unit = @tenantBrokerCommissionUnit,
                transaction_remarks = @transactionRemarks,
                air_conditioning = @airConditioning,
                private_toilet = @privateToilet,
                hot_water = @hotWater,
                kitchen = @kitchen,
                ventilation = @ventilation,
                unit_internet = @unitInternet,
                equipment_details = @equipmentDetails,
                unit_condition = @unitCondition,
                fixture_transfer = @fixtureTransfer,
                business_hours = @businessHoursRestrictions,
                other_conditions = @useConditions,
                permits_heavy_food = @permitsHeavyFood,
                permits_light_food = @permitsLightFood,
                permits_retail = @permitsRetail,
                permits_services = @permitsServices,
                permits_office = @permitsOffice,
                permits_other = @permitsOther,
                permitted_business_details = @permittedBusinessDetails,
                facilities_remarks = @facilitiesRemarks,
                updated_at = @updatedAt
            WHERE property_id = excluded.property_id;
            """;

        // Clear parameters before setting new ones because this method can be called multiple times in a loop.
        cmd.Parameters.Clear();

        cmd.Parameters.AddWithValue("@listingId", listing.Id);
        cmd.Parameters.AddWithValue("@propertyId", propertyId);
        cmd.Parameters.AddWithValue("@isUnitOwnership", listing.IsPropertyUnitOwnership ? 1 : 0); // bool to int
        cmd.Parameters.AddWithValue("@name", listing.Name);

        cmd.Parameters.AddWithValue("@floorGround", listing.FloorGround.ToString());
        cmd.Parameters.AddWithValue("@floorArea", (object?)listing.FloorArea?.ToString(CultureInfo.InvariantCulture) ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@floorNumber", (object?)listing.FloorNumber ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@usage", listing.Use.ToString());
        cmd.Parameters.AddWithValue("@useDetails", listing.UseDetails);
        cmd.Parameters.AddWithValue("@availabilityStatus", listing.CurrentStatus.ToString());
        cmd.Parameters.AddWithValue("@isAvailableForRent", listing.IsAvailableForRentNow ? 1 : 0);
        cmd.Parameters.AddWithValue("@availabilityMonth", listing.AvailabilityMonth.ToString());
        cmd.Parameters.AddWithValue("@availabilityPeriod", listing.AvailabilityPeriod.ToString());
        cmd.Parameters.AddWithValue("@isAvailableImmediately", listing.IsAvailableForImmediateMoveIn ? 1 : 0);
        cmd.Parameters.AddWithValue("@currentStatusCheckedAt", listing.OccupancyStatusCheckedAt?.ToString("O", CultureInfo.InvariantCulture) ?? string.Empty);

        cmd.Parameters.AddWithValue("@fudousanIdAdditionalCode", listing.FudousanIdAdditionalCode);
        cmd.Parameters.AddWithValue("@remarks", listing.Remarks);

        cmd.Parameters.AddWithValue("@chinryou", (object?)listing.Chinryou ?? DBNull.Value);

        cmd.Parameters.AddWithValue("@rentTax", listing.RentTax.ToString());
        cmd.Parameters.AddWithValue("@managementFee", (object?)listing.ManagementFee?.ToString(CultureInfo.InvariantCulture) ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@managementFeeTax", listing.ManagementFeeTax.ToString());
        cmd.Parameters.AddWithValue("@securityDeposit", (object?)listing.SecurityDeposit?.ToString(CultureInfo.InvariantCulture) ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@securityDepositUnit", listing.SecurityDepositUnit.ToString());
        cmd.Parameters.AddWithValue("@guaranteeDeposit", (object?)listing.GuaranteeDeposit?.ToString(CultureInfo.InvariantCulture) ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@guaranteeDepositUnit", listing.GuaranteeDepositUnit.ToString());
        cmd.Parameters.AddWithValue("@keyMoney", (object?)listing.KeyMoney?.ToString(CultureInfo.InvariantCulture) ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@keyMoneyUnit", listing.KeyMoneyUnit.ToString());
        cmd.Parameters.AddWithValue("@rightsMoney", (object?)listing.RightsMoney?.ToString(CultureInfo.InvariantCulture) ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@rightsMoneyUnit", listing.RightsMoneyUnit.ToString());
        cmd.Parameters.AddWithValue("@depositDeduction", (object?)listing.DepositDeduction?.ToString(CultureInfo.InvariantCulture) ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@depositDeductionUnit", listing.DepositDeductionUnit.ToString());
        cmd.Parameters.AddWithValue("@contractType", listing.ContractType.ToString());
        cmd.Parameters.AddWithValue("@contractPeriodYears", (object?)listing.ContractPeriodYears?.ToString(CultureInfo.InvariantCulture) ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@renewalFee", (object?)listing.RenewalFee?.ToString(CultureInfo.InvariantCulture) ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@renewalFeeUnit", listing.RenewalFeeUnit.ToString());
        cmd.Parameters.AddWithValue("@recontractFee", (object?)listing.RecontractFee?.ToString(CultureInfo.InvariantCulture) ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@recontractFeeUnit", listing.RecontractFeeUnit.ToString());
        cmd.Parameters.AddWithValue("@guarantorCompany", listing.GuarantorCompany);
        cmd.Parameters.AddWithValue("@insurance", listing.Insurance);
        cmd.Parameters.AddWithValue("@otherFees", listing.OtherFees);
        cmd.Parameters.AddWithValue("@contractConditions", listing.ContractConditions);
        cmd.Parameters.AddWithValue("@contractRemarks", listing.ContractRemarks);
        cmd.Parameters.AddWithValue("@handlingType", listing.HandlingType.ToString());
        cmd.Parameters.AddWithValue("@transactionType", listing.TransactionType.ToString());
        cmd.Parameters.AddWithValue("@lessorCommission", listing.LessorCommission);
        cmd.Parameters.AddWithValue("@lessorCommissionUnit", listing.LessorCommissionUnit.ToString());
        cmd.Parameters.AddWithValue("@lesseeCommission", listing.LesseeCommission);
        cmd.Parameters.AddWithValue("@lesseeCommissionUnit", listing.LesseeCommissionUnit.ToString());
        cmd.Parameters.AddWithValue("@listingBrokerCommission", listing.ListingBrokerCommission);
        cmd.Parameters.AddWithValue("@listingBrokerCommissionUnit", listing.ListingBrokerCommissionUnit.ToString());
        cmd.Parameters.AddWithValue("@tenantBrokerCommission", listing.TenantBrokerCommission);
        cmd.Parameters.AddWithValue("@tenantBrokerCommissionUnit", listing.TenantBrokerCommissionUnit.ToString());
        cmd.Parameters.AddWithValue("@transactionRemarks", listing.TransactionRemarks);
        cmd.Parameters.AddWithValue("@airConditioning", listing.HasAirConditioning ? 1 : 0);
        cmd.Parameters.AddWithValue("@privateToilet", listing.HasPrivateToilet ? 1 : 0);
        cmd.Parameters.AddWithValue("@hotWater", listing.HasHotWater ? 1 : 0);
        cmd.Parameters.AddWithValue("@kitchen", listing.HasKitchen ? 1 : 0);
        cmd.Parameters.AddWithValue("@ventilation", listing.HasVentilation ? 1 : 0);
        cmd.Parameters.AddWithValue("@unitInternet", listing.HasInternet ? 1 : 0);
        cmd.Parameters.AddWithValue("@equipmentDetails", listing.EquipmentDetails);
        cmd.Parameters.AddWithValue("@unitCondition", listing.UnitCondition.ToString());
        cmd.Parameters.AddWithValue("@fixtureTransfer", listing.FixtureTransfer.ToString());
        cmd.Parameters.AddWithValue("@businessHoursRestrictions", listing.BusinessHoursRestrictions);
        cmd.Parameters.AddWithValue("@useConditions", listing.UseConditions);
        cmd.Parameters.AddWithValue("@permitsHeavyFood", listing.PermitsHeavyFoodService ? 1 : 0);
        cmd.Parameters.AddWithValue("@permitsLightFood", listing.PermitsLightFoodService ? 1 : 0);
        cmd.Parameters.AddWithValue("@permitsRetail", listing.PermitsRetail ? 1 : 0);
        cmd.Parameters.AddWithValue("@permitsServices", listing.PermitsServices ? 1 : 0);
        cmd.Parameters.AddWithValue("@permitsOffice", listing.PermitsOffice ? 1 : 0);
        cmd.Parameters.AddWithValue("@permitsOther", listing.PermitsOther ? 1 : 0);
        cmd.Parameters.AddWithValue("@permittedBusinessDetails", listing.PermittedBusinessDetails);
        cmd.Parameters.AddWithValue("@facilitiesRemarks", listing.FacilitiesRemarks);

        cmd.Parameters.AddWithValue("@updatedAt", DateTimeOffset.UtcNow.ToString("s"));

        var result = cmd.ExecuteNonQuery();
        if (result != 1)
        {
            throw new InvalidOperationException("Commercial listing ID is already associated with a different property.");
        }

        cmd.Parameters.Clear();

        // 募集物件写真 rent_commercial_listing_pictures table - Insert or Update
        if (listing.Pictures.Count > 0)
        {
            foreach (var pic in listing.Pictures)
            {
                // Upsert
                var sqlUpsertListing = "INSERT INTO rent_commercial_listing_pictures (picture_id, listing_id, property_id, filename, type, description, is_main) VALUES (@picture_id, @listing_id, @property_id, @filename, @type, @description, @is_main) ";
                sqlUpsertListing += "ON CONFLICT(picture_id) ";
                sqlUpsertListing += "DO UPDATE SET filename = @filename, type = @type, description = @description, is_main = @is_main WHERE listing_id = @listing_id AND property_id = @property_id";
                var exec = true;

                cmd.CommandText = sqlUpsertListing;

                if (exec)
                {
                    // ループなので、前のパラメーターをクリアする。
                    cmd.Parameters.Clear();

                    cmd.Parameters.AddWithValue("@picture_id", pic.Id);
                    cmd.Parameters.AddWithValue("@listing_id", listing.Id);
                    cmd.Parameters.AddWithValue("@property_id", propertyId);
                    cmd.Parameters.AddWithValue("@filename", pic.ImageFilename);
                    cmd.Parameters.AddWithValue("@type", pic.PictureType.ToString());
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

                    if (cmd.ExecuteNonQuery() != 1) throw new InvalidOperationException("Commercial listing picture ID belongs to another listing.");
                }

            }
        }

        // 募集物件写真の削除リストを処理
        if (listing.PicturesToBeDeleted.Count > 0)
        {
            foreach (var delr in listing.PicturesToBeDeleted)
            {
                cmd.Parameters.Clear();
                cmd.CommandText = "DELETE FROM rent_commercial_listing_pictures WHERE picture_id = @id AND listing_id = @listingId AND property_id = @propertyId";
                cmd.Parameters.AddWithValue("@id", delr.Id);
                cmd.Parameters.AddWithValue("@listingId", listing.Id);
                cmd.Parameters.AddWithValue("@propertyId", propertyId);
                cmd.ExecuteNonQuery();
            }
            // let's not do this.
            //listing.UnitPicturesToBeDeleted.Clear();
        }

        // 募集物件図面 rent_commercial_listing_pdfs table - Insert or Update
        if (listing.Pdfs.Count > 0)
        {
            foreach (var pdf in listing.Pdfs)
            {
                // Upsert
                var sqlUpsertListing = "INSERT INTO rent_commercial_listing_pdfs (pdf_id, listing_id, property_id, filename, thumbnail_filename, type, description, is_main) VALUES (@pdf_id, @listing_id, @property_id, @filename, @thumbnail_filename, @type, @description, @is_main) ";
                sqlUpsertListing += "ON CONFLICT(pdf_id) ";
                sqlUpsertListing += "DO UPDATE SET filename = @filename, thumbnail_filename = @thumbnail_filename, type = @type, description = @description, is_main = @is_main WHERE listing_id = @listing_id AND property_id = @property_id";
                var exec = true;

                cmd.CommandText = sqlUpsertListing;

                if (exec)
                {
                    // ループなので、前のパラメーターをクリアする。
                    cmd.Parameters.Clear();

                    cmd.Parameters.AddWithValue("@pdf_id", pdf.Id);
                    cmd.Parameters.AddWithValue("@listing_id", listing.Id);
                    cmd.Parameters.AddWithValue("@property_id", propertyId);
                    cmd.Parameters.AddWithValue("@filename", pdf.PdfFilename);
                    cmd.Parameters.AddWithValue("@thumbnail_filename", pdf.ThumbnailFilename);
                    cmd.Parameters.AddWithValue("@type", pdf.PdfType.ToString());
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

                    if (cmd.ExecuteNonQuery() != 1) throw new InvalidOperationException("Commercial listing PDF ID belongs to another listing.");
                }

            }
        }

        // 募集物件図面の削除リストを処理
        if (listing.PdfsToBeDeleted.Count > 0)
        {
            foreach (var delr in listing.PdfsToBeDeleted)
            {
                cmd.Parameters.Clear();
                cmd.CommandText = "DELETE FROM rent_commercial_listing_pdfs WHERE pdf_id = @id AND listing_id = @listingId AND property_id = @propertyId";
                cmd.Parameters.AddWithValue("@id", delr.Id);
                cmd.Parameters.AddWithValue("@listingId", listing.Id);
                cmd.Parameters.AddWithValue("@propertyId", propertyId);
                cmd.ExecuteNonQuery();
            }
            // let's not do this.
            //listing.UnitPicturesToBeDeleted.Clear();
        }

        // 募集物件貸主 rent_lessors_commercial_listings - Insert or Update
        if (listing.Lessors.Count > 0)
        {
            foreach (var psn in listing.Lessors)
            {
                var sqlUpsert = "INSERT INTO rent_lessors_commercial_listings (lessor_id, property_id, property_context_type, listing_id) VALUES (@lessor_id, @property_id, @property_context_type, @listing_id) ";
                sqlUpsert += "ON CONFLICT(lessor_id, property_id, listing_id) DO NOTHING";
                cmd.CommandText = sqlUpsert;
                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("@lessor_id", psn.Id);
                cmd.Parameters.AddWithValue("@property_id", propertyId);
                cmd.Parameters.AddWithValue("@property_context_type", PropertyContextType.RentCommercial.ToString());
                cmd.Parameters.AddWithValue("@listing_id", listing.Id);
                cmd.ExecuteNonQuery();
            }
        }

        // 募集物件貸主の削除リストを処理
        if (listing.LessorsToBeDeleted.Count > 0)
        {
            foreach (var psn in listing.LessorsToBeDeleted)
            {
                cmd.Parameters.Clear();
                cmd.CommandText = "DELETE FROM rent_lessors_commercial_listings WHERE lessor_id = @id AND property_id = @propertyId AND listing_id = @listingId";
                cmd.Parameters.AddWithValue("@id", psn.Id);
                cmd.Parameters.AddWithValue("@propertyId", propertyId);
                cmd.Parameters.AddWithValue("@listingId", listing.Id);
                cmd.ExecuteNonQuery();
            }
        }

        // 募集物件宅建業者 brokers_commercial_listings - Insert or Update
        if (listing.Brokers.Count > 0)
        {
            foreach (var broker in listing.Brokers)
            {
                cmd.Parameters.Clear();
                cmd.CommandText = """
                    INSERT INTO brokers_commercial_listings
                        (broker_id, property_id, property_context_type, listing_id)
                    VALUES
                        (@brokerId, @propertyId, @propertyContextType, @listingId)
                    ON CONFLICT (broker_id, property_id, listing_id) DO NOTHING;
                    """;
                cmd.Parameters.AddWithValue("@brokerId", broker.Id);
                cmd.Parameters.AddWithValue("@propertyId", propertyId);
                cmd.Parameters.AddWithValue("@propertyContextType", PropertyContextType.RentCommercial.ToString());
                cmd.Parameters.AddWithValue("@listingId", listing.Id);
                cmd.ExecuteNonQuery();
            }
        }

        // 募集物件宅建業者の削除リストを処理
        if (listing.BrokersToBeDeleted.Count > 0)
        {
            foreach (var broker in listing.BrokersToBeDeleted)
            {
                cmd.Parameters.Clear();
                cmd.CommandText = """
                    DELETE FROM brokers_commercial_listings
                    WHERE broker_id = @brokerId AND property_id = @propertyId AND listing_id = @listingId;
                    """;
                cmd.Parameters.AddWithValue("@brokerId", broker.Id);
                cmd.Parameters.AddWithValue("@propertyId", propertyId);
                cmd.Parameters.AddWithValue("@listingId", listing.Id);
                cmd.ExecuteNonQuery();
            }
        }

        return result;
    }

    private static void MarkRentCommercialListingSaved(ZumenSearch.Domain.Entities.Rent.Commercials.Listing.Listing listing)
    {
        listing.SetIsModified(false);
        listing.SetStatus(EntityStatus.Saved);
        listing.SetPropertyStatus(EntityStatus.Saved);
        foreach (var picture in listing.Pictures)
        {
            picture.SetStatus(EntityStatus.Saved);
            picture.SetIsModified(false);
        }
        foreach (var pdf in listing.Pdfs)
        {
            pdf.SetStatus(EntityStatus.Saved);
            pdf.SetIsModified(false);
        }
    }

    public ZumenSearch.Application.Results.ResultWrapper UpsertRentCommercialListing(string propertyId, ZumenSearch.Domain.Entities.Rent.Commercials.Listing.Listing listing)
    {
        var res = new ResultWrapper();

        if (string.IsNullOrWhiteSpace(propertyId) || string.IsNullOrWhiteSpace(listing.Id) || !string.Equals(listing.PropertyId, propertyId, StringComparison.Ordinal))
        {
            res.IsError = true;
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
                cmd.Parameters.Clear();
                res.AffectedCount = DoUpsertRentCommercialListing(cmd, propertyId, listing);

                // Commit
                cmd.Transaction.Commit();
                MarkRentCommercialListingSaved(listing);

                // Clear deletion queues only after the commit succeeds.
                listing.LessorsToBeDeleted.Clear();
                listing.BrokersToBeDeleted.Clear();
            }
            catch (Exception ex)
            {
                cmd.Transaction.Rollback();

                SetDatabaseError(res, ex, "cmd.ExecuteNonQuery(), cmd.Transaction.Commit", "Failed to update database tables. Transaction.Rollback()", nameof(UpsertRentCommercialListing));

                return res;
            }
        }
        catch (Exception ex)
        {
            SetDatabaseError(res, ex, "connection.Open", "Failed to Connect to a SQLite database file", nameof(UpsertRentCommercialListing));
            return res;
        }
        finally
        {
            _readerWriterLock.ExitWriteLock();
        }

        //Debug.WriteLine(string.Format("{0} Entries Inserted to DB", res.AffectedCount.ToString()));

        return res;
    }

    public ZumenSearch.Application.Results.ListingSearchResultWrapper SelectRentCommercialListings(RentCommercialListingSearchCriteria criteria)
    {
        var res = new ListingSearchResultWrapper();

        _readerWriterLock.EnterReadLock();
        try
        {
            using var connection = new SqliteConnection(_connectionStringBuilder.ConnectionString);
            connection.Open();

            using var cmd = connection.CreateCommand();
            var conditions = new List<string> { "p.property_context_type = @propertyContextType" };
            cmd.Parameters.AddWithValue("@propertyContextType", PropertyContextType.RentCommercial.ToString());

            // Text search
            AddTextFilter(criteria.PropertyName, "p.name LIKE @propertyName ESCAPE '\\'", "propertyName");
            AddTextFilter(criteria.Address, "p.location_full LIKE @address ESCAPE '\\'", "address");
            AddTextFilter(
                criteria.LessorName,
                "((r.is_unit_ownership = 0 AND EXISTS (SELECT 1 FROM rent_lessors_properties AS relation INNER JOIN rent_lessors AS lessor ON lessor.lessor_id = relation.lessor_id WHERE relation.property_id = p.property_id AND relation.property_context_type = @propertyContextType AND lessor.name LIKE @lessorName ESCAPE '\\')) " +
                "OR (r.is_unit_ownership = 1 AND EXISTS (SELECT 1 FROM rent_lessors_commercial_listings AS relation INNER JOIN rent_lessors AS lessor ON lessor.lessor_id = relation.lessor_id WHERE relation.property_id = p.property_id AND relation.listing_id = l.listing_id AND relation.property_context_type = @propertyContextType AND lessor.name LIKE @lessorName ESCAPE '\\')))",
                "lessorName");
            AddTextFilter(criteria.FudousanId, "p.fudousan_id LIKE @fudousanId ESCAPE '\\'", "fudousanId");

            if (!string.IsNullOrWhiteSpace(criteria.Keyword) && criteria.Keyword.Trim() != "*")
            {
                AddTextFilter(criteria.Keyword, "(p.name LIKE @keyword ESCAPE '\\' OR l.name LIKE @keyword ESCAPE '\\' OR p.location_full LIKE @keyword ESCAPE '\\' OR p.fudousan_id LIKE @keyword ESCAPE '\\' OR " +
                    "p.train1_station_name LIKE @keyword ESCAPE '\\' OR p.train2_station_name LIKE @keyword ESCAPE '\\' OR p.train3_station_name LIKE @keyword ESCAPE '\\' OR p.train4_station_name LIKE @keyword ESCAPE '\\' OR " +
                    "EXISTS (SELECT 1 FROM rent_lessors AS lessor WHERE lessor.name LIKE @keyword ESCAPE '\\' AND ((r.is_unit_ownership = 0 AND EXISTS (SELECT 1 FROM rent_lessors_properties AS relation WHERE relation.property_id = p.property_id AND relation.property_context_type = @propertyContextType AND relation.lessor_id = lessor.lessor_id)) OR (r.is_unit_ownership = 1 AND EXISTS (SELECT 1 FROM rent_lessors_commercial_listings AS relation WHERE relation.property_id = p.property_id AND relation.listing_id = l.listing_id AND relation.property_context_type = @propertyContextType AND relation.lessor_id = lessor.lessor_id)))))", "keyword");
            }

            // Rent and other numeric limits
            AddNumberFilter(criteria.MinimumRent, "l.chinryou", "minimumRent", ">=");
            AddNumberFilter(criteria.MaximumRent, "l.chinryou", "maximumRent", "<=");
            AddNumberFilter(criteria.MaximumManagementFee, "CAST(l.kyoueki_fee AS NUMERIC)", "maximumManagementFee", "<=");

            if (criteria.NoSecurityDeposit)
            {
                conditions.Add("CAST(l.shikikin AS NUMERIC) = 0");
            }

            if (criteria.NoGuaranteeDeposit)
            {
                conditions.Add("CAST(l.guarantee_deposit AS NUMERIC) = 0");
            }

            if (criteria.NoKeyMoney)
            {
                conditions.Add("CAST(l.reikin AS NUMERIC) = 0");
            }

            // Station, line, and walking distance must match the same route.
            var hasStationFilter = !string.IsNullOrWhiteSpace(criteria.Station);
            var hasLineFilter = !string.IsNullOrWhiteSpace(criteria.Line);
            var hasWalkFilter = double.IsFinite(criteria.MaximumStationWalkMinutes);

            if (hasStationFilter || hasLineFilter || hasWalkFilter)
            {
                if (hasStationFilter)
                {
                    cmd.Parameters.AddWithValue("@station", "%" + EscapeLikeValue(criteria.Station) + "%");
                }

                if (hasLineFilter)
                {
                    cmd.Parameters.AddWithValue("@line", "%" + EscapeLikeValue(criteria.Line) + "%");
                }

                if (hasWalkFilter)
                {
                    cmd.Parameters.AddWithValue("@maximumWalk", criteria.MaximumStationWalkMinutes);
                }

                var matchingRoutes = new List<string>();

                for (var routeNumber = 1; routeNumber <= 4; routeNumber++)
                {
                    var routeConditions = new List<string>();

                    if (hasStationFilter)
                    {
                        routeConditions.Add($"p.train{routeNumber}_station_name LIKE @station ESCAPE '\\'");
                    }

                    if (hasLineFilter)
                    {
                        routeConditions.Add($"p.train{routeNumber}_line_name LIKE @line ESCAPE '\\'");
                    }

                    if (hasWalkFilter)
                    {
                        routeConditions.Add($"p.train{routeNumber}_ekitoho > 0 AND p.train{routeNumber}_ekitoho <= @maximumWalk");
                    }

                    matchingRoutes.Add("(" + string.Join(" AND ", routeConditions) + ")");
                }

                conditions.Add("(" + string.Join(" OR ", matchingRoutes) + ")");
            }

            // Property and listing types, building age, and floor area
            AddEnumFilter("r.commercial_kind", criteria.PropertyKinds.Select(value => value.ToString()), "propertyKind");
            AddEnumFilter("r.property_structure", criteria.PropertyStructures.Select(value => value.ToString()), "propertyStructure");
            AddNumberFilter(DateTime.Now.Year - criteria.MaximumBuildingAgeYears, "CAST(substr(r.built_year_month, 1, 4) AS REAL)", "minimumBuiltYear", ">=");
            AddNumberFilter(criteria.MinimumFloorArea, "CAST(l.floor_area AS NUMERIC)", "minimumFloorArea", ">=");
            AddNumberFilter(criteria.MaximumFloorArea, "CAST(l.floor_area AS NUMERIC)", "maximumFloorArea", "<=");

            if (criteria.Use is { } use)
            {
                conditions.Add("l.usage = @use");
                cmd.Parameters.AddWithValue("@use", use.ToString());
            }

            if (criteria.CurrentStatus is { } currentStatus)
            {
                conditions.Add("l.availability_status = @currentStatus");
                cmd.Parameters.AddWithValue("@currentStatus", currentStatus.ToString());
            }

            if (criteria.HandlingType is { } handlingType)
            {
                conditions.Add("l.handling_type = @handlingType");
                cmd.Parameters.AddWithValue("@handlingType", handlingType.ToString());
            }

            if (criteria.TransactionType is { } transactionType)
            {
                conditions.Add("l.transaction_type = @transactionType");
                cmd.Parameters.AddWithValue("@transactionType", transactionType.ToString());
            }

            // Commission limits and units
            AddCommissionFilter(criteria.MaximumLessorCommission, criteria.LessorCommissionUnit, "l.lessor_commission", "l.lessor_commission_unit", "lessorCommission");
            AddCommissionFilter(criteria.MaximumLesseeCommission, criteria.LesseeCommissionUnit, "l.lessee_commission", "l.lessee_commission_unit", "lesseeCommission");
            AddCommissionFilter(criteria.MaximumListingBrokerCommission, criteria.ListingBrokerCommissionUnit, "l.listing_broker_commission", "l.listing_broker_commission_unit", "listingBrokerCommission");
            AddCommissionFilter(criteria.MaximumTenantBrokerCommission, criteria.TenantBrokerCommissionUnit, "l.tenant_broker_commission", "l.tenant_broker_commission_unit", "tenantBrokerCommission");

            // Listing and property features
            AddFlag(criteria.IsAvailableForRent, "l.is_available_for_rent");
            AddFlag(criteria.ImmediateOccupancy, "l.is_available_immediately");
            AddFlag(criteria.HasAutolock, "r.security_autolock");
            AddFlag(criteria.HasElevator, "r.passenger_elevator_availability");
            AddFlag(criteria.HasAirConditioner, "l.air_conditioning");
            AddFlag(criteria.HasParking, "r.parking_availability");
            AddFlag(criteria.PermitsHeavyFoodService, "l.permits_heavy_food");
            AddFlag(criteria.PermitsLightFoodService, "l.permits_light_food");
            AddFlag(criteria.PermitsRetail, "l.permits_retail");
            AddFlag(criteria.PermitsServices, "l.permits_services");
            AddFlag(criteria.PermitsOffice, "l.permits_office");
            AddFlag(criteria.PermitsOther, "l.permits_other");

            // Build and execute the search query
            var whereClause = conditions.Count == 0
                ? string.Empty
                : " WHERE " + string.Join(" AND ", conditions);

            cmd.CommandText =
                "SELECT p.name AS propertyName, l.name AS listingName, " +
                "l.listing_id AS listingId, l.property_id AS propertyId, p.fudousan_id " +
                "FROM rent_commercial_units AS l " +
                "INNER JOIN properties AS p USING (property_id) " +
                "INNER JOIN rent_commercials AS r USING (property_id)" +
                whereClause +
                " ORDER BY p.name, l.name";

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                var propertyId = Convert.ToString(reader["propertyId"], CultureInfo.InvariantCulture);
                if (string.IsNullOrEmpty(propertyId))
                {
                    Debug.WriteLine("DataAccess::SelectRentCommercialListings: propertyId is null or empty for a rent commercial.");
                    continue;
                }

                var listingId = Convert.ToString(reader["listingId"], CultureInfo.InvariantCulture);
                if (string.IsNullOrEmpty(listingId))
                {
                    Debug.WriteLine("DataAccess::SelectRentCommercialListings: listingId is null or empty for a rent commercial listing.");
                    continue;
                }

                var resultItem = new ZumenSearch.Application.SearchResults.ListingSearchResultItem(
                    listingId,
                    propertyId,
                    PropertyContextType.RentCommercial,
                    Convert.ToString(reader["fudousan_id"], CultureInfo.InvariantCulture) ?? string.Empty);

                resultItem.SetName(Convert.ToString(reader["listingName"], CultureInfo.InvariantCulture) ?? string.Empty);

                var propertyName = Convert.ToString(reader["propertyName"], CultureInfo.InvariantCulture);
                if (!string.IsNullOrEmpty(propertyName))
                {
                    resultItem.PropertyName = propertyName;
                }

                resultItem.SetIsModified(false);
                res.ListingSearchResult.Add(resultItem);
            }

            // Local filter builders keep parameter handling consistent without splitting the method.
            void AddTextFilter(string value, string condition, string parameterName)
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    return;
                }

                conditions.Add(condition);
                cmd.Parameters.AddWithValue("@" + parameterName, "%" + EscapeLikeValue(value) + "%");
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

                var parameterNames = selectedValues
                    .Select((_, index) => "@" + parameterName + index.ToString(CultureInfo.InvariantCulture))
                    .ToArray();

                conditions.Add(column + " IN (" + string.Join(", ", parameterNames) + ")");

                for (var index = 0; index < selectedValues.Length; index++)
                {
                    cmd.Parameters.AddWithValue(parameterNames[index], selectedValues[index]);
                }
            }

            void AddFlag(bool selected, string column)
            {
                if (selected)
                {
                    conditions.Add(column + " = 1");
                }
            }

            void AddCommissionFilter(double maximum, ZumenSearch.Domain.Entities.Rent.Commercials.Listing.ListingCommissionUnitType? unit, string amountColumn, string unitColumn, string parameterName)
            {
                if (unit is { } commissionUnit)
                {
                    conditions.Add(unitColumn + " = @" + parameterName + "Unit");
                    cmd.Parameters.AddWithValue("@" + parameterName + "Unit", commissionUnit.ToString());
                }

                if (double.IsFinite(maximum))
                {
                    conditions.Add("CAST(REPLACE(" + amountColumn + ", ',', '') AS REAL) <= @" + parameterName);
                    cmd.Parameters.AddWithValue("@" + parameterName, maximum);
                }
            }
        }
        catch (Exception ex)
        {
            SetDatabaseError(res, ex, "connection.Open(), reader.Read()", "Failed to connect to or read a SQLite database file", nameof(SelectRentCommercialListings));
        }
        finally
        {
            _readerWriterLock.ExitReadLock();
        }

        return res;
    }

    public RentCommercialListingResultWrapper SelectRentCommercialListingById(string propertyId, string listingId, bool duplicate = false)
    {
        var res = new RentCommercialListingResultWrapper();

        if (string.IsNullOrEmpty(listingId))
        {
            res.IsError = true;
            Debug.WriteLine("DataAccess::SelectRentCommercialListingById: listingId is null or empty.");
            // TODO:
            return res;
        }

        if (string.IsNullOrEmpty(propertyId))
        {
            res.IsError = true;
            Debug.WriteLine("DataAccess::SelectRentCommercialListingById: propertyId is null or empty.");
            // TODO:
            return res;
        }

        _readerWriterLock.EnterReadLock();
        try
        {
            using var connection = new SqliteConnection(_connectionStringBuilder.ConnectionString);
            connection.Open();

            using var cmd = connection.CreateCommand();

            var propertyName = string.Empty;
            var isFound = false;
            var isUnitOwnership = false;
            var fudousanId = string.Empty;

            cmd.CommandText = "SELECT p.name, p.fudousan_id, r.is_unit_ownership FROM properties AS p INNER JOIN rent_commercials AS r ON r.property_id = p.property_id WHERE p.property_id = @propertyId";
            cmd.Parameters.AddWithValue("@propertyId", propertyId);
            using (var reader = cmd.ExecuteReader())
            {
                if (reader.Read())
                {
                    propertyName = reader.GetString(reader.GetOrdinal("name"));
                    isUnitOwnership = Convert.ToInt32(reader["is_unit_ownership"], CultureInfo.InvariantCulture) != 0;
                    fudousanId = reader.GetString(reader.GetOrdinal("fudousan_id"));
                    isFound = true;
                }
            }

            if (isFound)
            {
                cmd.CommandText = "SELECT * FROM rent_commercial_units WHERE listing_id = @listingId AND property_id = @propertyId";
                cmd.Parameters.AddWithValue("@listingId", listingId);
                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        res.PropertyName = propertyName;
                        res.Listing = GetRentCommercialListing(reader, propertyId, propertyName, isUnitOwnership, fudousanId, duplicate);
                    }
                }

                if (res.Listing is not null)
                {
                    SetRentCommercialListingChildValues(cmd, res.Listing, listingId, duplicate);
                }
            }
        }
        catch (Exception ex)
        {
            SetDatabaseError(res, ex, "connection.Open(), reader.Read()", "Failed to connect to or read a SQLite database file", nameof(SelectRentCommercialListingById));
        }
        finally
        {
            _readerWriterLock.ExitReadLock();
        }

        return res;
    }

    private static ZumenSearch.Domain.Entities.Rent.Commercials.Listing.Listing? GetRentCommercialListing(SqliteDataReader reader, string propertyId, string propertyName, bool isUnitOwnership, string fudousanId, bool duplicate)
    {
        var listingId = reader.GetString(reader.GetOrdinal("listing_id")) ?? string.Empty;
        if (string.IsNullOrEmpty(listingId))
        {
            Debug.WriteLine("DataAccess::GetRentCommercialListing: listing_id is null or empty.");
            return null;
        }

        var pId = reader.GetString(reader.GetOrdinal("property_id")) ?? string.Empty;
        if (!propertyId.Equals(pId, StringComparison.Ordinal))
        {
            Debug.WriteLine("DataAccess::GetRentCommercialListing: property_id is not Equals to given param.");
            return null;
        }

        // If duplicate is true, generate a new listing ID; otherwise, use the existing one.
        string id = duplicate ? Guid.CreateVersion7().ToString("N") : listingId;
        // Set the status based on whether it's a duplicate or not.
        EntityStatus status = duplicate ? EntityStatus.New : EntityStatus.Saved;

        // Create listing.
        var listing = new ZumenSearch.Domain.Entities.Rent.Commercials.Listing.Listing(id, status, propertyId, EntityStatus.Saved, isUnitOwnership, propertyName, fudousanId);

        // try?
        listing.SetName(reader.GetString(reader.GetOrdinal("name")) ?? string.Empty);
        listing.SetFloorGroundTypeLabelFromString(Convert.ToString(reader["floor_ground"], CultureInfo.InvariantCulture) ?? string.Empty);
        listing.SetFloorArea(reader["floor_area"] is DBNull ? null : Convert.ToDecimal(reader["floor_area"], CultureInfo.InvariantCulture));
        listing.SetFloorNumber(reader["floor_number"] is DBNull ? null : Convert.ToInt32(reader["floor_number"], CultureInfo.InvariantCulture));
        listing.SetUseTypeLabelFromString(Convert.ToString(reader["usage"], CultureInfo.InvariantCulture) ?? string.Empty);
        listing.SetUseDetails(Convert.ToString(reader["use_details"], CultureInfo.InvariantCulture) ?? string.Empty);
        listing.SetCurrentStatusTypeLabelFromString(Convert.ToString(reader["availability_status"], CultureInfo.InvariantCulture) ?? string.Empty);
        listing.SetIsAvailableForRentNow(Convert.ToInt32(reader["is_available_for_rent"], CultureInfo.InvariantCulture) != 0);
        listing.SetAvailabilityMonthTypeLabelFromString(Convert.ToString(reader["availability_month"], CultureInfo.InvariantCulture) ?? string.Empty);
        listing.SetAvailabilityPeriodTypeLabelFromString(Convert.ToString(reader["availability_period"], CultureInfo.InvariantCulture) ?? string.Empty);
        listing.SetIsAvailableForImmediateMoveIn(Convert.ToInt32(reader["is_available_immediately"], CultureInfo.InvariantCulture) != 0);
        var statusCheckedAt = reader.GetString(reader.GetOrdinal("current_status_checked_at"));
        listing.SetOccupancyStatusCheckedAt(string.IsNullOrEmpty(statusCheckedAt) ? null : DateTimeOffset.Parse(statusCheckedAt, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind));

        listing.SetFudousanIdAdditionalCode(Convert.ToString(reader["fudousan_id_additional_code"], CultureInfo.InvariantCulture) ?? string.Empty);
        listing.SetRemarks(Convert.ToString(reader["remarks"], CultureInfo.InvariantCulture) ?? string.Empty);
        listing.SetChinryou(reader["chinryou"] is DBNull ? null : Convert.ToDecimal(reader["chinryou"], CultureInfo.InvariantCulture));

        listing.SetRentTaxTypeLabelFromString(reader.GetString(reader.GetOrdinal("rent_tax")));
        listing.SetManagementFee(reader["kyoueki_fee"] is DBNull ? null : Convert.ToDecimal(reader["kyoueki_fee"], CultureInfo.InvariantCulture));
        listing.SetManagementFeeTaxTypeLabelFromString(reader.GetString(reader.GetOrdinal("management_fee_tax")));
        listing.SetSecurityDeposit(reader["shikikin"] is DBNull ? null : Convert.ToDecimal(reader["shikikin"], CultureInfo.InvariantCulture));
        listing.SetSecurityDepositUnitTypeLabelFromString(reader.GetString(reader.GetOrdinal("shikikin_unit")));
        listing.SetGuaranteeDeposit(reader["guarantee_deposit"] is DBNull ? null : Convert.ToDecimal(reader["guarantee_deposit"], CultureInfo.InvariantCulture));
        listing.SetGuaranteeDepositUnitTypeLabelFromString(reader.GetString(reader.GetOrdinal("guarantee_deposit_unit")));
        listing.SetKeyMoney(reader["reikin"] is DBNull ? null : Convert.ToDecimal(reader["reikin"], CultureInfo.InvariantCulture));
        listing.SetKeyMoneyUnitTypeLabelFromString(reader.GetString(reader.GetOrdinal("reikin_unit")));
        listing.SetRightsMoney(reader["rights_money"] is DBNull ? null : Convert.ToDecimal(reader["rights_money"], CultureInfo.InvariantCulture));
        listing.SetRightsMoneyUnitTypeLabelFromString(reader.GetString(reader.GetOrdinal("rights_money_unit")));
        listing.SetDepositDeduction(reader["deposit_deduction"] is DBNull ? null : Convert.ToDecimal(reader["deposit_deduction"], CultureInfo.InvariantCulture));
        listing.SetDepositDeductionUnitTypeLabelFromString(reader.GetString(reader.GetOrdinal("deposit_deduction_unit")));
        listing.SetContractTypeLabelFromString(reader.GetString(reader.GetOrdinal("contract_type")));
        listing.SetContractPeriodYears(reader["contract_period_years"] is DBNull ? null : Convert.ToDecimal(reader["contract_period_years"], CultureInfo.InvariantCulture));
        listing.SetRenewalFee(reader["renewal_fee"] is DBNull ? null : Convert.ToDecimal(reader["renewal_fee"], CultureInfo.InvariantCulture));
        listing.SetRenewalFeeUnitTypeLabelFromString(reader.GetString(reader.GetOrdinal("renewal_fee_unit")));
        listing.SetRecontractFee(reader["recontract_fee"] is DBNull ? null : Convert.ToDecimal(reader["recontract_fee"], CultureInfo.InvariantCulture));
        listing.SetRecontractFeeUnitTypeLabelFromString(reader.GetString(reader.GetOrdinal("recontract_fee_unit")));
        listing.SetGuarantorCompany(reader.GetString(reader.GetOrdinal("guarantor_company")));
        listing.SetInsurance(reader.GetString(reader.GetOrdinal("insurance")));
        listing.SetOtherFees(reader.GetString(reader.GetOrdinal("other_fees")));
        listing.SetContractConditions(reader.GetString(reader.GetOrdinal("contract_conditions")));
        listing.SetContractRemarks(reader.GetString(reader.GetOrdinal("contract_remarks")));

        listing.SetHandlingTypeLabelFromString(reader.GetString(reader.GetOrdinal("handling_type")));
        listing.SetTransactionTypeLabelFromString(reader.GetString(reader.GetOrdinal("transaction_type")));
        listing.SetLessorCommission(reader.GetString(reader.GetOrdinal("lessor_commission")));
        listing.SetLessorCommissionUnitTypeLabelFromString(reader.GetString(reader.GetOrdinal("lessor_commission_unit")));
        listing.SetLesseeCommission(reader.GetString(reader.GetOrdinal("lessee_commission")));
        listing.SetLesseeCommissionUnitTypeLabelFromString(reader.GetString(reader.GetOrdinal("lessee_commission_unit")));
        listing.SetListingBrokerCommission(reader.GetString(reader.GetOrdinal("listing_broker_commission")));
        listing.SetListingBrokerCommissionUnitTypeLabelFromString(reader.GetString(reader.GetOrdinal("listing_broker_commission_unit")));
        listing.SetTenantBrokerCommission(reader.GetString(reader.GetOrdinal("tenant_broker_commission")));
        listing.SetTenantBrokerCommissionUnitTypeLabelFromString(reader.GetString(reader.GetOrdinal("tenant_broker_commission_unit")));
        listing.SetTransactionRemarks(reader.GetString(reader.GetOrdinal("transaction_remarks")));

        listing.SetHasAirConditioning(Convert.ToInt32(reader["air_conditioning"], CultureInfo.InvariantCulture) != 0);
        listing.SetHasPrivateToilet(Convert.ToInt32(reader["private_toilet"], CultureInfo.InvariantCulture) != 0);
        listing.SetHasHotWater(Convert.ToInt32(reader["hot_water"], CultureInfo.InvariantCulture) != 0);
        listing.SetHasKitchen(Convert.ToInt32(reader["kitchen"], CultureInfo.InvariantCulture) != 0);
        listing.SetHasVentilation(Convert.ToInt32(reader["ventilation"], CultureInfo.InvariantCulture) != 0);
        listing.SetHasInternet(Convert.ToInt32(reader["unit_internet"], CultureInfo.InvariantCulture) != 0);
        listing.SetEquipmentDetails(reader.GetString(reader.GetOrdinal("equipment_details")));
        listing.SetUnitConditionTypeLabelFromString(reader.GetString(reader.GetOrdinal("unit_condition")));
        listing.SetFixtureTransferTypeLabelFromString(reader.GetString(reader.GetOrdinal("fixture_transfer")));
        listing.SetBusinessHoursRestrictions(reader.GetString(reader.GetOrdinal("business_hours")));
        listing.SetUseConditions(reader.GetString(reader.GetOrdinal("other_conditions")));
        listing.SetPermitsHeavyFoodService(Convert.ToInt32(reader["permits_heavy_food"], CultureInfo.InvariantCulture) != 0);
        listing.SetPermitsLightFoodService(Convert.ToInt32(reader["permits_light_food"], CultureInfo.InvariantCulture) != 0);
        listing.SetPermitsRetail(Convert.ToInt32(reader["permits_retail"], CultureInfo.InvariantCulture) != 0);
        listing.SetPermitsServices(Convert.ToInt32(reader["permits_services"], CultureInfo.InvariantCulture) != 0);
        listing.SetPermitsOffice(Convert.ToInt32(reader["permits_office"], CultureInfo.InvariantCulture) != 0);
        listing.SetPermitsOther(Convert.ToInt32(reader["permits_other"], CultureInfo.InvariantCulture) != 0);
        listing.SetPermittedBusinessDetails(reader.GetString(reader.GetOrdinal("permitted_business_details")));
        listing.SetFacilitiesRemarks(reader.GetString(reader.GetOrdinal("facilities_remarks")));

        // If duplicate is true, mark the listing as modified to indicate that it has unsaved changes.
        listing.SetIsModified(duplicate);

        return listing;
    }

    private static void SetRentCommercialListingChildValues(SqliteCommand cmd, ZumenSearch.Domain.Entities.Rent.Commercials.Listing.Listing listing, string listingId, bool duplicate)
    {
        // Use listingId, not listing.Id, because listing.Id may be a new ID if duplicate is true.

        // 募集物件写真
        cmd.CommandText = "SELECT * FROM rent_commercial_listing_pictures WHERE listing_id = @listingId";
        cmd.Parameters.Clear();
        cmd.Parameters.AddWithValue("@listingId", listingId);
        using (var reader = cmd.ExecuteReader())
        {
            while (reader.Read())
            {
                var picid = Convert.ToString(reader["picture_id"], CultureInfo.InvariantCulture) ?? string.Empty;

                // If duplicate is true, generate a new picture ID; otherwise, use the existing one.
                picid = duplicate ? Guid.CreateVersion7().ToString("N") : picid;

                var picpath = Convert.ToString(reader["filename"], CultureInfo.InvariantCulture) ?? string.Empty;
                if (!string.IsNullOrEmpty(picid) && !string.IsNullOrEmpty(picpath))
                {
                    var rlpic = new ZumenSearch.Domain.Entities.Rent.Commercials.Listing.ListingPicture(picid, picpath, EntityStatus.Saved)
                    {
                        Description = Convert.ToString(reader["description"], CultureInfo.InvariantCulture) ?? string.Empty
                    };

                    var strType = Convert.ToString(reader["type"], CultureInfo.InvariantCulture) ?? string.Empty;
                    if (!Enum.TryParse<ZumenSearch.Domain.Enums.Rent.Commercials.ListingPictureType>(strType, out var pictureType) || !Enum.IsDefined(pictureType))
                    {
                        Debug.WriteLine($"Invalid commercial listing picture type: {strType}. Defaulting to Unspecified.");
                        pictureType = ZumenSearch.Domain.Enums.Rent.Commercials.ListingPictureType.Unspecified;
                    }
                    rlpic.PictureType = pictureType;
                    rlpic.IsMain = Convert.ToInt32(reader["is_main"], CultureInfo.InvariantCulture) != 0; // int to bool
                    rlpic.SetIsModified(false);

                    listing.Pictures.Add(rlpic);
                }
                else
                {
                    Debug.WriteLine("Commercial listing picture has an empty ID or filename.");
                }
            }
        }

        // 募集物件PDF
        cmd.CommandText = "SELECT * FROM rent_commercial_listing_pdfs WHERE listing_id = @listingId";
        cmd.Parameters.Clear();
        cmd.Parameters.AddWithValue("@listingId", listingId);
        using (var reader = cmd.ExecuteReader())
        {
            while (reader.Read())
            {
                var pdfid = Convert.ToString(reader["pdf_id"], CultureInfo.InvariantCulture) ?? string.Empty;

                // If duplicate is true, generate a new picture ID; otherwise, use the existing one.
                pdfid = duplicate ? Guid.CreateVersion7().ToString("N") : pdfid;

                var pdfpath = Convert.ToString(reader["filename"], CultureInfo.InvariantCulture) ?? string.Empty;
                var thumbpath = Convert.ToString(reader["thumbnail_filename"], CultureInfo.InvariantCulture) ?? string.Empty;
                if (!string.IsNullOrWhiteSpace(pdfid) && !string.IsNullOrWhiteSpace(pdfpath) && !string.IsNullOrWhiteSpace(thumbpath))
                {
                    var rlpdf = new ZumenSearch.Domain.Entities.Rent.Commercials.Listing.ListingPdf(pdfid, pdfpath, thumbpath, EntityStatus.Saved)
                    {
                        Description = Convert.ToString(reader["description"], CultureInfo.InvariantCulture) ?? string.Empty
                    };

                    var strType = Convert.ToString(reader["type"], CultureInfo.InvariantCulture) ?? string.Empty;
                    if (!Enum.TryParse<ZumenSearch.Domain.Enums.Rent.Commercials.ListingPdfType>(strType, out var pdfType) || !Enum.IsDefined(pdfType))
                    {
                        Debug.WriteLine($"Invalid commercial listing PDF type: {strType}. Defaulting to Unspecified.");
                        pdfType = ZumenSearch.Domain.Enums.Rent.Commercials.ListingPdfType.Unspecified;
                    }
                    rlpdf.PdfType = pdfType;
                    rlpdf.IsMain = Convert.ToInt32(reader["is_main"], CultureInfo.InvariantCulture) != 0; // int to bool
                    rlpdf.SetIsModified(false);

                    listing.Pdfs.Add(rlpdf);
                }
                else
                {
                    //throw new InvalidDataException("Commercial listing PDF has an empty ID, filename or thumbnail filename.");
                    Debug.WriteLine("Commercial listing PDF has an empty ID, filename or thumbnail filename.");
                }
            }
        }

        // 募集物件貸主
        var lessorIds = new List<string>();
        cmd.CommandText = "SELECT * FROM rent_lessors_commercial_listings WHERE property_id = @propertyId AND listing_id = @listingId";
        cmd.Parameters.Clear();
        cmd.Parameters.AddWithValue("@propertyId", listing.PropertyId);
        cmd.Parameters.AddWithValue("@listingId", listingId);
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
                cmd.CommandText = "SELECT lessor_id, name, person_kind, name_last, name_first, name_company, name_company_type, name_company_type_position, phone_number, mobile_phone_number, contact_name, location_postal_code, location_full, location_machiaza_id, location_prefecture, location_county, location_city, location_ward, location_oaza, location_choume, location_edaban, remarks FROM rent_lessors WHERE lessor_id = @lessorId";
                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("@lessorId", lessId);
#pragma warning disable IDE0063 // Use simple 'using' statement
                using (var reader2 = cmd.ExecuteReader())
                {
                    while (reader2.Read())
                    {
                        var lessor = GetPerson(reader2, lessId);
                        if (lessor is not null) listing.Lessors.Add(lessor);
                    }
                }
#pragma warning restore IDE0063 // Use simple 'using' statement
            }
        }

        // 宅建業者（募集物件）
        cmd.CommandText = """
            SELECT b.broker_id, b.name, b.person_kind, b.name_last, b.name_first, b.name_company, b.name_company_type, b.name_company_type_position, b.phone_number, b.mobile_phone_number, b.contact_name, b.location_postal_code, b.location_full, b.location_machiaza_id, b.location_prefecture, b.location_county, b.location_city, b.location_ward, b.location_oaza, b.location_choume, b.location_edaban, b.remarks
            FROM brokers_commercial_listings AS association
            INNER JOIN brokers AS b ON b.broker_id = association.broker_id
            WHERE association.property_id = @propertyId AND association.listing_id = @listingId;
            """;
        cmd.Parameters.Clear();
        cmd.Parameters.AddWithValue("@propertyId", listing.PropertyId);
        cmd.Parameters.AddWithValue("@listingId", listingId);

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
                    listing.Brokers.Add(broker);
                }
            }
        }

        cmd.Parameters.Clear();
    }

    public ZumenSearch.Application.Results.ResultWrapper DeleteRentCommercialListing(string listingId)
    {
        var res = new ResultWrapper();

        if (string.IsNullOrEmpty(listingId))
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
                cmd.CommandText = "DELETE FROM rent_commercial_units WHERE listing_id = @listingId;";
                cmd.Parameters.AddWithValue("@listingId", listingId);
                res.AffectedCount = cmd.ExecuteNonQuery();

                cmd.Transaction.Commit();
            }
            catch (Exception ex)
            {
                cmd.Transaction.Rollback();

                SetDatabaseError(res, ex, "cmd.ExecuteNonQuery(), cmd.Transaction.Commit", "Failed to delete database record. Transaction.Rollback()", nameof(DeleteRentCommercialListing));
                return res;
            }
        }
        catch (Exception ex)
        {
            SetDatabaseError(res, ex, "connection.Open(), reader.Read()", "Failed to connect to a SQLite database file", nameof(DeleteRentCommercialListing));
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

    #region == Rent Lessor ==

    ZumenSearch.Application.Results.PersonsSearchResultWrapper IPersonRepository.SelectRentLessorsByKeyword(string keyword) => SelectRentLessorsByKeyword(keyword);

    ZumenSearch.Application.Results.PersonResultWrapper IPersonRepository.SelectRentLessorById(string lessorId) => SelectRentLessorById(lessorId);

    ResultWrapper IPersonRepository.DeleteRentLessor(string lessorId) => DeleteRentLessor(lessorId);

    public ResultWrapper UpsertRentLessor(ZumenSearch.Domain.Entities.Base.PersonBase lessor)
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
                var sqlInsertIntoRentLivingRoom = "INSERT INTO rent_lessors (lessor_id, name, person_kind, name_last, name_first, name_company, name_company_type, name_company_type_position, phone_number, mobile_phone_number, contact_name, location_postal_code, location_full, location_machiaza_id, location_prefecture, location_county, location_city, location_ward, location_oaza, location_choume, location_edaban, remarks) VALUES (@lessor_id, @name, @personKind, @name_last, @name_first, @name_company, @name_company_type, @name_company_type_position, @phone_number, @mobile_phone_number, @contact_name, @location_postal_code, @location_full, @location_machiaza_id, @location_prefecture, @location_county, @location_city, @location_ward, @location_oaza, @location_choume, @location_edaban, @remarks) ";
                sqlInsertIntoRentLivingRoom += "ON CONFLICT(lessor_id) ";
                sqlInsertIntoRentLivingRoom += "DO UPDATE SET name = @name, person_kind = @personKind, name_last = @name_last, name_first = @name_first, name_company = @name_company, name_company_type = @name_company_type, name_company_type_position = @name_company_type_position, phone_number = @phone_number, mobile_phone_number = @mobile_phone_number, contact_name = @contact_name, location_postal_code = @location_postal_code, location_full = @location_full, location_machiaza_id = @location_machiaza_id, location_prefecture = @location_prefecture, location_county = @location_county, location_city = @location_city, location_ward = @location_ward, location_oaza = @location_oaza, location_choume = @location_choume, location_edaban = @location_edaban, remarks = @remarks, updated_at = @updated_at";

                cmd.CommandText = sqlInsertIntoRentLivingRoom;

                cmd.Parameters.AddWithValue("@lessor_id", lessor.Id);
                cmd.Parameters.AddWithValue("@name", lessor.Name);
                cmd.Parameters.AddWithValue("@personKind", lessor.PersonKind.ToString());

                if (lessor is ZumenSearch.Domain.Entities.Person.PersonNatural naturalPerson)
                {
                    cmd.Parameters.AddWithValue("@name_last", naturalPerson.NameLast);
                    cmd.Parameters.AddWithValue("@name_first", naturalPerson.NameFirst);
                    // Clear legalPerson values
                    cmd.Parameters.AddWithValue("@name_company", string.Empty);
                    cmd.Parameters.AddWithValue("@name_company_type", string.Empty);
                    // TODO: check if int is ok
                    cmd.Parameters.AddWithValue("@name_company_type_position", 0);

                    cmd.Parameters.AddWithValue("@mobile_phone_number", naturalPerson.MobilePhoneNumber);
                    cmd.Parameters.AddWithValue("@contact_name", string.Empty);
                }
                else if (lessor is ZumenSearch.Domain.Entities.Person.PersonLegal legalPerson)
                {
                    // Clear naturalPerson values
                    cmd.Parameters.AddWithValue("@name_last", string.Empty);
                    cmd.Parameters.AddWithValue("@name_first", string.Empty);

                    cmd.Parameters.AddWithValue("@name_company", legalPerson.NameCompany);
                    cmd.Parameters.AddWithValue("@name_company_type", legalPerson.NameCompanyType);
                    // TODO: check if int is ok
                    cmd.Parameters.AddWithValue("@name_company_type_position", legalPerson.NameCompanyTypePosition);

                    cmd.Parameters.AddWithValue("@mobile_phone_number", string.Empty);
                    cmd.Parameters.AddWithValue("@contact_name", legalPerson.ContactName);
                }

                cmd.Parameters.AddWithValue("@phone_number", lessor.PhoneNumber);
                cmd.Parameters.AddWithValue("@location_postal_code", lessor.PostalCode);
                cmd.Parameters.AddWithValue("@location_full", lessor.Address);
                cmd.Parameters.AddWithValue("@location_machiaza_id", lessor.AddressLocation.MachiazaId);
                cmd.Parameters.AddWithValue("@location_prefecture", lessor.AddressLocation.Prefecture?.Name ?? string.Empty);
                cmd.Parameters.AddWithValue("@location_county", lessor.AddressLocation.CountyAndCity?.County ?? string.Empty);
                cmd.Parameters.AddWithValue("@location_city", lessor.AddressLocation.CountyAndCity?.City ?? string.Empty);
                cmd.Parameters.AddWithValue("@location_ward", lessor.AddressLocation.WardAndOaza?.Ward ?? string.Empty);
                cmd.Parameters.AddWithValue("@location_oaza", lessor.AddressLocation.WardAndOaza?.Oaza ?? string.Empty);
                cmd.Parameters.AddWithValue("@location_choume", lessor.AddressLocation.Choume?.Chou ?? string.Empty);
                cmd.Parameters.AddWithValue("@location_edaban", lessor.AddressLocation.Edaban);
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

                ZumenSearch.Domain.Enums.PersonKindType? enumKind = null;
                var kind = reader.GetString(reader.GetOrdinal("person_kind")) ?? string.Empty;
                if (!string.IsNullOrEmpty(kind))
                {
                    if (Enum.TryParse<ZumenSearch.Domain.Enums.PersonKindType>(kind, out var parsedKind))
                    {
                        enumKind = parsedKind;
                    }
                }

                if (enumKind is null)
                {
                    Debug.WriteLine("DataAccess::SelectRentLessorByKeyword: EnumPersonKind is null.");
                    continue;
                }

                ZumenSearch.Application.SearchResults.PersonSearchResultItem item;
                if (enumKind == ZumenSearch.Domain.Enums.PersonKindType.Natural)
                {
                    item = new ZumenSearch.Application.SearchResults.PersonSearchResultItem(s, ZumenSearch.Domain.Enums.PersonKindType.Natural);
                }
                else if (enumKind == ZumenSearch.Domain.Enums.PersonKindType.Legal)
                {
                    item = new ZumenSearch.Application.SearchResults.PersonSearchResultItem(s, ZumenSearch.Domain.Enums.PersonKindType.Legal);
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

    public ZumenSearch.Application.Results.PersonResultWrapper SelectRentLessorById(string lessorId)
    {
        var res = new ZumenSearch.Application.Results.PersonResultWrapper();

        ZumenSearch.Domain.Entities.Base.PersonBase? person = null; //new ZumenSearch.Domain.Entities.Base.PersonBase(lessorId, EnumEntityStatus.Saved);

        if (string.IsNullOrEmpty(lessorId))
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

            cmd.CommandText = "SELECT lessor_id, name, person_kind, name_last, name_first, name_company, name_company_type, name_company_type_position, phone_number, mobile_phone_number, contact_name, location_postal_code, location_full, location_machiaza_id, location_prefecture, location_county, location_city, location_ward, location_oaza, location_choume, location_edaban, remarks FROM rent_lessors WHERE lessor_id = @lessorId";
            cmd.Parameters.AddWithValue("@lessorId", lessorId);

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

                    person = GetPerson(reader, lessorId);

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

    private static string GetOptionalString(SqliteDataReader reader, string columnName)
    {
        try
        {
            var ordinal = reader.GetOrdinal(columnName);
            return ordinal >= 0 && !reader.IsDBNull(ordinal) ? Convert.ToString(reader.GetValue(ordinal), CultureInfo.InvariantCulture) ?? string.Empty : string.Empty;
        }
        catch (ArgumentOutOfRangeException)
        {
            return string.Empty;
        }
    }

    private static ZumenSearch.Domain.Entities.Base.PersonBase? GetPerson(SqliteDataReader reader, string personId)
    {
        // Do not try to access reader["person_id"] here because it may not be present in the SELECT query. Use the provided personId parameter instead.

        ZumenSearch.Domain.Entities.Base.PersonBase? person = null;

        ZumenSearch.Domain.Enums.PersonKindType? enumKind = null;
        var kind = reader.GetString(reader.GetOrdinal("person_kind")) ?? string.Empty;
        if (!string.IsNullOrEmpty(kind))
        {
            if (Enum.TryParse<ZumenSearch.Domain.Enums.PersonKindType>(kind, out var parsedKind))
            {
                enumKind = parsedKind;
            }
        }

        if (enumKind is null)
        {
            Debug.WriteLine("DataAccess::GetPerson: EnumPersonKind is null.");
            return null;
        }

        if (enumKind == ZumenSearch.Domain.Enums.PersonKindType.Natural)
        {
            person = new ZumenSearch.Domain.Entities.Person.PersonNatural(personId, EntityStatus.Saved);
        }
        else if (enumKind == ZumenSearch.Domain.Enums.PersonKindType.Legal)
        {
            person = new ZumenSearch.Domain.Entities.Person.PersonLegal(personId, EntityStatus.Saved);
        }

        if (person is null)
        {
            return null;
        }

        //s = Convert.ToString(reader["name"]) ?? "";
        person.SetName(reader.GetString(reader.GetOrdinal("name")) ?? string.Empty);

        if (person is ZumenSearch.Domain.Entities.Person.PersonNatural naturalPerson)
        {
            naturalPerson.NameLast = Convert.ToString(reader["name_last"], CultureInfo.InvariantCulture) ?? "";
            naturalPerson.NameFirst = Convert.ToString(reader["name_first"], CultureInfo.InvariantCulture) ?? "";

            naturalPerson.MobilePhoneNumber = reader.GetString(reader.GetOrdinal("mobile_phone_number"));
        }
        else if (person is ZumenSearch.Domain.Entities.Person.PersonLegal legalPerson)
        {
            legalPerson.NameCompany = Convert.ToString(reader["name_company"], CultureInfo.InvariantCulture) ?? "";
            legalPerson.NameCompanyType = Convert.ToString(reader["name_company_type"], CultureInfo.InvariantCulture) ?? "";
            legalPerson.NameCompanyTypePosition = Convert.ToInt32(reader["name_company_type_position"], CultureInfo.InvariantCulture);// int

            legalPerson.ContactName = reader.GetString(reader.GetOrdinal("contact_name"));
        }

        person.PhoneNumber = reader.GetString(reader.GetOrdinal("phone_number"));
        person.PostalCode = reader.GetString(reader.GetOrdinal("location_postal_code"));
        person.Address = reader.GetString(reader.GetOrdinal("location_full"));

        var machiazaId = GetOptionalString(reader, "location_machiaza_id");
        var prefectureName = GetOptionalString(reader, "location_prefecture");
        var county = GetOptionalString(reader, "location_county");
        var city = GetOptionalString(reader, "location_city");
        var ward = GetOptionalString(reader, "location_ward");
        var oaza = GetOptionalString(reader, "location_oaza");
        var choume = GetOptionalString(reader, "location_choume");

        person.AddressLocation.SetMachiazaId(machiazaId);
        person.AddressLocation.SetPrefecture(PrefectureMaster.Prefectures.FirstOrDefault(p => p.Name == prefectureName));
        person.AddressLocation.SetCountyAndCity(string.IsNullOrEmpty(county) && string.IsNullOrEmpty(city) ? null : new CountyAndCity(machiazaId, county, city));
        person.AddressLocation.SetWardAndOaza(string.IsNullOrEmpty(ward) && string.IsNullOrEmpty(oaza) ? null : new WardAndOaza(machiazaId, ward, oaza));
        person.AddressLocation.SetChoume(string.IsNullOrEmpty(choume) ? null : new Choume(machiazaId, choume));
        person.AddressLocation.SetEdaban(GetOptionalString(reader, "location_edaban"));

        person.Remarks = Convert.ToString(reader["remarks"], CultureInfo.InvariantCulture) ?? "";

        return person;
    }

    public ResultWrapper DeleteRentLessor(string lessorId)
    {
        var res = new ResultWrapper();

        if (string.IsNullOrEmpty(lessorId))
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
                cmd.CommandText = string.Format(CultureInfo.InvariantCulture, "DELETE FROM rent_lessors WHERE lessor_id = '{0}';", lessorId);
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

    public ResultWrapper UpsertSaleResidential(ZumenSearch.Domain.Entities.Sale.Residentials.Property property)
    {
        var res = new ResultWrapper();

        if (string.IsNullOrWhiteSpace(property.Id))
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
                property_context_type,
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
                property_context_type = excluded.property_context_type,
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

            command.Parameters.AddWithValue("@propertyId", property.Id);
            command.Parameters.AddWithValue("@name", property.Name);
            command.Parameters.AddWithValue("@propertyKind", property.PropertyContextType.ToString());
            command.Parameters.AddWithValue("@thumbnailFilename", property.ThumbnailFilename);

            // Address/Location
            command.Parameters.AddWithValue("@locPrefId", property.Address.Prefecture?.MunicipalityCode ?? string.Empty);
            command.Parameters.AddWithValue("@locPrefecture", property.Address.Prefecture?.Name ?? string.Empty);
            command.Parameters.AddWithValue("@locMachiazaId", property.Address.MachiazaId ?? string.Empty);
            command.Parameters.AddWithValue("@locCounty", property.Address.CountyAndCity?.County ?? string.Empty);
            command.Parameters.AddWithValue("@locCity", property.Address.CountyAndCity?.City ?? string.Empty);
            command.Parameters.AddWithValue("@locWard", property.Address.WardAndOaza?.Ward ?? string.Empty);
            command.Parameters.AddWithValue("@locOazaCho", property.Address.WardAndOaza?.Oaza ?? string.Empty);
            command.Parameters.AddWithValue("@locChoume", property.Address.Choume?.Chou ?? string.Empty);
            command.Parameters.AddWithValue("@locEdaban", property.Address.Edaban ?? string.Empty);
            command.Parameters.AddWithValue("@locLocationFull", property.Address.AddressFull ?? string.Empty);
            command.Parameters.AddWithValue("@locationLatitude", property.Address.LocationLatitude);
            command.Parameters.AddWithValue("@locationLongitude", property.Address.LocationLongitude);

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

            command.Parameters.AddWithValue("@propertyId", property.Id);
            command.Parameters.AddWithValue(
                "@buildingType",
                property.BuildingKind.Key.ToString());
            command.Parameters.AddWithValue(
                "@isUnitOwnership",
                property.IsUnitOwnership ? 1 : 0);
            command.Parameters.AddWithValue(
                "@buildingStructure",
                property.PropertyStructure.Key.ToString());
            command.Parameters.AddWithValue(
                "@floorCountAboveGround",
                property.FloorCountAboveGround);
            command.Parameters.AddWithValue(
                "@floorCountBasement",
                property.FloorCountBasement);
            command.Parameters.AddWithValue(
                "@totalUnitCount",
                property.TotalUnitCount);
            command.Parameters.AddWithValue(
                "@builtYearMonth",
                property.BuiltYearAndMonth.ToString("s"));
            command.Parameters.AddWithValue(
                "@fudousanId",
                property.FudousanId);
            command.Parameters.AddWithValue(
                "@fudousanIdAdditionalCode",
                property.FudousanIdAdditionalCode);
            command.Parameters.AddWithValue("@remarks", property.Remarks);
            command.Parameters.AddWithValue(
                "@updatedAt",
                DateTimeOffset.UtcNow.ToString("s"));

            res.AffectedCount = command.ExecuteNonQuery();

            transaction.Commit();

            property.SetStatus(EntityStatus.Saved);
            property.SetIsModified(false);
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
                  p.property_context_type
              FROM sale_residentials AS s
              INNER JOIN properties AS p
                  ON p.property_id = s.property_id
              WHERE p.property_context_type = @propertyKind
              ORDER BY p.name;
              """
                : """
              SELECT
                  p.property_id,
                  p.name,
                  p.property_context_type
              FROM sale_residentials AS s
              INNER JOIN properties AS p
                  ON p.property_id = s.property_id
              WHERE p.property_context_type = @propertyKind
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

                var item = new ZumenSearch.Application.SearchResults.PropertySearchResultItem(
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

    public ZumenSearch.Presentation.Features.Sale.Residentials.Models.PropertyResultWrapper SelectSaleResidentialById(string id)
    {
        var res = new ZumenSearch.Presentation.Features.Sale.Residentials.Models.PropertyResultWrapper();

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
                new ZumenSearch.Domain.Entities.Sale.Residentials.Property(
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
              AND property_context_type = @propertyKind;
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

    public ResultWrapper UpsertSaleResidentialListing(string saleId, ZumenSearch.Domain.Entities.Sale.Residentials.Listing room)
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
              AND property_context_type = @propertyKind;

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
                p.fudousan_id,
                p.name AS property_name,
                u.listing_id,
                u.name AS unit_name
            FROM sale_residential_units AS u
            INNER JOIN properties AS p
                ON p.property_id = u.property_id
            WHERE p.property_context_type = @propertyKind
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

                var item = new ZumenSearch.Application.SearchResults.ListingSearchResultItem(
                    listingId,
                    propertyId,
                    PropertyContextType.SaleResidential,
                    Convert.ToString(reader["fudousan_id"], CultureInfo.InvariantCulture) ?? string.Empty)
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

    public ZumenSearch.Presentation.Features.Sale.Residentials.Models.ListingResultWrapper SelectSaleResidentialListingById(string saleId, string roomId)
    {
        var res = new ZumenSearch.Presentation.Features.Sale.Residentials.Models.ListingResultWrapper();

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
                p.fudousan_id,
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
              AND p.property_context_type = @propertyKind;
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

            var room = new ZumenSearch.Domain.Entities.Sale.Residentials.Listing(
                listingId,
                EntityStatus.Saved,
                propertyId,
                EntityStatus.Saved,
                Convert.ToInt32(
                    reader["is_property_unit_ownership"], CultureInfo.InvariantCulture) != 0,
                propertyName,
                Convert.ToString(reader["fudousan_id"], CultureInfo.InvariantCulture) ?? string.Empty)/*
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

    ZumenSearch.Application.Results.PersonsSearchResultWrapper IPersonRepository.SelectBrokersByKeyword(string keyword) => SelectBrokersByKeyword(keyword);

    ZumenSearch.Application.Results.PersonResultWrapper IPersonRepository.SelectBrokerById(string brokerId) => SelectBrokerById(brokerId);

    ResultWrapper IPersonRepository.UpsertBroker(ZumenSearch.Domain.Entities.Base.PersonBase broker) => UpsertBroker(broker);

    ResultWrapper IPersonRepository.DeleteBroker(string brokerId) => DeleteBroker(brokerId);

    public ResultWrapper UpsertBroker(ZumenSearch.Domain.Entities.Base.PersonBase broker)
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
                    phone_number,
                    mobile_phone_number,
                    contact_name,
                    location_postal_code,
                    location_full,
                    location_machiaza_id,
                    location_prefecture,
                    location_county,
                    location_city,
                    location_ward,
                    location_oaza,
                    location_choume,
                    location_edaban,
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
                    @phoneNumber,
                    @mobilePhoneNumber,
                    @contactName,
                    @locationPostalCode,
                    @locationFull,
                    @locationMachiazaId,
                    @locationPrefecture,
                    @locationCounty,
                    @locationCity,
                    @locationWard,
                    @locationOaza,
                    @locationChoume,
                    @locationEdaban,
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
                    phone_number = excluded.phone_number,
                    mobile_phone_number = excluded.mobile_phone_number,
                    contact_name = excluded.contact_name,
                    location_postal_code = excluded.location_postal_code,
                    location_full = excluded.location_full,
                    location_machiaza_id = excluded.location_machiaza_id,
                    location_prefecture = excluded.location_prefecture,
                    location_county = excluded.location_county,
                    location_city = excluded.location_city,
                    location_ward = excluded.location_ward,
                    location_oaza = excluded.location_oaza,
                    location_choume = excluded.location_choume,
                    location_edaban = excluded.location_edaban,
                    remarks = excluded.remarks,
                    updated_at = excluded.updated_at;
                """;

            command.Parameters.AddWithValue("@brokerId", broker.Id);
            command.Parameters.AddWithValue("@name", broker.Name);
            command.Parameters.AddWithValue("@personKind", broker.PersonKind.ToString());
            command.Parameters.AddWithValue("@phoneNumber", broker.PhoneNumber);
            command.Parameters.AddWithValue("@mobilePhoneNumber", broker is PersonNatural naturalPerson ? naturalPerson.MobilePhoneNumber : string.Empty);
            command.Parameters.AddWithValue("@contactName", broker is PersonLegal legalPerson ? legalPerson.ContactName : string.Empty);
            command.Parameters.AddWithValue("@locationPostalCode", broker.PostalCode);
            command.Parameters.AddWithValue("@locationFull", broker.Address);
            command.Parameters.AddWithValue("@locationMachiazaId", broker.AddressLocation.MachiazaId);
            command.Parameters.AddWithValue("@locationPrefecture", broker.AddressLocation.Prefecture?.Name ?? string.Empty);
            command.Parameters.AddWithValue("@locationCounty", broker.AddressLocation.CountyAndCity?.County ?? string.Empty);
            command.Parameters.AddWithValue("@locationCity", broker.AddressLocation.CountyAndCity?.City ?? string.Empty);
            command.Parameters.AddWithValue("@locationWard", broker.AddressLocation.WardAndOaza?.Ward ?? string.Empty);
            command.Parameters.AddWithValue("@locationOaza", broker.AddressLocation.WardAndOaza?.Oaza ?? string.Empty);
            command.Parameters.AddWithValue("@locationChoume", broker.AddressLocation.Choume?.Chou ?? string.Empty);
            command.Parameters.AddWithValue("@locationEdaban", broker.AddressLocation.Edaban);
            command.Parameters.AddWithValue("@remarks", broker.Remarks);
            command.Parameters.AddWithValue("@updatedAt", DateTimeOffset.UtcNow.ToString("s"));

            if (broker is ZumenSearch.Domain.Entities.Person.PersonNatural natural)
            {
                command.Parameters.AddWithValue("@nameLast", natural.NameLast);
                command.Parameters.AddWithValue("@nameFirst", natural.NameFirst);
                command.Parameters.AddWithValue("@nameCompany", string.Empty);
                command.Parameters.AddWithValue("@nameCompanyType", string.Empty);
                command.Parameters.AddWithValue("@nameCompanyTypePosition", 0);
            }
            else if (broker is ZumenSearch.Domain.Entities.Person.PersonLegal legal)
            {
                command.Parameters.AddWithValue("@nameLast", string.Empty);
                command.Parameters.AddWithValue("@nameFirst", string.Empty);
                command.Parameters.AddWithValue("@nameCompany", legal.NameCompany);
                command.Parameters.AddWithValue("@nameCompanyType", legal.NameCompanyType);
                command.Parameters.AddWithValue("@nameCompanyTypePosition",legal.NameCompanyTypePosition);
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
        var searchAll = string.IsNullOrWhiteSpace(keyword) || keyword.Trim() == "*";

        _readerWriterLock.EnterReadLock();

        try
        {
            using var connection = new SqliteConnection(_connectionStringBuilder.ConnectionString);

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
                command.Parameters.AddWithValue("@keyword", $"%{keyword.Trim()}%");
            }

            using var reader = command.ExecuteReader();

            while (reader.Read())
            {
                var brokerId = Convert.ToString(reader["broker_id"], CultureInfo.InvariantCulture);

                if (string.IsNullOrWhiteSpace(brokerId))
                {
                    continue;
                }

                var personKindText = Convert.ToString(reader["person_kind"], CultureInfo.InvariantCulture) ?? string.Empty;

                if (!Enum.TryParse<ZumenSearch.Domain.Enums.PersonKindType>(personKindText, out var personKind) || personKind == ZumenSearch.Domain.Enums.PersonKindType.Undetermined)
                {
                    continue;
                }

                var item = new ZumenSearch.Application.SearchResults.PersonSearchResultItem(brokerId, personKind)
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
            SetDatabaseError(res, ex, "connection.Open(), reader.Read()", "Failed to read brokers.", nameof(SelectBrokersByKeyword));
        }
        finally
        {
            _readerWriterLock.ExitReadLock();
        }

        return res;
    }

    public ZumenSearch.Application.Results.PersonResultWrapper SelectBrokerById(string brokerId)
    {
        var res = new ZumenSearch.Application.Results.PersonResultWrapper();

        if (string.IsNullOrWhiteSpace(brokerId))
        {
            res.IsError = true;
            res.Error.Description = "Broker ID is empty.";
            return res;
        }

        _readerWriterLock.EnterReadLock();

        try
        {
            using var connection = new SqliteConnection(_connectionStringBuilder.ConnectionString);

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
            phone_number,
            mobile_phone_number,
            contact_name,
            location_postal_code,
            location_full,
            location_machiaza_id,
            location_prefecture,
            location_county,
            location_city,
            location_ward,
            location_oaza,
            location_choume,
            location_edaban,
            remarks
        FROM brokers
        WHERE broker_id = @brokerId;
        """;

            command.Parameters.AddWithValue("@brokerId", brokerId);

            using var reader = command.ExecuteReader();

            if (reader.Read())
            {
                res.Person = GetPerson(reader, brokerId);

                if (res.Person is not null)
                {
                    res.Person.SetStatus(EntityStatus.Saved);
                    res.Person.SetIsModified(false);
                }
            }
        }
        catch (Exception ex)
        {
            SetDatabaseError(res, ex, "connection.Open(), reader.Read()", "Failed to read broker.", nameof(SelectBrokerById));
        }
        finally
        {
            _readerWriterLock.ExitReadLock();
        }

        return res;
    }

    public ResultWrapper DeleteBroker(string brokerId)
    {
        var res = new ResultWrapper();

        if (string.IsNullOrWhiteSpace(brokerId))
        {
            res.IsError = true;
            res.Error.Description = "Broker ID is empty.";
            return res;
        }

        _readerWriterLock.EnterWriteLock();

        try
        {
            using var connection = new SqliteConnection(_connectionStringBuilder.ConnectionString);

            connection.Open();

            using var command = connection.CreateCommand();
            command.CommandText = """
                DELETE FROM brokers
                WHERE broker_id = @brokerId;
                """;

            command.Parameters.AddWithValue("@brokerId", brokerId);

            res.AffectedCount = command.ExecuteNonQuery();
        }
        catch (Exception ex)
        {
            SetDatabaseError(res, ex, "command.ExecuteNonQuery()", "Failed to delete broker.", nameof(DeleteBroker));
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

    private static string EscapeLikeValue(string value)
    {
        return value.Trim()
            .Replace("\\", "\\\\", StringComparison.Ordinal)
            .Replace("%", "\\%", StringComparison.Ordinal)
            .Replace("_", "\\_", StringComparison.Ordinal);
    }

    public void Dispose()
    {
        Debug.WriteLine("DataAccessService Dispose().");

        _readerWriterLock?.Dispose();

        GC.SuppressFinalize(this);
    }
}
