angular.module('virtoCommerce.simpleExportImportModule')
.controller('virtoCommerce.simpleExportImportModule.pricesWidgetController', ['$scope', 'platformWebApp.bladeNavigationService', 'virtoCommerce.pricingModule.prices', function ($scope, bladeNavigationService, prices) {
    var blade = $scope.widget.blade;

    function refresh() {
        $scope.priceCount = '...';

        prices.search({
            priceListId: blade.currentEntityId,
            take: 0
        }, function (data) {
            $scope.priceCount = data.totalCount;
        });
    }

    $scope.openBlade = function () {
        var newBlade = {
            id: "simpleImportExportPricelistChild",
            currency: blade.currentEntity.currency,
            currentEntity: blade.currentEntity,
            currentEntityId: blade.currentEntityId,
            parentWidgetRefresh: refresh,
            title: blade.title,
            subtitle: 'pricing.blades.pricelist-item-list.subtitle',
            controller: 'virtoCommerce.simpleExportImportModule.pricelistItemListController',
            template: 'Modules/$(VirtoCommerce.SimpleExportImport)/Scripts/blades/pricelist-item-list.tpl.html'
        };

        bladeNavigationService.showBlade(newBlade, blade);
    };

    refresh();
}]);
