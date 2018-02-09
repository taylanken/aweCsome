﻿using AweCsomeO365.Attributes;
using AweCsomeO365.Attributes.TableAttributes;
using Microsoft.SharePoint.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace AweCsomeO365
{
    public static class EntityHelper
    {
        public static string GetInternalNameFromEntityType(Type entityType)
        {
            var internalNameAttribute = entityType.GetCustomAttribute<InternalNameAttribute>();
            return internalNameAttribute == null ? entityType.Name : internalNameAttribute.InternalName;
        }

        public static string GetDisplayNameFromEntityType(Type entityType)
        {
            var displayNameAttribute = entityType.GetCustomAttribute<DisplayNameAttribute>();
            return displayNameAttribute == null ? entityType.Name : displayNameAttribute.DisplayName;
        }

        public static int GetListTemplateType(Type entityType)
        {
            var listTemplateTypeAttribute = entityType.GetCustomAttribute<ListTemplateTypeAttribute>();
            return listTemplateTypeAttribute == null ? (int)ListTemplateType.GenericList : listTemplateTypeAttribute.TemplateTypeId;
        }


    }
}