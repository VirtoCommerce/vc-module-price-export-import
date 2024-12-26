angular.module('virtoCommerce.priceExportImportModule')
    .controller('virtoCommerce.priceExportImportModule.importPreviewController', ['$scope', 'virtoCommerce.priceExportImportModule.import', '$filter', 'platformWebApp.bladeNavigationService', 'uiGridConstants', 'platformWebApp.uiGridHelper', 'platformWebApp.bladeUtils', 'platformWebApp.dialogService', '$translate', 'platformWebApp.settings', function ($scope, importResources, $filter, bladeNavigationService, uiGridConstants, uiGridHelper, bladeUtils, dialogService, $translate, settings) {
        $scope.uiGridConstants = uiGridConstants;

        var blade = $scope.blade;

        blade.importStrategyTypes = {
            createNewOnly: "createOnly",
            updateExistingOnly: "updateOnly",
            createAndUpdate: "createAndUpdate"
        }

        blade.importStrategy = blade.importStrategyTypes.updateExistingOnly;

        blade.refresh = () => {
            blade.isLoading = true;
            $scope.showUnparsedRowsWarning = false;

            importResources.preview({ filePath: blade.csvFilePath}, (data) => {
                blade.currentEntities = data.results;
                blade.totalCount = data.totalCount;
                $scope.pageSettings.totalItems = Math.min(10, blade.totalCount);
                getInvalidRowsCount();
                blade.isLoading = false;
            }, (error) => { bladeNavigationService.setError('Error ' + error.status, blade); });
        };

        blade.toolbarCommands = [
            {
                name: "platform.commands.import",
                icon: 'fa fa-download',
                canExecuteMethod: () => true ,
                executeMethod: () => {
                    const importDataRequest = {
                        pricelistId: blade.priceListId,
                        importMode: blade.importStrategy,
                        filePath: blade.csvFilePath
                    }
                    importResources.run(importDataRequest,
                        (data) => {
                            var newBlade = {
                                id: 'priceImportProcessing',
                                notification: data,
                                headIcon: "fa fa-download",
                                title: 'priceExportImport.blades.import-processing.title',
                                controller: 'virtoCommerce.priceExportImportModule.importProcessingController',
                                template:
                                    'Modules/$(VirtoCommerce.PriceExportImport)/Scripts/blades/import-processing.tpl.html'
                            };

                            bladeNavigationService.showBlade(newBlade, blade);
                        }
                    );
                },
            },
            {
                name: "priceExportImport.blades.import-preview.upload-new",
                icon: 'fa fa-download',
                canExecuteMethod: () => true,
                executeMethod: () => {
                    bladeNavigationService.closeBlade(blade);
                }
            }
        ];

        // ui-grid
        $scope.setGridOptions = (gridOptions) => {
            $scope.gridOptions = gridOptions;
            bladeUtils.initializePagination($scope);
        };

        function getInvalidRowsCount() {
            $scope.previewCount = _.min([blade.totalCount, $scope.pageSettings.totalItems]);

            if (blade.currentEntities.length < $scope.previewCount) {
                $scope.unparsedRowsCount = $scope.previewCount - blade.currentEntities.length;
                $scope.showUnparsedRowsWarning = true;
            }
        }

    }]);
