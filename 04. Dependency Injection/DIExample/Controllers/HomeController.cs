using Microsoft.AspNetCore.Mvc;
using ServiceContracts;
using Services;

namespace DIExample.Controllers;

public class HomeController : Controller
{
   private readonly ICitiesService _citiesService;

   //constructor
   // we are using constructor injection to get the object of CitiesService class
   public HomeController(ICitiesService citiesService)
   {
      //create object of CitiesService class
      // we are using Dependency Injection (DI) to get the object of CitiesService class
      _citiesService = citiesService; //new CitiesService(); // we are not creating object using new keyword, instead we are using DI
   }

   [Route("/")]
   public IActionResult Index()
   {
      List<string> cities = _citiesService.GetCities();
      return View(cities);
   }

}
