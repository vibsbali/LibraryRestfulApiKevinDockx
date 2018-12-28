using System.Collections.Generic;

namespace Library.Api.Models.HateosLinks
{
    public abstract class LinkedResourceBaseDto
    {
        public List<LinkDto> Links { get; set; } = new List<LinkDto>();
    }
}
