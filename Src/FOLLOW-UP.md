# Follow-up work

Review Rent Residentials and Rent Commercials together unless a task identifies a different scope. Priorities are not a suggested work order; confirm behavior and choose fixes before implementation. Paths below identify likely starting points, not an exhaustive list of files to edit.

## 1. Higher-severity risks: lost or inconsistent data

### Ownership consistency

**Files:** Rent Residential and Commercial property and listing view models.

Unsaved ownership changes immediately reach open listing editors, which can save before the property is saved. This is a design choice; discarding the property change does not undo a completed listing save.

### Persistence and search consistency

**Files:** `Infrastructure\DataAccess\DataAccessService.cs`; Rent Residential and Commercial repositories and search-result handling; property and listing view models.

- [x] Listing deletion does not remove lessor and broker associations because the shared association tables do not reference listing rows. Review direct and property-editor deletion in both implementations; property-level contact searches can match remaining associations.
- [X] Both property upserts mark individual pictures and PDFs saved and unmodified before transaction commit. If a later statement or commit fails, rollback does not restore those flags. Review callers and retries; defer state changes until successful commit where appropriate.
- [X] Both listing upserts resolve an ID conflict without verifying that the existing row belongs to the supplied property. An ID collision or inconsistent caller data can update another property's listing while child and association writes use the supplied property ID. Review identity and ownership checks, including property-editor handoffs.
- [ ] Existing listing search results can retain old property names after a rename and deleted listings after property deletion or a property-editor listing deletion. Opening a missing listing only logs the absence. Review result synchronization and user-facing feedback in both workflows.

### Numeric input and search

**Files:** Rent Residential and Commercial listing view models, search criteria and search pages; `Infrastructure\DataAccess\DataAccessService.cs`.

- [ ] Both listing editors validate normalized commission input but save the original text; commission searches cast stored text without Unicode normalization. Residential also saves original floor-area and contract-fee text after normalized validation, while its floor-area, management-fee, security-deposit, and key-money searches cast without normalization; full-width digits can be interpreted as zero. Commercial stores those latter fields as normalized decimals. Review consistent storage and searches, including previously saved values.
- [ ] Residential stores unspecified listing floor area as empty text, which numeric search casts to zero; Commercial uses NULL, which does not match numeric bounds. Both commission upper-bound searches cast unspecified empty values to zero. Decide consistent unspecified-value semantics.
- [ ] Residential management-fee search labels its maximum in yen but compares amounts without checking whether the unit is yen or months. Both searches accept a commission maximum without a unit and compare percentage and yen values against the same numeric bound. Review unit-aware filtering and intended unitless limits together, including Commercial behavior.

## 2. Straightforward fixes and usability

### Validation, display, and feedback

**Files:** Rent Residential and Commercial property and listing view models; their basic, location, and media pages.

- [ ] Consider earlier property identifier validation: reject a non-empty 不動産番号 shorter than 13 digits or 特定コード shorter than 4 characters before applying editor values, and navigate to the basic page. Domain setters already reject incomplete values.
- [ ] Both listing editors silently reject invalid 特定コード characters in view-model setters while their text boxes have no matching input handler. Displayed text may differ from the saved value; verify interactively and review feedback and input handling.
- [ ] Coordinate input is validated by address setters during property save. Consider earlier validation and navigation to the location page in both editors.

### Search usability

**Files:** Rent Residential and Commercial search criteria and pages.

- [ ] Both search pages have no explicit way to clear selected handling type, transaction type, or commission unit. Selecting 未指定 for handling or transaction filters for listings stored with that value instead of removing the filter. Verify controls interactively and review reset behavior and labels.
- [x] Residential and Commercial keyword searches cover property name, listing name, full address, real-estate ID, station name, and applicable lessor name; placeholders state the scope.

## 3. Edge cases and exception handling

### Save, initialization, and input timing

**Files:** Rent Residential and Commercial property and listing view models, `Views\Listing\BasicPage.xaml`, and Residential `Views\FacilitiesPage.xaml`.

- [ ] Property and listing editors catch and report population failures but can leave partially populated editors available for later changes and saves. Review initialization failure handling and protection against saving incomplete values in both implementations.
- [ ] Address selection follows Residential location-master matching. Review behavior when master data is unavailable or incomplete, including saving unmatched components.
- [ ] Listing contract and transaction typed-label selections use reference-based change detection. Investigate whether a different label instance with the same key can unnecessarily mark either editor dirty during binding updates.

### Search boundaries and dates

**Files:** Rent Residential and Commercial search criteria and pages; `Infrastructure\DataAccess\DataAccessService.cs`.

- [ ] Searches use double-precision bounds and SQLite numeric conversions; high-precision decimals or very large whole-yen rents may compare differently at a boundary than entered. Define supported ranges and required precision.
- [ ] Building-age search compares years without construction month, so results can exceed the requested age. Unknown dates use a 1900 sentinel that sufficiently large limits can match and that is ambiguous with an actual 1900 date. Review boundary and unknown-date semantics.

### Media and filesystem lifecycle

### Unlikely edge cases

**Files:** Rent Residential and Commercial property and listing view models, media view models, editor windows, and relevant data-access code.

- [ ] Property and listing duplication continue after individual media copy failures and copy with overwrite enabled. If a PDF copies but its thumbnail fails, the PDF is omitted from the editor, and successful save clears tracking of the copied PDF, potentially leaving it unreferenced. Review partial copies, missing attachments, and destination collisions without automatically retrying deletion.
- [ ] After a failed property or listing PDF import, both implementations report the import failure but only log subsequent failures to delete partial PDFs or thumbnails. Review cleanup failure reporting without automatically retrying deletion.

**Files:** Rent Residential and Commercial property view models.

- [ ] Applying property values mutates the entity before validation and persistence complete, so discard after a failed save can send an unsaved ownership value instead of the last saved one. Review both property editors.

**Files:** Rent Residential and Commercial property and listing view models; contact-selection dialogs and person view models; `Presentation\Shell\ViewModels\MainViewModel.cs`.

- [ ] Contact selection can race with contact updates or deletion while loading, and confirmation can overlap updates that replace the displayed wrapper. Review property and listing editors, including messages arriving during save.

**Files:** `Presentation\Shell\ViewModels\MainViewModel.cs`, `Presentation\Shell\Views\MainWindow.xaml.cs`, Rent Residential and Commercial property and listing editor windows and view models, and search-result pages.

- [ ] Consider a shared busy flag to prevent overlapping opening, deletion, and saving operations across entry points.

**Files:** Rent Residential and Commercial `PropertyViewModel.cs` and `ListingViewModel.cs`; their listing editor windows and media view models; `Infrastructure\DataAccess\DataAccessService.cs`.

- [ ] Review shared mutable listing state between the property editor and open listing editors. Full-object copying and broader coordination are not planned at this stage.

## 4. Future work

### Accessibility

- [ ] Review accessible names and screen-reader behavior throughout the app, and add or correct labels where needed. Include the Residential listing commission fields and unit selectors, and the contract fee-unit selectors, using Commercial as a reference.


