using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Our.Umbraco.NestedContent.Models
{
    public class ContentTypeConfigurations
    {
        public List<ContentTypeConfiguration> Configurations
        {
            get; set;
        }
    }

    public class ContentTypeConfiguration
    {
        public string ncAlias
        {
            get; set;
        }

        public string ncTabAlias
        {
            get; set;
        }

        public string nameTemplate
        {
            get; set;
        }
    }
}
