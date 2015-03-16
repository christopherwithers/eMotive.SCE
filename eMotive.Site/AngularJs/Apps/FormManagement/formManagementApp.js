var formManagementApp = angular.module('formManagementApp', ['ngRoute', 'formServices', 'mgcrea.ngStrap', 'ngSanitize', 'ui.sortable'/*,'httpPreConfig'*/]);

formManagementApp.config(function ($routeProvider, $sceDelegateProvider) {
    var dir = siteRoot + "AngularJs/Apps/FormManagement/Templates/";

    $routeProvider.when('/Home', {
        controller: 'HomeController',
        templateUrl: dir + 'Home.html'
    }).
        when('/Form/Create', {
            controller: 'FormCreateController',
            templateUrl: dir + 'Pages/Form/Create.html'
        }).
        when('/Form/Edit', {
            controller: 'FormEditController',
            templateUrl: dir + 'Pages/Form/Edit.html'
        }).
        when('/Collection/Create', {
            controller: 'CollectionCreateController',
            templateUrl: dir + 'Pages/Collection/Create.html'
        }).
        when('/Collection/Edit', {
            controller: 'CollectionEditController',
            templateUrl: dir + 'Pages/Collection/Edit.html'
        }).
    otherwise({
        redirectTo: '/Home'
    });

    $sceDelegateProvider.resourceUrlWhitelist(['^(?:http(?:s)?:\/\/)?(?:[^\.]+\.)?\(vimeo|youtube)\.com(/.*)?$', 'self']);
});



formManagementApp.controller("HomeController", function ($scope, $formServices, $location/*, httpPreConfig*/) {//look into this httpPreConfig! Can we push it to directive?

    var tab = $location.search()["Tab"];

    $formServices.getAllForms().then(function (data) {
        $scope.forms = data.Result;
    });

    $formServices.getAllFormLists().then(function (data) {
        $scope.dropDownLists = data.Result;
    });

    $scope.tabs = [
      {
          "title": "Forms",
          "template": siteRoot + "AngularJs/Apps/FormManagement/Templates/Tabs/FormHome.html"
      },
      {
          "title": "Lists",
          "template": siteRoot + "AngularJs/Apps/FormManagement/Templates/Tabs/FormCollectionHome.html"
      }
    ];

    $scope.tabs.activeTab = tab || 0;
});

formManagementApp.controller("FormCreateController", function ($scope, $formServices, $location) {
    $scope.tab = $location.search()["Tab"];

    $formServices.newForm().then(function (data) {
        $scope.form = data.Result;

    });

    $formServices.getAllFormTypes().then(function (data) {
        $scope.formTypes = data.Result;
    });

    $formServices.getAllFormLists().then(function (data) {
        $scope.lists = data.Result;
    });

    $scope.sortableOptions = {
        stop: function (e, ui) {

            var order = 1;
            angular.forEach($scope.form.Fields, function (value, key) {
                value.Order = order;
                order++;
            });

        },
        axis: 'y',
        helper: function (e, tr) {
            var $originals = tr.children();
            var $helper = tr.clone();
            $helper.children().each(function (index) {
                // Set helper cell sizes to match the original sizes
                $(this).width($originals.eq(index).width());
            });
            return $helper;
        }
    };

    $scope.saveForm = function () {

        $formServices.createForm($scope.form).then(function (data) {
            if (data.Success) {
                $location.path('/Home').replace();
            } else {
                $scope.errors = data.Errors[0]; //todo: need to break the string of errors into a UL or similar?
            }
        });
    };

    var findHighest = function (values) {
        var highestFound = false;
        var highest = 0;

        angular.forEach(values, function (value, key) {
            if (value.Order > highest || highestFound === false) {
                highest = value.Order;
                highestFound = true;
            }
        });

        return highest;
    }

    $scope.checkForSelect = function (type, index) {
        if (type != "DropDownList") {
            $scope.form.Fields[index].ListID = 0;
        }
    };

    $scope.addItem = function (text) {
        $scope.form.Fields.push({
            Field: '',
            FormId: $scope.form.ID,
            ID: 0,
            ListID: 0,
            Order: findHighest($scope.form.Fields) + 1,
            Type: 'Text'
        });
    };

    $scope.removeItem = function (index) {
        $scope.form.Fields.splice(index, 1);

        var order = 1;
        angular.forEach($scope.form.Fields, function (value, key) {
            value.Order = order;
            order++;
        });

    };

});

