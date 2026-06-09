app.service('profileService', ['$rootScope', '$http', '$routeParams', '$location', 'ajaxService', function ($rootScope, $http, $routeParams, $location, ajaxService) {
    "use strict";
    var self = this;
    this.$routeParams = $routeParams;
    this.$location = $location;
    this.userProfile = {};
    this.userTenants = [];
    this.refreshProfile = function () {
        return ajaxService.ajaxGet($rootScope.domain + "/api/Account/GetProfile/")
            .then(function (data) {
                if (data.status != 200)
                    return;
                self.userProfile = data.data;
                $rootScope.userProfile = self.userProfile;
                document.cookie = "UserFullName=" + self.userProfile.FullName;
                document.cookie = "UserPhoneNumber=" + self.userProfile.PhoneNumber;
                document.cookie = "UserEmail=" + self.userProfile.Email;
                document.cookie = "UserImageName=" + self.userProfile.ImageName;
                document.cookie = "WalletAmount=" + self.userProfile.WalletAmount;
                location.reload();
                return self.userProfile;
            });
    };
    this.getUserProfile = function () {
        if (self.userProfile != null)
            return self.userProfile;
        else
            return {}
    }
}]);
