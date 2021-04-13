angular.module('virtoCommerce.simpleExportImportModule')
    .controller('virtoCommerce.simpleExportImportModule.importPreviewController', ['$scope', 'virtoCommerce.simpleExportImportModule.import', '$filter', 'platformWebApp.bladeNavigationService', 'uiGridConstants', 'platformWebApp.uiGridHelper', 'platformWebApp.bladeUtils', 'platformWebApp.dialogService', '$translate', 'platformWebApp.settings', function ($scope, importResources, $filter, bladeNavigationService, uiGridConstants, uiGridHelper, bladeUtils, dialogService, $translate, settings) {
        $scope.uiGridConstants = uiGridConstants;

        var blade = $scope.blade;

        blade.importPermission = "import:access";

        blade.importStrategyTypes = {
            createNewOnly: "createOnly",
            updateExistingOnly: "updateOnly",
            createAndUpdate: "createAndUpdate"
        }

        blade.importStrategy = blade.importStrategyTypes.updateExistingOnly;

        blade.refresh = () => {
            blade.isLoading = true;

            importResources.preview({ fileUrl: blade.csvFileUrl}, (data) => {
                blade.currentEntities = data.results;
                blade.totalCount = data.totalCount;
                $scope.pageSettings.totalItems = 10;
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
                        fileUrl: blade.csvFileUrl
                    }
                    importResources.run(importDataRequest, (data) => {
                        var newBlade = {
                            id: 'simpleImportProcessing',
                            notification: data,
                            headIcon: "fa fa-download",
                            title: 'simpleExportImport.blades.import-processing.title',
                            controller: 'virtoCommerce.simpleExportImportModule.importProcessingController',
                            template: 'Modules/$(VirtoCommerce.SimpleExportImport)/Scripts/blades/import-processing.tpl.html'
                        };

                        bladeNavigationService.showBlade(newBlade, blade);
                    })
                },
                permission: blade.importPermission
            }
        ];

        // ui-grid
        $scope.setGridOptions = (gridOptions) => {
            $scope.gridOptions = gridOptions;
            bladeUtils.initializePagination($scope);
        };

    }]);
