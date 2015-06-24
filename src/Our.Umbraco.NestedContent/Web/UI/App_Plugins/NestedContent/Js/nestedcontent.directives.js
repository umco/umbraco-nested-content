angular.module("umbraco.directives").directive('nestedContentEditor', [

    function () {

        var link = function ($scope, element, attrs, ctrl) {
            $scope.model = $scope.ngModel;

            var tab = $scope.ngModel.tabs[0];

            if ($scope.tabAlias) {
                angular.forEach($scope.ngModel.tabs, function (value, key) {
                    if (value.alias == $scope.tabAlias) {
                        tab = value;
                        return;
                    }
                });
            }

            $scope.tab = tab;
        }

        return {
            restrict: "E",
            replace: true,
            templateUrl: "/App_Plugins/NestedContent/Views/nestedcontent.editor.html",
            scope: {
                ngModel: '=',
                tabAlias: '='
            },
            link: link
        };

    }
]);
