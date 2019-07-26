using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ASPFileUploaderAPI.ValueObject
{
    public class CompositeForm
    {
        public CompositeForm() { }

        public string name { get; set; }
        public string email { get; set; }
        public string photo { get; set; }
    }
}