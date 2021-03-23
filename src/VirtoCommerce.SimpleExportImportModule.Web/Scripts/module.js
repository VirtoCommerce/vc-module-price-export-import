// Call this to register your module to main application
var moduleName = "virtoCommerce.simpleExportImportModule";

if (AppDependencies !== undefined) {
    AppDependencies.push(moduleName);
}

angular.module(moduleName, [])
    .run(
        ['platformWebApp.toolbarService',
            function (toolbarService) {
                toolbarService.register({
                    name: "Import", icon: 'fa fa-download',
                    executeMethod: function (blade) {
                        console.log('test: ' + this.name + this.icon + blade);
                    },
                    canExecuteMethod: function () { return true; },
                    index: 5
                }, 'virtoCommerce.pricingModule.pricelistItemListController'); 
            }
        ]);
    
