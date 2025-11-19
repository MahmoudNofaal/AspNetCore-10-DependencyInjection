using ServiceContracts;
using Services;

namespace DIExample;

public class Program
{
   public static void Main(string[] args)
   {
      var builder = WebApplication.CreateBuilder(args);
      
      builder.Services.AddControllersWithViews();

      builder.Services.Add(new ServiceDescriptor(typeof(ICitiesService), // we should add the interface type here
                                                      typeof(CitiesService), // then we tell which class to create object for the interface
                                                      ServiceLifetime.Transient)); // and the lifetime of the service

      var app = builder.Build();

      app.UseStaticFiles();
      app.UseRouting();
      app.MapControllers();

      app.Run();
   }
}
