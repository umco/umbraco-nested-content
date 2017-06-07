angular.module('umbraco.services').factory('Our.Umbraco.NestedContent.Services.NestedContentCallbacks',

  function() {
    // Define available callbacks
    var callbacks = {
      'ncBeforeFormSubmitting': []
    };

    return {

      callbacks: callbacks,

      call: function(cb, args) {
        cb = this.callbacks[cb];
        if ( cb && Array.isArray(cb) && cb.length ) {
          cb.forEach(function(func) {
            typeof func != "function" || func(args);
          });
        }
      }

    };
  }

);
