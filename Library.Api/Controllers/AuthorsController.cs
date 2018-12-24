using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Library.Api.Models;
using Library.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Library.Api.Controllers
{
   [Route("api/authors")]
   public class AuthorsController : ControllerBase
   {
      private readonly ILibraryRepository _libraryRepository;
      private readonly ILogger<AuthorsController> _logger;

      public AuthorsController(ILibraryRepository libraryRepository, ILogger<AuthorsController> logger)
      {
         _libraryRepository = libraryRepository;
         _logger = logger;
      }

      [HttpGet]
      public IActionResult GetAuthors()
      {

         var authors = _libraryRepository.GetAuthors();
         var authorsDto = Mapper.Map<IEnumerable<AuthorDto>>(authors);

         return Ok(authorsDto.ToList());

         //exception has been factored into Startup.cs
         //return StatusCode(500, "An unexpected error has occured");

      }

      [HttpGet("{id}")]
      public IActionResult GetAuthors(Guid id)
      {
         var author = _libraryRepository.GetAuthor(id);

         if (author == null)
         {
            return NotFound();
         }

         var authorDto = Mapper.Map<AuthorDto>(author);
         return Ok(authorDto);
      }
   }
}
