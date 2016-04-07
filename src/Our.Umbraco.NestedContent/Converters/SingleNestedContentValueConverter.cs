using System;
using Our.Umbraco.NestedContent.Extensions;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Core.PropertyEditors;

namespace Our.Umbraco.NestedContent.Converters
{
    [PropertyValueCache(PropertyCacheValue.All, PropertyCacheLevel.Content)]
    [PropertyValueType(typeof(IPublishedContent))]
    public class SingleNestedContentValueConverter : PropertyValueConverterBase
    {
        public override bool IsConverter(PublishedPropertyType propertyType)
        {
            return propertyType.IsSingleNestedContentProperty();
        }

        public override object ConvertDataToSource(PublishedPropertyType propertyType, object source, bool preview)
        {
            try
            {
                return propertyType.ConvertPropertyToNestedContent(source);
            }
            catch (Exception e)
            {
                LogHelper.Error<SingleNestedContentValueConverter>("Error converting value", e);
            }

            return null;
        }
    }
}