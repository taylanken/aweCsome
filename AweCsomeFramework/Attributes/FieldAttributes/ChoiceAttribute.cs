﻿using Microsoft.SharePoint.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AweCsome.Attributes.FieldAttributes
{
    public class ChoiceAttribute : Attribute
    {


        public enum DisplayChoicesTypes { DropDown, RadioButtons, CheckBoxes }
        public string[] Choices { get; set; }
        public DisplayChoicesTypes DisplayChoices { get; set; }
        public bool AllowFillIn { get; set; }
        public string DefaultValue { get; set; }
        public const string AssociatedFieldType = nameof(FieldType.Choice);

        public ChoiceAttribute() { }
        public ChoiceAttribute(Type enumType) {
            Choices=Enum.GetNames(enumType);
        }
    }
}
