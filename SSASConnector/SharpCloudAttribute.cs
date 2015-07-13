using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SSASConnector
{
    public class SharpCloudAttribute
    {
        public string SharpCloudAttributeName { get; set; }
        public string CubeMemberName { get; set; }
        public int CubeTableMappingIndex { get; set; }
        public string AttributeDataType { get; set; }
        public int ArrayColumnIndex { get; set; }
    }
}
