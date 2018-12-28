using System.Collections.Generic;

namespace Library.Api.Models.HateosLinks
{
   public class LinkedCollectionResourceWrapperDto<T> : LinkedResourceBaseDto
       where T : LinkedResourceBaseDto
   {
       public IEnumerable<T> Value { get; set; }

       public LinkedCollectionResourceWrapperDto(IEnumerable<T> value)
       {
           Value = value;
       }
   }
}
