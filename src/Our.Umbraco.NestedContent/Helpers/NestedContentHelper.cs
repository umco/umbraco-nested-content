using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using Our.Umbraco.NestedContent.Extensions;
using Our.Umbraco.NestedContent.Models;
using Our.Umbraco.NestedContent.PropertyEditors;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence;

namespace Our.Umbraco.NestedContent.Helpers
{
    internal static class NestedContentHelper
    {
        private const string CacheKeyPrefix = "Our.Umbraco.NestedContent.GetPreValuesCollectionByDataTypeId_";

        public static PreValueCollection GetPreValuesCollectionByDataTypeId(int dtdId)
        {
            var preValueCollection = (PreValueCollection)ApplicationContext.Current.ApplicationCache.RuntimeCache.GetCacheItem(
                string.Concat(CacheKeyPrefix, dtdId),
                () => ApplicationContext.Current.Services.DataTypeService.GetPreValuesCollectionByDataTypeId(dtdId));

            return preValueCollection;
        }

        public static void ClearCache(int dtdId)
        {
            ApplicationContext.Current.ApplicationCache.RuntimeCache.ClearCacheItem(
                string.Concat(CacheKeyPrefix, dtdId));
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

        public static void RemapDocTypeAlias(string oldAlias, string newAlias, Transaction transaction = null)
        {
            var db = ApplicationContext.Current.DatabaseContext.Database;

            // Update references in property data
            // We do 2 very similar replace statements, but one is without spaces in the JSON, the other is with spaces 
            // as we can't guarantee what format it will actually get saved in
            var sql1 = string.Format(@"UPDATE cmsPropertyData
SET dataNtext = CAST(REPLACE(REPLACE(CAST(dataNtext AS nvarchar(max)), '""ncContentTypeAlias"":""{0}""', '""ncContentTypeAlias"":""{1}""'), '""ncContentTypeAlias"": ""{0}""', '""ncContentTypeAlias"": ""{1}""') AS ntext)
WHERE dataNtext LIKE '%""ncContentTypeAlias"":""{0}""%' OR dataNtext LIKE '%""ncContentTypeAlias"": ""{0}""%'", oldAlias, newAlias);

            // Update references in prevalue
            // We do 2 very similar replace statements, but one is without spaces in the JSON, the other is with spaces 
            // as we can't guarantee what format it will actually get saved in
            var sql2 = string.Format(@"UPDATE cmsDataTypePreValues
SET [value] = CAST(REPLACE(REPLACE(CAST([value] AS nvarchar(max)), '""ncAlias"":""{0}""', '""ncAlias"":""{1}""'), '""ncAlias"": ""{0}""', '""ncAlias"": ""{1}""') AS ntext)
WHERE [value] LIKE '%""ncAlias"":""{0}""%' OR  [value] LIKE '%""ncAlias"": ""{0}""%'", oldAlias, newAlias);

            if (transaction == null)
            {
                using (var tr = db.GetTransaction())
                {
                    db.Execute(sql1);
                    db.Execute(sql2);
                    tr.Complete();
                }
            }
            else
            {
                db.Execute(sql1);
                db.Execute(sql2);
            }
        }

        public static void RemapPropertyAlias(string docTypeAlias, string oldAlias, string newAlias, Transaction transaction = null)
        {
            var db = ApplicationContext.Current.DatabaseContext.Database;

            // Update references in property data
            // We have to do it in code because there could be nested JSON so 
            // we need to make sure it only replaces at the specific level only
            Action doQuery = () =>
            {
                var rows = GetPropertyDataRows(docTypeAlias);
                foreach (var row in rows)
                {
                    var tokens = row.Data.SelectTokens(string.Format("$..[?(@.ncContentTypeAlias == '{0}' && @.{1})]", docTypeAlias, oldAlias)).ToList();
                    if (tokens.Any())
                    {
                        foreach (var token in tokens)
                        {
                            token[oldAlias].Rename(newAlias);
                        }
                        db.Execute("UPDATE [cmsPropertyData] SET [dataNtext] = @0 WHERE [id] = @1", row.RawData, row.Id);
                    }
                }
            };

            if (transaction == null)
            {
                using (var tr = db.GetTransaction())
                {
                    doQuery();
                    tr.Complete();
                }
            }
            else
            {
                doQuery();
            }
        }

        public static void RemapDocTypeTabAlias(string docTypeAlias, string oldAlias, string newAlias, Transaction transaction = null)
        {
            var db = ApplicationContext.Current.DatabaseContext.Database;

            // Update references in prevalue 
            // We do 2 very similar replace statements, but one is without spaces in the JSON, the other is with spaces 
            // as we can't guarantee what format it will actually get saved in
            var sql1 = string.Format(@"UPDATE cmsDataTypePreValues
SET [value] = CAST(REPLACE(REPLACE(CAST([value] AS nvarchar(max)), '""ncTabAlias"":""{0}""', '""ncTabAlias"":""{1}""'), '""ncTabAlias"": ""{0}""', '""ncTabAlias"": ""{1}""') AS ntext)
WHERE [value] LIKE '%""ncAlias"":""{2}""%' OR  [value] LIKE '%""ncAlias"": ""{2}""%'", oldAlias, newAlias, docTypeAlias);

            if (transaction == null)
            {
                using (var tr = db.GetTransaction())
                {
                    db.Execute(sql1);
                    tr.Complete();
                }
            }
            else
            {
                db.Execute(sql1);
            }
        }

        // TODO: RemapNestedContentNameTemplate?

        private static IEnumerable<JsonDbRow> GetPropertyDataRows(string docTypeAlias)
        {
            var db = ApplicationContext.Current.DatabaseContext.Database;
            return db.Query<JsonDbRow>(string.Format(
                @"SELECT [id], [dataNtext] as [rawdata] FROM cmsPropertyData WHERE dataNtext LIKE '%""ncContentTypeAlias"":""{0}""%' OR dataNtext LIKE '%""ncContentTypeAlias"": ""{0}""%'",
                docTypeAlias)).ToList();
        }
    }
}