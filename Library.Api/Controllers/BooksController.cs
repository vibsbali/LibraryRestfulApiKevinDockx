using System;
using System.Collections.Generic;
using AutoMapper;
using Library.Api.Entities;
using Library.Api.Helpers;
using Library.Api.Models;
using Library.Api.Models.BookDtos;
using Library.Api.Services;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Library.Api.Controllers
{
   [Route("api/authors/{authorId}/books")]
   public class BooksController : ControllerBase
   {
      private readonly ILibraryRepository _libraryRepository;
      private readonly ILogger<BooksController> _logger;

      public BooksController(ILibraryRepository libraryRepository, ILogger<BooksController> logger)
      {
         _libraryRepository = libraryRepository;
         _logger = logger;
      }

      [HttpGet]
      public IActionResult GetBooksForAuthor(Guid authorId)
      {
         if (!_libraryRepository.AuthorExists(authorId))
         {
            return NotFound();
         }

         var booksForAuthorFromRepo = _libraryRepository.GetBooksForAuthor(authorId);

         var booksForAuthor = Mapper.Map<IEnumerable<BookDto>>(booksForAuthorFromRepo);

         _logger.LogInformation(200, $"Return books for author {authorId}");
         return Ok(booksForAuthor);
      }

      [HttpGet("{bookId}", Name = "GetBookForAuthor")]
      public IActionResult GetBookForAuthor(Guid authorId, Guid bookId)
      {
         var bookFromRepo = _libraryRepository.GetBookForAuthor(authorId, bookId);

         if (bookFromRepo == null)
         {
            return NotFound();
         }

         var result = Mapper.Map<BookDto>(bookFromRepo);
         return Ok(result);
      }

      [HttpPost]
      //[FromBody] signifies that the incoming request should be de-serialised into bookDto dto
      public IActionResult CreateBookForAuthor(Guid authorId, [FromBody] BookForCreationDto bookDto)
      {
         if (bookDto == null)
         {
            return BadRequest();
         }

         if (bookDto.Description == bookDto.Title)
         {
            ModelState.AddModelError(nameof(BookForCreationDto), "The provided description should be different from the title.");
         }

         if (!ModelState.IsValid)
         {
            // return 422
            return new UnprocessableEntityObjectResult(ModelState);
         }

         if (!_libraryRepository.AuthorExists(authorId))
         {
            return NotFound();
         }

         var book = Mapper.Map<Book>(bookDto);

         _libraryRepository.AddBookForAuthor(authorId, book);
         if (!_libraryRepository.Save())
         {
            throw new Exception($"Creating a book for author {authorId} failed on save.");
         }

         var bookToReturn = Mapper.Map<BookDto>(book);
         return CreatedAtRoute("GetBookForAuthor", new { authorId = authorId, bookId = book.Id }, bookToReturn);
      }

      [HttpDelete("{id}")]
      public IActionResult DeleteBookForAuthor(Guid authorId, Guid id)
      {
         if (!_libraryRepository.AuthorExists(authorId))
         {
            return NotFound();
         }

         var bookForAuthorFromRepo = _libraryRepository.GetBookForAuthor(authorId, id);
         if (bookForAuthorFromRepo == null)
         {
            return NotFound();
         }

         //EF has cascade delete on by default. When we delete author the books are deleted automatically
         _libraryRepository.DeleteBook(bookForAuthorFromRepo);

         if (!_libraryRepository.Save())
         {
            throw new Exception($"Deleting book {id} for author {authorId} failed on save.");
         }

         _logger.LogInformation(100, $"Book {id} for author {authorId} was deleted.");

         return NoContent();
      }

      [HttpPut("{id}")]
      public IActionResult UpdateBookForAuthor(Guid authorId, Guid id, [FromBody] BookForUpdateDto bookDto)
      {
         if (bookDto == null)
         {
            return BadRequest();
         }

         if (bookDto.Title == bookDto.Description)
         {
            ModelState.AddModelError(nameof(BookForUpdateDto), "The provided description should be different from the title.");
         }

         if (!ModelState.IsValid)
         {
            // return 422
            return new UnprocessableEntityObjectResult(ModelState);
         }

         if (!_libraryRepository.AuthorExists(authorId))
         {
            return NotFound();
         }

         var bookForAuthorFromRepo = _libraryRepository.GetBookForAuthor(authorId, id);
         if (bookForAuthorFromRepo == null)
         {
            return NotFound();
         }

         //this will copy the values from bookDto to book we found in the repo
         Mapper.Map(bookDto, bookForAuthorFromRepo);

         _libraryRepository.UpdateBookForAuthor(bookForAuthorFromRepo);

         if (!_libraryRepository.Save())
         {
            throw new Exception($"Updating book {id} for author {authorId} failed on save.");
         }

         return NoContent();
      }

      //For patch read JsonPatch Specification
      //Content-Type in Headers is application/json-patch+json and request looks like so for replacing title
      /*
       *[
            {
	            "op": "replace",
	            "path": "/title",
	            "value": "A Game of Thrones"
            }	
         ]
       *
       */
      [HttpPatch("{id}")]
      public IActionResult PartiallyUpdateBookForAuthor(Guid authorId, Guid id,
         [FromBody] JsonPatchDocument<BookForUpdateDto> patchDocument)
      {
         if (patchDocument == null)
         {
            return BadRequest();
         }

         if (!_libraryRepository.AuthorExists(authorId))
         {
            return NotFound();
         }

         var bookForAuthorFromRepo = _libraryRepository.GetBookForAuthor(authorId, id);
         if (bookForAuthorFromRepo == null)
         {
            return NotFound();
         }

         //Apply Patch Document
         BookForUpdateDto bookToPatch = Mapper.Map<BookForUpdateDto>(bookForAuthorFromRepo);
         
         //add validation
         //since we are using JsonPatchDocument and not BookForUpdateDto we need to add custom validation
         //any errors on ModelState will apply to patch document
         patchDocument.ApplyTo(bookToPatch, ModelState);
         if (bookToPatch.Title == bookToPatch.Description)
         {
            ModelState.AddModelError(nameof(BookForUpdateDto), "The provided description should be different from the title.");
         }

         //This will trigger validation and any errors will end up in ModelState
         TryValidateModel(bookToPatch);

         if (!ModelState.IsValid)
         {
            return new UnprocessableEntityObjectResult(ModelState);
         }

         Mapper.Map(bookToPatch, bookForAuthorFromRepo);
         _libraryRepository.UpdateBookForAuthor(bookForAuthorFromRepo);

         if (!_libraryRepository.Save())
         {
            throw new Exception($"Patching book {id} for author {authorId} failed on save.");
         }

         return NoContent();
      }
   }
}
