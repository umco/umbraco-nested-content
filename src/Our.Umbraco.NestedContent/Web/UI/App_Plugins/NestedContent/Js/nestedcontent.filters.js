// Filter to take a node id and grab it's name instead
// Usage: {{ pickerAlias | ncNodeName }}

// Cache for node names so we don't make a ton of requests
var ncNodeNameCache = {
    id: "",
    keys: {}
};

angular.module("umbraco.filters").filter("ncNodeName", function (editorState, entityResource) {

    return function (input) {

        // Check we have a value at all
        if (input === "" || input.toString() === "0")
            return "";

        var currentNode = editorState.getCurrent();

        // Ensure a unique cache per editor instance
        var key = "ncNodeName_" + currentNode.key;
        if (ncNodeNameCache.id !== key) {
            ncNodeNameCache.id = key;
            ncNodeNameCache.keys = {};
        }

        // See if there is a value in the cache and use that
        if (ncNodeNameCache.keys[input]) {
            return ncNodeNameCache.keys[input];
        }

        // No value, so go fetch one 
        // We'll put a temp value in the cache though so we don't 
        // make a load of requests while we wait for a response
        ncNodeNameCache.keys[input] = "Loading...";

        entityResource.getById(input, "Document")
            .then(function (ent) {
                ncNodeNameCache.keys[input] = ent.name;
            });

        // Return the current value for now
        return ncNodeNameCache.keys[input];
    };

});

// Filter to take a node id and grab it's name instead
// Usage: {{ pickerAlias | ncMediaName }}

// Cache for node names so we don't make a ton of requests
var ncMediaNameCache = {
    id: "",
    keys: {}
}

angular.module("umbraco.filters").filter("ncMediaName", function (editorState, entityResource) {

    return function (input) {

        // Check we have a value at all
        if (input == "" || input.toString() == "0")
            return "";

        var currentNode = editorState.getCurrent();

        // Ensure a unique cache per editor instance
        var key = "ncMediaName_" + currentNode.key;
        if (ncMediaNameCache.id != key) {
            ncMediaNameCache.id = key;
            ncMediaNameCache.keys = {};
        }

        // See if there is a value in the cache and use that
        if (ncMediaNameCache.keys[input]) {
            return ncMediaNameCache.keys[input];
        }

        // No value, so go fetch one 
        // We'll put a temp value in the cache though so we don't 
        // make a load of requests while we wait for a response
        ncMediaNameCache.keys[input] = "Loading...";

        entityResource.getById(input, "Media")
            .then(function (ent) {
                ncMediaNameCache.keys[input] = ent.name;
            });

        // Return the current value for now
        return ncMediaNameCache.keys[input];
    }

});
