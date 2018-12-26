using System;
using System.Collections.Generic;
using Library.Api.Models.BookDtos;

namespace Library.Api.Models
{
   public class AuthorForCreationDto
   {
      public string FirstName { get; set; }
      public string LastName { get; set; }
      public DateTimeOffset DateOfBirth { get; set; }
      public string Genre { get; set; }

      //following property is useful for creating books along with Author
      //this is for creating child objects along with parent objects
      public ICollection<BookForCreationDto> Books { get; set; }
         = new List<BookForCreationDto>();
   }
}