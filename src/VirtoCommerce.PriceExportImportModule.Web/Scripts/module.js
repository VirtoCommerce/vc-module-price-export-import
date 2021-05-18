// Call this to register your module to main application
var moduleName = "virtoCommerce.priceExportImportModule";

if (AppDependencies !== undefined) {
    AppDependencies.push(moduleName);
}

angular.module(moduleName, [])
    .run(
        ['virtoCommerce.featureManagerSubscriber', 'platformWebApp.toolbarService', 'platformWebApp.bladeNavigationService', 'platformWebApp.dialogService', 'platformWebApp.settings',
            function (featureManagerSubscriber, toolbarService, bladeNavigationService, dialogService, settings) {
                featureManagerSubscriber.onLoginStatusChanged('PriceExportImport', () => {

                    toolbarService.register({
                        name: "platform.commands.export", icon: 'fa fa-upload',
                        executeMethod: function (blade) {
                            const csvExportDelimiter = ';';
                            const csvExportProvider = 'CsvExportProvider';
                            const csvPropertyInfos = [
                                {
                                    fullName: "Code",
                                    group: "TabularPrice",
                                    displayName: "SKU",
                                    isRequired: false,
                                },
                                {
                                    fullName: "ProductName",
                                    group: "TabularPrice",
                                    displayName: "Product Name",
                                    isRequired: false,
                                },
                                {
                                    fullName: "Currency",
                                    group: "TabularPrice",
                                    displayName: "Currency",
                                    isRequired: false,
                                },
                                {
                                    fullName: "List",
                                    group: "TabularPrice",
                                    displayName: "List price",
                                    isRequired: false,
                                },
                                {
                                    fullName: "Sale",
                                    group: "TabularPrice",
                                    displayName: "Sale price",
                                    isRequired: false,
                                },
                                {
                                    fullName: "MinQuantity",
                                    group: "TabularPrice",
                                    displayName: "Min quantity",
                                    isRequired: false,
                                },
                                {
                                    fullName: "ModifiedDate",
                                    group: "TabularPrice",
                                    displayName: "Modified",
                                    isRequired: false,
                                },
                                {
                                    fullName: "StartDate",
                                    group: "TabularPrice",
                                    displayName: "Valid from",
                                    isRequired: false,
                                },
                                {
                                    fullName: "EndDate",
                                    group: "TabularPrice",
                                    displayName: "Valid to",
                                    isRequired: false,
                                },
                                {
                                    fullName: "CreatedDate",
                                    group: "TabularPrice",
                                    displayName: "Created date",
                                    isRequired: false,
                                },
                                {
                                    fullName: "CreatedBy",
                                    group: "TabularPrice",
                                    displayName: "Created by",
                                    isRequired: false,
                                },
                                {
                                    fullName: "ModifiedBy",
                                    group: "TabularPrice",
                                    displayName: "Modified By",
                                    isRequired: false,
                                }
                            ];
                            const exportDataRequest = {
                                exportTypeName: 'VirtoCommerce.PricingModule.Data.ExportImport.ExportablePrice',
                                dataQuery: {
                                    exportTypeName: 'PriceExportDataQuery'
                                }
                            };

                            const selectedRows = blade.$scope.gridApi.selection.getSelectedRows();
                            const isAllSelected = blade.$scope.gridApi.selection.getSelectAllState() || !selectedRows.length;
                            exportDataRequest.dataQuery.isAllSelected = isAllSelected;
                            exportDataRequest.dataQuery.objectIds = [];
                            if (!isAllSelected && selectedRows) {
                                const priceIds = _.pluck(_.flatten(_.pluck(selectedRows, 'prices')), 'id');
                                exportDataRequest.dataQuery.objectIds = priceIds;
                            }

                            exportDataRequest.dataQuery.productIds = [];

                            if ((exportDataRequest.dataQuery.productIds && exportDataRequest.dataQuery.productIds.length)
                                || (!isAllSelected)) {
                                exportDataRequest.dataQuery.productIds = _.map(selectedRows, function (product) {
                                    return product.productId;
                                });
                            }

                            const searchCriteria = blade.getSearchCriteria();

                            if (isAllSelected || (searchCriteria.pricelistIds && searchCriteria.pricelistIds.length > 0) || searchCriteria.keyword !== '') {
                                exportDataRequest.dataQuery.isAnyFilterApplied = true;
                            }

                            angular.extend(exportDataRequest.dataQuery, searchCriteria);

                            const selectedItemsCount = isAllSelected ? blade.totalItems : selectedRows.length;
                            settings.getValues({ id: 'PriceExportImport.Export.LimitOfLines' }, (value) => {
                                const exportLimit = value[0];
                                const validationError = selectedItemsCount > exportLimit;
                                const dialog = {
                                    id: "priceExportDialog",
                                    exportAll: isAllSelected ? true : false,
                                    totalItemsCount: blade.totalItems,
                                    selectedItemsCount,
                                    exportLimit: exportLimit,
                                    validationError,
                                    advancedExport: function () {
                                        if (exportDataRequest.providerConfig) {
                                            delete exportDataRequest.providerConfig;
                                        }

                                        this.no();
                                        const newBlade = {
                                            id: 'priceExport',
                                            title: 'pricing.blades.exporter.priceTitle',
                                            subtitle: 'pricing.blades.exporter.priceSubtitle',
                                            controller: 'virtoCommerce.exportModule.exportSettingsController',
                                            template: 'Modules/$(VirtoCommerce.Export)/Scripts/blades/export-settings.tpl.html',
                                            exportDataRequest: exportDataRequest
                                        };
                                        bladeNavigationService.showBlade(newBlade, blade);
                                    },
                                    callback: function (confirm) {
                                        if (confirm) {
                                            exportDataRequest.providerConfig = {};
                                            exportDataRequest.providerConfig.delimiter = csvExportDelimiter;
                                            exportDataRequest.providerConfig.type = 'CsvProviderConfiguration';
                                            exportDataRequest.providerName = csvExportProvider;
                                            exportDataRequest.dataQuery.includedProperties = csvPropertyInfos;
                                            delete exportDataRequest.dataQuery.skip;
                                            delete exportDataRequest.dataQuery.take;
                                            delete exportDataRequest.dataQuery.IsPreview;
                                            blade.isExporting = true;
                                            const progressBlade = {
                                                id: 'exportProgress',
                                                title: 'export.blades.export-progress.title',
                                                controller: 'virtoCommerce.exportModule.exportProgressController',
                                                template: 'Modules/$(VirtoCommerce.Export)/Scripts/blades/export-progress.tpl.html',
                                                exportDataRequest: exportDataRequest,
                                                onCompleted: function () {
                                                    blade.isExporting = false;
                                                }
                                            };
                                            bladeNavigationService.showBlade(progressBlade, blade);
                                        }
                                    }
                                }
                                dialogService.showDialog(dialog, 'Modules/$(VirtoCommerce.PriceExportImport)/Scripts/dialogs/price-export-dialog.tpl.html', 'platformWebApp.confirmDialogController');
                            });
                        },
                        canExecuteMethod: function () { return true; },
                        index: 4
                    }, 'virtoCommerce.pricingModule.pricelistItemListController');

                    toolbarService.register({
                        name: "platform.commands.import",
                        icon: 'fa fa-download',
                        executeMethod: function (blade) {
                            const newBlade = {
                                id: 'priceImportFileUpload',
                                title: 'priceExportImport.blades.file-upload.title',
                                subtitle: 'priceExportImport.blades.file-upload.subtitle',
                                priceListId: blade.currentEntityId,
                                controller: 'virtoCommerce.priceExportImportModule.fileUploadController',
                                template: 'Modules/$(VirtoCommerce.PriceExportImport)/Scripts/blades/file-upload.tpl.html'
                            };
                            bladeNavigationService.showBlade(newBlade, blade);
                        },
                        canExecuteMethod: function () { return true; },
                        index: 5
                    }, 'virtoCommerce.pricingModule.pricelistItemListController');

                });
            }
        ]);

