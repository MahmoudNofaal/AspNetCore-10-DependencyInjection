namespace Services;

public class CitiesService
{

   private List<string> _cities;

   public CitiesService()
   {
      this._cities = new List<string>
      {
         "New York",
         "Los Angeles",
         "Chicago",
         "Houston",
         "Phoenix"
      };
   }

   public List<string> GetCities()
   {

      return this._cities;
   }

}
