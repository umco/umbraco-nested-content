using System.Linq;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Our.Umbraco.NestedContent.Extensions;
using Our.Umbraco.NestedContent.Helpers;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Editors;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.Services;
using Umbraco.Web.PropertyEditors;

namespace Our.Umbraco.NestedContent.PropertyEditors
{
    [PropertyEditor(NestedContentPropertyEditor.PropertyEditorAlias, "Nested Content", "/App_Plugins/NestedContent/Views/nestedcontent.html", ValueType = "JSON")]
    public class NestedContentPropertyEditor : PropertyEditor
    {
        public const string PropertyEditorAlias = "Our.Umbraco.NestedContent";

        private IDictionary<string, object> _defaultPreValues;
        public override IDictionary<string, object> DefaultPreValues
        {
            get { return _defaultPreValues; }
            set { _defaultPreValues = value; }
        }

        public NestedContentPropertyEditor()
        {
            // Setup default values
            _defaultPreValues = new Dictionary<string, object>
            {
                {"docTypeGuids", "[]"},
                {"minItems", 0},
                {"maxItems", 0},
                {"confirmDeletes", 1}
            };
        }

        #region Pre Value Editor

        protected override PreValueEditor CreatePreValueEditor()
        {
            return new NestedContentPreValueEditor();
        }

        internal class NestedContentPreValueEditor : PreValueEditor
        {
            [PreValueField("docTypeGuids", "Doc Types", "/App_Plugins/NestedContent/Views/nestedcontent.doctypepicker.html", Description = "Select the doc types to use as the data blueprint.")]
            public string[] DocTypeGuids { get; set; }

            [PreValueField("tabAlias", "Tab", "textstring", Description = "Enter the alias of the tab whos properties should be displayed. If left blank, the first tab on the doc type will be used.")]
            public string TabAlias { get; set; }

            [PreValueField("nameTemplate", "Name Template", "textstring", Description = "Enter an angular expression to evaluate against each item for its name.")]
            public string NameTemplate { get; set; }

            [PreValueField("minItems", "Min Items", "number", Description = "Set the minimum number of items allowed.")]
            public string MinItems { get; set; }

            [PreValueField("maxItems", "Max Items", "number", Description = "Set the maximum number of items allowed.")]
            public string MaxItems { get; set; }

            [PreValueField("confirmDeletes", "Confirm Deletes", "boolean", Description = "Set whether item deletions should require confirming.")]
            public string ConfirmDeletes { get; set; }

            [PreValueField("hideLabel", "Hide Label", "boolean", Description = "Set whether to hide the editor label and have the list take up the full width of the editor window.")]
            public string HideLabel { get; set; }
        }

        #endregion

        #region Value Editor

        protected override PropertyValueEditor CreateValueEditor()
		{
            return new NestedContentPropertyValueEditor(base.CreateValueEditor());
		}

        internal class NestedContentPropertyValueEditor : PropertyValueEditorWrapper
        {
            public NestedContentPropertyValueEditor(PropertyValueEditor wrapped)
                : base(wrapped)
            {
                Validators.Add(new NestedContentValidator());
            }

            internal ServiceContext Services
            {
                get { return ApplicationContext.Current.Services; }
            }

            public override void ConfigureForDisplay(PreValueCollection preValues)
            {
                base.ConfigureForDisplay(preValues);

                var asDictionary = preValues.AsPreValueDictionary();
                if (asDictionary.ContainsKey("hideLabel"))
                {
                    var boolAttempt = asDictionary["hideLabel"].TryConvertTo<bool>();
                    if (boolAttempt.Success)
                    {
                        HideLabel = boolAttempt.Result;
                    }
                }
            }

            #region DB to String

