using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using TMDbLib.Client;
using TMDbLib.Objects.Movies;
using TMDbLib.Objects.TvShows;

namespace TVShower.MovieDbApi
{
    public class MovieDbProvider
    {
        TMDbClient _client;

        public MovieDbProvider()
        {
            _client = new TMDbClient(Properties.Resources.apiKey);
        }

        public string GetTvShow(int id)
        {
            TvShow show = _client.GetTvShowAsync(id).Result;
            return show.Name;
        }

        public int SearchTvShow(string name)
        {
            var searchResult = _client.SearchTvShowAsync(name).Result;
            foreach (var item in searchResult.Results)
            {
                var details = _client.GetTvShowTranslationsAsync(item.Id).Result;

                foreach(var title in details.Translations)
                {
                    if (title.Data.Name == name)
                    {
                        return item.Id;
                    }
                }
            }
            return -1;
        }

    }
}
