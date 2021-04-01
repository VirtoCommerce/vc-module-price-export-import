angular.module('virtoCommerce.simpleExportImportModule')
    .factory('virtoCommerce.simpleExportImportModule.import', ['$resource', '$q', function ($resource, $q) {
        return $resource('api/pricing/import', null,
            {
                preview: { method: 'POST', url: 'api/pricing/import/preview'},
                run: { method: 'POST', url: 'api/pricing/import/run'},
                cancel: { method: 'POST', url: 'api/pricing/import/cancel'}
            });

    }])
