angular.module("umbraco.directives").directive('nestedContentEditor', [

    function () {

        var link = function ($scope, element, attrs, ctrl) {
            $scope.nodeContext = $scope.model = $scope.ngModel;

            var tab = $scope.ngModel.tabs[0];

            if ($scope.tabAlias) {
                angular.forEach($scope.ngModel.tabs, function (value, key) {
                    if (value.alias.toLowerCase() == $scope.tabAlias.toLowerCase()) {
                        tab = value;
                        return;
                    }
                });
            }

            $scope.tab = tab;

            var unsubscribe = $scope.$on("ncSyncVal", function (ev, args) {
                if (args.id === $scope.model.id) {
                    $scope.$broadcast("formSubmitting", { scope: $scope });
                }
            });

            $scope.$on('$destroy', function () {
                unsubscribe();
            });
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

//angular.module("umbraco.directives").directive('nestedContentSubmitWatcher', function () {
//    var link = function (scope) {
//        // call the load callback on scope to obtain the ID of this submit watcher
//        var id = scope.loadCallback();
//        scope.$on("formSubmitting", function (ev, args) {
//            // on the "formSubmitting" event, call the submit callback on scope to notify the nestedContent controller to do it's magic
//            if (id === scope.activeSubmitWatcher) {
//                scope.submitCallback();
//            }
//        });
//    }
    
//    return {
//        restrict: "E",
//        replace: true,
//        template: "",
//        scope: {
//            loadCallback: '=',
//            submitCallback: '=',
//            activeSubmitWatcher: '='
//        },
//        link: link
//    }
//});