angular.module('virtoCommerce.simpleExportImportModule')
.controller('virtoCommerce.simpleExportImportModule.fileUploadController',
    ['FileUploader', '$document', '$scope', '$timeout', 'platformWebApp.bladeNavigationService', 'platformWebApp.assets.api',
    function(FileUploader, $document, $scope, $timeout, bladeNavigationService, assetsApi) {
        const blade = $scope.blade;
        const oneKb = 1024;
        const oneMb = 1024 * oneKb;
        const maxCsvSize = oneMb;
        blade.headIcon = 'fas fa-file-alt';
        blade.isLoading = false;
        $scope.showUploadResult = false;
        $scope.uploadedFile = {};

        blade.toolbarCommands = [{
            name: "platform.commands.cancel",
            icon: 'fa fa-times',
            executeMethod: () => {
                $scope.bladeClose();
            },
            canExecuteMethod: () => true
        }];

        function initialize () {
            let uploader = $scope.uploader = new FileUploader({
                scope: $scope,
                headers: {Accept: 'application/json'},
                url: 'api/platform/assets?folderUrl=tmp',
                method: 'POST',
                autoUpload: true,
                removeAfterUpload: true,
                filters: [
                {
                    name: 'onlyCsv',
                    fn: (item) => {
                        $scope.uploadedFile.name = item.name;
                        if (!uploader.isHTML5) {
                            return true;
                        } else {
                            let result = /^.*\.(csv)$/.test(item.name);
                            $scope.fileTypeError = !result;
                            return result;
                        }
                    }
                },
                {
                    name: 'csvMaxSize',
                    fn: (item) => {
                        $scope.uploadedFile.name = item.name;
                        if (item.size <= maxCsvSize) {
                            $scope.uploadedFile.size = formatFileSize(item.size);
                            return true;
                        } else {
                            $scope.csvMaxSizeError = true;
                            return false;
                        }
                    }
                }]
            });

            uploader.onWhenAddingFileFailed = () => {
                $scope.showUploadResult = true;
            };

            uploader.onAfterAddingAll = () => {
                bladeNavigationService.setError(null, blade);
            };

            uploader.onBeforeUploadItem = () => {
                if (blade.csvFileUrl) {
                    $scope.deleteUploadedItem();
                }

                $scope.showUploadResult = false;
                $scope.fileTypeError = false;
                $scope.csvMaxSizeError = false;
            };

            uploader.onSuccessItem = (_, asset) => {
                $scope.showUploadResult = true;
                blade.csvFileUrl = asset[0].relativeUrl;
            };

            uploader.onErrorItem = (element, response, status) => {
                bladeNavigationService.setError(`${element._file.name} failed: ${response.message ? response.message : status}`, blade);
            };

        }

        $scope.bladeClose = () => {
            if (blade.csvFileUrl) {
                $scope.deleteUploadedItem();
            }

            bladeNavigationService.closeBlade(blade);
        }

        $scope.browse = () => {
            $timeout(() => $document[0].querySelector('#selectPriceCsv').click());
        }

        $scope.deleteUploadedItem = () => {
            $scope.showUploadResult = false;
            assetsApi.remove({urls: [blade.csvFileUrl]},
                () => {},
                (error) => bladeNavigationService.setError('Error ' + error.status, blade));
            blade.csvFileUrl = null;
        }

        $scope.showPreview = () => {
            var newBlade = {
                id: 'simpleImportPreview',
                csvFileUrl: blade.csvFileUrl,
                headIcon: "fas fa-file-csv",
                title: 'simpleExportImport.blades.import-preview.title',
                subtitle: 'simpleExportImport.blades.import-preview.subtitle',
                controller: 'virtoCommerce.simpleExportImportModule.importPreviewController',
                template: 'Modules/$(VirtoCommerce.SimpleExportImport)/Scripts/blades/import-preview.tpl.html'
            };

            bladeNavigationService.showBlade(newBlade, blade);
        }

        function formatFileSize(bytes, decimals = 2) {
            if (bytes === 0) return '0 Bytes';

            const kilobyte = 1024;
            const dm = decimals < 0 ? 0 : decimals;
            const sizes = ['Bytes', 'KB', 'MB'];

            const i = Math.floor(Math.log(bytes) / Math.log(kilobyte));

            return parseFloat((bytes / Math.pow(kilobyte, i)).toFixed(dm)) + ' ' + sizes[i];
        }

        initialize();
    }]);
