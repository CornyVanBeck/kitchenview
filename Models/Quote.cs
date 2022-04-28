using System.Collections.Generic;
using System.Runtime.Serialization;

namespace kitchenview.Models
{
    [DataContract]
    public class QuoteOfTheDay : IQuote
    {
        [DataMember(Name = "quote", EmitDefaultValue = true, IsRequired = true)]
        public string Quote { get; set; }

        [DataMember(Name = "author", EmitDefaultValue = true, IsRequired = true)]
        public string Author { get; set; }

        [DataMember(Name = "lanuage", EmitDefaultValue = true)]
        public string? Language { get; set; }

        [DataMember(Name = "image", EmitDefaultValue = true)]
        public string? Image { get; set; }

        [DataMember(Name = "pk", EmitDefaultValue = false)]
        public int? Pk { get; set; }

        [DataMember(Name = "likes", EmitDefaultValue = false)]
        public int? Likes { get; set; }

        [DataMember(Name = "tags", EmitDefaultValue = true)]
        public IEnumerable<string>? tags { get; set; }
    }
}