angular.module('virtoCommerce.simpleExportImportModule')
    .controller('virtoCommerce.simpleExportImportModule.importPreviewController', ['$scope', 'virtoCommerce.simpleExportImportModule.import', '$filter', 'platformWebApp.bladeNavigationService', 'uiGridConstants', 'platformWebApp.uiGridHelper', 'platformWebApp.bladeUtils', 'platformWebApp.dialogService', '$translate', 'platformWebApp.settings', function ($scope, importResources, $filter, bladeNavigationService, uiGridConstants, uiGridHelper, bladeUtils, dialogService, $translate, settings) {
        $scope.uiGridConstants = uiGridConstants;

        var blade = $scope.blade;

        blade.importPermission = "import:access";

        blade.importStrategyTypes = {
            createNewOnly: "createNewOnly",
            updateExistingOnly: "updateExistingOnly",
            createAndUpdate: "createAndUpdate"
        }

        blade.importStrategy = blade.importStrategyTypes.updateExistingOnly;

        blade.ignoreUnknownSku = false;

        blade.refresh = function () {
            blade.isLoading = true;

            importResources.preview({fileUrl: blade.fileUrl}, function (data) {
                blade.currentEntities = data.records;
                blade.totalCount = data.totalCount;
                $scope.pageSettings.totalItems = 10;

                blade.isLoading = false;
            }, function (error) { bladeNavigationService.setError('Error ' + error.status, blade); });
        };

        blade.toolbarCommands = [
            {
                name: "platform.commands.import",
                icon: 'fa fa-download',
                canExecuteMethod: function () {
                    return true;
                },
                executeMethod: function () {
                   
                },
                permission: blade.importPermission
            },
            {
                name: "simpleExportImport.commands.upload-new",
                icon: 'fa fa-download',
                canExecuteMethod: function () {
                    return true;
                },
                executeMethod: function () {

                },
                permission: blade.importPermission
            }
        ];

        // ui-grid
        $scope.setGridOptions = function (gridOptions) {
            $scope.gridOptions = gridOptions;

            bladeUtils.initializePagination($scope);
        };

    }]);
