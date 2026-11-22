app.controller('blogController', ['$rootScope', '$scope', '$routeParams', '$location', 'NgTableParams', 'ajaxService', 'Upload', '$timeout',
    function ($rootScope, $scope, $routeParams, $location, NgTableParams, ajaxService, Upload, $timeout) {
        "use strict";
        var self = this;
        self.item = {
            Title: '',
            Description: '',
            FileName: '',
            FileType: '',
            SubsetId: '',
            Details: [],
        };
        this.initializeController = function () {
            ajaxService.WhoseMenu();

            try {
                var FileUploadBlogCover = document.getElementById("FileUploadBlogCover");
                var btnFileUploadBlogCover = document.getElementById("btnFileUploadBlogCover");
                btnFileUploadBlogCover.onclick = function () {
                    FileUploadBlogCover.click();
                };
            } catch { }

            try {
                var fileUpload_Image = document.getElementById("fileUpload_Image");
                var btnFileUpload_Image = document.getElementById("btnFileUpload_Image");
                btnFileUpload_Image.onclick = function () {
                    fileUpload_Image.click();
                };
            } catch { }

            self.fileType = fileType;
            self.inputType = inputType;

            if ($location.path() == '/blogs') 
                self.getAllBlogs();
        };

        self.tableParams = new NgTableParams($rootScope.tableParams["blogController_tableParams"] ||
        {
            page: 1,
            count: 15,
        },
            {
                filterDelay: 300,
                getData: function (params) {
                    $rootScope.tableParams["blogController_tableParams"] = params._params;
                    let url = $rootScope.domain + "/api/Blog/GetAll";
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
                Description: self.item.Description,
                FileName: self.item.FileName,
                Details: self.item.Details,
            }
            return ajaxService.ajaxPost($rootScope.domain + "/api/Blog/AddOrUpdate", model).then(function (data) {
                if (data.status != 200)
                    return;
                location.reload();
            });
        };
        self.addTextToDetail = function () {
            self.item.Details.push({
                Id: ajaxService.generateGUID(),
                Key: self.inputType.Text,
                Value: self.Text
            });
        };
        self.addImageToDetail = function () {
            self.item.Details.push({
                Id: ajaxService.generateGUID(),
                Key: self.inputType.Image,
                Value: self.FileName
            });
        };
        self.getRow = function (row) {
            self.Id = row.Id;
            self.item = {
                ...row
            };
        };
        self.delete = function () {
            return ajaxService.ajaxGet($rootScope.domain + "/api/Blog/Delete/" + self.Id).then(function (data) {
                if (data.status != 200)
                    return;
                location.reload();
            });
        };
        self.getAllBlogs = function () {
            return ajaxService.ajaxGet($rootScope.domain + "/api/Blog/GetAll").then(function (data) {
                if (data.status != 200)
                    return;
                self.blogs = data.data;
            });
        };
        self.deleteDetail = function (id) {
            self.item.Details = self.item.Details.filter(m => m.Id != id);
        };
        self.getById = function () {
            return ajaxService.ajaxGet($rootScope.domain + "/api/Blog/GetById/" + self.Id).then(function (data) {
                if (data.status != 200)
                    return;
                self.item = {
                    ...data.data
                };
            });
        };
        self.hideModal = function (modal) {
            $('#' + modal).modal('hide');
        };

        $scope.UploadBlogCover = function (files) {
            $scope.SelectedFiles = files;
            if ($scope.SelectedFiles && $scope.SelectedFiles.length) {
                Upload.upload({
                    url: $rootScope.domain + '/File/UploadBlogCover/',
                    data: {
                        files: $scope.SelectedFiles
                    }
                }).then(function (response) {
                    $timeout(function () {
                        $scope.Result = response.data;
                        self.item.FileName = response.data;
                        self.item.FileType = ajaxService.getFileType(response.data);
                    });
                }, function (response) {
                    if (response.status > 0) {
                        var errorMsg = response.status + ': ' + response.data;
                        alert(errorMsg);
                    }
                }, function (evt) {
                    var element = angular.element(document.querySelector('#dvProgress'));
                    $scope.Progress = Math.min(100, parseInt(100.0 * evt.loaded / evt.total));
                    element.html('<div style="width: ' + $scope.Progress * 2 + 'px">' + $scope.Progress + '%</div>');
                });
            }
        };
        $scope.UploadImage = function (files) {
            $scope.SelectedFiles = files;
            if ($scope.SelectedFiles && $scope.SelectedFiles.length) {
                Upload.upload({
                    url: $rootScope.domain + '/File/UploadBlogImage/',
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
                    var element = angular.element(document.querySelector('#dvProgress_Image'));
                    $scope.Progress = Math.min(100, parseInt(100.0 * evt.loaded / evt.total));
                    element.html('<div style="width: ' + $scope.Progress * 2 + 'px">' + $scope.Progress + '%</div>');
                });
            }
        };
    }]);


