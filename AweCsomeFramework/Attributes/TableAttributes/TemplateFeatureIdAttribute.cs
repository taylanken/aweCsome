﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AweCsome.Attributes.TableAttributes
{
    public class TemplateFeatureIdAttribute:Attribute
    {
        public Guid TemplateFeatureId { get; set; }
        public TemplateFeatureIdAttribute(Guid templateFeatureId)
        {
            TemplateFeatureId = templateFeatureId;
        }
    }
}
