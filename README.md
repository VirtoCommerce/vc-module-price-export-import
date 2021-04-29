# Price Export&Import module
If you want to transfer a large amount of price information between Virto Commerce and another system,
then you can use a specially-formatted spreadsheet to import or export that data. Virto Commerce uses CSV (semicolon-separated value) files to perform this kind of bulk task.

The business goal for the module is to provide to non-technical not high skilled business users (like "category manager") who works with prices on a daily basis and don't understand the database structure to work comfortably with prices export and import functionality using it for price management.

![Main-Screen](docs/media/main-screen.png)

!!!note
    If you want to automated transferring information from 3rd party system, like ERP, then see API, Integration Middleware approach and Azure Logic Apps connectors.


## Business scenarios
* I need to export two price lists to make comparing. 
* I need to change multiple prices in the price list of few hundreds of records in the price list.
* I need to make a bulk price update (+5% for everything) for a price list.
* I need to add prices for a batch of new products added to the catalog.


## Documentation
* [Module Documentation](https://virtocommerce.com/docs/latest/modules/simple-export-import/)
* [View on GitHub](docs/index.md)

## Development
    The C# interfaces and implementation can be changed in next releases.

## References

* Deploy: https://virtocommerce.com/docs/latest/developer-guide/deploy-module-from-source-code/
* Installation: https://www.virtocommerce.com/docs/latest/user-guide/modules/
* Home: https://virtocommerce.com
* Community: https://www.virtocommerce.org
* [Download Latest Release](https://github.com/VirtoCommerce/vc-module-simple-export-import/releases/latest)

## License

Copyright (c) Virto Solutions LTD.  All rights reserved.

Licensed under the Virto Commerce Open Software License (the "License"); you
may not use this file except in compliance with the License. You may
obtain a copy of the License at

http://virtocommerce.com/opensourcelicense

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or
implied.
