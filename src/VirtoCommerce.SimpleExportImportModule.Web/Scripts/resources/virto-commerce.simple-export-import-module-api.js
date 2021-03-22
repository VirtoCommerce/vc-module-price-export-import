angular.module('virtoCommerce.simpleExportImportModule')
    .factory('virtoCommerce.simpleExportImportModule.webApi', ['$resource', function ($resource) {
        return $resource('api/VirtoCommerceSimpleExportImport');
}]);
