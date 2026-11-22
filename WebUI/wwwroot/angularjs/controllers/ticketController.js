app.controller('ticketController', ['$scope', '$routeParams', '$rootScope', '$http', 'ajaxService', '$location', 'NgTableParams', 'Upload', '$timeout',
    function ($scope, $routeParams, $rootScope, $http, ajaxService, $location, NgTableParams, Upload, $timeout) {
        "use strict";
        var self = this;
        self.item = {
            CreateDate: '',
            LastModifiedDate: '',
            Number: '',
            Priority: '',
            Status: '',
            Title: '',
            UserFullName: '',
            Messages: [],
            FileName: '',
            TextMessage: ''
        };
        self.resetItem = function () {
            self.Id = undefined;
            self.item = {
                CreateDate: '',
                LastModifiedDate: '',
                Number: '',
                Priority: '',
                Status: '',
                Title: '',
                UserFullName: '',
                Messages: [],
                FileName: '',
                TextMessage: ''
            };
        };
        self.initializeController = function () {
            ajaxService.WhoseMenu();

            self.ticketStatuses = ticketStatus.Labels;
            self.priorities = priority.Labels;
        };

        self.tableParams = new NgTableParams($rootScope.tableParams["ticketController_tableParams"] ||
        {
            page: 1,
            count: 15,
        },
            {
                filterDelay: 300,
                getData: function (params) {
                    $rootScope.tableParams["ticketController_tableParams"] = params._params;

                    let url = "";
                    if ($location.path().includes("/admin/tickets"))
                        url = $rootScope.domain + "/api/Ticket/GetAll";
                    else if ($location.path().includes("/user/tickets"))
                        url = $rootScope.domain + "/api/Ticket/GetAllByUserId";

                    return ajaxService.ajaxPaged(url, params._params).then(function (data) {
                        if (data.status != 200)
                            return;
                        params.total(data.data.count);
                        return data.data.items;
                    });
                }
            });

        self.addTicket = function () {
            const model = {
                Id: emptyGuid,
                UserId: '',
                Title: self.item.Title,
                Priority: self.item.Priority,
                TextMessage: self.item.TextMessage,
                FileName: self.item.FileName == undefined ? '' : self.item.FileName,
            }
            return ajaxService.ajaxPost($rootScope.domain + "/api/Ticket/AddOrUpdate", model).then(function (data) {
                if (data.status != 200)
                    return;
                location.reload();
            });
        }
        self.addOrUpdateTicketMessage = function () {
            const model = {
                Id: emptyGuid,
                UserId: '',
                TextMessage: self.item.TextMessage,
                FileName: self.FileName == undefined ? '' : self.FileName,
                TicketId: self.Id,
            }
            return ajaxService.ajaxPost($rootScope.domain + "/api/Ticket/AddOrUpdateTicketMessage", model).then(function (data) {
                if (data.status != 200)
                    return;
                location.reload();
            });
        }
        self.getRow = function (row) {
            self.Id = row.Id;
            self.getById();
        };
        self.getById = function (row) {
            return ajaxService.ajaxGet($rootScope.domain + "/api/Ticket/GetById/" + self.Id).then(function (data) {
                if (data.status != 200)
                    return;
                self.item = {
                    ...data.data
                };
            });
        };
        $scope.UploadTicketFile = function (files) {
            $scope.SelectedFiles = files;
            if ($scope.SelectedFiles && $scope.SelectedFiles.length) {
                Upload.upload({
                    url: $rootScope.domain + '/File/UploadTicketFile/',
                    data: {
                        files: $scope.SelectedFiles
                    }
                }).then(function (response) {
                    $timeout(function () {
                        $scope.Result = response.data;
                        self.item.FileName = response.data;
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

        $scope.UploadTicketMessageFile = function (files) {
            $scope.SelectedFiles = files;
            if ($scope.SelectedFiles && $scope.SelectedFiles.length) {
                Upload.upload({
                    url: $rootScope.domain + '/File/UploadTicketFile/',
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
                    var element = angular.element(document.querySelector('#dvProgress2'));
                    $scope.Progress = Math.min(100, parseInt(100.0 * evt.loaded / evt.total));
                    element.html('<div style="width: ' + $scope.Progress + '%">' + $scope.Progress + '%</div>');
                });
            }
        };
    }]);
