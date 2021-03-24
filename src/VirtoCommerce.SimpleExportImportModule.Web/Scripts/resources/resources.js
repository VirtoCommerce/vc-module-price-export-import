angular.module('virtoCommerce.simpleExportImportModule')
    .factory('virtoCommerce.simpleExportImportModule.import', ['$resource', '$q', function ($resource, $q) {

        var mockData = {
            totalCount: 10000,
            records: [
                {
                    pricelistId: "0456b3ebc0a24c0ab7054ec9a5cea78e",
                    currency: "USD",
                    productId: "6d4f5835f5f8459590da29528b175ff7",
                    sale: 9,
                    list: 9.3,
                    minQuantity: 1,
                    effectiveValue: 9,
                    createdDate: "2019-09-17T10:01:41.61Z",
                    modifiedDate: "2021-03-23T15:39:15.7543998Z",
                    createdBy: "unknown",
                    modifiedBy: "admin",
                    id: "04f8a097520d4a36969d66498e10f183",
                    product: {
                        code: "41MY02",
                        name:
                            "1-1/4\" Steel Hex Flange Bolt, Grade 5, Zinc Plated Finish, 5/16\"-18 Dia/Thread Size, 50 PK",
                        catalogId: "7829d35f417e4dd98851f51322f32c23",
                        categoryId: "3db665f7c95e46c3890c4a208d8af73d",
                        outline: "3db665f7c95e46c3890c4a208d8af73d",
                        path: "Flange Bolts",
                        titularItemId: "cd2f18d2dba742dd832f45c82508f16e",
                        mainProductId: "cd2f18d2dba742dd832f45c82508f16e",
                        isBuyable: true,
                        isActive: true,
                        trackInventory: true,
                        maxQuantity: 0,
                        minQuantity: 0,
                        productType: "Physical",
                        startDate: "2018-04-27T18:08:15.54Z",
                        priority: 0,
                        imgSrc: "http://localhost:10645/assets/catalog/7829d/41MY02/41MY01.jpg",
                        images: [
                            {
                                relativeUrl: "http://localhost:10645/assets/catalog/7829d/41MY02/41MY01.jpg",
                                url: "http://localhost:10645/assets/catalog/7829d/41MY02/41MY01.jpg",
                                sortOrder: 1,
                                typeId: "Image",
                                group: "images",
                                name: "41MY01.jpg",
                                isInherited: false,
                                seoObjectType: "Image",
                                seoInfos: [
                                    {
                                        name: "41MY01.jpg",
                                        semanticUrl: "catalog/7829d/41MY02/41MY01.jpg",
                                        isActive: true
                                    }
                                ],
                                id: "95cdd400b14b44af8fe8c8fa4d56f3df"
                            }
                        ],
                        seoObjectType: "CatalogProduct",
                        isInherited: false,
                        createdDate: "2018-04-27T18:08:15.543Z",
                        modifiedDate: "2018-04-27T18:08:15.543Z",
                        createdBy: "admin",
                        modifiedBy: "admin",
                        id: "6d4f5835f5f8459590da29528b175ff7"
                    }
                },
                {
                    pricelistId: "0456b3ebc0a24c0ab7054ec9a5cea78e",
                    currency: "USD",
                    productId: "0b726c8546e24c2d81edf3cd777e6316",
                    list: 16.15,
                    minQuantity: 1,
                    effectiveValue: 16.15,
                    createdDate: "2019-09-17T10:01:41.613Z",
                    modifiedDate: "2019-09-17T10:01:41.613Z",
                    createdBy: "unknown",
                    modifiedBy: "unknown",
                    id: "0663928796074c439429bad3da05574a",
                    product: {
                        code: "22A425",
                        name: "1\" Steel Hex Flange Bolt, Grade 8, Plain Finish, 7/16\"-14 Dia/Thread Size, 25 PK",
                        catalogId: "7829d35f417e4dd98851f51322f32c23",
                        categoryId: "3db665f7c95e46c3890c4a208d8af73d",
                        outline: "3db665f7c95e46c3890c4a208d8af73d",
                        path: "Flange Bolts",
                        isBuyable: true,
                        isActive: true,
                        trackInventory: true,
                        maxQuantity: 0,
                        minQuantity: 0,
                        productType: "Physical",
                        startDate: "2018-04-27T20:10:08.797Z",
                        priority: 0,
                        imgSrc: "http://localhost:10645/assets/catalog/7829d/22A425/22A458.jpg",
                        images: [
                            {
                                relativeUrl: "http://localhost:10645/assets/catalog/7829d/22A425/22A458.jpg",
                                url: "http://localhost:10645/assets/catalog/7829d/22A425/22A458.jpg",
                                sortOrder: 1,
                                typeId: "Image",
                                group: "images",
                                name: "22A458.jpg",
                                isInherited: false,
                                seoObjectType: "Image",
                                seoInfos: [
                                    {
                                        name: "22A458.jpg",
                                        semanticUrl: "catalog/7829d/22A425/22A458.jpg",
                                        isActive: true
                                    }
                                ],
                                id: "a23ae5675f2048649c6d137d3bb645cb"
                            }
                        ],
                        seoObjectType: "CatalogProduct",
                        isInherited: false,
                        createdDate: "2018-04-27T20:10:08.797Z",
                        modifiedDate: "2020-10-22T13:13:57.6595013Z",
                        createdBy: "admin",
                        modifiedBy: "admin",
                        id: "0b726c8546e24c2d81edf3cd777e6316"
                    }
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
        //return $resource('api/pricing/prices/import', null, {
        //    preview: { method: 'PUT', url: 'api/pricing/prices/import/preview' }
        //});


        
    }])
