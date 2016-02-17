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
            return propertyType.PropertyEditorAlias.InvariantEquals(NestedContentPropertyEditor.PropertyEditorAlias);
        }

        public override object ConvertDataToSource(PublishedPropertyType propertyType, object source, bool preview)
        {
            try
            {
                using (DisposableTimer.DebugDuration<NestedContentValueConverter>(string.Format("ConvertDataToSource ({0})", propertyType.DataTypeId)))
                {
                    if (source != null && !source.ToString().IsNullOrWhiteSpace())
                    {
                        var rawValue = JsonConvert.DeserializeObject<List<object>>(source.ToString());
                        var processedValue = new List<IPublishedContent>();

                        var preValueCollection = NestedContentHelper.GetPreValuesCollectionByDataTypeId(propertyType.DataTypeId);
                        var preValueDictionary = preValueCollection.AsPreValueDictionary();

                        for (var i = 0; i < rawValue.Count; i++)
                        {
                            var item = (JObject)rawValue[i];

                            // Convert from old style (v.0.1.1) data format if necessary
                            // - Please note: This call has virtually no impact on rendering performance for new style (>v0.1.1).
                            //                Even so, this should be removed eventually, when it's safe to assume that there is
                            //                no longer any need for conversion.
                            NestedContentHelper.ConvertItemValueFromV011(item, propertyType.DataTypeId, ref preValueCollection);

                            var contentTypeAlias = NestedContentHelper.GetContentTypeAliasFromItem(item);
                            if (string.IsNullOrEmpty(contentTypeAlias))
                            {
                                continue;
                            }

                            var publishedContentType = PublishedContentType.Get(PublishedItemType.Content, contentTypeAlias);
                            if (publishedContentType == null)
                            {
                                continue;
                            }

                            var propValues = item.ToObject<Dictionary<string, object>>();
                            var properties = new List<IPublishedProperty>();

                            foreach (var jProp in propValues)
                            {
                                var propType = publishedContentType.GetPropertyType(jProp.Key);
                                if (propType != null)
                                {
                                    properties.Add(new DetachedPublishedProperty(propType, jProp.Value));
                                }
                            }

                            // Parse out the name manually
                            object nameObj = null;
                            if (propValues.TryGetValue("name", out nameObj))
                            {
                                // Do nothing, we just want to parse out the name if we can
                            }

                            // Get the current request node we are embedded in
                            var pcr = UmbracoContext.Current.PublishedContentRequest;
                            var containerNode = pcr != null && pcr.HasPublishedContent ? pcr.PublishedContent : null;

                            processedValue.Add(new DetachedPublishedContent(
                                nameObj == null ? null : nameObj.ToString(),
                                publishedContentType,
                                properties.ToArray(),
                                containerNode,
                                i));
                        }

                        // Detect min/max items == 1 and just return a single IPublishedContent
                        int minItems, maxItems;
                        if (preValueDictionary.ContainsKey("minItems") && int.TryParse(preValueDictionary["minItems"], out minItems) && minItems == 1
                            && preValueDictionary.ContainsKey("maxItems") && int.TryParse(preValueDictionary["maxItems"], out maxItems) && maxItems == 1)
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