﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AweCsome.Attributes.IgnoreAttributes
{
    public class IgnoreOnCreationAttribute:Attribute
    {
        public bool IgnoreOnCreation { get; set; } = true;
    }
}
