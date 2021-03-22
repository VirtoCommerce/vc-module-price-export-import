// Call this to register your module to main application
var moduleName = "virtoCommerce.simpleExportImportModule";

if (AppDependencies !== undefined) {
    AppDependencies.push(moduleName);
}

angular.module(moduleName, [])
    .config(['$stateProvider', '$urlRouterProvider',
        function ($stateProvider, $urlRouterProvider) {
            $stateProvider
                .state('workspace.virtoCommerceSimpleExportImportModuleState', {
                    url: '/virtoCommerce.simpleExportImportModule',
                    templateUrl: '$(Platform)/Scripts/common/templates/home.tpl.html',
                    controller: [
                        '$scope', 'platformWebApp.bladeNavigationService', function ($scope, bladeNavigationService) {
                            var newBlade = {
                                id: 'blade1',
                                controller: 'virtoCommerce.simpleExportImportModule.helloWorldController',
                                template: 'Modules/$(virtoCommerce.simpleExportImportModule)/Scripts/blades/hello-world.html',
                                isClosingDisabled: true
                            };
                            bladeNavigationService.showBlade(newBlade);
                        }
                    ]
                });
        }
    ])
    .run(['platformWebApp.mainMenuService', 'platformWebApp.widgetService', '$state',
        function (mainMenuService, widgetService, $state) {
            //Register module in main menu
            var menuItem = {
                path: 'browse/virtoCommerce.simpleExportImportModule',
                icon: 'fa fa-cube',
                title: 'VirtoCommerce.SimpleExportImportModule',
                priority: 100,
                action: function () { $state.go('workspace.virtoCommerceSimpleExportImportModuleState'); },
                permission: 'virtoCommerceSimpleExportImportModule:access'
            };
            mainMenuService.addMenuItem(menuItem);
        }
    ]);
