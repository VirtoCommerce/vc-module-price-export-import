angular.module('virtoCommerce.simpleExportImportModule')
.controller('virtoCommerce.simpleExportImportModule.fileUploadController',
    ['FileUploader', '$document', '$scope', '$timeout', 'platformWebApp.bladeNavigationService', 'platformWebApp.assets.api', 'virtoCommerce.simpleExportImportModule.import', '$translate', 'platformWebApp.settings',
        function (FileUploader, $document, $scope, $timeout, bladeNavigationService, assetsApi, importResources, $translate, settings) {
        const blade = $scope.blade;
        const oneKb = 1024;
        const oneMb = 1024 * oneKb;
        $scope.maxCsvSize = oneMb;
        blade.headIcon = 'fas fa-file-alt';
        blade.isLoading = false;
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
            resetState();

            settings.getValues({ id: 'SimpleExportImport.Import.FileMaxSize' }, (value) => {
                if (!!value) {
                    $scope.maxCsvSize = value[0] * oneMb;
                }
            });

            let uploader = $scope.uploader = new FileUploader({
                scope: $scope,
                headers: {Accept: 'application/json'},
                url: 'api/platform/assets?folderUrl=tmp',
                method: 'POST',
                autoUpload: true,
                removeAfterUpload: true,
                filters: [
                    {
                        name: 'sameFile',
                        fn: (item) => $scope.uploadedFile.name !== item.name
                    },
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
                    }, {
                        name: 'csvMaxSize',
                        fn: (item) => {
                            $scope.uploadedFile.name = item.name;
                            if (item.size <= $scope.maxCsvSize) {
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
                    $scope.tmpCsvInfo = {};
                    $scope.tmpCsvInfo.name = $scope.uploadedFile.name;
                    $scope.tmpCsvInfo.size = $scope.uploadedFile.size;
                    $scope.deleteUploadedItem();
                }
            };

            uploader.onSuccessItem = (__, asset) => {
                blade.csvFileUrl = asset[0].relativeUrl;

                if (!_.isEmpty($scope.tmpCsvInfo)) {
                    $scope.uploadedFile.name = $scope.tmpCsvInfo.name;
                    $scope.uploadedFile.size = $scope.tmpCsvInfo.size;
                    $scope.tmpCsvInfo = {};
                }

                importResources.validate({ fileUrl: blade.csvFileUrl }, (data) => {
                    $scope.csvValidationErrors = data.errors;
                    $scope.internalCsvError = !!$scope.csvValidationErrors.length;
                    $scope.showUploadResult = true;
                }, (error) => { bladeNavigationService.setError('Error ' + error.status, blade); });

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

        $scope.deleteUploadedItem = (showConfirmation) => {
            if (showConfirmation) {
                bladeNavigationService.showConfirmationIfNeeded(true, true, blade, () => { bladeNavigationService.closeChildrenBlades(blade, removeCsv); }, () => {}, "simpleExportImport.dialogs.csv-file-delete.title", "simpleExportImport.dialogs.csv-file-delete.subtitle");
            } else {
                bladeNavigationService.closeChildrenBlades(blade, removeCsv);
            }
        }

        $scope.showPreview = () => {
            var newBlade = {
                id: 'simpleImportPreview',
                csvFileUrl: blade.csvFileUrl,
                priceListId: blade.priceListId,
                headIcon: "fas fa-file-csv",
                title: 'simpleExportImport.blades.import-preview.title',
                subtitle: 'simpleExportImport.blades.import-preview.subtitle',
                controller: 'virtoCommerce.simpleExportImportModule.importPreviewController',
                template: 'Modules/$(VirtoCommerce.SimpleExportImport)/Scripts/blades/import-preview.tpl.html'
            };

            bladeNavigationService.showBlade(newBlade, blade);
        }

        $scope.translateErrorCode = (error) => {
            var translateKey = 'simpleExportImport.validation-errors.' + error.errorCode;
            var result = $translate.instant(translateKey, error.properties);
            return result === translateKey ? errorCode : result;
        }

        function removeCsv() {
            assetsApi.remove({urls: [blade.csvFileUrl]},
                () => { },
                (error) => bladeNavigationService.setError('Error ' + error.status, blade)
            );

            resetState();
        }

        function resetState() {
            $scope.uploadedFile = {};
            blade.csvFileUrl = null;

            $scope.showUploadResult = false;
            $scope.fileTypeError = false;
            $scope.csvMaxSizeError = false;
            $scope.internalCsvError = false;
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
