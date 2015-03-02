using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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
    public class NestedContentValueConverter : PropertyValueConverterBase
    {
        private UmbracoHelper _umbraco;
        internal UmbracoHelper Umbraco
        {
            get { return _umbraco ?? (_umbraco = new UmbracoHelper(UmbracoContext.Current)); }
        }

        public override bool IsConverter(PublishedPropertyType propertyType)
        {
            return propertyType.PropertyEditorAlias.InvariantEquals(NestedContentPropertyEditor.PropertyEditorAlias);
        }

        public override object ConvertDataToSource(PublishedPropertyType propertyType, object source, bool preview)
        {
            try
			{
                using (var timer = DisposableTimer.DebugDuration<NestedContentValueConverter>(string.Format("ConvertDataToSource ({0})", propertyType.DataTypeId)))
                {
                    if (source != null && !source.ToString().IsNullOrWhiteSpace())
                    {
                        var rawValue = JsonConvert.DeserializeObject<List<object>>(source.ToString());
                        var processedValue = new List<IPublishedContent>();

                        var preValue = NestedContentHelper.GetPreValuesDictionaryByDataTypeId(propertyType.DataTypeId);
                        var contentType = NestedContentHelper.GetContentTypeFromPreValue(propertyType.DataTypeId);
                        if (contentType == null)
                            return null;

                        var publishedContentType = PublishedContentType.Get(PublishedItemType.Content, contentType.Alias);
                        if (publishedContentType == null)
                            return null;

                        for (var i = 0; i < rawValue.Count; i++)
                        {
                            var o = rawValue[i];
                            var propValues = ((JObject)o).ToObject<Dictionary<string, object>>();
                            var properties = new List<IPublishedProperty>();

                            foreach (var jProp in propValues)
                            {
                                var propType = publishedContentType.GetPropertyType(jProp.Key);
                                if (propType != null)
                                {
                                    properties.Add(new DetachedPublishedProperty(propType, jProp.Value));
                                }
                            }

                            processedValue.Add(new DetachedPublishedContent(null, publishedContentType, properties.ToArray()));
                        }

                        // Detect min/max items == 1 and just return a single IPublishedContent
                        int minItems, maxItems;
                        if (preValue.ContainsKey("minItems") && int.TryParse(preValue["minItems"], out minItems) && minItems == 1
                            && preValue.ContainsKey("maxItems") && int.TryParse(preValue["maxItems"], out maxItems) && maxItems == 1)
                        {
                            return processedValue.FirstOrDefault();
                        }

                        return processedValue;
                    }
                }
			}
            catch (Exception e)
            {
                LogHelper.Error<NestedContentValueConverter>("Error converting value", e);
            }

            return null;
        }
    }
}
