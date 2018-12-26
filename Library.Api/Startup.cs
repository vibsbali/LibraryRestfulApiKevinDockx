using Library.Api.Entities;
using Library.Api.Helpers;
using Library.Api.Models;
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
            setupAction.OutputFormatters.Add(new XmlDataContractSerializerOutputFormatter());
            setupAction.InputFormatters.Add(new XmlDataContractSerializerInputFormatter());
         });

         // register the DbContext on the container, getting the connection string from
         // appSettings (note: use this during development; in a production environment,
         // it's better to store the connection string in an environment variable)
         var connectionString = Configuration["connectionStrings:libraryDBConnectionString"];
         services.AddDbContext<LibraryContext>(o => o.UseSqlServer(connectionString));

         // register the repository
         services.AddScoped<ILibraryRepository, LibraryRepository>();

         services.AddTransient<IPropertyMappingService, PropertyMappingService>();
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
                  map => map.MapFrom(src => src.DateOfBirth.GetCurrentAge()));

            cfg.CreateMap<Book, BookDto>();

            cfg.CreateMap<AuthorForCreationDto, Author>();

            cfg.CreateMap<BookForCreationDto, Book>();

            cfg.CreateMap<BookForUpdateDto, Book>()
               .ReverseMap();
         });

         app.UseMvc();
      }
   }
}
