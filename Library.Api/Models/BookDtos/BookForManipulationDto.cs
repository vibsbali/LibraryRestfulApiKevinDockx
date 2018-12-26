using System.ComponentModel.DataAnnotations;

namespace Library.Api.Models.BookDtos
{
   public abstract class BookForManipulationDto
   {
      [Required(ErrorMessage = "You should fill out a title.")]
      [MaxLength(100, ErrorMessage = "The title shouldn't have more than 100 characters")]
      public virtual string Title { get; set; }

      [MaxLength(500, ErrorMessage = "The description shouldn't have more than 500 characters.")]
      public virtual string Description { get; set; }
   }
}
