angular.module('httpPreConfig', [], function ($provide, $httpProvider) {
    $provide.factory('httpPreConfig', ['$http', '$rootScope', function ($http, $rootScope) {
        $http.defaults.transformResponse.push(function(data) {
            $rootScope.Something = { success: data.success, message: "blah blah" };
            return data;
        });
        return $http;
    }]);
});