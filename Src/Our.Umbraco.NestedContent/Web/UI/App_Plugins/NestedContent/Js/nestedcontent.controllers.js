angular.module("umbraco").controller("Our.Umbraco.NestedContent.Controllers.DocTypePickerController", [

    "$scope",
    "Our.Umbraco.NestedContent.Resources.NestedContentResources",

    function ($scope, ncResources) {

        $scope.add = function() {
            $scope.model.value.push({
                    // As per PR #4, all stored content type aliases must be prefixed "nc" for easier recognition.
                    // For good measure we'll also prefix the tab alias "nc" 
                    ncAlias: "",
                    ncTabAlias: "",
                    nameTemplate: ""
                }
            );
        }

        $scope.remove = function (index) {
            $scope.model.value.splice(index, 1);
        }

        $scope.sortableOptions = {
            axis: 'y',
            cursor: "move",
            handle: ".icon-navigation"
        };

        ncResources.getContentTypes().then(function (docTypes) {
            $scope.model.docTypes = docTypes;
        });

        if (!$scope.model.value) {
            $scope.model.value = [];
            $scope.add();
        }
    }
]);

angular.module("umbraco").controller("Our.Umbraco.NestedContent.Controllers.NestedContentPropertyEditorController", [

    "$scope",
    "$interpolate",
    "contentResource",
    "Our.Umbraco.NestedContent.Resources.NestedContentResources",

    function ($scope, $interpolate, contentResource, ncResources) {

        //$scope.model.config.contentTypes;
        //$scope.model.config.minItems;
        //$scope.model.config.maxItems;
        //console.log($scope);

        var inited = false;

        _.each($scope.model.config.contentTypes, function (contentType) {
            contentType.nameExp = !!contentType.nameTemplate
                ? $interpolate(contentType.nameTemplate)
                : undefined;
        });

        $scope.nodes = [];
        $scope.currentNode = undefined;
        $scope.scaffolds = undefined;
        $scope.sorting = false;

        $scope.minItems = $scope.model.config.minItems || 0;
        $scope.maxItems = $scope.model.config.maxItems || 0;

        if ($scope.maxItems == 0)
            $scope.maxItems = 1000;

        $scope.singleMode = $scope.minItems == 1 && $scope.maxItems == 1;

        $scope.overlayMenu = {
            show: false,
            style: {}
        };

        $scope.addNode = function (alias) {
            var scaffold = $scope.getScaffold(alias);
            var newNode = angular.copy(scaffold);
            newNode.id = guid();
            newNode.ncContentTypeAlias = alias;

            $scope.nodes.push(newNode);
            $scope.currentNode = newNode;

            $scope.closeNodeTypePicker();
        };

        $scope.openNodeTypePicker = function () {
            if ($scope.nodes.length >= $scope.maxItems) {
                return;
            }

            // this could be used for future limiting on node types
            $scope.overlayMenu.scaffolds = [];
            _.each($scope.scaffolds, function (scaffold) {
                var icon = scaffold.icon;
                // workaround for when no icon is chosen for a doctype
                if (icon == ".sprTreeFolder") {
                    icon = "icon-folder";
                }
                $scope.overlayMenu.scaffolds.push({
                    alias: scaffold.contentTypeAlias,
                    name: scaffold.contentTypeName,
                    icon: icon
                });
            });

            if ($scope.overlayMenu.scaffolds.length == 0) {
                return;
            }

            if ($scope.overlayMenu.scaffolds.length == 1) {
                // only one scaffold type - no need to display the picker
                $scope.addNode($scope.scaffolds[0].contentTypeAlias);
                return;
            }

            // calculate overlay position
            // - yeah... it's jQuery (ungh!) but that's how the Grid does it.
            var offset = $(event.target).offset();
            var scrollTop = $(event.target).closest(".umb-panel-body").scrollTop();
            if (offset.top < 400) {
                $scope.overlayMenu.style.top = 300 + scrollTop;
            }
            else {
                $scope.overlayMenu.style.top = offset.top - 150 + scrollTop;
            }
            $scope.overlayMenu.show = true;
        };

        $scope.closeNodeTypePicker = function () {
            $scope.overlayMenu.show = false;
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

            var contentType = $scope.getContentTypeConfig($scope.model.value[idx].ncContentTypeAlias);

            if (contentType != null && contentType.nameExp) {
                var newName = contentType.nameExp($scope.model.value[idx]); // Run it against the stored dictionary value, NOT the node object
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

        $scope.getScaffold = function (alias) {
            return _.find($scope.scaffolds, function (scaffold) {
                return scaffold.contentTypeAlias == alias;
            });
        }

        $scope.getContentTypeConfig = function (alias) {
            return _.find($scope.model.config.contentTypes, function (contentType) {
                return contentType.ncAlias == alias;
            });
        }

        // Initialize
        var scaffoldsLoaded = 0;
        $scope.scaffolds = [];
        _.each($scope.model.config.contentTypes, function (contentType) {
            contentResource.getScaffold(-20, contentType.ncAlias).then(function(scaffold) {
                // remove all tabs except the specified tab
                var tab = _.find(scaffold.tabs, function(tab) {
                    return tab.id != 0 && (tab.alias == contentType.ncTabAlias || contentType.ncTabAlias == "");
                })
                scaffold.tabs = [];
                if (tab) {
                    scaffold.tabs.push(tab);
                }

                // Store the scaffold object
                $scope.scaffolds.push(scaffold);

                scaffoldsLoaded++;
                InitIfAllScaffoldsHaveLoaded();
            }, function(error) {
                scaffoldsLoaded++;
                InitIfAllScaffoldsHaveLoaded();
            });
        });

        function InitIfAllScaffoldsHaveLoaded() {
            // Initialize when all scaffolds have loaded
            if ($scope.model.config.contentTypes.length == scaffoldsLoaded) {
                // Convert stored nodes
                if ($scope.model.value) {
                    for (var i = 0; i < $scope.model.value.length; i++) {
                        var item = $scope.model.value[i];
                        var scaffold = $scope.getScaffold(item.ncContentTypeAlias);
                        if (scaffold == null) {
                            // No such scaffold - the content type might have been deleted. We need to skip it.
                            continue;
                        }
                        var node = angular.copy(scaffold);
                        node.id = guid();
                        node.ncContentTypeAlias = item.ncContentTypeAlias;

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
                        var node = angular.copy($scope.scaffolds[0]);
                        node.id = guid();
                        $scope.nodes.push(node);
                    }
                }

                // If there is only one item, set it as current node
                if ($scope.singleMode) {
                    $scope.currentNode = $scope.nodes[0];
                }

                inited = true;
            }
        }

        $scope.$watch("nodes", function () {
            if (inited) {
                var newValues = [];
                for (var i = 0; i < $scope.nodes.length; i++) {
                    var node = $scope.nodes[i];
                    var newValue = {
                        name: node.name,
                        ncContentTypeAlias: node.ncContentTypeAlias
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