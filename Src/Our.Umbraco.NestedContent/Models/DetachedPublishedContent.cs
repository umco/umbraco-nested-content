using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Web.Models;

namespace Our.Umbraco.NestedContent.Models
{
    internal class DetachedPublishedContent : PublishedContentBase
    {
        private readonly string _name;
        private readonly PublishedContentType _contentType;
        private readonly IEnumerable<IPublishedProperty> _properties;
        private readonly bool _isPreviewing;
        private readonly IPublishedContent _parent;
        private readonly int _sortOrder;
        private readonly string _writerName = null;
        private readonly string _creatorName = null;
        private readonly int _writerId = 0;
        private readonly int _creatorId = 0;
        private readonly DateTime _createDate = DateTime.MinValue;
        private readonly DateTime _updateDate = DateTime.MinValue;
        private readonly Guid _version = Guid.Empty;
        private readonly int _level = 0;

        public DetachedPublishedContent(
            string name,
            PublishedContentType contentType,
            IPublishedContent parent,
            int sortOrder,
            IEnumerable<IPublishedProperty> properties,
            bool isPreviewing = false)
        {
            _name = name;
            _contentType = contentType;
            _parent = parent;
            _sortOrder = sortOrder;
            _properties = properties;
            _isPreviewing = isPreviewing;

            if(parent != null)
            {
                // duplicate property values from the parent (hosting) IPublished content onto this
                _writerName = parent.WriterName;
                _creatorName = parent.CreatorName;
                _writerId = parent.WriterId;
                _creatorId = parent.CreatorId;
                _createDate = parent.CreateDate;
                _updateDate = parent.UpdateDate;
                _version = parent.Version;
                _level = parent.Level + 1;
            }
        }

        public override int Id
        {
            get { return 0; }
        }

        public override string Name
        {
            get { return _name; }
        }

        public override bool IsDraft
        {
            get { return _isPreviewing; }
        }

        public override PublishedItemType ItemType
        {
            get { return PublishedItemType.Content; }
        }

        public override PublishedContentType ContentType
        {
            get { return _contentType; }
        }

        public override string DocumentTypeAlias
        {
            get { return _contentType.Alias; }
        }

        public override int DocumentTypeId
        {
            get { return _contentType.Id; }
        }

        public override ICollection<IPublishedProperty> Properties
        {
            get { return _properties.ToArray(); }
        }

        public override IPublishedProperty GetProperty(string alias)
        {
            return _properties.FirstOrDefault(x => x.PropertyTypeAlias.InvariantEquals(alias));
        }

        public override IPublishedProperty GetProperty(string alias, bool recurse)
        {
            if (recurse)
                throw new NotSupportedException();

            return GetProperty(alias);
        }

        public override IPublishedContent Parent
        {
            get { return _parent; }
        }

        public override IEnumerable<IPublishedContent> Children
        {
            get { return Enumerable.Empty<IPublishedContent>(); }
        }

        public override int TemplateId
        {
            get { return 0; }
        }

        public override int SortOrder
        {
            get { return _sortOrder; }
        }

        public override string UrlName
        {
            get { return null; }
        }

        public override string WriterName
        {
            get { return _writerName; }
        }

        public override string CreatorName
        {
            get { return _creatorName; }
        }

        public override int WriterId
        {
            get { return _writerId; }
        }

        public override int CreatorId
        {
            get { return _creatorId; }
        }

        public override string Path
        {
            get { return null; }
        }

        public override DateTime CreateDate
        {
            get { return _createDate; }
        }

        public override DateTime UpdateDate
        {
            get { return _updateDate; }
        }

        public override Guid Version
        {
            get { return _version; }
        }

        public override int Level
        {
            get { return _level; }
        }
    }
}
