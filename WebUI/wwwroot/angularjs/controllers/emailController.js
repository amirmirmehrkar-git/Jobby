app.controller('emailController', ['$scope', '$routeParams', '$rootScope', '$http', 'ajaxService', '$location', 'NgTableParams', 'Upload', '$timeout',
    function ($scope, $routeParams, $rootScope, $http, ajaxService, $location, NgTableParams, Upload, $timeout) {
        "use strict";
        var self = this;

        self.TotalEmails = 0;
        self.Unreads = 0;
        self.AICount = 0;

        self.emails = [
            {
                Sender: 'Sarah Johnson',
                Messeage: 'Re: Frontend Developer Position - Interview Invitation',
                SuggestTitle: 'Frontend Developer at TechCorp',
                CreateDate: '2025/11/18 06:45',
                Answer: '',
            },
            {
                Sender: 'HR Department',
                Messeage: 'Application Received - React Developer',
                SuggestTitle: 'React Developer at StartupXYZ',
                CreateDate: '2025/11/18 22:15',
                Answer: '',
            },
        ];

        self.email = {
            Sender: '',
            Messeage: '',
            SuggestTitle: '',
        };
        self.resetItem = function () {
            self.Id = undefined;
            self.email = {
                Sender: '',
                Messeage: '',
                SuggestTitle: '',
                CreateDate: '',
                Answer: '',
            };
        };
        self.initializeController = function () {
            ajaxService.WhoseMenu();
        };
        self.generateAnswerForEmail = function (row) {
            const model = {
                Messeage: self.email.Messeage,
            }
            return ajaxService.ajaxPost($rootScope.domain + "/api/Email/GenerateAnswerForEmail", model).then(function (data) {
                if (data.status != 200)
                    return;
                self.email.Answer = data.data;
            });
        }
        self.getRow = function (row) {
            self.resetItem();

            self.Id = row.Id;
            self.email.Sender = row.Sender;
            self.email.Messeage = row.Messeage;
            self.email.SuggestTitle = row.SuggestTitle;
        };
    }]);