            public override string ConvertDbToString(Property property, PropertyType propertyType, IDataTypeService dataTypeService)
            {
                // Convert / validate value
                if (property.Value == null || string.IsNullOrWhiteSpace(property.Value.ToString()))
                    return string.Empty;

                var value = JsonConvert.DeserializeObject<List<object>>(property.Value.ToString());
                if (value == null)
                    return string.Empty;

                // Process value
                for (var i = 0; i < value.Count; i++)
                {
                    var o = value[i];
                    var propValues = ((JObject)o);

                    var contentType = NestedContentHelper.GetContentTypeFromItem(propValues);
                    if(contentType == null)
                    {
                        continue;
                    }

                    var propValueKeys = propValues.Properties().Select(x => x.Name).ToArray();

                    foreach (var propKey in propValueKeys)
                    {
                        var propType = contentType.PropertyTypes.FirstOrDefault(x => x.Alias == propKey);
                        if (propType == null)
                        {
                            if(IsSystemPropertyKey(propKey) == false)
                            {
                                // Property missing so just delete the value
                                propValues[propKey] = null;
                            }
                        }
                        else
                        {
                            // Create a fake property using the property abd stored value
                            var prop = new Property(propType, propValues[propKey] == null ? null : propValues[propKey].ToString());

                            // Lookup the property editor
                            var propEditor = PropertyEditorResolver.Current.GetByAlias(propType.PropertyEditorAlias);

                            // Get the editor to do it's conversion, and store it back
                            propValues[propKey] = propEditor.ValueEditor.ConvertDbToString(prop, propType,
                                ApplicationContext.Current.Services.DataTypeService);
                        }

                    }
                }

                // Update the value on the property
                property.Value = JsonConvert.SerializeObject(value);

                // Pass the call down
                return base.ConvertDbToString(property, propertyType, dataTypeService);
            }

            #endregion

            #region DB to Editor

            public override object ConvertDbToEditor(Property property, PropertyType propertyType, IDataTypeService dataTypeService)
            {
                if (property.Value == null || string.IsNullOrWhiteSpace(property.Value.ToString()))
                    return string.Empty;

                var value = JsonConvert.DeserializeObject<List<object>>(property.Value.ToString());
                if (value == null)
                    return string.Empty;

                // Process value
                for (var i = 0; i < value.Count; i++)
                {
                    var o = value[i];
                    var propValues = ((JObject)o);

                    var contentType = NestedContentHelper.GetContentTypeFromItem(propValues);
                    if(contentType == null)
                    {
                        continue;
                    }

                    var propValueKeys = propValues.Properties().Select(x => x.Name).ToArray();

                    foreach (var propKey in propValueKeys)
                    {
                        var propType = contentType.PropertyTypes.FirstOrDefault(x => x.Alias == propKey);
                        if (propType == null)
                        {
                            if(IsSystemPropertyKey(propKey) == false)
                            {
                                // Property missing so just delete the value
                                propValues[propKey] = null;
                            }
                        }
                        else
                        {
                            // Create a fake property using the property abd stored value
                            var prop = new Property(propType, propValues[propKey] == null ? null : propValues[propKey].ToString());

                            // Lookup the property editor
                            var propEditor = PropertyEditorResolver.Current.GetByAlias(propType.PropertyEditorAlias);

                            // Get the editor to do it's conversion
                            var newValue = propEditor.ValueEditor.ConvertDbToEditor(prop, propType,
                                ApplicationContext.Current.Services.DataTypeService);

                            // Store the value back
                            propValues[propKey] = (newValue == null) ? null : JToken.FromObject(newValue);
                        }

                    }
                }

                // Update the value on the property
                property.Value = JsonConvert.SerializeObject(value);

                // Pass the call down
                return base.ConvertDbToEditor(property, propertyType, dataTypeService);
            }

            #endregion

            #region Editor to DB

