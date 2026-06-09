app.config(['$routeProvider', function ($routeProvider) {
    $routeProvider
        .when("/", {
            templateUrl: "/angularjs/templates/index.html?v=0.0.3"
        })
        .when("/index", {
            templateUrl: "/angularjs/templates/index.html?v=0.0.3"
        })
        .when("/signUp", {
            templateUrl: "/angularjs/templates/signUp.html?v=0.0.3"
        })
        .when("/login", {
            templateUrl: "/angularjs/templates/login.html?v=0.0.3"
        }).when("/login/:id", {
            templateUrl: "/angularjs/templates/login.html?v=0.0.3"
        })
        .when("/blogs", {
            templateUrl: "/angularjs/templates/blogs.html?v=0.0.3"
        })
       

        //Start_Admin
        .when("/admin/dashboard", {
            templateUrl: "/angularjs/templates/admin/dashboard.html?v=0.0.3"
        })
        .when("/admin/users", {
            templateUrl: "/angularjs/templates/admin/users.html?v=0.0.3"
        })
        .when("/admin/tickets", {
            templateUrl: "/angularjs/templates/admin/tickets.html?v=0.0.3"
        })
        .when("/admin/blogs", {
            templateUrl: "/angularjs/templates/admin/blogs.html?v=0.0.3"
        })
        .when("/admin/setting", {
            templateUrl: "/angularjs/templates/admin/setting.html?v=0.0.3"
        })
        ///End_Admin

        //Start_User
        .when("/user/dashboard", {
            templateUrl: "/angularjs/templates/user/dashboard.html?v=0.0.3"
        })
        .when("/user/smartUpload", {
            templateUrl: "/angularjs/templates/user/smartUpload.html?v=0.0.3"
        })
         .when("/user/createResume", {
            templateUrl: "/angularjs/templates/user/createResume.html?v=0.0.3"
        })
        .when("/user/setting", {
            templateUrl: "/angularjs/templates/user/setting.html?v=0.0.3"
        })
        .when("/user/jobTracker", {
            templateUrl: "/angularjs/templates/user/jobTracker.html?v=0.0.3"
        })
        .when("/user/tickets", {
            templateUrl: "/angularjs/templates/user/tickets.html?v=0.0.3"
        })
        .when("/user/profile", {
            templateUrl: "/angularjs/templates/user/profile.html?v=0.0.3"
        })
        .when("/user/skills", {
            templateUrl: "/angularjs/templates/user/skills.html?v=0.0.3"
        })
        .when("/user/experiences", {
            templateUrl: "/angularjs/templates/user/experiences.html?v=0.0.3"
        })
        .when("/user/educations", {
            templateUrl: "/angularjs/templates/user/educations.html?v=0.0.3"
        })
        .when("/user/certificates", {
            templateUrl: "/angularjs/templates/user/certificates.html?v=0.0.3"
        })
        .when("/user/documents", {
            templateUrl: "/angularjs/templates/user/documents.html?v=0.0.3"
        })
        .when("/user/transactions", {
            templateUrl: "/angularjs/templates/user/transactions.html?v=0.0.3"
        })
        .when("/user/emailCenter", {
            templateUrl: "/angularjs/templates/user/emailCenter.html?v=0.0.3"
        })
        .when("/user/chooseArea", {
            templateUrl: "/angularjs/templates/user/chooseArea.html?v=0.0.3"
        })
    ///End_User
}]);