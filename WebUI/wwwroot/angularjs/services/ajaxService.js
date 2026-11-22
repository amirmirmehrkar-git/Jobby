var ajaxServiceCancellers = {};
app.service('ajaxService', ['$rootScope', '$http', '$routeParams', '$location', '$q',
    function ($rootScope, $http, $routeParams, $location, $q) {
        "use strict";
        Object.keys(ajaxServiceCancellers).map(function (canceller) {
            canceller.resolve("user cancelled");
        })
        this.$routeParams = $routeParams;
        this.$location = $location;
        $http.defaults.headers.common['lang'] = 'fa';
        this.getCookie = function (cname) {
            let name = cname + "=";
            let decodedCookie = decodeURIComponent(document.cookie);
            let ca = decodedCookie.split(';');
            for (let i = 0; i < ca.length; i++) {
                let c = ca[i];
                while (c.charAt(0) == ' ') {
                    c = c.substring(1);
                }
                if (c.indexOf(name) == 0) {
                    return c.substring(name.length, c.length);
                }
            }
            return "";
        };
        this.generateGUID = function () {
            var d = new Date().getTime();
            var uuid = 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, function (c) {
                var r = (d + Math.random() * 16) % 16 | 0;
                d = Math.floor(d / 16);
                return (c == 'x' ? r : (r & 0x3 | 0x8)).toString(16);
            });
            return uuid;
        };
        this.ToRial = function (str) {
            //str = str.replace(/\,/g, '');
            var objRegex = new RegExp('(-?[0-9]+)([0-9]{3})');
            while (objRegex.test(str)) {
                str = str.replace(objRegex, '$1,$2');
            }
            return str;
        };
        this.ajaxPost = function (route, data, successFunction, errorFunction) {
            $http.defaults.headers.common['Token'] = this.getCookie('Token');
            $http.defaults.headers.common['TokenId'] = this.getCookie('TokenId');
            return $http.post(route, data).then(function (data) {
                return data;
            }, function (response, failed) {
                console.log(response)
                if (response.status == 400 && response.data && response.data.Messages) {
                    response.data.Messages.forEach(function (msg) {
                        WarningMsg(msg);
                    });
                } else if (response.status == 401) {
                    ErrorMsg("اعتبار این نشست منقضی شده است، لطفا دوباره وارد شوید!");
                    self.authorize(self.$location.$path);
                }
                else if (response.status == 403) {
                    WarningMsg("You dont have permission to do this action");
                } else if (response.status == 406)
                    WarningMsg("دو درخواست یکسان دریافت شد!");
                else
                    ErrorMsg("The Operation has been failed.");
                return response;
            });
        }
        this.ajaxGet = function (route, successFunction, errorFunction) {
            $http.defaults.headers.common['Token'] = this.getCookie('Token');
            $http.defaults.headers.common['TokenId'] = this.getCookie('TokenId');
            self = this;
            return $http.get(route).then(function (data) {
                return data;
            }, function (response) {
                console.log(response)
                if (response.status == 400 && response.data && response.data.Messages) {
                    response.data.Messages.forEach(function (msg) {
                        WarningMsg(msg);
                    });
                }
                else if (response.status == 401) {
                    ErrorMsg("اعتبار این نشست منقضی شده است، لطفا دوباره وارد شوید!");
                    self.authorize(self.$location.$path);
                } else if (response.status == 403) {
                    WarningMsg("You dont have permission to do this action");
                } else if (response.status == 406) {
                    WarningMsg("دو درخواست یکسان دریافت شد!");
                }
                else
                    ErrorMsg("The Operation has been failed.");
                return response;
            });
        }
        this.ajaxPaged = function (route, params, successFunction, errorFunction) {
            $http.defaults.headers.common['Token'] = this.getCookie('Token');
            $http.defaults.headers.common['TokenId'] = this.getCookie('TokenId');
            let mapParams = params ? {
                pageSize: params.count,
                pageCount: params.page,
                sorting: params.sorting,
                filter: {}
            } : {
                pageSize: 100,
                pageCount: 1,
                sorting: {},
                filter: {}
            }
            if (mapParams && params)
                Object.keys(params.filter).forEach((val) => {
                    mapParams.filter[val] = [];
                    if (Array.isArray(params.filter[val]))
                        mapParams.filter[val] = params.filter[val];
                    else
                        mapParams.filter[val].push(params.filter[val]);
                });

            self = this;
            if (ajaxServiceCancellers[route]) {
                ajaxServiceCancellers[route].resolve("user cancelled");
            }
            ajaxServiceCancellers[route] = $q.defer();
            return $http.post(route, mapParams, { timeout: ajaxServiceCancellers[route].promise }).then(function (data) {
                return data;
            }, function (response) {
                console.log(response)
                if (response.status == 400 && response.data && response.data.Messages) {
                    response.data.Messages.forEach(function (msg) {
                        WarningMsg(msg);
                    });
                }
                else if (response.status == 401) {
                    ErrorMsg("اعتبار این نشست منقضی شده است، لطفا دوباره وارد شوید!");
                    self.authorize(self.$location.$path);
                } else if (response.status == 403) {
                    WarningMsg("You dont have permission to do this action");
                }
                else if (response.status == 406) {
                    WarningMsg("دو درخواست یکسان دریافت شد!");
                } else if (response.status != -1)
                    ErrorMsg("The Operation has been failed.");
                return response;
            });
        }
        this.authorize = function () {
            location.href = '/account?ReturnUrl=' + location.pathname;
        };
        this.showHideElement = function (id, displayName) {
            var elem = document.getElementById(id);
            if (elem != null) {
                elem.style.display = displayName;
            }
        };
        this.addOrRemoveClass = function (id, className, added) {
            var elem = document.getElementById(id);
            if (elem != null) {
                if (added) 
                    elem.classList.add(className);
                else 
                    elem.classList.remove(className);
            }
        };
        this.showAdminPage = function () {
            this.showHideElement("publicMenu", 'none');
            this.showHideElement("userMenu", 'none');
            this.showHideElement("adminMenu", 'block');
            this.showHideElement("mainFooter", 'none');

            this.addOrRemoveClass("publicMenu", 'd-flex', false);
            this.addOrRemoveClass("userMenu", 'd-flex', false);
            this.addOrRemoveClass("adminMenu", 'd-flex', true);
        };
        this.showUserPage = function () {
            this.showHideElement("publicMenu", 'none');
            this.showHideElement("userMenu", 'block');
            this.showHideElement("adminMenu", 'none');
            this.showHideElement("mainFooter", 'none');

            this.addOrRemoveClass("publicMenu", 'd-flex', false);
            this.addOrRemoveClass("userMenu", 'd-flex', true);
            this.addOrRemoveClass("adminMenu", 'd-flex', false);
        };
        this.showPublicPages = function () {
            this.showHideElement("publicMenu", 'block');
            this.showHideElement("userMenu", 'none');
            this.showHideElement("adminMenu", 'none');
            this.showHideElement("mainFooter", 'block');

            this.addOrRemoveClass("publicMenu", 'd-flex',true);
            this.addOrRemoveClass("userMenu", 'd-flex', false);
            this.addOrRemoveClass("adminMenu", 'd-flex', false);
        };
        this.WhoseMenu = function () {
            this.changeSection();

            if ($location.path().includes("/admin")) {
                this.showAdminPage();
            }
            else if ($location.path().includes("/user")) {
                this.showUserPage();
            }
            else {
                this.showPublicPages();
            }

            try {
                if (document.getElementById("sidebar").classList.contains("active")) {
                    document.getElementById("sidebar").classList.toggle("active");
                }
            } catch { }
        };

        $rootScope.selectedDashboard = false;
        $rootScope.selectedSkills = false;
        $rootScope.selectedExperiences = false;
        $rootScope.selectedEducations = false;
        $rootScope.selectedCertificate = false;
        $rootScope.selectedDocuments = false;
        $rootScope.selectedJobTracker = false;
        $rootScope.selectedEmailCenter = false;
        $rootScope.selectedTicket = false;
        $rootScope.selectedSetting = false;
        $rootScope.selectedTransactions = false;

        $rootScope.adminSelectedDashboard = false;
        $rootScope.adminSelectedUsers = false;
        $rootScope.adminSelectedTicket = false;
        $rootScope.adminSelectedSetting = false;
        $rootScope.adminSelectedBlogs = false;

        this.changeSection = function () {
            if ($location.path().includes("/admin/dashboard")) {
                $rootScope.adminSelectedDashboard = true;
                $rootScope.adminSelectedUsers = false;
                $rootScope.adminSelectedTicket = false;
                $rootScope.adminSelectedSetting = false;
                $rootScope.adminSelectedBlogs = false;
            }
            else if ($location.path().includes("/admin/users")) {
                $rootScope.adminSelectedDashboard = false;
                $rootScope.adminSelectedUsers = true;
                $rootScope.adminSelectedTicket = false;
                $rootScope.adminSelectedSetting = false;
                $rootScope.adminSelectedBlogs = false;
            }
            else if ($location.path().includes("/admin/blogs")) {
                $rootScope.adminSelectedDashboard = false;
                $rootScope.adminSelectedUsers = false;
                $rootScope.adminSelectedTicket = false;
                $rootScope.adminSelectedSetting = false;
                $rootScope.adminSelectedBlogs = true;
            }
            else if ($location.path().includes("/admin/setting")) {
                $rootScope.adminSelectedDashboard = false;
                $rootScope.adminSelectedUsers = false;
                $rootScope.adminSelectedTicket = false;
                $rootScope.adminSelectedSetting = true;
                $rootScope.adminSelectedBlogs = false;
            }
            else if ($location.path().includes("/admin/tickets")) {
                $rootScope.adminSelectedDashboard = false;
                $rootScope.adminSelectedUsers = false;
                $rootScope.adminSelectedTicket = true;
                $rootScope.adminSelectedSetting = false;
                $rootScope.adminSelectedBlogs = false;
            }
            else if ($location.path().includes("/user/dashboard")) {
                $rootScope.selectedDashboard = true;

                $rootScope.selectedSkills = false;
                $rootScope.selectedExperiences = false;
                $rootScope.selectedEducations = false;
                $rootScope.selectedCertificate = false;
                $rootScope.selectedDocuments = false;

                $rootScope.selectedJobTracker = false;
                $rootScope.selectedTicket = false;
                $rootScope.selectedSetting = false;
                $rootScope.selectedTransactions = false;
                $rootScope.selectedEmailCenter = false;
            }
            else if ($location.path().includes("/user/skills")) {
                $rootScope.selectedDashboard = false;

                $rootScope.selectedSkills = true;
                $rootScope.selectedExperiences = false;
                $rootScope.selectedEducations = false;
                $rootScope.selectedCertificate = false;
                $rootScope.selectedDocuments = false;

                $rootScope.selectedJobTracker = false;
                $rootScope.selectedTicket = false;
                $rootScope.selectedSetting = false;
                $rootScope.selectedTransactions = false;
                $rootScope.selectedEmailCenter = false;
            }
            else if ($location.path().includes("/user/experiences")) {
                $rootScope.selectedDashboard = false;

                $rootScope.selectedSkills = false;
                $rootScope.selectedExperiences = true;
                $rootScope.selectedEducations = false;
                $rootScope.selectedCertificate = false;
                $rootScope.selectedDocuments = false;

                $rootScope.selectedJobTracker = false;
                $rootScope.selectedTicket = false;
                $rootScope.selectedSetting = false;
                $rootScope.selectedTransactions = false;
                $rootScope.selectedEmailCenter = false;
            }
            else if ($location.path().includes("/user/educations")) {
                $rootScope.selectedDashboard = false;

                $rootScope.selectedSkills = false;
                $rootScope.selectedExperiences = false;
                $rootScope.selectedEducations = true;
                $rootScope.selectedCertificate = false;
                $rootScope.selectedDocuments = false;

                $rootScope.selectedJobTracker = false;
                $rootScope.selectedTicket = false;
                $rootScope.selectedSetting = false;
                $rootScope.selectedTransactions = false;
                $rootScope.selectedEmailCenter = false;
            }
            else if ($location.path().includes("/user/certificates")) {
                $rootScope.selectedDashboard = false;

                $rootScope.selectedSkills = false;
                $rootScope.selectedExperiences = false;
                $rootScope.selectedEducations = false;
                $rootScope.selectedCertificate = true;
                $rootScope.selectedDocuments = false;

                $rootScope.selectedJobTracker = false;
                $rootScope.selectedTicket = false;
                $rootScope.selectedSetting = false;
                $rootScope.selectedTransactions = false;
                $rootScope.selectedEmailCenter = false;
            }
            else if ($location.path().includes("/user/documents")) {
                $rootScope.selectedDashboard = false;

                $rootScope.selectedSkills = false;
                $rootScope.selectedExperiences = false;
                $rootScope.selectedEducations = false;
                $rootScope.selectedCertificate = false;
                $rootScope.selectedDocuments = true;

                $rootScope.selectedJobTracker = false;
                $rootScope.selectedTicket = false;
                $rootScope.selectedSetting = false;
                $rootScope.selectedTransactions = false;
                $rootScope.selectedEmailCenter = false;
            }

            else if ($location.path().includes("/user/jobTracker")) {
                $rootScope.selectedDashboard = false;

                $rootScope.selectedSkills = false;
                $rootScope.selectedExperiences = false;
                $rootScope.selectedEducations = false;
                $rootScope.selectedCertificate = false;
                $rootScope.selectedDocuments = false;

                $rootScope.selectedJobTracker = true;
                $rootScope.selectedTicket = false;
                $rootScope.selectedSetting = false;
                $rootScope.selectedTransactions = false;
                $rootScope.selectedEmailCenter = false;
            }
            else if ($location.path().includes("/user/tickets")) {
                $rootScope.selectedDashboard = false;

                $rootScope.selectedSkills = false;
                $rootScope.selectedExperiences = false;
                $rootScope.selectedEducations = false;
                $rootScope.selectedCertificate = false;
                $rootScope.selectedDocuments = false;

                $rootScope.selectedJobTracker = false;
                $rootScope.selectedTicket = true;
                $rootScope.selectedSetting = false;
                $rootScope.selectedTransactions = false;
                $rootScope.selectedEmailCenter = false;
            }
            else if ($location.path().includes("/user/setting")) {
                $rootScope.selectedDashboard = false;

                $rootScope.selectedSkills = false;
                $rootScope.selectedExperiences = false;
                $rootScope.selectedEducations = false;
                $rootScope.selectedCertificate = false;
                $rootScope.selectedDocuments = false;

                $rootScope.selectedJobTracker = false;
                $rootScope.selectedTicket = false;
                $rootScope.selectedSetting = true;
                $rootScope.selectedTransactions = false;
                $rootScope.selectedEmailCenter = false;
            }
            else if ($location.path().includes("/user/transactions")) {
                $rootScope.selectedDashboard = false;

                $rootScope.selectedSkills = false;
                $rootScope.selectedExperiences = false;
                $rootScope.selectedEducations = false;
                $rootScope.selectedCertificate = false;
                $rootScope.selectedDocuments = false;

                $rootScope.selectedJobTracker = false;
                $rootScope.selectedTicket = false;
                $rootScope.selectedSetting = false;
                $rootScope.selectedTransactions = true;
                $rootScope.selectedEmailCenter = false;
            }
            else if ($location.path().includes("/user/emailCenter")) {
                $rootScope.selectedDashboard = false;

                $rootScope.selectedSkills = false;
                $rootScope.selectedExperiences = false;
                $rootScope.selectedEducations = false;
                $rootScope.selectedCertificate = false;
                $rootScope.selectedDocuments = false;

                $rootScope.selectedJobTracker = false;
                $rootScope.selectedTicket = false;
                $rootScope.selectedSetting = false;
                $rootScope.selectedTransactions = false;
                $rootScope.selectedEmailCenter = true;
            }
        }
    }]);

