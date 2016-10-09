using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GisSmartTools.RS
{
    public class GRS : ReferenceSystem
    {
        public GRS(OSGeo.OSR.SpatialReference sr) { this.spetialReference = sr; }
        public GRS() : this(null) { }
    }
}
