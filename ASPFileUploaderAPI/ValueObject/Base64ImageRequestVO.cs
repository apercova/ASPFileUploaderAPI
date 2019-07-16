using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace ASPFileUploaderAPI.ValueObject
{
    public class Base64ImageRequestVO : ValueObject
    {
        public Base64ImageRequestVO() { }

        [Required]
        public string Name { get; set; }
        [Required]
        public string Data { get; set; }
        public string FileName { get; set; }
        protected override IEnumerable<object> GetAtomicValues()
        {
            yield return Name;
            yield return Data;
            yield return FileName;
        }
    }
}