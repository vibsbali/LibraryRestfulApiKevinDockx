using Library.Api.Entities;
using Library.Api.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
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
         services.AddMvc();

         // register the DbContext on the container, getting the connection string from
         // appSettings (note: use this during development; in a production environment,
         // it's better to store the connection string in an environment variable)
         var connectionString = Configuration["connectionStrings:libraryDBConnectionString"];
         services.AddDbContext<LibraryContext>(o => o.UseSqlServer(connectionString));

         // register the repository
         services.AddScoped<ILibraryRepository, LibraryRepository>();
      }

      // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
      public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory, LibraryContext libraryContext)
      {
         if (env.IsDevelopment())
         {
            app.UseDeveloperExceptionPage();
         }
         else
         {
            app.UseExceptionHandler();
         }
         app.UseMvc();
      }
   }
}
