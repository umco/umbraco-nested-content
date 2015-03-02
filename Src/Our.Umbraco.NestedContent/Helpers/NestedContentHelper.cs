using System;
using System.Collections;
using System.Linq;
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

        public static IContentType GetContentTypeFromPreValue(int dtdId)
        {
            var preValueCollection = GetPreValuesCollectionByDataTypeId(dtdId);

            return GetContentTypeFromPreValue(preValueCollection);
        }

        public static IContentType GetContentTypeFromPreValue(PreValueCollection preValues)
        {
            var preValuesDict = preValues.AsPreValueDictionary();

            Guid contentTypeGuid;
            if (!preValuesDict.ContainsKey("docTypeGuid") || !Guid.TryParse(preValuesDict["docTypeGuid"], out contentTypeGuid))
                return null;

            var contentTypeAlias = ApplicationContext.Current.Services.ContentTypeService.GetAliasByGuid(Guid.Parse(preValuesDict["docTypeGuid"]));
            var contentType = ApplicationContext.Current.Services.ContentTypeService.GetContentType(contentTypeAlias);

            if (contentType == null || contentType.PropertyTypes == null)
                return null;

            return contentType;
        }
    }
}
