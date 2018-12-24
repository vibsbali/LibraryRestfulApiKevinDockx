using System;
using System.Collections.Generic;
using Library.Api.Entities;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Library.Api
{
   public class Program
   {
      public static void Main(string[] args)
      {
         var host = BuildWebHost(args);
         InitializeDatabase(host);
         host.Run();
      }

      public static IWebHost BuildWebHost(string[] args) =>
          WebHost.CreateDefaultBuilder(args)
              .UseStartup<Startup>()
              .Build();

      public static void InitializeDatabase(IWebHost host)
      {
         // first, clear the database.  This ensures we can always start 
         // fresh with each demo.  Not advised for production environments, obviously :-)

         using (var scope = host.Services.CreateScope())
         {
            var services = scope.ServiceProvider;

            try
            {
               SeedData.Initialize(services);
            }
            catch (Exception ex)
            {
               var logger = services.GetRequiredService<ILogger<Program>>();
               logger.LogError(ex, "An error occurred seeding the DB.");
            }
         }
      }
   }
}
