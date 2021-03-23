angular.module('virtoCommerce.simpleExportImportModule')
    .controller('virtoCommerce.simpleExportImportModule.importPreviewController', ['$scope', 'virtoCommerce.simpleExportImportModule.import', '$filter', 'platformWebApp.bladeNavigationService', 'uiGridConstants', 'platformWebApp.uiGridHelper', 'platformWebApp.bladeUtils', 'platformWebApp.dialogService', '$translate', 'platformWebApp.settings', function ($scope, importResources, $filter, bladeNavigationService, uiGridConstants, uiGridHelper, bladeUtils, dialogService, $translate, settings) {
        $scope.uiGridConstants = uiGridConstants;
        
        var blade = $scope.blade;

        blade.importPermission = "import:access";
       

        blade.refresh = function () {
            blade.isLoading = true;

            importResources.preview({fileUrl: blade.fileUrl}, function (data) {
                blade.currentEntities = data.records;
                $scope.pageSettings.totalItems = data.totalCount;

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

            gridOptions.onRegisterApi = function (gridApi) {
                gridApi.core.on.sortChanged($scope, function () {
                    if (!blade.isLoading) blade.refresh();
                });
            };

            bladeUtils.initializePagination($scope);
        };

        //No need to call this because page 'pageSettings.currentPage' is watched!!! It would trigger subsequent duplicated req...
        blade.refresh();
    }]);
