angular.module('formServices', []).factory('$formServices', function ($http) {
    return {
        newForm: function () {
            return $http.get(siteRoot + "api/Forms/New").then(function (result) {
                return result.data;
            });
        },
        getAllForms: function () {
            return $http.get(siteRoot + "api/Forms/").then(function (result) {
                return result.data;
            });
        },
        getForms: function (ids) {
            return $http.get(siteRoot + "api/Forms/", { params: { Ids: ids } }).then(function (result) {
                return result.data;
            });
        },
        createForm: function (data) {
            return $http.post(siteRoot + "api/Forms/", { Form: data }).then(function (result) {
                return result.data;
            });
        },
        updateForm: function (data) {
            return $http.put(siteRoot + "api/Forms/", { Form: data }).then(function (result) {
                return result.data;
            });
        },
        newFormList: function () {
            return $http.get(siteRoot + "api/Forms/FormList/New").then(function (result) {
                return result.data;
            });
        },
        getAllFormLists: function () {
            return $http.get(siteRoot + "api/Forms/FormList/").then(function (result) {
                return result.data;
            });
        },
        getAllFormTypes: function () {
            return $http.get(siteRoot + "api/Forms/Types").then(function (result) {
                return result.data;
            });
        },
        getFormLists: function (ids) {
            return $http.get(siteRoot + "api/Forms/FormList/", { params: { Ids: ids } }).then(function (result) {
                return result.data;
            });
        },
        createFormList: function (data) {
            return $http.post(siteRoot + "api/Forms/FormList/", { FormList: data }).then(function (result) {
                return result.data;
            });
        },
        updateFormList: function (data) {
            return $http.put(siteRoot + "api/Forms/FormList/", { FormList: data }).then(function (result) {
                return result.data;
            });
        },
        deleteFormList: function (id) {

            return $http.delete(siteRoot + "api/Forms/FormList/", { params: { Id: id } }).then(function (result) {
                return result.data;
            });
        }
    };
});