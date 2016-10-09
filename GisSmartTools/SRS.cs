using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GisSmartTools.RS
{
    public class SRS : ReferenceSystem
    {
        
        public SRS(OSGeo.OSR.SpatialReference sr) { this.spetialReference = sr; }
        public SRS() : this(null) { }

    }
}
