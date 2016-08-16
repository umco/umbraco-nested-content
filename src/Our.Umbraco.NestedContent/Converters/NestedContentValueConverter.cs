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
    [PropertyValueType(typeof(IEnumerable<IPublishedContent>))]
    public class NestedContentValueConverter : PropertyValueConverterBase
    {
        public override bool IsConverter(PublishedPropertyType propertyType)
        {
            return propertyType.IsNestedContentProperty() && !propertyType.IsSingleNestedContentProperty();
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
    }
}