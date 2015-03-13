angular.module("umbraco").controller("Our.Umbraco.NestedContent.Controllers.DocTypePickerController", [

    "$scope",
    "Our.Umbraco.NestedContent.Resources.NestedContentResources",

    function ($scope, ncResources) {
        ncResources.getContentTypes().then(function (docTypes) {
            $scope.model.docTypes = docTypes;
        });
    }

]);

angular.module("umbraco").controller("Our.Umbraco.NestedContent.Controllers.NestedContentPropertyEditorController", [

    "$scope",
    "$interpolate",
    "contentResource",
    "Our.Umbraco.NestedContent.Resources.NestedContentResources",

    function ($scope, $interpolate, contentResource, ncResources) {

        //$scope.model.config.docTypeGuid;
        //$scope.model.config.tabAlias;
        //$scope.model.config.nameTemplate;
        //$scope.model.config.minItems;
        //$scope.model.config.maxItems;
        //console.log($scope);

        var inited = false;
        var nameExp = !!$scope.model.config.nameTemplate
            ? $interpolate($scope.model.config.nameTemplate)
            : undefined;

        $scope.nodes = [];
        $scope.currentNode = undefined;
        $scope.scaffold = undefined;
        $scope.sorting = false;

        $scope.tabAlias = $scope.model.config.tabAlias;
        $scope.minItems = $scope.model.config.minItems || 0;
        $scope.maxItems = $scope.model.config.maxItems || 0;

        if ($scope.maxItems == 0)
            $scope.maxItems = 1000;

        $scope.singleMode = $scope.minItems == 1 && $scope.maxItems == 1;

        $scope.addNode = function () {
            if ($scope.nodes.length < $scope.maxItems) {
                var newNode = angular.copy($scope.scaffold);
                newNode.id = guid();

                $scope.nodes.push(newNode);
                $scope.currentNode = newNode;
            }
        };

        $scope.editNode = function (idx) {
            if ($scope.currentNode && $scope.currentNode.id == $scope.nodes[idx].id) {
                $scope.currentNode = undefined;
            } else {
                $scope.currentNode = $scope.nodes[idx];
            }
        };

        $scope.deleteNode = function (idx) {
            if ($scope.nodes.length > $scope.model.config.minItems) {
                if ($scope.model.config.confirmDeletes && $scope.model.config.confirmDeletes == 1) {
                    if (confirm("Are you sure you want to delete this item?")) {
                        $scope.nodes.splice(idx, 1);
                    }
                } else {
                    $scope.nodes.splice(idx, 1);
                }
            }
        };

        $scope.getName = function (idx) {

            var name = "Item " + (idx + 1);

            if (nameExp)
            {
                var newName = nameExp($scope.model.value[idx]); // Run it against the stored dictionary value, NOT the node object
                if (newName && (newName = $.trim(newName))) {
                    name = newName;
                }
            }

            // Update the nodes actual name value
            if ($scope.nodes[idx].name != newName) {
                $scope.nodes[idx].name = name;
            }

            return name;
        };

        $scope.sortableOptions = {
            axis: 'y',
            cursor: "move",
            handle: ".nested-content__icon--move",
            start: function (ev, ui) {
                // Yea, yea, we shouldn't modify the dom, sue me
                $("#nested-content--" + $scope.model.id + " .umb-rte textarea").each(function () {
                    tinymce.execCommand('mceRemoveEditor', false, $(this).attr('id'));
                    $(this).css("visibility", "hidden");
                });
                $scope.$apply(function () {
                    $scope.sorting = true;
                });
            },
            stop: function (ev, ui) {
                $("#nested-content--" + $scope.model.id + " .umb-rte textarea").each(function () {
                    tinymce.execCommand('mceAddEditor', true, $(this).attr('id'));
                    $(this).css("visibility", "visible");
                });
                $scope.$apply(function () {
                    $scope.sorting = false;
                });
            }
        };

        // Initialize
        ncResources.getContentTypeAliasByGuid($scope.model.config.docTypeGuid).then(function (data1) {
            contentResource.getScaffold(-20, data1.alias).then(function (data2) {

                // Ignore the generic properties tab
                data2.tabs.pop();

                // Store the scaffold object
                $scope.scaffold = data2;

                // Convert stored nodes
                if ($scope.model.value) {
                    for (var i = 0; i < $scope.model.value.length; i++) {
                        var item = $scope.model.value[i];
                        var node = angular.copy($scope.scaffold);
                        node.id = guid();

                        for (var t = 0; t < node.tabs.length; t++) {
                            var tab = node.tabs[t];
                            for (var p = 0; p < tab.properties.length; p++) {
                                var prop = tab.properties[p];

                                // Force validation to occur server side as this is the 
                                // only way we can have consistancy between mandatory and
                                // regex validation messages. Not ideal, but it works.
                                prop.validation = {
                                    mandatory: false,
                                    pattern: ""
                                };

                                if (item[prop.alias]) {
                                    prop.value = item[prop.alias];
                                }
                            }
                        }

                        $scope.nodes.push(node);
                    }
                }

                // Enforce min items
                if ($scope.nodes.length < $scope.model.config.minItems) {
                    for (var i = $scope.nodes.length; i < $scope.model.config.minItems; i++) {
                        var node = angular.copy($scope.scaffold);
                        node.id = guid();
                        $scope.nodes.push(node);
                    }
                }

                // If there is only one item, set it as current node
                if ($scope.singleMode) {
                    $scope.currentNode = $scope.nodes[0];
                }

                inited = true;

            });
        });

        $scope.$watch("nodes", function () {
            if (inited) {
                var newValues = [];
                for (var i = 0; i < $scope.nodes.length; i++) {
                    var node = $scope.nodes[i];
                    var newValue = {
                        name: node.name
                    };
                    for (var t = 0; t < node.tabs.length; t++) {
                        var tab = node.tabs[t];
                        for (var p = 0; p < tab.properties.length; p++) {
                            var prop = tab.properties[p];
                            if (typeof prop.value !== "function") {
                                newValue[prop.alias] = prop.value;
                            }
                        }
                    }
                    newValues.push(newValue);
                }
                $scope.model.value = newValues;
            }
        }, true);

        var guid = function () {
            function _p8(s) {
                var p = (Math.random().toString(16) + "000000000").substr(2, 8);
                return s ? "-" + p.substr(0, 4) + "-" + p.substr(4, 4) : p;
            }
            return _p8() + _p8(true) + _p8(true) + _p8();
        };
    }

]);