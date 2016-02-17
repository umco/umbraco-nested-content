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
    [PropertyValueType(typeof(IPublishedContent))] 
    public class SingleNestedContentValueConverter : PropertyValueConverterBase
    {
        private UmbracoHelper _umbraco;
        internal UmbracoHelper Umbraco
        {
            get { return _umbraco ?? (_umbraco = new UmbracoHelper(UmbracoContext.Current)); }
        }

        public override bool IsConverter(PublishedPropertyType propertyType)
        {
            return propertyType.IsSingleNestedContentProperty();
        }

        public override object ConvertDataToSource(PublishedPropertyType propertyType, object source, bool preview)
        {
            try
            {
                propertyType.ConvertPropertyToNestedContent(source);
            }
            catch (Exception e)
            {
                LogHelper.Error<SingleNestedContentValueConverter>("Error converting value", e);
            }

            return null;
        }
    }
}