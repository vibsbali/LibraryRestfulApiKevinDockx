﻿using System;
using System.Collections.Generic;
using AutoMapper;
using Library.Api.Entities;
using Library.Api.Models;
using Library.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace Library.Api.Controllers
{
    [Route("api/authors/{authorId}/books")]
    public class BooksController : ControllerBase
    {
        private readonly ILibraryRepository _libraryRepository;

        public BooksController(ILibraryRepository libraryRepository)
        {
            _libraryRepository = libraryRepository;
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
            return CreatedAtRoute("GetBookForAuthor", new {authorId = authorId, bookId = book.Id}, bookToReturn);
        }
    }
}
