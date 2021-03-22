angular.module('virtoCommerce.simpleExportImportModule')
    .controller('virtoCommerce.simpleExportImportModule.helloWorldController', ['$scope', 'virtoCommerce.simpleExportImportModule.webApi', function ($scope, api) {
        var blade = $scope.blade;
        blade.title = 'VirtoCommerce.SimpleExportImportModule';

        blade.refresh = function () {
            api.get(function (data) {
                blade.title = 'virtoCommerce.simpleExportImportModule.blades.hello-world.title';
                blade.data = data.result;
                blade.isLoading = false;
            });
        };

        blade.refresh();
    }]);
