using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Library.Api.Models.AuthorDtos
{
   //This is version 2 of our class
   public class AuthorForCreationWithDateOfDeathDto
   {
       public string FirstName { get; set; }
       public string LastName { get; set; }
       public DateTimeOffset DateOfBirth { get; set; }
       public DateTimeOffset? DateOfDeath { get; set; }
       public string Genre { get; set; }
   }
}
