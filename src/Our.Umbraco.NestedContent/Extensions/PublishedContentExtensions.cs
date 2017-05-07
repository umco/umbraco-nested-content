using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Models.PublishedContent;

namespace Our.Umbraco.NestedContent.Extensions
{
    internal static class PublishedContentExtensions
    {
        public static IEnumerable<IPublishedContent> TryCreateTypedModels(this IEnumerable<IPublishedContent> contentItems)
        {
            return contentItems?.Select(item => item.TryCreateTypedModel());
        }

        public static IPublishedContent TryCreateTypedModel(this IPublishedContent content)
        {
            var factoryResolver = PublishedContentModelFactoryResolver.Current;

            return factoryResolver?.HasValue == true ? factoryResolver.Factory?.CreateModel(content) : content;
        }

        public static Type GetModelType(string alias)
        {
            var typedModels = PluginManager.Current?.ResolveTypesWithAttribute<PublishedContentModel, PublishedContentModelAttribute>();

            return typedModels?.FirstOrDefault(t => t.GetCustomAttribute<PublishedContentModelAttribute>(false)?.ContentTypeAlias?.Equals(alias, StringComparison.OrdinalIgnoreCase) == true);
        }

        public static Type GetModelType(this IPublishedContent content)
        {
            return GetModelType(content?.DocumentTypeAlias);
        }
    }
}
