using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Models.PublishedContent;

namespace Our.Umbraco.NestedContent.Extensions
{
    internal static class PublishedContentExtensions
    {
        private static readonly MethodInfo _castMethod = typeof(Enumerable).GetMethod("Cast", new[] { typeof(IEnumerable) });

        public static IEnumerable<IPublishedContent> TryCreateTypedModels(this IEnumerable<IPublishedContent> contentItems)
        {
            if (contentItems == null) return null;

            var list = contentItems.Select(item => item.TryCreateTypedModel()).ToArray();
            var distinctTypes = list.ToLookup(i => i.GetType());

            if (distinctTypes.Count != 1) return list;

            var first = distinctTypes.First();
            if (first == null) return null;

            var type = first.Key;
            var castMethod = _castMethod.MakeGenericMethod(type);
            var typedlist = castMethod.Invoke(list, new object[] { list });

            return typedlist as IEnumerable<IPublishedContent>;
        }

        public static IPublishedContent TryCreateTypedModel(this IPublishedContent content)
        {
            var factoryResolver = PublishedContentModelFactoryResolver.Current;

            if (factoryResolver != null && factoryResolver.HasValue && factoryResolver.Factory != null)
            {
                return factoryResolver.Factory.CreateModel(content);
            }

            return content;
        }

        public static Type GetModelType(string alias)
        {
            if (PluginManager.Current == null) return null;

            var typedModels = PluginManager.Current.ResolveTypesWithAttribute<PublishedContentModel, PublishedContentModelAttribute>();

            if (typedModels == null) return null;

            var type = (from t in typedModels
                let attr = t.GetCustomAttribute<PublishedContentModelAttribute>(false)
                where attr != null && !attr.ContentTypeAlias.IsNullOrWhiteSpace() && attr.ContentTypeAlias.Equals(alias, StringComparison.OrdinalIgnoreCase)
                select t).FirstOrDefault();

            return type;
        }

        public static Type GetModelType(this IPublishedContent content)
        {
            return GetModelType(content != null ? content.DocumentTypeAlias : null);
        }
    }
}