formManagementApp.controller("FormEditController", function ($scope, $formServices, $location) {

    $scope.tab = $location.search()["Tab"];

    $formServices.getForms($location.search()["Id"]).then(function (data) {
        $scope.form = data.Result[0];
    });
    $formServices.getAllFormTypes().then(function (data) {
        $scope.formTypes = data.Result;
    });

    $formServices.getAllFormLists().then(function (data) {
        $scope.lists = data.Result;
    });

    $scope.sortableOptions = {
        stop: function (e, ui) {

            var order = 1;
            angular.forEach($scope.form.Fields, function (value, key) {
                value.Order = order;
                order++;
            });
            
        },
        axis: 'y',
        helper: function (e, tr) {
            var $originals = tr.children();
            var $helper = tr.clone();
            $helper.children().each(function (index) {
                // Set helper cell sizes to match the original sizes
                $(this).width($originals.eq(index).width());
            });
            return $helper;
        }
    };

    $scope.saveForm = function () {

        $formServices.updateForm($scope.form).then(function (data) {
            if (data.Success) {
                $location.path('/Home').replace();
            } else {
                $scope.errors = data.Errors[0]; //todo: need to break the string of errors into a UL or similar?
            }
        });
    };

    var findHighest = function (values) {
        var highestFound = false;
        var highest = 0;

        angular.forEach(values, function (value, key) {
            if (value.Order > highest || highestFound === false) {
                highest = value.Order;
                highestFound = true;
            }
        });

        return highest;
    }

    $scope.checkForSelect = function (type, index) {
        if (type != "DropDownList") {
            $scope.form.Fields[index].ListID = 0;
        }
    };

    $scope.addItem = function (text) {
        $scope.form.Fields.push({
            Field: '',
            FormId: $scope.form.ID,
            ID: 0,
            ListID: 0,
            Order: findHighest($scope.form.Fields) + 1,
            Type: 'Text'
        });
    };

    $scope.removeItem = function (index) {
        $scope.form.Fields.splice(index, 1);

        var order = 1;
        angular.forEach($scope.form.Fields, function (value, key) {
            value.Order = order;
            order++;
        });

    };


});

formManagementApp.controller("CollectionCreateController", function ($scope, $formServices, $location) {
    $scope.tab = $location.search()["Tab"];

    $formServices.newFormList().then(function (data) {
        $scope.dropDownList = data.Result;

    });


    $scope.saveDropDown = function () {
        $formServices.createFormList($scope.dropDownList).then(function (data) {
            if (data.Success) {
                $location.path('/Home').replace();
            } else {
                $scope.errors = data.Errors[0]; //todo: need to break the string of errors into a UL or similar?
            }
        });
    };


    $scope.addItem = function (text) {
        $scope.dropDownList.Collection.push({
            Value: 0,
            Text: ''
        });
    };

    $scope.removeItem = function (index) {
        $scope.dropDownList.Collection.splice(index, 1);
    };


});

formManagementApp.controller("CollectionEditController", function ($scope, $formServices, $location) {

    $scope.tab = $location.search()["Tab"];

    $formServices.getFormLists($location.search()["Id"]).then(function (data) {
        $scope.dropDownList = data.Result[0];
    });


    $scope.saveDropDown = function () {
        $formServices.updateFormList($scope.dropDownList).then(function (data) {
            if (data.Success) {
                $location.path('/Home').replace();
            } else {
                $scope.errors = data.Errors[0]; //todo: need to break the string of errors into a UL or similar?
            }
        });
    };


    $scope.addItem = function (text) {
        $scope.dropDownList.Collection.push({
            Value: 0,
            Text: ''
        });
    };

    $scope.removeItem = function (index) {
        $scope.dropDownList.Collection.splice(index, 1);
    };


});
