using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Library.Api.Helpers
{
   public class AuthorsResourceParameters
   {
      public const int MaxPageSize = 20;
      private const int MinPageSize = 5;

      public int PageNumber { get; set; } = 1;

      private int _pageSize = MinPageSize;

      public int PageSize
      {
         get => _pageSize;
         set => _pageSize = value > MaxPageSize ? MaxPageSize : value;
      }

      public string Genre { get; set; }

      public string SearchQuery { get; set; }
   }
}
