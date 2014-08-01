
namespace AMSAPP
{
    using System;
    using System.Collections.Generic;
    
    public partial class ComputerEvent
    {
        public int Id { get; set; }
        public string EventType { get; set; }
        public System.DateTime EventOn { get; set; }
    }
}
