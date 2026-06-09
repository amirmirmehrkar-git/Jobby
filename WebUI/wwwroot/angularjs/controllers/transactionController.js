app.controller('transactionController', ['$scope', '$routeParams', '$rootScope', '$http', 'ajaxService', '$location', 'NgTableParams', 'Upload', '$timeout',
    function ($scope, $routeParams, $rootScope, $http, ajaxService, $location, NgTableParams, Upload, $timeout) {
        "use strict";
        var self = this;

        self.item = {
            Amount: '',
        };
        self.resetItem = function () {
            self.Id = undefined;
            self.item = {
                Amount: '',
            };
        };
        self.initializeController = function () {
            ajaxService.WhoseMenu();

            self.paymentStates = paymentState.Labels;
        };

        self.tableParams = new NgTableParams($rootScope.tableParams["transactionController_tableParams"] ||
        {
            page: 1,
            count: 15,
        },
            {
                filterDelay: 300,
                getData: function (params) {
                    $rootScope.tableParams["transactionController_tableParams"] = params._params;

                    let url = $rootScope.domain + "/api/Transaction/GetAllByUserId";

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
                Amount: self.item.Amount,
            }
            return ajaxService.ajaxPost($rootScope.domain + "/api/Transaction/AddOrUpdate", model).then(function (data) {
                if (data.status != 200)
                    return;

                var walletAmount = ajaxService.getCookie('WalletAmount');
                var a = parseInt(walletAmount) + parseInt(data.data);
                document.cookie = "WalletAmount=" + parseInt(a);
                location.reload();
            });
        };
    }]);
