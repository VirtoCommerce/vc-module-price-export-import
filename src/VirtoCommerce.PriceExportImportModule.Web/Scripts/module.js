// Call this to register your module to main application
var moduleName = "virtoCommerce.priceExportImportModule";

if (AppDependencies !== undefined) {
    AppDependencies.push(moduleName);
}

angular.module(moduleName, [])
    .run(
        ['virtoCommerce.featureManagerSubscriber', 'platformWebApp.widgetService', 'platformWebApp.authService',
            function (featureManagerSubscriber, widgetService, authService) {
                featureManagerSubscriber.onLoginStatusChanged('SimpleExportImport', () => {

                    widgetService.registerWidget({
                        isVisible: function (blade) { return blade.controller === 'virtoCommerce.pricingModule.pricelistDetailController' && !blade.isNew && authService.checkPermission('pricing:read'); },
                        controller: 'virtoCommerce.priceExportImportModule.pricesWidgetController',
                        template: 'Modules/$(VirtoCommerce.PriceExportImport)/Scripts/widgets/pricesWidget.tpl.html'
                    }, 'pricelistDetail');

                });
            }
        ]);

