app.controller('documentController', ['$scope', '$routeParams', '$rootScope', '$http', 'ajaxService', '$location', 'NgTableParams', 'Upload', '$timeout',
    function ($scope, $routeParams, $rootScope, $http, ajaxService, $location, NgTableParams, Upload, $timeout) {
        "use strict";
        var self = this;

        self.item = {
            Title: '',
            FileName: '',
            FileType: '',
        };
        self.resetItem = function () {
            self.Id = undefined;
            self.item = {
                Title: '',
                FileName: '',
                FileType: '',
            };
        };
        self.initializeController = function () {
            ajaxService.WhoseMenu();

            self.fileTypes = fileType.Labels;
        };

        self.tableParams = new NgTableParams($rootScope.tableParams["documentController_tableParams"] ||
        {
            page: 1,
            count: 15,
        },
            {
                filterDelay: 300,
                getData: function (params) {
                    $rootScope.tableParams["documentController_tableParams"] = params._params;

                    let url = $rootScope.domain + "/api/Document/GetAllByUserId";

                    return ajaxService.ajaxPaged(url, params._params).then(function (data) {
                        if (data.status != 200)
                            return;
                        params.total(data.data.count);
                        return data.data.items;
                    });
                }
            });

        self.addOrUpdate = function () {
            const model = {
                Id: self.Id ? self.Id : emptyGuid,
                Title: self.item.Title,
                FileName: self.FileName,
            }
            return ajaxService.ajaxPost($rootScope.domain + "/api/Document/AddOrUpdate", model).then(function (data) {
                if (data.status != 200)
                    return;
                location.reload();
            });
        }
        self.getRow = function (row) {
            self.Id = row.Id;
            self.item.Title = row.Title;
            self.item.FileName = row.FileName;
            self.item.FileType = row.FileType;
        };
        self.delete = function () {
            return ajaxService.ajaxGet($rootScope.domain + "/api/Document/Delete/" + self.Id).then(function (data) {
                if (data.status != 200)
                    return;
                location.reload();
            });
        };
        $scope.Upload = function (files) {
            $scope.SelectedFiles = files;
            if ($scope.SelectedFiles && $scope.SelectedFiles.length) {
                Upload.upload({
                    url: $rootScope.domain + '/File/UploadDocument/',
                    data: {
                        files: $scope.SelectedFiles
                    }
                }).then(function (response) {
                    $timeout(function () {
                        $scope.Result = response.data;
                        self.FileName = response.data;
                    });
                }, function (response) {
                    if (response.status > 0) {
                        var errorMsg = response.status + ': ' + response.data;
                        alert(errorMsg);
                    }
                }, function (evt) {
                    var element = angular.element(document.querySelector('#dvProgress1'));
                    $scope.Progress = Math.min(100, parseInt(100.0 * evt.loaded / evt.total));
                    element.html('<div style="width: ' + $scope.Progress + '%">' + $scope.Progress + '%</div>');
                });
            }
        };
    }]);
