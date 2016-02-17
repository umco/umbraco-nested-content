using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Our.Umbraco.NestedContent.Extensions;
using Our.Umbraco.NestedContent.Helpers;
using Our.Umbraco.NestedContent.Models;
using Our.Umbraco.NestedContent.PropertyEditors;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Core.PropertyEditors;
using Umbraco.Web;

namespace Our.Umbraco.NestedContent.Converters
{
    [PropertyValueCache(PropertyCacheValue.All, PropertyCacheLevel.Content)]
    [PropertyValueType(typeof(IEnumerable<IPublishedContent>))] 
    public class NestedContentValueConverter : PropertyValueConverterBase
    {
        private UmbracoHelper _umbraco;
        internal UmbracoHelper Umbraco
        {
            get { return _umbraco ?? (_umbraco = new UmbracoHelper(UmbracoContext.Current)); }
        }

        public override bool IsConverter(PublishedPropertyType propertyType)
        {
            return propertyType.IsNestedContentProperty() && !propertyType.IsSingleNestedContentProperty();
        }

        public override object ConvertDataToSource(PublishedPropertyType propertyType, object source, bool preview)
        {
            try
            {
                return propertyType.ConvertPropertyToNestedContent(source);
            }
            catch (Exception e)
            {
                LogHelper.Error<NestedContentValueConverter>("Error converting value", e);
            }

            return null;
        }
    }
}