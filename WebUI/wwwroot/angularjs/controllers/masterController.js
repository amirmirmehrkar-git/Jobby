var appTimezone = "Asia/Tehran";
var app = angular.module('myapp', ['ngRoute', 'ngTable', 'ngFileUpload', 'ngLoadingSpinner', 'ADM-dateTimePicker', 'ngIntlTelInput', 'isteven-multi-select'])
    .config(['$rootScopeProvider', 'ADMdtpProvider', 'ngIntlTelInputProvider', function ($rootScopeProvider, ADMdtp, ngIntlTelInputProvider) {
        $rootScopeProvider.digestTtl(20);
        moment.loadPersian({ usePersianDigits: false, dialect: 'persian-modern' })
        moment.dialect = 'persian-modern';
        ADMdtp.setOptions({
            calType: 'jalali',
            format: 'YYYY/MM/DD hh:mm',
            default: '',
            zIndex: 9999,
            multiple: false,
            transition: true,
            autoClose: true,
            freezeInput: true,
            minuteStep: 5
        });
        ngIntlTelInputProvider.set({ initialCountry: 'ir', preferredCountries: ['ir', 'om'] });
    }]);
app.controller('masterController', ['$scope', '$routeParams', '$rootScope', '$http', 'profileService', 'ajaxService', '$location',
    function ($scope, $routeParams, $rootScope, $http, profileService, ajaxService, $location) {
        "use strict";
        $http.defaults.headers.common['lang'] = 'fa';
        var self = this;
        $rootScope.UserFullName = '-';
        $rootScope.UserType = '0';
        $rootScope.loadFirstPage = true;

        $rootScope.domain = '';
        //$rootScope.domain = 'https://localhost:44305';
        //$rootScope.uiDomain = 'https://localhost:44306';

        self.item = {
            Email: '',
            Password: '',
            FirstName: '',
            LastName: '',
        };
        self.initializeController = function () {
            moment.tz.add("Asia/Tehran|LMT TMT +0330 +04 +05 +0430|-3p.I -3p.I -3u -40 -50 -4u|01234325252525252525252525252525252525252525252525252525252525252525252525252525252525252525252525252|-2btDp.I 1d3c0 1huLT.I TXu 1pz0 sN0 vAu 1cL0 1dB0 1en0 pNB0 UL0 1cN0 1dz0 1cp0 1dz0 1cp0 1dz0 1cp0 1dz0 1cp0 1dz0 1cN0 1dz0 1cp0 1dz0 1cp0 1dz0 1cp0 1dz0 1cN0 1dz0 1cp0 1dz0 1cp0 1dz0 1cp0 1dz0 1cN0 1dz0 64p0 1dz0 1cN0 1dz0 1cp0 1dz0 1cp0 1dz0 1cp0 1dz0 1cN0 1dz0 1cp0 1dz0 1cp0 1dz0 1cp0 1dz0 1cN0 1dz0 1cp0 1dz0 1cp0 1dz0 1cp0 1dz0 1cN0 1dz0 1cp0 1dz0 1cp0 1dz0 1cp0 1dz0 1cN0 1dz0 1cp0 1dz0 1cp0 1dz0 1cp0 1dz0 1cp0 1dz0 1cN0 1dz0 1cp0 1dz0 1cp0 1dz0 1cp0 1dz0 1cN0 1dz0 1cp0 1dz0 1cp0 1dz0 1cp0 1dz0|14e6");
            $rootScope.tableParams = {};
            self.Id = $routeParams.id;
            self.item.Email = ajaxService.getCookie('Email');
            clearInterval(ajaxService.getCookie('timerId'));
            ajaxService.WhoseMenu();

            if ($location.path().includes("/login") && $routeParams.id != undefined
                && $routeParams.id != null && $routeParams.id != '') {
                document.cookie = "TokenId=" + $routeParams.id;
                profileService.refreshProfile();
                $location.path('user/dashboard');
            }
        };
        self.goTo = function (path) {
            $location.path(path);
        };
        this.getProfile = function () {
            return profileService.getUserProfile();
        };

        self.getRole = function () {
            return ajaxService.getCookie('RoleName');
        };

        self.signUp = function () {
            if (!$scope.formSignUp.$valid) {
                return;
            }

            //if (self.item.FirstName == '' || self.item.FirstName == undefined
            //    || self.item.LastName == '' || self.item.LastName == undefined
            //    || self.item.Email == '' || self.item.Email == undefined
            //    || self.item.Password == '' || self.item.Password == undefined) {

            //    WarningMsg("Please enter all the required information completely.");
            //    return;
            //}

            const model = {
                FirstName: self.item.FirstName,
                LastName: self.item.LastName,
                Email: self.item.Email,
                Password: self.item.Password,
            };

            document.cookie = "Email=" + self.item.Email;
            document.cookie = "Password=" + self.item.Password;

            return ajaxService.ajaxPost($rootScope.domain + "/api/Account/SignUp", model).then(function (data) {
                if (data.status != 200)
                    return;
                location.reload();
            });
        };
        self.login = function () {
            if (!$scope.form.$valid) {
                return;
            }

            const model = {
                Email: self.item.Email,
                Password: self.item.Password,
            };
            return ajaxService.ajaxPost($rootScope.domain + "/api/Account/Login", model).then(function (data) {
                if (data.status != 200)
                    return;
                if (data.data.success) {

                    document.cookie = "TokenId=" + data.data.TokenId;
                    document.cookie = "Token=" + data.data.Token;
                    document.cookie = "UserRole=" + data.data.role;

                    profileService.refreshProfile();

                    if (data.data.role == 'Admin') {
                        $location.path('admin/dashboard');
                    }
                    else if (data.data.role == 'User') {
                        $location.path('user/dashboard');
                    }
                }
            });
        };
        self.loginWithLinkedIn = function () {
            //return ajaxService.ajaxGet($rootScope.domain + "/api/Account/linkedin-login").then(function (data) {
            //    if (data.status != 200)
            //        return;
            //});
            window.location.href =
                "https://localhost:44306/api/account/linkedin-login";
        };
    }]);
