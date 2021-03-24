angular.module('virtoCommerce.simpleExportImportModule')
    .factory('virtoCommerce.simpleExportImportModule.import', ['$resource', '$q', function ($resource, $q) {

        var mockData = {
            totalCount: 10000,
            records: [
                {
                    "productId": "6d4f5835f5f8459590da29528b175ff7",
                    "product": {
                        "code": "41MY02",
                        "name": "1-1/4\" Steel Hex Flange Bolt, Grade 5, Zinc Plated Finish, 5/16\"-18 Dia/Thread Size, 50 PK",
                        "catalogId": "7829d35f417e4dd98851f51322f32c23",
                        "categoryId": "3db665f7c95e46c3890c4a208d8af73d",
                        "outline": "3db665f7c95e46c3890c4a208d8af73d",
                        "path": "Flange Bolts",
                        "titularItemId": "cd2f18d2dba742dd832f45c82508f16e",
                        "mainProductId": "cd2f18d2dba742dd832f45c82508f16e",
                        "isBuyable": true,
                        "isActive": true,
                        "trackInventory": true,
                        "maxQuantity": 0,
                        "minQuantity": 0,
                        "productType": "Physical",
                        "startDate": "2018-04-27T18:08:15.54Z",
                        "priority": 0,
                        "imgSrc": "http://localhost:10645/assets/catalog/7829d/41MY02/41MY01.jpg",
                        "images": [
                            {
                                "relativeUrl": "http://localhost:10645/assets/catalog/7829d/41MY02/41MY01.jpg",
                                "url": "http://localhost:10645/assets/catalog/7829d/41MY02/41MY01.jpg",
                                "sortOrder": 1,
                                "typeId": "Image",
                                "group": "images",
                                "name": "41MY01.jpg",
                                "isInherited": false,
                                "seoObjectType": "Image",
                                "seoInfos": [
                                    {
                                        "name": "41MY01.jpg",
                                        "semanticUrl": "catalog/7829d/41MY02/41MY01.jpg",
                                        "isActive": true
                                    }
                                ],
                                "id": "95cdd400b14b44af8fe8c8fa4d56f3df"
                            }
                        ],
                        "seoObjectType": "CatalogProduct",
                        "isInherited": false,
                        "createdDate": "2018-04-27T18:08:15.543Z",
                        "modifiedDate": "2018-04-27T18:08:15.543Z",
                        "createdBy": "admin",
                        "modifiedBy": "admin",
                        "id": "6d4f5835f5f8459590da29528b175ff7"
                    },
                    "prices": [
                        {
                            "pricelistId": "0456b3ebc0a24c0ab7054ec9a5cea78e",
                            "currency": "USD",
                            "productId": "6d4f5835f5f8459590da29528b175ff7",
                            "sale": 9,
                            "list": 9.3,
                            "minQuantity": 1,
                            "effectiveValue": 9,
                            "createdDate": "2019-09-17T10:01:41.61Z",
                            "modifiedDate": "2021-03-23T15:39:15.7543998Z",
                            "createdBy": "unknown",
                            "modifiedBy": "admin",
                            "id": "04f8a097520d4a36969d66498e10f183"
                        }
                    ]
                },
                {
                    "productId": "0b726c8546e24c2d81edf3cd777e6316",
                    "product": {
                        "code": "22A425",
                        "name": "1\" Steel Hex Flange Bolt, Grade 8, Plain Finish, 7/16\"-14 Dia/Thread Size, 25 PK",
                        "catalogId": "7829d35f417e4dd98851f51322f32c23",
                        "categoryId": "3db665f7c95e46c3890c4a208d8af73d",
                        "outline": "3db665f7c95e46c3890c4a208d8af73d",
                        "path": "Flange Bolts",
                        "isBuyable": true,
                        "isActive": true,
                        "trackInventory": true,
                        "maxQuantity": 0,
                        "minQuantity": 0,
                        "productType": "Physical",
                        "startDate": "2018-04-27T20:10:08.797Z",
                        "priority": 0,
                        "imgSrc": "http://localhost:10645/assets/catalog/7829d/22A425/22A458.jpg",
                        "images": [
                            {
                                "relativeUrl": "http://localhost:10645/assets/catalog/7829d/22A425/22A458.jpg",
                                "url": "http://localhost:10645/assets/catalog/7829d/22A425/22A458.jpg",
                                "sortOrder": 1,
                                "typeId": "Image",
                                "group": "images",
                                "name": "22A458.jpg",
                                "isInherited": false,
                                "seoObjectType": "Image",
                                "seoInfos": [
                                    {
                                        "name": "22A458.jpg",
                                        "semanticUrl": "catalog/7829d/22A425/22A458.jpg",
                                        "isActive": true
                                    }
                                ],
                                "id": "a23ae5675f2048649c6d137d3bb645cb"
                            }
                        ],
                        "seoObjectType": "CatalogProduct",
                        "isInherited": false,
                        "createdDate": "2018-04-27T20:10:08.797Z",
                        "modifiedDate": "2020-10-22T13:13:57.6595013Z",
                        "createdBy": "admin",
                        "modifiedBy": "admin",
                        "id": "0b726c8546e24c2d81edf3cd777e6316"
                    },
                    "prices": [
                        {
                            "pricelistId": "0456b3ebc0a24c0ab7054ec9a5cea78e",
                            "currency": "USD",
                            "productId": "0b726c8546e24c2d81edf3cd777e6316",
                            "list": 16.15,
                            "minQuantity": 1,
                            "effectiveValue": 16.15,
                            "createdDate": "2019-09-17T10:01:41.613Z",
                            "modifiedDate": "2019-09-17T10:01:41.613Z",
                            "createdBy": "unknown",
                            "modifiedBy": "unknown",
                            "id": "0663928796074c439429bad3da05574a"
                        }
                    ]
                },
                {
                    "productId": "b4d4522d8cde446e98651f4c45cf452c",
                    "product": {
                        "code": "4RXG8",
                        "name": "1-3/4\" Steel Hex Flange Bolt, Grade 8, Plain Finish, 3/4\"-10 Dia/Thread Size, 65 PK",
                        "catalogId": "7829d35f417e4dd98851f51322f32c23",
                        "categoryId": "3db665f7c95e46c3890c4a208d8af73d",
                        "outline": "3db665f7c95e46c3890c4a208d8af73d",
                        "path": "Flange Bolts",
                        "titularItemId": "3f604bc4a3d147358a4e5e77ae064a2b",
                        "mainProductId": "3f604bc4a3d147358a4e5e77ae064a2b",
                        "isBuyable": true,
                        "isActive": true,
                        "trackInventory": true,
                        "maxQuantity": 0,
                        "minQuantity": 0,
                        "productType": "Physical",
                        "startDate": "2018-04-27T20:26:59.923Z",
                        "priority": 0,
                        "imgSrc": "http://localhost:10645/assets/catalog/7829d/4RXG8/41MY01.jpg",
                        "images": [
                            {
                                "relativeUrl": "http://localhost:10645/assets/catalog/7829d/4RXG8/41MY01.jpg",
                                "url": "http://localhost:10645/assets/catalog/7829d/4RXG8/41MY01.jpg",
                                "sortOrder": 1,
                                "typeId": "Image",
                                "group": "images",
                                "name": "41MY01.jpg",
                                "isInherited": false,
                                "seoObjectType": "Image",
                                "seoInfos": [
                                    {
                                        "name": "41MY01.jpg",
                                        "semanticUrl": "catalog/7829d/4RXG8/41MY01.jpg",
                                        "isActive": true
                                    }
                                ],
                                "id": "36b6d76b1eb6413eb86f50c012517eae"
                            }
                        ],
                        "seoObjectType": "CatalogProduct",
                        "isInherited": false,
                        "createdDate": "2018-04-27T20:26:59.923Z",
                        "modifiedDate": "2018-04-27T20:26:59.923Z",
                        "createdBy": "admin",
                        "modifiedBy": "admin",
                        "id": "b4d4522d8cde446e98651f4c45cf452c"
                    },
                    "prices": [
                        {
                            "pricelistId": "0456b3ebc0a24c0ab7054ec9a5cea78e",
                            "currency": "USD",
                            "productId": "b4d4522d8cde446e98651f4c45cf452c",
                            "list": 117.5,
                            "minQuantity": 1,
                            "effectiveValue": 117.5,
                            "createdDate": "2019-09-17T10:01:41.717Z",
                            "modifiedDate": "2019-09-17T10:01:41.717Z",
                            "createdBy": "unknown",
                            "modifiedBy": "unknown",
                            "id": "0a03968d2ef84670b9fa1f893daef868"
                        }
                    ]
                },
                {
                    "productId": "2228545259524745b99875bb51111e97",
                    "product": {
                        "code": "38CP99",
                        "name": "20mm Steel Hex Flange Bolt, Class 8.8, Zinc Plated Finish, M8-1.25 Dia/Thread Size, 100 PK",
                        "catalogId": "7829d35f417e4dd98851f51322f32c23",
                        "categoryId": "3db665f7c95e46c3890c4a208d8af73d",
                        "outline": "3db665f7c95e46c3890c4a208d8af73d",
                        "path": "Flange Bolts",
                        "titularItemId": "e1c323dc1dc04a3286cd5871c0e8bbd2",
                        "mainProductId": "e1c323dc1dc04a3286cd5871c0e8bbd2",
                        "isBuyable": true,
                        "isActive": true,
                        "trackInventory": true,
                        "maxQuantity": 0,
                        "minQuantity": 0,
                        "productType": "Physical",
                        "startDate": "2018-04-27T19:06:49.653Z",
                        "priority": 0,
                        "imgSrc": "http://localhost:10645/assets/catalog/7829d/38CP99/41MY01.jpg",
                        "images": [
                            {
                                "relativeUrl": "http://localhost:10645/assets/catalog/7829d/38CP99/41MY01.jpg",
                                "url": "http://localhost:10645/assets/catalog/7829d/38CP99/41MY01.jpg",
                                "sortOrder": 1,
                                "typeId": "Image",
                                "group": "images",
                                "name": "41MY01.jpg",
                                "isInherited": false,
                                "seoObjectType": "Image",
                                "seoInfos": [
                                    {
                                        "name": "41MY01.jpg",
                                        "semanticUrl": "catalog/7829d/38CP99/41MY01.jpg",
                                        "isActive": true
                                    }
                                ],
                                "id": "beb466de310c489497075993b98a5e90"
                            }
                        ],
                        "seoObjectType": "CatalogProduct",
                        "isInherited": false,
                        "createdDate": "2018-04-27T19:06:49.653Z",
                        "modifiedDate": "2018-04-27T19:06:49.653Z",
                        "createdBy": "admin",
                        "modifiedBy": "admin",
                        "id": "2228545259524745b99875bb51111e97"
                    },
                    "prices": [
                        {
                            "pricelistId": "0456b3ebc0a24c0ab7054ec9a5cea78e",
                            "currency": "USD",
                            "productId": "2228545259524745b99875bb51111e97",
                            "list": 18.4,
                            "minQuantity": 1,
                            "effectiveValue": 18.4,
                            "createdDate": "2019-09-17T10:01:41.613Z",
                            "modifiedDate": "2019-09-17T10:01:41.613Z",
                            "createdBy": "unknown",
                            "modifiedBy": "unknown",
                            "id": "0d10053269924a8ab98e7550ececd72c"
                        }
                    ]
                },
                {
                    "productId": "283390c6dd8844efaa9882501f2879a9",
                    "product": {
                        "code": "22A449",
                        "name": "1-1/2\" Steel Hex Flange Bolt, Grade 8, Plain Finish, 1/2\"-20 Dia/Thread Size, 25 PK",
                        "catalogId": "7829d35f417e4dd98851f51322f32c23",
                        "categoryId": "3db665f7c95e46c3890c4a208d8af73d",
                        "outline": "3db665f7c95e46c3890c4a208d8af73d",
                        "path": "Flange Bolts",
                        "titularItemId": "508d2a0584ad4e0e9811577f00b735c8",
                        "mainProductId": "508d2a0584ad4e0e9811577f00b735c8",
                        "isBuyable": true,
                        "isActive": true,
                        "trackInventory": true,
                        "maxQuantity": 0,
                        "minQuantity": 0,
                        "productType": "Physical",
                        "startDate": "2018-04-27T20:41:58.3Z",
                        "priority": 0,
                        "imgSrc": "http://localhost:10645/assets/catalog/7829d/22A449/22A458.jpg",
                        "images": [
                            {
                                "relativeUrl": "http://localhost:10645/assets/catalog/7829d/22A449/22A458.jpg",
                                "url": "http://localhost:10645/assets/catalog/7829d/22A449/22A458.jpg",
                                "sortOrder": 1,
                                "typeId": "Image",
                                "group": "images",
                                "name": "22A458.jpg",
                                "isInherited": false,
                                "seoObjectType": "Image",
                                "seoInfos": [
                                    {
                                        "name": "22A458.jpg",
                                        "semanticUrl": "catalog/7829d/22A449/22A458.jpg",
                                        "isActive": true
                                    }
                                ],
                                "id": "681df5f20a5d49159e1fb7b3e5fa3b74"
                            }
                        ],
                        "seoObjectType": "CatalogProduct",
                        "isInherited": false,
                        "createdDate": "2018-04-27T20:41:58.3Z",
                        "modifiedDate": "2018-04-27T20:41:58.3Z",
                        "createdBy": "admin",
                        "modifiedBy": "admin",
                        "id": "283390c6dd8844efaa9882501f2879a9"
                    },
                    "prices": [
                        {
                            "pricelistId": "0456b3ebc0a24c0ab7054ec9a5cea78e",
                            "currency": "USD",
                            "productId": "283390c6dd8844efaa9882501f2879a9",
                            "list": 20.4,
                            "minQuantity": 1,
                            "effectiveValue": 20.4,
                            "createdDate": "2019-09-17T10:01:41.613Z",
                            "modifiedDate": "2019-09-17T10:01:41.613Z",
                            "createdBy": "unknown",
                            "modifiedBy": "unknown",
                            "id": "0e260a7caff94801afa7ba15add0de59"
                        }
                    ]
                },
                {
                    "productId": "ccff63918246463db2c820f84ea2ba14",
                    "product": {
                        "code": "157A22",
                        "name": "35mm Steel Hex Flange Bolt, Class 10.9, Plain Finish, M10-1.50 Dia/Thread Size, 350 PK",
                        "catalogId": "7829d35f417e4dd98851f51322f32c23",
                        "categoryId": "3db665f7c95e46c3890c4a208d8af73d",
                        "outline": "3db665f7c95e46c3890c4a208d8af73d",
                        "path": "Flange Bolts",
                        "titularItemId": "fb46499c4f7e4c78a09233e52372368f",
                        "mainProductId": "fb46499c4f7e4c78a09233e52372368f",
                        "isBuyable": true,
                        "isActive": true,
                        "trackInventory": true,
                        "maxQuantity": 0,
                        "minQuantity": 0,
                        "productType": "Physical",
                        "startDate": "2018-04-27T20:34:58.997Z",
                        "priority": 0,
                        "imgSrc": "http://localhost:10645/assets/catalog/7829d/157A22/41MY01.jpg",
                        "images": [
                            {
                                "relativeUrl": "http://localhost:10645/assets/catalog/7829d/157A22/41MY01.jpg",
                                "url": "http://localhost:10645/assets/catalog/7829d/157A22/41MY01.jpg",
                                "sortOrder": 1,
                                "typeId": "Image",
                                "group": "images",
                                "name": "41MY01.jpg",
                                "isInherited": false,
                                "seoObjectType": "Image",
                                "seoInfos": [
                                    {
                                        "name": "41MY01.jpg",
                                        "semanticUrl": "catalog/7829d/157A22/41MY01.jpg",
                                        "isActive": true
                                    }
                                ],
                                "id": "241b164a5f9244efb108435e971b6239"
                            }
                        ],
                        "seoObjectType": "CatalogProduct",
                        "isInherited": false,
                        "createdDate": "2018-04-27T20:34:59.013Z",
                        "modifiedDate": "2018-04-27T20:34:59.013Z",
                        "createdBy": "admin",
                        "modifiedBy": "admin",
                        "id": "ccff63918246463db2c820f84ea2ba14"
                    },
                    "prices": [
                        {
                            "pricelistId": "0456b3ebc0a24c0ab7054ec9a5cea78e",
                            "currency": "USD",
                            "productId": "ccff63918246463db2c820f84ea2ba14",
                            "list": 155.4,
                            "minQuantity": 1,
                            "effectiveValue": 155.4,
                            "createdDate": "2019-09-17T10:01:41.717Z",
                            "modifiedDate": "2019-09-17T10:01:41.717Z",
                            "createdBy": "unknown",
                            "modifiedBy": "unknown",
                            "id": "115ff9bdcf1e49df86a4242ec0de278a"
                        }
                    ]
                },
                {
                    "productId": "c827222f6d87496183728413e7203256",
                    "product": {
                        "code": "21A915",
                        "name": "2\" Steel Carriage Bolt, Grade 8, Plain Finish, 5/8-11 Dia/Thread Size, 25 PK",
                        "catalogId": "7829d35f417e4dd98851f51322f32c23",
                        "categoryId": "4fbaca886f014767a52f3f38b9df648f",
                        "outline": "4fbaca886f014767a52f3f38b9df648f",
                        "path": "Carriage Bolts",
                        "isBuyable": true,
                        "isActive": true,
                        "trackInventory": true,
                        "maxQuantity": 0,
                        "minQuantity": 0,
                        "productType": "Physical",
                        "startDate": "2018-04-27T14:56:34.22Z",
                        "priority": 0,
                        "imgSrc": "http://localhost:10645/assets/catalog/7829d/21A915/21A915.jpg",
                        "images": [
                            {
                                "relativeUrl": "http://localhost:10645/assets/catalog/7829d/21A915/21A915.jpg",
                                "url": "http://localhost:10645/assets/catalog/7829d/21A915/21A915.jpg",
                                "sortOrder": 1,
                                "typeId": "Image",
                                "group": "images",
                                "name": "21A915.jpg",
                                "isInherited": false,
                                "seoObjectType": "Image",
                                "seoInfos": [
                                    {
                                        "name": "21A915.jpg",
                                        "semanticUrl": "catalog/7829d/21A915/21A915.jpg",
                                        "isActive": true
                                    }
                                ],
                                "id": "ffe80e0a89ea432bba244a7ef9665b1e"
                            }
                        ],
                        "seoObjectType": "CatalogProduct",
                        "isInherited": false,
                        "createdDate": "2018-04-27T14:56:34.22Z",
                        "modifiedDate": "2020-10-22T13:13:57.6606374Z",
                        "createdBy": "admin",
                        "modifiedBy": "admin",
                        "id": "c827222f6d87496183728413e7203256"
                    },
                    "prices": [
                        {
                            "pricelistId": "0456b3ebc0a24c0ab7054ec9a5cea78e",
                            "currency": "USD",
                            "productId": "c827222f6d87496183728413e7203256",
                            "list": 75.25,
                            "minQuantity": 1,
                            "effectiveValue": 75.25,
                            "createdDate": "2019-09-17T10:01:41.613Z",
                            "modifiedDate": "2019-09-17T10:01:41.613Z",
                            "createdBy": "unknown",
                            "modifiedBy": "unknown",
                            "id": "124bd77d80774434ba462f0cd8fea0ae"
                        }
                    ]
                },
                {
                    "productId": "24cd1f338dfc4d89ad68633932a4225e",
                    "product": {
                        "code": "54FT59",
                        "name": "16mm Stainless Steel Carriage Bolt, A2, Plain Finish, M5-0.80 Dia/Thread Size, 100 PK",
                        "catalogId": "7829d35f417e4dd98851f51322f32c23",
                        "categoryId": "4fbaca886f014767a52f3f38b9df648f",
                        "outline": "4fbaca886f014767a52f3f38b9df648f",
                        "path": "Carriage Bolts",
                        "isBuyable": true,
                        "isActive": true,
                        "trackInventory": true,
                        "maxQuantity": 0,
                        "minQuantity": 0,
                        "productType": "Physical",
                        "startDate": "2018-04-27T15:45:09.477Z",
                        "priority": 0,
                        "imgSrc": "http://localhost:10645/assets/catalog/7829d/54FT59/164W33.jpg",
                        "images": [
                            {
                                "relativeUrl": "http://localhost:10645/assets/catalog/7829d/54FT59/164W33.jpg",
                                "url": "http://localhost:10645/assets/catalog/7829d/54FT59/164W33.jpg",
                                "sortOrder": 1,
                                "typeId": "Image",
                                "group": "images",
                                "name": "164W33.jpg",
                                "isInherited": false,
                                "seoObjectType": "Image",
                                "seoInfos": [
                                    {
                                        "name": "164W33.jpg",
                                        "semanticUrl": "catalog/7829d/54FT59/164W33.jpg",
                                        "isActive": true
                                    }
                                ],
                                "id": "bec088fcc6204b5d8af7c0993de798db"
                            }
                        ],
                        "seoObjectType": "CatalogProduct",
                        "isInherited": false,
                        "createdDate": "2018-04-27T15:45:09.477Z",
                        "modifiedDate": "2020-10-22T13:13:57.6596421Z",
                        "createdBy": "admin",
                        "modifiedBy": "admin",
                        "id": "24cd1f338dfc4d89ad68633932a4225e"
                    },
                    "prices": [
                        {
                            "pricelistId": "0456b3ebc0a24c0ab7054ec9a5cea78e",
                            "currency": "USD",
                            "productId": "24cd1f338dfc4d89ad68633932a4225e",
                            "list": 13.2,
                            "minQuantity": 1,
                            "effectiveValue": 13.2,
                            "createdDate": "2019-09-17T10:01:41.613Z",
                            "modifiedDate": "2019-09-17T10:01:41.613Z",
                            "createdBy": "unknown",
                            "modifiedBy": "unknown",
                            "id": "16e55be159e94bd7ac4b57a5ae7a8d17"
                        }
                    ]
                },
                {
                    "productId": "508d2a0584ad4e0e9811577f00b735c8",
                    "product": {
                        "code": "22A447",
                        "name": "1\" Steel Hex Flange Bolt, Grade 8, Plain Finish, 1/2\"-20 Dia/Thread Size, 25 PK",
                        "catalogId": "7829d35f417e4dd98851f51322f32c23",
                        "categoryId": "3db665f7c95e46c3890c4a208d8af73d",
                        "outline": "3db665f7c95e46c3890c4a208d8af73d",
                        "path": "Flange Bolts",
                        "isBuyable": true,
                        "isActive": true,
                        "trackInventory": true,
                        "maxQuantity": 0,
                        "minQuantity": 0,
                        "productType": "Physical",
                        "startDate": "2018-04-27T20:38:22.673Z",
                        "priority": 0,
                        "imgSrc": "http://localhost:10645/assets/catalog/7829d/22A447/22A458.jpg",
                        "images": [
                            {
                                "relativeUrl": "http://localhost:10645/assets/catalog/7829d/22A447/22A458.jpg",
                                "url": "http://localhost:10645/assets/catalog/7829d/22A447/22A458.jpg",
                                "sortOrder": 1,
                                "typeId": "Image",
                                "group": "images",
                                "name": "22A458.jpg",
                                "isInherited": false,
                                "seoObjectType": "Image",
                                "seoInfos": [
                                    {
                                        "name": "22A458.jpg",
                                        "semanticUrl": "catalog/7829d/22A447/22A458.jpg",
                                        "isActive": true
                                    }
                                ],
                                "id": "e42219a199b64656b13fb764d7a9a41c"
                            }
                        ],
                        "seoObjectType": "CatalogProduct",
                        "isInherited": false,
                        "createdDate": "2018-04-27T20:38:22.673Z",
                        "modifiedDate": "2020-10-22T13:13:57.6597443Z",
                        "createdBy": "admin",
                        "modifiedBy": "admin",
                        "id": "508d2a0584ad4e0e9811577f00b735c8"
                    },
                    "prices": [
                        {
                            "pricelistId": "0456b3ebc0a24c0ab7054ec9a5cea78e",
                            "currency": "USD",
                            "productId": "508d2a0584ad4e0e9811577f00b735c8",
                            "list": 18.35,
                            "minQuantity": 1,
                            "effectiveValue": 18.35,
                            "createdDate": "2019-09-17T10:01:41.613Z",
                            "modifiedDate": "2019-09-17T10:01:41.613Z",
                            "createdBy": "unknown",
                            "modifiedBy": "unknown",
                            "id": "17972fd0a9db4bb0bd4ec8b749a1930f"
                        }
                    ]
                },
                {
                    "productId": "0fb539fb0cb64ace95e9b53381560518",
                    "product": {
                        "code": "5ZMR2",
                        "name": "1-1/2\" Steel Carriage Bolt, Grade 5, Zinc Plated Finish, 1/4\"-20 Dia/Thread Size, 100 PK",
                        "catalogId": "7829d35f417e4dd98851f51322f32c23",
                        "categoryId": "4fbaca886f014767a52f3f38b9df648f",
                        "outline": "4fbaca886f014767a52f3f38b9df648f",
                        "path": "Carriage Bolts",
                        "titularItemId": "ec235043d51848249e90ef170c371a1c",
                        "mainProductId": "ec235043d51848249e90ef170c371a1c",
                        "isBuyable": true,
                        "isActive": true,
                        "trackInventory": true,
                        "maxQuantity": 0,
                        "minQuantity": 0,
                        "productType": "Physical",
                        "startDate": "2018-04-27T14:34:43.29Z",
                        "priority": 0,
                        "imgSrc": "http://localhost:10645/assets/catalog/7829d/5ZMR2/5ZMR1.jpg",
                        "images": [
                            {
                                "relativeUrl": "http://localhost:10645/assets/catalog/7829d/5ZMR2/5ZMR1.jpg",
                                "url": "http://localhost:10645/assets/catalog/7829d/5ZMR2/5ZMR1.jpg",
                                "sortOrder": 1,
                                "typeId": "Image",
                                "group": "images",
                                "name": "5ZMR1.jpg",
                                "isInherited": false,
                                "seoObjectType": "Image",
                                "seoInfos": [
                                    {
                                        "name": "5ZMR1.jpg",
                                        "semanticUrl": "catalog/7829d/5ZMR2/5ZMR1.jpg",
                                        "isActive": true
                                    }
                                ],
                                "id": "77efeecc4a6e492f902235f5583a8464"
                            }
                        ],
                        "seoObjectType": "CatalogProduct",
                        "isInherited": false,
                        "createdDate": "2018-04-27T14:34:43.29Z",
                        "modifiedDate": "2018-04-27T14:34:43.29Z",
                        "createdBy": "admin",
                        "modifiedBy": "admin",
                        "id": "0fb539fb0cb64ace95e9b53381560518"
                    },
                    "prices": [
                        {
                            "pricelistId": "0456b3ebc0a24c0ab7054ec9a5cea78e",
                            "currency": "USD",
                            "productId": "0fb539fb0cb64ace95e9b53381560518",
                            "list": 18.6,
                            "minQuantity": 1,
                            "effectiveValue": 18.6,
                            "createdDate": "2019-09-17T10:01:41.613Z",
                            "modifiedDate": "2019-09-17T10:01:41.613Z",
                            "createdBy": "unknown",
                            "modifiedBy": "unknown",
                            "id": "1832fef4c61846bda038e3f7484982f2"
                        }
                    ]
                }
            ]
        };

        var mock = function () { };

        var mock = { };

        mock.preview = function (request, success, error) {
            var deferred = $q.defer();
            var promise = deferred.promise;
            promise.then(success, error);
            deferred.resolve(mockData);
        };    

        return mock;

    }])
