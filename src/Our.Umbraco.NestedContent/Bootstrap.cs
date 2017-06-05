using System;
using Newtonsoft.Json;
using Our.Umbraco.NestedContent.Helpers;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Sync;
using Umbraco.Web.Cache;

namespace Our.Umbraco.NestedContent
{
    public class Bootstrap : ApplicationEventHandler
    {
        protected override void ApplicationStarted(UmbracoApplicationBase umbracoApplication, ApplicationContext applicationContext)
        {
            CacheRefresherBase<DataTypeCacheRefresher>.CacheUpdated += DataTypeCacheRefresher_Updated;
        }

        private void DataTypeCacheRefresher_Updated(DataTypeCacheRefresher sender, CacheRefresherEventArgs e)
        {
            if (e.MessageType == MessageType.RefreshByJson)
            {
                var payload = JsonConvert.DeserializeObject<JsonPayload[]>((string)e.MessageObject);
                if (payload != null)
                {
                    foreach (var item in payload)
                    {
                        NestedContentHelper.ClearCache(item.Id);
                    }
                }
            }
        }

        private class JsonPayload
        {
            public Guid UniqueId { get; set; }
            public int Id { get; set; }
        }
    }
}