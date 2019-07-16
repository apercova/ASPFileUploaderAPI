using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ASPFileUploaderAPI.ValueObject
{
    public class FormFileResponseVO : ValueObject
    {
        private FormFileResponseVO() { }
        public FormFileResponseVO(string Name, string FileName, long? Length, string Sha1, string Uri, string InfoUri)
        {
            this.Name = Name;
            this.FileName = FileName;
            this.Length = Length != null ? Length : -1;
            this.Sha1 = Sha1;
            this.Uri = Uri;
            this.InfoUri = InfoUri;
        }

        public string Name { get; }
        public string FileName { get; }
        public long? Length { get; }
        public string Sha1 { get; }
        public string Uri { get; }
        public string InfoUri { get; }

        protected override IEnumerable<object> GetAtomicValues()
        {
            yield return Name;
            yield return FileName;
            yield return Uri;
            yield return InfoUri;
            yield return Length;
            yield return Sha1;
        }
    }
}