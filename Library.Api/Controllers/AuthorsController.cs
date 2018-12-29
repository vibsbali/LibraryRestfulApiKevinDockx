using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Library.Api.Entities;
using Library.Api.Helpers;
using Library.Api.Models;
using Library.Api.Models.AuthorDtos;
using Library.Api.Models.HateosLinks;
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
      private readonly IPropertyMappingService _propertyMappingService;
      private readonly ITypeHelperService _typeHelperService;

      public AuthorsController(ILibraryRepository libraryRepository, ILogger<AuthorsController> logger,
         IPropertyMappingService propertyMappingService, ITypeHelperService typeHelperService)
      {
         _libraryRepository = libraryRepository;
         _logger = logger;
         _propertyMappingService = propertyMappingService;
         _typeHelperService = typeHelperService;
      }

      //Filtering: http://.../api/authors?genre=Fantasy
      //Searching: http://.../api/authors?searchQuery=King
      //Paging:    http://.../api/authors?pageNumber=1&pageSize=5
      //Ordering:  http://.../api/authors?orderBy=name
      //Sorting:   http://.../api/authors?orderBy=name
      //Shaping:   http://.../api/authors?fields=id,name (Data Shaping allow consumer to select resources fields)
      //TODO following
      //Include child resources : http://.../api/authors?expand=books  
      //Shape included child resources : http://.../api/authors?fields=id,name,books.title
      //Complex filters : http://.../api/authors?genere=contains('horror')
      [HttpGet(Name = "GetAuthors")]
      public IActionResult GetAuthors(AuthorsResourceParameters authorsResourceParameters,
            [FromHeader(Name = "Accept")] string mediaType)
      {
         if (!_propertyMappingService.ValidMappingExistsFor<AuthorDto, Author>(authorsResourceParameters.OrderBy))
         {
            return BadRequest("Invalid property in query string");
         }

         if (!_typeHelperService.TypeHasProperties<AuthorDto>(authorsResourceParameters.Fields))
         {
            return BadRequest("Invalid fields in query string");
         }

         var authorsFromRepo = _libraryRepository.GetAuthors(authorsResourceParameters);
         var authors = Mapper.Map<IEnumerable<AuthorDto>>(authorsFromRepo);
      

         //send json only when requested
         if (mediaType == "application/vnd.excentric.hateoas+json")
         {
            var paginationMetadata = new
            {
               totalCount = authorsFromRepo.TotalCount,
               pageSize = authorsFromRepo.PageSize,
               currentPage = authorsFromRepo.CurrentPage,
               totalPages = authorsFromRepo.TotalPages
            };

            Response.Headers.Add("X-Pagination",
               Newtonsoft.Json.JsonConvert.SerializeObject(paginationMetadata));

            var links = CreateLinksForAuthors(authorsResourceParameters, authorsFromRepo.HasNext,
               authorsFromRepo.HasPrevious);

            //returns an expando object
            var shapedAuthors = authors.ShapeData(authorsResourceParameters.Fields);

            var shapedAuthorsWithLinks = shapedAuthors.Select(author =>
            {
               var authorAsDictionary = author as IDictionary<string, object>;
               var authorLinks = CreateLinksForAuthor(
                  (Guid)authorAsDictionary["Id"], authorsResourceParameters.Fields);

               authorAsDictionary.Add("links", authorLinks);

               return authorAsDictionary;
            });

            //We could have used strongly typed like we did in books controller
            var linkedCollectionResource = new
            {
               value = shapedAuthorsWithLinks,
               links = links
            };
            
            return Ok(linkedCollectionResource);
         }

         //otherwise do not include hateos links
         var previousPageLink = authorsFromRepo.HasPrevious ?
            CreateAuthorsResourceUri(authorsResourceParameters,
               ResourceUriType.PreviousPage) : null;

         var nextPageLink = authorsFromRepo.HasNext ?
            CreateAuthorsResourceUri(authorsResourceParameters,
               ResourceUriType.NextPage) : null;

         var paginationMetadataWithLinks = new
         { 
            totalCount = authorsFromRepo.TotalCount,
            pageSize = authorsFromRepo.PageSize,
            currentPage = authorsFromRepo.CurrentPage,
            totalPages = authorsFromRepo.TotalPages,
            previousPageLink = previousPageLink,
            nextPageLink = nextPageLink,
         };

         Response.Headers.Add("X-Pagination",
            Newtonsoft.Json.JsonConvert.SerializeObject(paginationMetadataWithLinks));

         return Ok(authors.ShapeData(authorsResourceParameters.Fields));
      }

      private string CreateAuthorsResourceUri(
         AuthorsResourceParameters authorsResourceParameters,
         ResourceUriType type)
      {
         switch (type)
         {
            case ResourceUriType.PreviousPage:
               return Url.Link("GetAuthors",
                  new
                  {
                     fields = authorsResourceParameters.Fields,
                     orderby = authorsResourceParameters.OrderBy,
                     searchQuery = authorsResourceParameters.SearchQuery,
                     genre = authorsResourceParameters.Genre,
                     pageNumber = authorsResourceParameters.PageNumber - 1,
                     pageSize = authorsResourceParameters.PageSize
                  });
            case ResourceUriType.NextPage:
               return Url.Link("GetAuthors",
                  new
                  {
                     fields = authorsResourceParameters.Fields,
                     orderby = authorsResourceParameters.OrderBy,
                     searchQuery = authorsResourceParameters.SearchQuery,
                     genre = authorsResourceParameters.Genre,
                     pageNumber = authorsResourceParameters.PageNumber + 1,
                     pageSize = authorsResourceParameters.PageSize
                  });
            case ResourceUriType.Current:
            default:
               return Url.Link("GetAuthors",
                  new
                  {
                     fields = authorsResourceParameters.Fields,
                     orderby = authorsResourceParameters.OrderBy,
                     searchQuery = authorsResourceParameters.SearchQuery,
                     genre = authorsResourceParameters.Genre,
                     pageNumber = authorsResourceParameters.PageNumber,
                     pageSize = authorsResourceParameters.PageSize
                  });
         }
      }

      //Shaping:   http://.../api/authors/...{authorId}..?fields=id,name
      //By Assigning Name to this method we can use it by name 
      [HttpGet("{id}", Name = "GetAuthor")]
      public IActionResult GetAuthor(Guid id, [FromQuery] string fields)
      {
         if (!_typeHelperService.TypeHasProperties<AuthorDto>(fields))
         {
            return BadRequest("Invalid fields in query string");
         }

         var author = _libraryRepository.GetAuthor(id);

         if (author == null)
         {
            return NotFound();
         }

         var authorDto = Mapper.Map<AuthorDto>(author);

         var links = CreateLinksForAuthor(id, fields);

         var linkedResourceToReturn = authorDto.ShapeData(fields)
            as IDictionary<string, object>;

         linkedResourceToReturn.Add("links", links);

         return Ok(linkedResourceToReturn);
      }

      [HttpPost(Name = "CreateAuthor")]
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

         //we are passing null because no data shaping
         var links = CreateLinksForAuthor(authorToReturn.Id, null);

         var linkedResourceToReturn = authorToReturn.ShapeData(null)
            as IDictionary<string, object>;

         linkedResourceToReturn.Add("links", links);

         return CreatedAtRoute("GetAuthor", new { id = linkedResourceToReturn["id"] }, linkedResourceToReturn);

         //GetAuthor is name given to method GetAuthor
         //Id is the Id for Author
         //authorToReturn will be serialised in the body
         //return CreatedAtRoute("GetAuthor", new { id = authorToReturn.Id }, authorToReturn);
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

      [HttpDelete("{id}", Name = "DeleteAuthor")]
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

      private IEnumerable<LinkDto> CreateLinksForAuthor(Guid id, string fields)
      {
         var links = new List<LinkDto>();

         if (string.IsNullOrWhiteSpace(fields))
         {
            links.Add(
               new LinkDto(Url.Link("GetAuthor", new { id = id }),
                  "self",
                  "GET"));
         }
         else
         {
            links.Add(
               new LinkDto(Url.Link("GetAuthor", new { id = id, fields = fields }),
                  "self",
                  "GET"));
         }

         links.Add(
            new LinkDto(Url.Link("DeleteAuthor", new { id = id }),
               "delete_author",
               "DELETE"));

         links.Add(
            new LinkDto(Url.Link("CreateBookForAuthor", new { authorId = id }),
               "create_book_for_author",
               "POST"));

         links.Add(
            new LinkDto(Url.Link("GetBooksForAuthor", new { authorId = id }),
               "books",
               "GET"));

         return links;
      }

      private IEnumerable<LinkDto> CreateLinksForAuthors(
         AuthorsResourceParameters authorsResourceParameters,
         bool hasNext, bool hasPrevious)
      {
         var links = new List<LinkDto>();

         // self 
         links.Add(
            new LinkDto(CreateAuthorsResourceUri(authorsResourceParameters,
                  ResourceUriType.Current)
               , "self", "GET"));

         if (hasNext)
         {
            links.Add(
               new LinkDto(CreateAuthorsResourceUri(authorsResourceParameters,
                     ResourceUriType.NextPage),
                  "nextPage", "GET"));
         }

         if (hasPrevious)
         {
            links.Add(
               new LinkDto(CreateAuthorsResourceUri(authorsResourceParameters,
                     ResourceUriType.PreviousPage),
                  "previousPage", "GET"));
         }

         return links;
      }
   }
}
