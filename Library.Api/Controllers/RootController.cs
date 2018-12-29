using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Library.Api.Models.HateosLinks;
using Microsoft.AspNetCore.Mvc;

namespace Library.Api.Controllers
{
    [Route("api")]
    public class RootController : ControllerBase
    {
      [HttpGet(Name = "GetRoot")]
      public IActionResult GetRoot([FromHeader(Name = "Accept")] string mediaType)
      {
          if (mediaType == "application/vnd.excentric.hateoas+json")
          {
              var links = new List<LinkDto>();

              links.Add(
                  new LinkDto(Url.Link("GetRoot", new { }),
                      "self",
                      "GET"));

              links.Add(
                  new LinkDto(Url.Link("GetAuthors", new { }),
                      "authors",
                      "GET"));

              links.Add(
                  new LinkDto(Url.Link("CreateAuthor", new { }),
                      "create_author",
                      "POST"));

              return Ok(links);
          }

          return NoContent();
      }
   }
}
