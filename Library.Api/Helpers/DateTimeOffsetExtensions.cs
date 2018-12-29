using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Library.Api.Helpers
{
   public static class DateTimeOffsetExtensions
   {
      public static int GetCurrentAge(this DateTimeOffset dateTimeOffset, DateTimeOffset? dateOfDeath)
      {
         var dateToCalculateUptill = DateTime.UtcNow;
         if (dateOfDeath != null)
         {
            dateToCalculateUptill = dateOfDeath.Value.UtcDateTime;
         }

         int age = dateToCalculateUptill.Year - dateTimeOffset.Year;

         if (dateToCalculateUptill < dateTimeOffset.AddYears(age))
         {
            age--;
         }

         return age;
      }
   }
}
