using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMDbLib.Client;
using TMDbLib.Objects.Movies;

namespace TVShower.MovieDbApi
{
    class MovieDbProvider
    {
        TMDbClient _client;

        public MovieDbProvider()
        {
            _client = new TMDbClient(Properties.Resources.apiKey);
        }

        public string GetMovie(int id)
        {
            Movie movie = _client.GetMovieAsync(id).Result;
            return movie.Title;
        }

        public IEnumerable<int> SearchTvShow(string name)
        {
            var searchResult = _client.SearchTvShowAsync(name).Result;
            foreach (var item in searchResult.Results)
            {
                yield return item.Id;
            }
        }

    }
}
