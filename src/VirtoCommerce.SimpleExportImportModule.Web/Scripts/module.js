// Call this to register your module to main application
var moduleName = "virtoCommerce.simpleExportImportModule";

if (AppDependencies !== undefined) {
    AppDependencies.push(moduleName);
}

angular.module(moduleName, [])
    .run(
        ['virtoCommerce.featureManagerSubscriber', 'platformWebApp.widgetService', 'platformWebApp.authService',
            function (featureManagerSubscriber, widgetService, authService) {
                featureManagerSubscriber.onLoginStatusChanged('SimpleExportImport', () => {

                    widgetService.registerWidget({
                        isVisible: function (blade) { return blade.controller === 'virtoCommerce.pricingModule.pricelistDetailController' && authService.checkPermission('pricing:read'); },
                        controller: 'virtoCommerce.simpleExportImportModule.pricesWidgetController',
                        template: 'Modules/$(VirtoCommerce.SimpleExportImport)/Scripts/widgets/pricesWidget.tpl.html'
                    }, 'pricelistDetail');

                });
            }
        ]);