            public override object ConvertEditorToDb(ContentPropertyData editorValue, object currentValue)
            {
                if (editorValue.Value == null || string.IsNullOrWhiteSpace(editorValue.Value.ToString()))
                    return null;

                var value = JsonConvert.DeserializeObject<List<object>>(editorValue.Value.ToString());
                if (value == null)
                    return string.Empty;

                // Process value
                for (var i = 0; i < value.Count; i++)
                {
                    var o = value[i];
                    var propValues = ((JObject)o);

                    var contentType = NestedContentHelper.GetContentTypeFromItem(propValues);
                    if(contentType == null)
                    {
                        continue;
                    }

                    var propValueKeys = propValues.Properties().Select(x => x.Name).ToArray();

                    foreach (var propKey in propValueKeys)
                    {
                        var propType = contentType.PropertyTypes.FirstOrDefault(x => x.Alias == propKey);
                        if (propType == null)
                        {
                            if(IsSystemPropertyKey(propKey) == false)
                            {
                                // Property missing so just delete the value
                                propValues[propKey] = null;
                            }
                        }
                        else
                        {
                            // Fetch the property types prevalue
                            var propPreValues = Services.DataTypeService.GetPreValuesCollectionByDataTypeId(
                                    propType.DataTypeDefinitionId);

                            // Lookup the property editor
                            var propEditor = PropertyEditorResolver.Current.GetByAlias(propType.PropertyEditorAlias);

                            // Create a fake content property data object
                            var contentPropData = new ContentPropertyData(
                                propValues[propKey] == null ? null : propValues[propKey].ToString(), propPreValues,
                                new Dictionary<string, object>());

                            // Get the property editor to do it's conversion
                            var newValue = propEditor.ValueEditor.ConvertEditorToDb(contentPropData, propValues[propKey]);

                            // Store the value back
                            propValues[propKey] = (newValue == null) ? null : JToken.FromObject(newValue);
                        }

                    }
                }

                return JsonConvert.SerializeObject(value);
            }

            #endregion
        }

        internal class NestedContentValidator : IPropertyValidator
        {
            public IEnumerable<ValidationResult> Validate(object rawValue, PreValueCollection preValues, PropertyEditor editor)
            {
                var value = JsonConvert.DeserializeObject<List<object>>(rawValue.ToString());
                if (value == null)
                    yield break;

                for (var i = 0; i < value.Count; i++)
                {
                    var o = value[i];
                    var propValues = ((JObject)o);

                    var contentType = NestedContentHelper.GetContentTypeFromItem(propValues);
                    if(contentType == null)
                    {
                        continue;
                    }

                    var propValueKeys = propValues.Properties().Select(x => x.Name).ToArray();

                    foreach (var propKey in propValueKeys)
                    {
                        var propType = contentType.PropertyTypes.FirstOrDefault(x => x.Alias == propKey);
                        if (propType != null)
                        {
                            // It would be better to pass this off to the individual property editors
                            // to validate themselves and pass the result down, however a lot of the
                            // validation checking code in core seems to be internal so for now we'll
                            // just replicate the mandatory / regex validation checks ourselves.
                            // This does of course mean we will miss any custom validators a property
                            // editor may have registered by itself, and it also means we can only
                            // validate to a single depth so having a complex property editor in a 
                            // doc type could get passed validation if it can't be validated from it's
                            // stored value alone.

                            // Check mandatory
                            if (propType.Mandatory)
                            {
                                if (propValues[propKey] == null)
                                    yield return new ValidationResult("Item " + (i + 1) + " '" + propType.Name + "' cannot be null", new[] { propKey });
                                else if (propValues[propKey].ToString().IsNullOrWhiteSpace())
                                    yield return new ValidationResult("Item " + (i + 1) + " '" + propType.Name + "' cannot be empty", new[] { propKey });
                            }

                            // Check regex
                            if (!propType.ValidationRegExp.IsNullOrWhiteSpace()
                                && propValues[propKey] != null && !propValues[propKey].ToString().IsNullOrWhiteSpace())
                            {
                                var regex = new Regex(propType.ValidationRegExp);
                                if (!regex.IsMatch(propValues[propKey].ToString()))
                                {
                                    yield return new ValidationResult("Item " + (i + 1) + " '" + propType.Name + "' is invalid, it does not match the correct pattern", new[] { propKey });
                                }
                            }
                        }
                    }
                }
            }
        }

        #endregion

        private static bool IsSystemPropertyKey(string propKey)
        {
            return propKey == "name" || propKey == "contentTypeAlias";
        }
	}
}
