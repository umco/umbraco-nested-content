using System;
using System.Collections.Generic;
using System.Linq;
using Our.Umbraco.NestedContent.Extensions;
using Umbraco.Web.Editors;
using Umbraco.Web.Mvc;
using System.Web.Http.ModelBinding;

namespace Our.Umbraco.NestedContent.Web.Controllers
{
    [PluginController("NestedContent")]
    public class NestedContentApiController : UmbracoAuthorizedJsonController
    {
        [System.Web.Http.HttpGet]
        public IEnumerable<object> GetContentTypes()
        {
            return Services.ContentTypeService.GetAllContentTypes()
                .OrderBy(x => x.SortOrder)
                .Select(x => new
                {
                    id = x.Id,
                    guid = x.Key,
                    name = x.Name,
                    alias = x.Alias,
                    icon = x.Icon
                });
        }
    }
}
