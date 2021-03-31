// Call this to register your module to main application
var moduleName = "virtoCommerce.simpleExportImportModule";

if (AppDependencies !== undefined) {
    AppDependencies.push(moduleName);
}

angular.module(moduleName, [])
    .run(
        ['platformWebApp.toolbarService', 'platformWebApp.bladeNavigationService', 'virtoCommerce.featureManagerSubscriber',
            function (toolbarService, bladeNavigationService, featureManagerSubscriber) {
                featureManagerSubscriber.onLoginStatusChanged('SimpleExportImport', () => {
                    toolbarService.register({
                        name: "platform.commands.import", icon: 'fa fa-download',
                        executeMethod: function (blade) {
                            var newBlade = {
                                id: 'simpleImportFileUpload',
                                title: 'simpleExportImport.blades.file-upload.title',
                                subtitle: 'simpleExportImport.blades.file-upload.subtitle',
                                priceListId: blade.currentEntityId,
                                controller: 'virtoCommerce.simpleExportImportModule.fileUploadController',
                                template: 'Modules/$(VirtoCommerce.SimpleExportImport)/Scripts/blades/file-upload.tpl.html'
                            };
                            bladeNavigationService.showBlade(newBlade, blade);
                        },
                        canExecuteMethod: function () { return true; },
                        index: 5
                    }, 'virtoCommerce.pricingModule.pricelistItemListController');
                });
            }
        ]);

