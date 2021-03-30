angular.module('virtoCommerce.simpleExportImportModule')
.controller('virtoCommerce.simpleExportImportModule.importProcessingController', ['$scope',
    function ($scope) {
        var blade = $scope.blade;
        blade.isLoading = false;

        $scope.$on("new-notification-event", function (event, notification) {
            if (blade.notification && notification.id === blade.notification.id) {
                angular.copy(notification, blade.notification);
            }
        });

        blade.toolbarCommands = [{
            name: 'platform.commands.cancel',
            icon: 'fa fa-times',
            canExecuteMethod: function() {
                return blade.notification && !blade.notification.finished;
            },
            executeMethod: function() {
            }
        }];

    }]);
