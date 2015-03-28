using System;
using System.Collections;
using System.Linq;
using Newtonsoft.Json.Linq;
using Our.Umbraco.NestedContent.Extensions;
using Umbraco.Core;
using Umbraco.Core.Models;
using System.Collections.Generic;

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

        public static IDictionary<string, string> GetPreValuesDictionaryByDataTypeId(int dtdId)
        {
            return GetPreValuesCollectionByDataTypeId(dtdId).AsPreValueDictionary();
        }

        public static string GetContentTypeAliasFromItem(JObject item)
        {
            var contentTypeAliasProperty = item["ncContentTypeAlias"];
            if(contentTypeAliasProperty == null)
            {
                return null;
            }
            return contentTypeAliasProperty.ToObject<string>();
        }

        public static IContentType GetContentTypeFromItem(JObject item)
        {
            var contentTypeAlias = GetContentTypeAliasFromItem(item);
            if(string.IsNullOrEmpty(contentTypeAlias))
            {
                return null;
            }
            return ApplicationContext.Current.Services.ContentTypeService.GetContentType(contentTypeAlias);
        }
    }
}
