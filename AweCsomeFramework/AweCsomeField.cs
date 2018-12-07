﻿using AweCsome.Attributes.FieldAttributes;
using AweCsome.Attributes.IgnoreAttributes;
using log4net;
using Microsoft.SharePoint.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace AweCsome
{
    public class AweCsomeField : IAweCsomeField
    {
        private ILog _log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        public const string SuffixId = "Id";
        public const string SuffixIds = "Ids";
        public const string Title = "Title";


        public void AddFieldToList(List sharePointList, PropertyInfo property, Dictionary<string, Guid> lookupTableIds)
        {
            var ignoreOnCreationAttribute = property.GetCustomAttribute<IgnoreOnCreationAttribute>();
            if (ignoreOnCreationAttribute != null && ignoreOnCreationAttribute.IgnoreOnCreation) return;
            var addToDefaultViewAttribute = property.GetCustomAttribute<AddToDefaultViewAttribute>();


            string fieldXml = GetFieldCreationXml(property, lookupTableIds);
            Field field = sharePointList.Fields.AddFieldAsXml(fieldXml, addToDefaultViewAttribute != null, AddFieldOptions.AddFieldInternalNameHint);
        }

        private string GetFieldCreationXml(PropertyInfo property, Dictionary<string, Guid> lookupTableIds)
        {
            Type propertyType = property.PropertyType;

            string internalName = EntityHelper.GetInternalNameFromProperty(property);
            string displayName = EntityHelper.GetDisplayNameFromProperty(property);
            string description = EntityHelper.GetDescriptionFromEntityType(propertyType);


            bool isRequired = PropertyIsRequired(property);
            bool isUnique = IsTrue(propertyType.GetCustomAttribute<UniqueAttribute>()?.IsUnique);
            FieldType fieldType = EntityHelper.GetFieldType(property);
            bool isMulti = IsMulti(propertyType);
            if (fieldType == FieldType.Lookup) EntityHelper.RemoveLookupIdFromFieldName(isMulti, ref internalName, ref displayName);

            GetFieldCreationAdditionalXmlForFieldType(fieldType, property, lookupTableIds, out string fieldAttributes, out string fieldAdditional);
            string fieldTypeString = fieldType.ToString();
            if (fieldAttributes == null) fieldAttributes = string.Empty;
            if (fieldAttributes == null) fieldAdditional = string.Empty;
            if (isMulti)
            {
                if (fieldType != FieldType.Choice) fieldAttributes += " Mult='TRUE'";
                fieldTypeString += "Multi";
            }
            var indexAttribute = property.GetCustomAttribute<IndexAttribute>();
            if (indexAttribute != null) fieldAttributes += " Indexed='TRUE'";

            string csomCreateCaml = $"<Field Type='{fieldTypeString}' Name='{internalName}' DisplayName='{displayName}' StaticName='{internalName}'";
            if (isRequired) csomCreateCaml += " Required='TRUE'";
            if (isUnique) csomCreateCaml += " EnforceUniqueValues='TRUE'";
            csomCreateCaml += $" {fieldAttributes}";
            if (string.IsNullOrWhiteSpace(fieldAdditional))
            {
                csomCreateCaml += "/>";
            }
            else
            {
                csomCreateCaml += ">";
                csomCreateCaml += fieldAdditional;
                csomCreateCaml += "</Field>";
            }
            return csomCreateCaml;
        }

        private void GetFieldCreationAdditionalXmlForFieldType(FieldType fieldType, PropertyInfo property, Dictionary<string, Guid> lookupTableIds, out string fieldAttributes, out string fieldAdditional)
        {
            fieldAttributes = string.Empty;
            fieldAdditional = string.Empty;
            switch (fieldType)
            {

                case FieldType.Boolean:
                    GetFieldCreationDetailsBoolean(property, out fieldAdditional);
                    break;
                case FieldType.Choice:
                    GetFieldCreationDetailsChoice(property, out fieldAttributes, out fieldAdditional);
                    break;
                case FieldType.Currency:
                    GetFieldCreationDetailsCurrency(property, out fieldAttributes);
                    break;
                case FieldType.DateTime:
                    GetFieldCreationDetailsDateTime(property, out fieldAttributes, out fieldAdditional);
                    break;
                case FieldType.Lookup:
                    GetFieldCreationDetailsLookup(property, lookupTableIds, out fieldAttributes);
                    break;
                case FieldType.Note:
                    GetFieldCreationDetailsNote(property, out fieldAttributes);
                    break;

                case FieldType.Number:
                    GetFieldCreationDetailsNumber(property, out fieldAttributes);
                    break;
                case FieldType.Text:
                    GetFieldCreationDetailsText(property, out fieldAttributes, out fieldAdditional);
                    break;
                case FieldType.URL:
                    GetFieldCreationDetailsUrl(property, out fieldAttributes);
                    break;
                case FieldType.User:
                    GetFieldCreationDetailsUser(property, out fieldAttributes);
                    break;
                default:
                    throw new NotImplementedException($"FieldType {fieldType} is unexpected and cannot be created");


            }
        }

        #region FieldCreationProperties


        private void GetFieldCreationDetailsBoolean(PropertyInfo property, out string fieldAdditional)
        {
            fieldAdditional = null;
            var booleanAttribute = property.GetCustomAttribute<BooleanAttribute>();
            if (booleanAttribute != null) fieldAdditional = $"<Default>{(booleanAttribute.DefaultValue ? "1" : "0")}</Default>";
        }



        private void GetFieldCreationDetailsChoice(PropertyInfo property, out string fieldAttributes, out string fieldAdditional)
        {
            fieldAttributes = null;
            fieldAdditional = null;
            string[] choices = null;
            var choiceAttribute = property.GetCustomAttribute<ChoiceAttribute>();
            if (choiceAttribute != null)
            {
                fieldAttributes = $"Format='{choiceAttribute.DisplayChoices}'";
                if (choiceAttribute.Choices != null) choices = choiceAttribute.Choices;
                if (choiceAttribute.DefaultValue != null) fieldAdditional = $"<Default>{choiceAttribute.DefaultValue}</Default>";
                if (choiceAttribute.AllowFillIn) fieldAttributes += " FillInChoice='TRUE'";
            }
            Type propertyType = property.PropertyType;
            if (choices == null) choices = propertyType.GetEnumDisplaynames().Values.ToArray();
            if (choices == null && propertyType.IsEnum) choices = Enum.GetNames(propertyType);
            string choiceXml = string.Empty;
            if (choices != null)
            {
                foreach (string choice in choices)
                {
                    choiceXml += $"<CHOICE>{choice}</CHOICE>";
                }
            }
            fieldAdditional += $"<CHOICES>{choiceXml}</CHOICES>";
        }

        private void GetFieldCreationDetailsCurrency(PropertyInfo property, out string fieldAttributes)
        {
            fieldAttributes = null;
            var currencyAttribute = property.GetCustomAttribute<CurrencyAttribute>();
            if (currencyAttribute != null)
            {
                fieldAttributes = $"Commas='{(currencyAttribute.NumberOfDecimalPlaces == null || currencyAttribute.NumberOfDecimalPlaces == 0 ? "FALSE" : "TRUE")}'";
                if (currencyAttribute.Min.HasValue) fieldAttributes += $" Min={currencyAttribute.Min}";
                if (currencyAttribute.Max.HasValue) fieldAttributes += $" Max={currencyAttribute.Max}";
                if (currencyAttribute.CurrencyLocaleId != null) fieldAttributes += $" LCID='{currencyAttribute.CurrencyLocaleId}'";
            }
        }

        private void GetFieldCreationDetailsDateTime(PropertyInfo property, out string fieldAttributes, out string fieldAdditional)
        {
            fieldAttributes = null;
            fieldAdditional = null;

            var dateTimeAttribute = property.GetCustomAttribute<DateTimeAttribute>();
            if (dateTimeAttribute != null)
            {
                fieldAttributes = $"Format='{dateTimeAttribute.DateTimeFormat}'";
                if (dateTimeAttribute.DefaultValue != null) fieldAdditional = $"<Default>{dateTimeAttribute.DefaultValue}</Default>";
                // TODO: FriendlyFormat
            }
        }

        public static string GetLookupListName(PropertyInfo property, out string fieldname)
        {
            fieldname = "Title";
            var lookupAttribute = property.GetCustomAttribute<LookupBaseAttribute>(true);
            if (lookupAttribute == null)
            {
                Type propertyType = property.PropertyType;

                if (propertyType.IsArray)
                {
                    propertyType = propertyType.GetElementType();
                }
                if (propertyType.GetProperty(SuffixId) != null)
                {
                    return propertyType.Name;
                }
            }
            else
            {
                fieldname = lookupAttribute.Field;
                return lookupAttribute.List;
            }
            return null;
        }

        private void GetFieldCreationDetailsLookup(PropertyInfo property, Dictionary<string, Guid> lookupTableIds, out string fieldAttributes)
        {
            string list = GetLookupListName(property, out string field);
            if (list == null) throw new Exception("Missing list-information for Lookup-Field");
            fieldAttributes = $"List='{lookupTableIds[list]}' ShowField='{field}'";
        }

        private void GetFieldCreationDetailsNote(PropertyInfo property, out string fieldAttributes)
        {
            var noteAttribute = property.GetCustomAttribute<NoteAttribute>();
            // Can't be null (And if it is this SHOULD throw an error)
            fieldAttributes = $"NumLines='{noteAttribute.NumberOfLinesForEditing}' RichText='{noteAttribute.AllowRichText}'";
            // TODO: AppendChangesToExistingText
        }

        private void GetFieldCreationDetailsNumber(PropertyInfo property, out string fieldAttributes)
        {
            fieldAttributes = null;
            var numberAttribute = property.GetCustomAttribute<NumberAttribute>();
            if (numberAttribute != null)
            {
                fieldAttributes = $"Commas='{(numberAttribute.NumberOfDecimalPlaces == null || numberAttribute.NumberOfDecimalPlaces == 0 ? "FALSE" : "TRUE")}'";
                if (numberAttribute.MinHasValue) fieldAttributes += $" Min={numberAttribute.Min}";
                if (numberAttribute.MaxHasValue) fieldAttributes += $" Max={numberAttribute.Max}";
                if (numberAttribute.ShowAsPercentage) fieldAttributes += $" Percentage='TRUE'";
            }
        }

        private void GetFieldCreationDetailsText(PropertyInfo property, out string fieldAttributes, out string fieldAdditional)
        {
            fieldAttributes = null;
            fieldAdditional = null;
            var textAttribute = property.GetCustomAttribute<TextAttribute>();
            if (textAttribute != null)
            {
                fieldAttributes = $"MaxLength={textAttribute.MaxCharacters}";
                if (textAttribute.DefaultValue != null) fieldAdditional = $"<Default>{textAttribute.DefaultValue}</Default>";
            }
        }

        private void GetFieldCreationDetailsUrl(PropertyInfo property, out string fieldAttributes)
        {
            fieldAttributes = null;
            var urlAttribute = property.GetCustomAttribute<UrlAttribute>();
            if (urlAttribute != null)
            {
                fieldAttributes = $"Format='{urlAttribute.UrlFieldFormatType}'";
            }
            else
            {
                fieldAttributes = $"Format='{nameof(UrlFieldFormatType.Hyperlink)}'";
            }
        }

        private void GetFieldCreationDetailsUser(PropertyInfo property, out string fieldAttributes)
        {
            fieldAttributes = null;
            var userAttribute = property.GetCustomAttribute<UserAttribute>();
            if (userAttribute != null)
            {
                fieldAttributes = $"UserSelectionMode='{userAttribute.FieldUserSelectionMode}'";
                if (userAttribute.UserSelectionScope.HasValue) fieldAttributes += $" UserSelectionScope='{userAttribute.UserSelectionScope}'";
            }
        }

        #endregion FieldCreationProperties

        private bool IsMulti(Type propertyType)
        {
            return propertyType.IsArray || propertyType.IsGenericList() || propertyType.IsDictionary();
        }

        private bool IsTrue(bool? value)
        {
            return value.HasValue && value.Value;
        }

        private bool PropertyIsRequired(PropertyInfo property)
        {
            var isRequiredAttribute = property.GetCustomAttribute<RequiredAttribute>();
            if (isRequiredAttribute != null) return isRequiredAttribute.IsRequired;

            Type propertyType = property.PropertyType;
            if (propertyType.IsGenericType)
            {
                return propertyType.GetGenericTypeDefinition() != typeof(Nullable<>);
            }
            return false;
        }

        private string GetEnumCaml(Type enumType)
        {
            string[] enumValues = Enum.GetNames(enumType);
            string enumCaml = "<CHOICES>";
            foreach (var enumValue in enumValues)
            {
                enumCaml += $"<CHOICE>{enumValue}</CHOICE>";
            }
            enumCaml += "</CHOICES>\n";
            return enumCaml;
        }

        public void ChangeDisplaynameFromField(List sharePointList, PropertyInfo property)
        {
            var internalName = EntityHelper.GetInternalNameFromProperty(property);
            var displayName = EntityHelper.GetDisplayNameFromProperty(property);

            var fieldToChange = sharePointList.Fields.GetByInternalNameOrTitle(internalName);
            fieldToChange.Title = displayName;
            fieldToChange.Update();
        }

        public Field GetFieldDefinition(List sharePointList, PropertyInfo property)
        {
            return sharePointList.Fields.GetByInternalNameOrTitle(EntityHelper.GetInternalNameFromProperty(property));
        }
    }
}
