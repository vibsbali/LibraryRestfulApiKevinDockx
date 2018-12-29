using System.Linq;
using Library.Api.Entities;
using Library.Api.Helpers;
using Library.Api.Models;
using Library.Api.Models.AuthorDtos;
using Library.Api.Models.BookDtos;
using Library.Api.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Internal;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Serialization;

namespace Library.Api
{
   public class Startup
   {
      public Startup(IConfiguration configuration)
      {
         Configuration = configuration;
      }

      public IConfiguration Configuration { get; }

      // This method gets called by the runtime. Use this method to add services to the container.
      public void ConfigureServices(IServiceCollection services)
      {
         services.AddMvc(setupAction =>
            {
               setupAction.ReturnHttpNotAcceptable = true;
               
               setupAction.InputFormatters.Add(new XmlDataContractSerializerInputFormatter());
               
               //add custom input media type for json and xml
               setupAction.InputFormatters
                  .OfType<JsonInputFormatter>().First()
                  .SupportedMediaTypes.Add("application/vnd.excentric.v2.hateoas+json");
               
               setupAction.InputFormatters
                  .OfType<XmlDataContractSerializerInputFormatter>().First()
                  .SupportedMediaTypes.Add("application/vnd.excentric.v2.hateoas+xml");

               setupAction.OutputFormatters.Add(new XmlDataContractSerializerOutputFormatter());
               //add custom media type
               setupAction.OutputFormatters
                  .OfType<JsonOutputFormatter>().First()
                  .SupportedMediaTypes.Add("application/vnd.excentric.hateoas+json");
            })
            .AddXmlDataContractSerializerFormatters()
            .AddJsonOptions(options =>
            {
               options.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
            });

         // register the DbContext on the container, getting the connection string from
         // appSettings (note: use this during development; in a production environment,
         // it's better to store the connection string in an environment variable)
         var connectionString = Configuration["connectionStrings:libraryDBConnectionString"];
         services.AddDbContext<LibraryContext>(o => o.UseSqlServer(connectionString));

         // register the repository
         services.AddScoped<ILibraryRepository, LibraryRepository>();

         services.AddTransient<IPropertyMappingService, PropertyMappingService>();

         services.AddTransient<ITypeHelperService, TypeHelperService>();
      }

      // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
      public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILogger<Startup> logger)
      {
         if (env.IsDevelopment())
         {
            app.UseDeveloperExceptionPage();
         }
         else
         {
            //this will handle all the 500 status codes 
            app.UseExceptionHandler(appBuilder =>
            {
               appBuilder.Run(async context =>
               {
                  var exceptionHandlerFeature = context.Features.Get<IExceptionHandlerFeature>();
                  if (exceptionHandlerFeature != null)
                  {
                     logger.LogError(500, exceptionHandlerFeature.Error, exceptionHandlerFeature.Error.Message);
                  }
                  context.Response.StatusCode = 500;
                  await context.Response.WriteAsync("An unexpected fault happened. Try again later");
               });
            });
         }

         AutoMapper.Mapper.Initialize(cfg =>
         {
            cfg.CreateMap<Author, AuthorDto>()
               .ForMember(dest => dest.Name, map => map.MapFrom(src => $"{src.FirstName} {src.LastName}"))
               .ForMember(dest => dest.Age,
                  map => map.MapFrom(src => src.DateOfBirth.GetCurrentAge(src.DateOfDeath)));

            cfg.CreateMap<Book, BookDto>();

            cfg.CreateMap<AuthorForCreationDto, Author>();

            cfg.CreateMap<AuthorForCreationWithDateOfDeathDto, Author>();

            cfg.CreateMap<BookForCreationDto, Book>();

            cfg.CreateMap<BookForUpdateDto, Book>()
               .ReverseMap();
         });

         app.UseMvc();
      }
   }
}
