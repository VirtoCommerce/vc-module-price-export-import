angular.module('virtoCommerce.priceExportImportModule')
    .factory('virtoCommerce.priceExportImportModule.import', ['$resource', '$q', function ($resource, $q) {
        return $resource('api/pricing/import', null,
            {
                preview: { method: 'POST', url: 'api/pricing/import/preview'},
                run: { method: 'POST', url: 'api/pricing/import/run'},
                validate: { method: 'POST', url: 'api/pricing/import/validate' },
                cancel: { method: 'POST', url: 'api/pricing/import/cancel'}
            });

    }])
