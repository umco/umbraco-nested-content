using System;
using System.Collections.Generic;
using Our.Umbraco.NestedContent.Extensions;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Core.PropertyEditors;

namespace Our.Umbraco.NestedContent.Converters
{
    [PropertyValueCache(PropertyCacheValue.All, PropertyCacheLevel.Content)]
    public class NestedContentValueConverter : PropertyValueConverterBase, IPropertyValueConverterMeta
    {
        public override bool IsConverter(PublishedPropertyType propertyType)
        {
            return propertyType.IsNestedContentProperty();
        }

        public override object ConvertDataToSource(PublishedPropertyType propertyType, object source, bool preview)
        {
            try
            {
                return propertyType.ConvertPropertyToNestedContent(source, preview);
            }
            catch (Exception e)
            {
                LogHelper.Error<NestedContentValueConverter>("Error converting value", e);
            }

            return null;
        }
        public Type GetPropertyValueType(PublishedPropertyType propertyType)
        {
        }

        public PropertyCacheLevel GetPropertyCacheLevel(PublishedPropertyType propertyType, PropertyCacheValue cacheValue)
        {
            return PropertyCacheLevel.Content;
        }
    }
}