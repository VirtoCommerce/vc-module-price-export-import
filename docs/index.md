# Overview
If you want to transfer a large amount of price information between Virto Commerce and another system,
then you can use a specially-formatted spreadsheet to import or export that data. Virto Commerce uses CSV (semicolon-separated value) files to perform this kind of bulk task.

The business goal for the module is to provide to non-technical not high skilled business users (like "category manager") who works with prices on a daily basis and don't understand the database structure to work comfortably with prices export and import functionality using it for price management.

![Main-Scree](media/main-screen.png)

!!!note
    If you want to automated transferring information from 3rd party system, like ERP, then see API, Integration Middleware approach and Azure Logic Apps connectors.


## Business scenarios
* I need to export two price lists to make comparing. 
* I need to change multiple prices in the price list of few hundreds of records in the price list.
* I need to make a bulk price update (+5% for everything) for a price list.
* I need to add prices for a batch of new products added to the catalog.

## Using CSV

### Get a sample CSV file
You can export and view a sample priceline CSV file to use as a template.

The sample file includes many columns but for import only the SKU column, min quantity and prices will be used, other columns will be skipped
![Main-Screen](media\Screenshot_12.png)

### Priceline CSV file format for import
The first line should be Header: `SKU`;`List price`;`Sale price`;`Min quantity`. Each column must be separated by a semicolon. Product SKU, Min quantity and List price values are required for creation and updating prices. Priceline KEY - the pair SKU + min quantity, so it should not be duplicated in CSV file. Where SKU = product Item code and Min quantity = minimum product quantity needed for specifided price

 Example:

 ![csv file](media\Screenshot_9.png)

SKU | List price | Sale price | Min quantity 
---| --- | --- |
54FT67 | 20 | 19 | 1
41MY54 | 22 | 20 | 2


## Export priceline
### Run price export from the price list
#### User exports all prices from the price list
1. The user opens price list blade
2. The user clicks the “Export” icon
3. The system opens “Price export dialog screen” with the text “All NN prices will be exported”
4. User confirms export
5. The system opens the processing blade where the link to download file appears when the processing is finished
6. The user clicks the link
7. File is downloaded 
#### User exports selected prices from the price list
1. The user opens price list blade, Selects NN pricelines
2. The user clicks the “Export” icon
3. The system opens “Price export dialog screen” with the text “Selected NN prices will be exported”
4. User confirms export
5. The system opens the processing blade where the link to download file appears when the processing is finished
6. The user clicks the link
7. File is downloaded
#### User exports all prices from the filtered price list
1. The user opens price list blade, Filtered pricelines
2. The user clicks the “Export” icon
3. The system opens “Price export dialog screen” with the text “NN prices will be exported”
4. User confirms export
5. The system opens the processing blade where the link appears when the processing is finished
6. The user clicks the link
7. File is downloaded with filtered pricelines

## Import priceline                       
### Update existing lines with import
1. The user opens price list blade
2. The user clicks the “Import” icon
3. The system opens Upload CSV  blade with drag-n-drop
4. User browses and Upload CSV file, clicks "Preview"
5. The system opens the Preview blade where the first 10 lines from uploaded CSV file
6. The user selects an option “Update existing only”
6. The user clicks "Import" icon
5. The system opens the Import progress blade where the Total count, Lines created, Lines updated, Error count display. Error report download URL with link to download error report file appears when the processing is finished with some errors.
6. System only updates existing lines.

### Create new lines with import
1. The user opens price list blade
2. The user clicks the “Import” icon
3. The system opens Upload CSV  blade with drag-n-drop
4. User browses and Upload CSV file, clicks "Preview"
5. The system opens the Preview blade where the first 10 lines from uploaded CSV file
6. The user selects an option “Create new only”
6. The user clicks "Import" icon
5. The system opens the Import progress blade where the Total count, Lines created, Lines updated, Error count display. Error report download URL with link to download error report file appears when the processing is finished with some errors.
6. System only creates new lines.

### Update and Create lines with import
1. The user opens price list blade
2. The user clicks the “Import” icon
3. The system opens Upload CSV  blade with drag-n-drop
4. User browses and Upload CSV file, clicks "Preview"
5. The system opens the Preview blade where the first 10 lines from uploaded CSV file
6. The user selects an option “Create&Update”
6. The user clicks "Import" icon
5. The system opens the Import progress blade where the Total count, Lines created, Lines updated, Error count display. Error report download URL with link to download error report file appears when the processing is finished with some errors.
6. System updates existing lines and create new lines.
![price import](media\Screenshot_3.png)
![price import](media\Screenshot_4.png)    

## Advanced settings
There are default limit values exist in the system

* Limit for number of lines to export = 10000 by default
Ask system administrator to change it throught an environment variable for `PriceExportImport__Export__LimitOfLines`
* Limit for number of lines to import = 10000 by default
Ask system administrator to change it throught an environment variable for `PriceExportImport__Import__LimitOfLines`
* Limit for size of csv file = 1mb by default
Ask system administrator to change it throught an environment variable for `PriceExportImport__Import__FileMaxSize`

## Development
The C# interfaces and implementation can be changed in next releases.
