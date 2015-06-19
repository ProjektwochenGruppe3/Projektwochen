using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Shapes;

namespace Editor
{
    public class DockTag
    {
        public Line DockLine {get;set;}
        public bool IsEnd { get; set; }
        public bool IsInput { get; set; }
        public string DataType { get; set; }
        public Ellipse OtherDockPoint { get; set; }
        public bool IsReady { get; set; }
    }
}