function toggleMenu() {
    document.getElementById("sidebar").classList.toggle("active");
}

app.filter('persianDate', function () {
    return function (input, format = "jYYYY/jMM/jDD") {
        if (input == null)
            return '----';
        var output = moment(input).tz(appTimezone).format(format);
        return output;
    }
});
app.filter('NumberToMoney', function () {
    return function (input) {

        if (input < 1) {
            return input;
        }

        //
        var MomayezConvertedText = input;
        var doubleStatus = 0;

        for (var j = 0; j < MomayezConvertedText.length; j++) {
            if (MomayezConvertedText[j] == '.') {
                doubleStatus = (MomayezConvertedText.length - j) - 1;
            }
        }

        if (doubleStatus == 0) {
            MomayezConvertedText += ".00";
        }
        if (doubleStatus == 1) {
            MomayezConvertedText += '0';
        }
        if (doubleStatus == 2) {

        }
        else {
            for (var i = 0; i < MomayezConvertedText.length; i++) {
                if (MomayezConvertedText[i] == '.') {
                    MomayezConvertedText = MomayezConvertedText.substring(0, i + 3);
                }
            }
        }

        var Ashar = MomayezConvertedText.substring(MomayezConvertedText.length - 3, 3);
        var Rond = MomayezConvertedText.substring(0, MomayezConvertedText.length - 3);

        if (Rond.length <= 3) {
            return Rond;
        }

        var counter = 0;
        var final = "";

        for (var i = Rond.length - 1; i >= 0; i--) {
            counter++;
            final += Rond[i];
            if (counter % 3 == 0) {
                if (i != 0) {
                    final += ',';

                }
            }
        }

        var final2 = "";
        for (var i = final.length - 1; i >= 0; i--) {
            final2 += final[i];
        }
        return final2;
        ///

    };
});
app.filter('first40Chars', function () {
    return function (input) {
        if (!input) return '';
        if (input.length <= 40)
            return input;
        return input.substring(0, 40) + '...';
    };
});