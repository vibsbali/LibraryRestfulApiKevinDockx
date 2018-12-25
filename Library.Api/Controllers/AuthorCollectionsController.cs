using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Library.Api.Entities;
using Library.Api.Helpers;
using Library.Api.Models;
using Library.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;

namespace Library.Api.Controllers
{
    [Route("api/authorcollections")]
    public class AuthorCollectionsController : ControllerBase
    {
        private readonly ILibraryRepository _libraryRepository;

        public AuthorCollectionsController(ILibraryRepository libraryRepository)
        {
            _libraryRepository = libraryRepository;
        }

      [HttpPost]
      public IActionResult CreateAuthorCollection([FromBody] IEnumerable<AuthorForCreationDto> authorCollection)
      {
          if (authorCollection == null)
          {
              return BadRequest();
          }

          var authorEntities = Mapper.Map<IEnumerable<Author>>(authorCollection);

          foreach (var author in authorEntities)
          {
              _libraryRepository.AddAuthor(author);
          }

          if (!_libraryRepository.Save())
          {
              throw new Exception("Creating an author collection failed on save.");
          }

          var authorCollectionToReturn = Mapper.Map<IEnumerable<AuthorDto>>(authorEntities);
          var idsAsString = string.Join(",",
              authorCollectionToReturn.Select(a => a.Id));

          return CreatedAtRoute("GetAuthorCollection",
              new { ids = idsAsString },
              authorCollectionToReturn);
         
      }

        // (key1,key2, ...)
        //We are using custom model binder called ArrayModelBinder 

        [HttpGet("({ids})", Name = "GetAuthorCollection")]
        public IActionResult GetAuthorCollection(
            [ModelBinder(BinderType = typeof(ArrayModelBinder))] IEnumerable<Guid> ids)
        {
            if (ids == null)
            {
                return BadRequest();
            }

            ids = ids.ToList();
            var authorEntities = _libraryRepository.GetAuthors(ids);

            if (ids.Count() != authorEntities.Count())
            {
                return NotFound();
            }

            var authorsToReturn = Mapper.Map<IEnumerable<AuthorDto>>(authorEntities);
            return Ok(authorsToReturn);
        }
   }
}
