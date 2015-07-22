using System;
using System.Linq;
using Newtonsoft.Json.Linq;
using Our.Umbraco.NestedContent.Extensions;
using Our.Umbraco.NestedContent.PropertyEditors;
using Umbraco.Core;
using Umbraco.Core.Models;

namespace Our.Umbraco.NestedContent.Helpers
{
    internal static class NestedContentHelper
    {
        public static PreValueCollection GetPreValuesCollectionByDataTypeId(int dtdId)
        {
            var preValueCollection = (PreValueCollection)ApplicationContext.Current.ApplicationCache.RuntimeCache.GetCacheItem(
                string.Concat("Our.Umbraco.NestedContent.GetPreValuesCollectionByDataTypeId_", dtdId),
                () => ApplicationContext.Current.Services.DataTypeService.GetPreValuesCollectionByDataTypeId(dtdId));

            return preValueCollection;
        }

        public static string GetContentTypeAliasFromItem(JObject item)
        {
            var contentTypeAliasProperty = item[NestedContentPropertyEditor.ContentTypeAliasPropertyKey];
            if (contentTypeAliasProperty == null)
            {
                return null;
            }

            return contentTypeAliasProperty.ToObject<string>();
        }

        public static IContentType GetContentTypeFromItem(JObject item)
        {
            var contentTypeAlias = GetContentTypeAliasFromItem(item);
            if (string.IsNullOrEmpty(contentTypeAlias))
            {
                return null;
            }

            return ApplicationContext.Current.Services.ContentTypeService.GetContentType(contentTypeAlias);
        }

        #region Conversion from v0.1.1 data formats

        public static void ConvertItemValueFromV011(JObject item, int dtdId, ref PreValueCollection preValues)
        {
            var contentTypeAlias = GetContentTypeAliasFromItem(item);
            if (contentTypeAlias != null)
            {
                // the item is already in >v0.1.1 format
                return;
            }

            // old style (v0.1.1) data, let's attempt a conversion
            // - get the prevalues (if they're not loaded already)
            preValues = preValues ?? GetPreValuesCollectionByDataTypeId(dtdId);

            // - convert the prevalues (if necessary)
            ConvertPreValueCollectionFromV011(preValues);

            // - get the content types prevalue as JArray
            var preValuesAsDictionary = preValues.AsPreValueDictionary();
            if (!preValuesAsDictionary.ContainsKey(ContentTypesPreValueKey) || string.IsNullOrEmpty(preValuesAsDictionary[ContentTypesPreValueKey]) != false)
            {
                return;
            }

            var preValueContentTypes = JArray.Parse(preValuesAsDictionary[ContentTypesPreValueKey]);
            if (preValueContentTypes.Any())
            {
                // the only thing we can really do is assume that the item is the first available content type 
                item[NestedContentPropertyEditor.ContentTypeAliasPropertyKey] = preValueContentTypes.First().Value<string>("ncAlias");
            }
        }

        public static void ConvertPreValueCollectionFromV011(PreValueCollection preValueCollection)
        {
            if (preValueCollection == null)
            {
                return;
            }

            var persistedPreValuesAsDictionary = preValueCollection.AsPreValueDictionary();

            // do we have a "docTypeGuid" prevalue and no "contentTypes" prevalue?
            if (persistedPreValuesAsDictionary.ContainsKey("docTypeGuid") == false || persistedPreValuesAsDictionary.ContainsKey(ContentTypesPreValueKey))
            {
                // the prevalues are already in >v0.1.1 format
                return;
            }

            // attempt to parse the doc type guid
            Guid guid;
            if (Guid.TryParse(persistedPreValuesAsDictionary["docTypeGuid"], out guid) == false)
            {
                // this shouldn't happen... but just in case.
                return;
            }

            // find the content type
            var contentType = ApplicationContext.Current.Services.ContentTypeService.GetAllContentTypes().FirstOrDefault(c => c.Key == guid);
            if (contentType == null)
            {
                return;
            }

            // add a prevalue in the format expected by the new (>0.1.1) content type picker/configurator
            preValueCollection.PreValuesAsDictionary[ContentTypesPreValueKey] = new PreValue(
                string.Format(@"[{{""ncAlias"": ""{0}"", ""ncTabAlias"": ""{1}"", ""nameTemplate"": ""{2}"", }}]",
                    contentType.Alias,
                    persistedPreValuesAsDictionary["tabAlias"],
                    persistedPreValuesAsDictionary["nameTemplate"]
                    )
                );
        }

        private static string ContentTypesPreValueKey
        {
            get { return NestedContentPropertyEditor.NestedContentPreValueEditor.ContentTypesPreValueKey; }
        }

        #endregion
    }
}