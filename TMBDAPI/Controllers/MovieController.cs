using Microsoft.AspNetCore.Mvc;
using ML;
using Newtonsoft.Json.Linq;
using NuGet.Protocol;
using System.Net.Http.Headers;
using IHostingEnvironment = Microsoft.AspNetCore.Hosting.IHostingEnvironment;

namespace PL.Controllers
{
    public class MovieController : Controller
    {
        private IHostingEnvironment environment;
        private IConfiguration configuration;

        public MovieController(IHostingEnvironment _environment, IConfiguration _configuration)
        {
            environment = _environment;
            configuration = _configuration;
        }

        [HttpGet]
        public IActionResult GetAll()
        {
            ML.Movie movie = new ML.Movie();

            using (HttpClient client = new HttpClient()) 
            {
                string webApi = configuration["TMDBAPI"];
                string token = configuration["Token"];
                string accepted = configuration["Accepted"];

                client.BaseAddress = new Uri(webApi);
                client.DefaultRequestHeaders.Add("Authorization", token);
                client.DefaultRequestHeaders.Add("Accept", accepted);
                
                var responseTask = client.GetAsync("movie/popular");
                responseTask.Wait();

                var resultTask = responseTask.Result;

                if (resultTask.IsSuccessStatusCode) 
                {
                    var readTask = resultTask.Content.ReadAsStringAsync();
                    dynamic jsonTask = JObject.Parse(readTask.Result.ToString());
                    
                    movie.Movies = new List<object>();

                    foreach (var movieList in jsonTask.results)
                    {
                        ML.Movie movies = Newtonsoft.Json.JsonConvert.DeserializeObject<ML.Movie>(movieList.ToString());
                        movies.backdrop_path = "https://www.themoviedb.org/t/p/w600_and_h900_bestv2/"+movieList.backdrop_path;
                        movies.poster_path = "https://www.themoviedb.org/t/p/w600_and_h900_bestv2/"+movieList.poster_path;
                        movie.Movies.Add(movies);
                    }

                }
               

            }

            return View(movie);
        }
        
        [HttpGet]
        public IActionResult Favoritos() 
        {
            ML.Movie movie = new ML.Movie();

            using (HttpClient client = new HttpClient()) 
            {
                string webApi = configuration["TMDBAPI"];
                string token = configuration["Token"];
                string accepted = configuration["Accepted"];
                string options = "?language=en-US&page=1&sort_by=created_at.desc";

                client.BaseAddress = new Uri(webApi);
                client.DefaultRequestHeaders.Add("Authorization", token);
                client.DefaultRequestHeaders.Add("Accept", accepted);
                var responseTask = client.GetAsync("account/20267438/favorite/movies"+options);
                responseTask.Wait();

                var resultTask = responseTask.Result;

                if (resultTask.IsSuccessStatusCode)
                {
                    var readTask = resultTask.Content.ReadAsStringAsync();
                    dynamic jsonTask = JObject.Parse(readTask.Result.ToString());

                    movie.Movies = new List<object>();

                    foreach (var movieList in jsonTask.results)
                    {
                        ML.Movie movies = Newtonsoft.Json.JsonConvert.DeserializeObject<ML.Movie>(movieList.ToString());
                        movies.backdrop_path = "https://www.themoviedb.org/t/p/w600_and_h900_bestv2/" + movieList.backdrop_path;
                        movies.poster_path = "https://www.themoviedb.org/t/p/w600_and_h900_bestv2/" + movieList.poster_path;
                        movie.Movies.Add(movies);
                    }

                }
            }
                return View(movie);
        }

        public IActionResult AddFavorito(int idMovie) 
        {
            using (HttpClient client = new HttpClient()) 
            {
                string webApi = configuration["TMDBAPI"];
                string token = configuration["Token"];
                string accepted = configuration["Accepted"];
                //string body = "{\"media_type\": \"movie\",\"media_id\":"+idMovie+",\"favorite\": true}";
                ML.Favorito favorito = new ML.Favorito("movie", idMovie, true);

                client.BaseAddress = new Uri(webApi);
                client.DefaultRequestHeaders.Add("Authorization", token);
                client.DefaultRequestHeaders.Add("Accept", accepted);
                //client.DefaultRequestHeaders.Add("Content-Type", accepted);

                //var responseTask = client.PostAsJsonAsync("account/20267438/favorite", body);
                var responseTask = client.PostAsJsonAsync<ML.Favorito>("account/20267438/favorite", favorito);
                responseTask.Wait();
                if (responseTask.IsCompletedSuccessfully)
                {
                    return RedirectToAction("GetAll");
                }
                else 
                {
                    return RedirectToAction("Modal");
                }
            }
            
        }
    }
}
