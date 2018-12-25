using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Library.Api.Entities;
using Library.Api.Models;
using Library.Api.Services;
using Microsoft.AspNetCore.Http;
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

      //By Assigning Name to this method we can use it by name 
      [HttpGet("{id}", Name = "GetAuthor")]
      public IActionResult GetAuthor(Guid id)
      {
         var author = _libraryRepository.GetAuthor(id);

         if (author == null)
         {
            return NotFound();
         }

         var authorDto = Mapper.Map<AuthorDto>(author);
         return Ok(authorDto);
      }

      [HttpPost]
      public IActionResult CreateAuthor([FromBody] AuthorForCreationDto author)
      {
         if (author == null)
         {
            return BadRequest();
         }

         var authorEntity = Mapper.Map<Author>(author);
         _libraryRepository.AddAuthor(authorEntity);
         if (!_libraryRepository.Save())
         {
            //global exception handler will catch the error
            throw new ApplicationException("Creating an author failed on save");
            //return StatusCode(500, "A problem happened with handling your request");
         }

         var authorToReturn = Mapper.Map<AuthorDto>(authorEntity);

         //GetAuthor is name given to method GetAuthor
         //Id is the Id for Author
         //authorToReturn will be serialised in the body
         return CreatedAtRoute("GetAuthor", new {id = authorToReturn.Id}, authorToReturn);
      }

      [HttpPost("{id}")]
      public IActionResult BlockAuthorCreation(Guid id)
      {
         if (_libraryRepository.AuthorExists(id))
         {
            return new StatusCodeResult(StatusCodes.Status409Conflict);
         }

         return NotFound();
      }

      [HttpDelete("{id}")]
      public IActionResult DeleteAuthor(Guid id)
      {
         var authorFromRepo = _libraryRepository.GetAuthor(id);
         if (authorFromRepo == null)
         {
            return NotFound();
         }

         _libraryRepository.DeleteAuthor(authorFromRepo);

         if (!_libraryRepository.Save())
         {
            throw new Exception($"Deleting author {id} failed on save.");
         }

         return NoContent();
      }
   }
}
