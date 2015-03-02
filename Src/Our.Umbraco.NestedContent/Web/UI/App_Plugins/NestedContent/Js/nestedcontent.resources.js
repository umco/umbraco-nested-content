angular.module('umbraco.resources').factory('Our.Umbraco.NestedContent.Resources.NestedContentResources',
    function ($q, $http, umbRequestHelper) {
        return {
            getContentTypeAliasByGuid: function (guid) {
                var url = "/umbraco/backoffice/NestedContent/NestedContentApi/GetContentTypeAliasByGuid?guid=" + guid;
                return umbRequestHelper.resourcePromise(
                    $http.get(url),
                    'Failed to retrieve datatype alias by guid'
                );
            },
            getContentTypes: function () {
                var url = "/umbraco/backoffice/NestedContent/NestedContentApi/GetContentTypes";
                return umbRequestHelper.resourcePromise(
                    $http.get(url),
                    'Failed to retrieve content types'
                );
            },
        };
    });