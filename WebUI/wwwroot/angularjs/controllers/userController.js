app.controller('userController', ['$scope', '$routeParams', '$rootScope', '$http', 'profileService', 'ajaxService', '$location', 'NgTableParams', 'Upload', '$timeout',
    function ($scope, $routeParams, $rootScope, $http, profileService, ajaxService, $location, NgTableParams, Upload, $timeout) {
        "use strict";
        var self = this;
        self.btnDownload = false;

        var chk_EmailResponses = document.getElementById('chk_EmailResponses');

        $rootScope.user = {
            JobTitle: '',
            AboutMe: '',
        };

        self.item = {
            FirstName: '',
            LastName: '',
            Email: '',
            PhoneNumber: '',
            Password: '',
            NeededJob: '',
            LinkedInUserName: '',
            LinkedInPassword: '',
            StepStoneUserName: '',
            StepStonePassword: '',
            XingUserName: '',
            XingPassword: '',
            ApplyType: 0,
        };
        self.resetItem = function () {
            self.Id = undefined;
            self.item = {
                FirstName: '',
                LastName: '',
                Email: '',
                PhoneNumber: '',
                Password: '',
                NeededJob: '',
                LinkedInUserName: '',
                LinkedInPassword: '',
                StepStoneUserName: '',
                StepStonePassword: '',
                XingUserName: '',
                XingPassword: '',
                ApplyType: 0,
            };
        };
        self.initializeController = function () {
            ajaxService.WhoseMenu();

            self.applyTypes = applyType.Labels;

            if ($location.path().includes("/user/setting") || $location.path().includes("/user/createResume")) {
                self.getProfile();
            }

            if ($location.path().includes("/user/setting")) {
                self.getAllSettingsByUserId();
            }
        };
        self.tableParams = new NgTableParams($rootScope.tableParams["userController_tableParams"] ||
        {
            page: 1,
            count: 15,
        },
            {
                filterDelay: 300,
                getData: function (params) {
                    $rootScope.tableParams["userController_tableParams"] = params._params;

                    let url = "";
                    if ($location.path().includes("/admin/admins"))
                        url = $rootScope.domain + "/api/User/GetAllAdmins";
                    else if ($location.path().includes("/admin/users"))
                        url = $rootScope.domain + "/api/User/GetAllUsers";

                    return ajaxService.ajaxPaged(url, params._params).then(function (data) {
                        if (data.status != 200)
                            return;
                        params.total(data.data.count);
                        return data.data.items;
                    });
                }
            });

        self.tableParamsGetAllResume = new NgTableParams($rootScope.tableParams["userController_tableParams"] ||
        {
            page: 1,
            count: 15,
        },
            {
                filterDelay: 300,
                getData: function (params) {
                    $rootScope.tableParams["userController_tableParams"] = params._params;

                    let url = $rootScope.domain + "/api/Document/GetAllResumeByUserId";

                    return ajaxService.ajaxPaged(url, params._params).then(function (data) {
                        if (data.status != 200)
                            return;
                        params.total(data.data.count);
                        return data.data.items;
                    });
                }
            });

        self.addOrUpdateUser = function () {
            if (!$scope.formAddOrUpdateUser.$valid) {
                return;
            }
            const model = {
                Id: self.Id ? self.Id : emptyGuid,
                FirstName: self.item.FirstName,
                LastName: self.item.LastName,
                PhoneNumber: self.item.PhoneNumber,
                Password: self.item.Password,
                Shba: self.item.Shba,
                BankAccount: self.item.BankAccount,
                CardNumber: self.item.CardNumber,
                Active: chkActive.checked == true ? true : false,
            }
            return ajaxService.ajaxPost($rootScope.domain + "/api/User/AddOrUpdateUser", model).then(function (data) {
                if (data.status != 200)
                    return;
                location.reload();
            });
        };
        self.updateInformation = function () {
            const model = {
                JobTitle: $rootScope.user.JobTitle,
                AboutMe: $rootScope.user.AboutMe,
            }
            return ajaxService.ajaxPost($rootScope.domain + "/api/User/UpdateInformation", model).then(function (data) {
                if (data.status != 200)
                    return;
                location.reload();
            });
        };

        self.getRow = function (row) {
            self.Id = row.Id;
            self.item.FirstName = row.FirstName;
            self.item.LastName = row.LastName;
            self.item.Email = row.Email;
            self.item.PhoneNumber = row.PhoneNumber;
            self.item.Password = row.Password;
        };
        self.delete = function () {
            return ajaxService.ajaxGet($rootScope.domain + "/api/User/Delete/" + self.Id).then(function (data) {
                if (data.status != 200)
                    return;
                location.reload();
            });
        };
        self.loginWithToken = function (user) {
            $rootScope.spinnerActive = true;
            $http.get($rootScope.domain + "/api/account/loginWithToken/" + user.Id).then(function (result) {
                const data = result.data;
                if (data && data.success) {

                    document.cookie = "TokenId=" + data.TokenId;
                    document.cookie = "Token=" + data.Token;
                    document.cookie = "UserRole=" + data.role;

                    profileService.refreshProfile();

                    if (data.role == 'Admin') {
                        window.location.replace('#!/admin/dashboard');
                    }
                    else if (data.role == 'User') {
                        window.location.replace('#!/user/dashboard');
                    }
                }
                else {
                    self.modelState = data.errors;
                    $rootScope.spinnerActive = false;
                }
            }, function (response, failed) {
                $rootScope.spinnerActive = false;
                if (response.status == 400) {
                    self.modelState = response.data.ModelState.Messages;
                    return;
                }
                if (response.status != 200)
                    return;
            });
        };
        self.updateProfile = function () {
            const model = {
                FirstName: self.item.FirstName,
                LastName: self.item.LastName,
                NeededJob: self.item.NeededJob,
                PhoneNumber: self.item.PhoneNumber,
                AboutMe: self.item.AboutMe,
                ImageName: self.item.ImageName,
                LinkedInUserName: self.item.LinkedInUserName,
                LinkedInPassword: self.item.LinkedInPassword,
                StepStoneUserName: self.item.StepStoneUserName,
                StepStonePassword: self.item.StepStonePassword,
                XingUserName: self.item.XingUserName,
                XingPassword: self.item.XingPassword,
                ApplyType: self.item.ApplyType,
            }
            return ajaxService.ajaxPost($rootScope.domain + "/api/User/UpdateProfile", model).then(function (data) {
                if (data.status != 200)
                    return;
                location.reload();
            });
        };
        self.getProfile = function () {
            return ajaxService.ajaxGet($rootScope.domain + "/api/Account/GetProfile/").then(function (data) {
                if (data.status != 200)
                    return;
                self.item = {
                    ...data.data
                };

                $rootScope.user.JobTitle = self.item.NeededJob;
                $rootScope.user.AboutMe = self.item.AboutMe;
            });
        };
        self.addOrUpdateResume = function () {
            const model = {
                FileName: self.FileName,
            }
            return ajaxService.ajaxGet($rootScope.domain + "/api/Document/AddOrUpdateResume/" + self.FileName).then(function (data) {
                if (data.status != 200)
                    return;
                location.reload();
            });
        };
        self.getRow = function (row) {
            self.Id = row.Id;
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
                    url: $rootScope.domain + '/File/UploadResume/',
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
        $scope.UploadUserImage = function (files) {
            $scope.SelectedFiles = files;
            if ($scope.SelectedFiles && $scope.SelectedFiles.length) {
                Upload.upload({
                    url: $rootScope.domain + '/File/UploadProfile/',
                    data: {
                        files: $scope.SelectedFiles
                    }
                }).then(function (response) {
                    $timeout(function () {
                        $scope.Result = response.data;
                        self.item.ImageName = response.data;
                    });
                }, function (response) {
                    if (response.status > 0) {
                        var errorMsg = response.status + ': ' + response.data;
                        alert(errorMsg);
                    }
                }, function (evt) {
                });
            }
        };
        self.changePassword = function () {
            return ajaxService.ajaxGet($rootScope.domain + "/api/Account/ChangePassword/"
                + self.currentPassword + "/" + self.newPassword + "/" + self.confirmPassword).then(function (data) {
                    if (data.status != 200)
                        return;
                    location.reload();
                });
        };
        self.deleteAccount = function () {
            return ajaxService.ajaxGet($rootScope.domain + "/api/Account/DeleteAccount").then(function (data) {
                if (data.status != 200)
                    return;
                location.href = '';
            });
        };
        self.getAllSettingsByUserId = function () {
            return ajaxService.ajaxGet($rootScope.domain + "/api/Setting/GetAllSettingsByUserId").then(function (data) {
                if (data.status != 200)
                    return;
                self.userSettings = data.data;

                self.userSettings.filter(m => m.Key == 'EmailResponses')[0]
                ? chk_EmailResponses.checked = self.userSettings.filter(m => m.Key == 'EmailResponses')[0].Value
                    : chk_EmailResponses.checked = false;

                self.userSettings.filter(m => m.Key == 'ApplicationUpdates')[0]
                    ? chk_ApplicationUpdates.checked = self.userSettings.filter(m => m.Key == 'ApplicationUpdates')[0].Value
                    : chk_ApplicationUpdates.checked = false;

                self.userSettings.filter(m => m.Key == 'InterviewReminders')[0]
                    ? chk_InterviewReminders.checked = self.userSettings.filter(m => m.Key == 'InterviewReminders')[0].Value
                    : chk_InterviewReminders.checked = false;

                self.userSettings.filter(m => m.Key == 'WeeklyDigest')[0]
                    ? chk_WeeklyDigest.checked = self.userSettings.filter(m => m.Key == 'WeeklyDigest')[0].Value
                    : chk_WeeklyDigest.checked = false;

                self.userSettings.filter(m => m.Key == 'RejectionNotifications')[0]
                    ? chk_RejectionNotifications.checked = self.userSettings.filter(m => m.Key == 'RejectionNotifications')[0].Value
                    : chk_RejectionNotifications.checked = false;

                self.userSettings.filter(m => m.Key == 'SmartApplicationTiming')[0]
                    ? chk_SmartApplicationTiming.checked = self.userSettings.filter(m => m.Key == 'SmartApplicationTiming')[0].Value
                    : chk_SmartApplicationTiming.checked = false;

                self.userSettings.filter(m => m.Key == 'AutoThankYouEmails')[0]
                    ? chk_AutoThankYouEmails.checked = self.userSettings.filter(m => m.Key == 'AutoThankYouEmails')[0].Value
                    : chk_AutoThankYouEmails.checked = false;

                self.userSettings.filter(m => m.Key == 'AutoFollowUpMessages')[0]
                    ? chk_AutoFollowUpMessages.checked = self.userSettings.filter(m => m.Key == 'AutoFollowUpMessages')[0].Value
                    : chk_AutoFollowUpMessages.checked = false;

                self.userSettings.filter(m => m.Key == 'RequestFeedbackAfterRejection')[0]
                    ? chk_RequestFeedbackAfterRejection.checked = self.userSettings.filter(m => m.Key == 'RequestFeedbackAfterRejection')[0].Value
                    : chk_RequestFeedbackAfterRejection.checked = false;

                self.userSettings.filter(m => m.Key == 'WeekendApplications')[0]
                    ? chk_WeekendApplications.checked = self.userSettings.filter(m => m.Key == 'WeekendApplications')[0].Value
                    : chk_WeekendApplications.checked = false;
            });
        };
        self.addOrUpdateSetting = function (key) {
            return ajaxService.ajaxGet($rootScope.domain + "/api/Setting/AddOrUpdate/" + key).then(function (data) {
                if (data.status != 200)
                    return;
                location.reload();
            });
        };
    }]);
