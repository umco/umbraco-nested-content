using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Our.Umbraco.NestedContent.Helpers;
using Umbraco.Core.Models;
using Umbraco.Core.Models.PublishedContent;

namespace Our.Umbraco.NestedContent.Extensions
{
    internal static class PreValueCollectionExtensions
    {
        public static IDictionary<string, string> AsPreValueDictionary(this PreValueCollection preValue)
        {
            return preValue.PreValuesAsDictionary.ToDictionary(x => x.Key, x => x.Value.Value);
        }

        public static bool TryGetPreValue<T>(this PublishedPropertyType publishedProperty, string key, out T value)
        {
            if (publishedProperty == null) throw new ArgumentNullException("publishedProperty");
            if (string.IsNullOrEmpty(key)) throw new ArgumentNullException("key");

            var preValuesCollection = NestedContentHelper.GetPreValuesCollectionByDataTypeId(publishedProperty.DataTypeId);
            if (preValuesCollection != null)
            {
                var prevalueDictionary = preValuesCollection.PreValuesAsDictionary;
            
                PreValue prevalue;
                if (prevalueDictionary != null && prevalueDictionary.TryGetValue(key, out prevalue))
                {
                    var converter = TypeDescriptor.GetConverter(typeof(T));
                    value = (T)converter.ConvertFromString(prevalue.Value);
                    return true;
                }
            }

            value = default(T);
            return false;
        }
    }
}