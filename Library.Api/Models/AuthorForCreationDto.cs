using System;

namespace Library.Api.Models
{
   public class AuthorForCreationDto
   {
      public string FirstName { get; set; }
      public string LastName { get; set; }
      public DateTimeOffset DateOfBirth { get; set; }
      public string Genre { get; set; }
   }
}