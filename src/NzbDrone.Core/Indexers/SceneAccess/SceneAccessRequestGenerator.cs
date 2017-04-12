using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Http;
using NzbDrone.Core.IndexerSearch.Definitions;

namespace NzbDrone.Core.Indexers.SceneAccess
{
    public class SceneAccessRequestGenerator : IIndexerRequestGenerator
    {
        public SceneAccessSettings Settings { get; set; }

        public Int32 MaxPages { get; set; }
        public Int32 PageSize { get; set; }

        private NameValueCollection MagicMethodStrings;
        private int[] RssCategories;

        public SceneAccessRequestGenerator()
        {
            MaxPages = 1;
            PageSize = 25;
            MagicMethodStrings = new NameValueCollection {
                {"browse", "method=2&c8=8&c22=22&c7=7"},
                {"archive", "method=1&c4=4"},
                {"nonscene", "method=2&c41=41&c42=42&c43=43"}
            };
            RssCategories = new int[] {8, 4, 22, 7, 41, 42, 43};
        }

        public virtual IndexerPageableRequestChain GetRecentRequests()
        {
            var pageableRequests = new IndexerPageableRequestChain();
            pageableRequests.Add(GetRssRequests());

            return pageableRequests;
        }

        public virtual IndexerPageableRequestChain GetSearchRequests(SingleEpisodeSearchCriteria searchCriteria)
        {
            var pageableRequests = new IndexerPageableRequestChain();

            foreach (var queryTitle in searchCriteria.QueryTitles)
            {
                pageableRequests.Add(GetPagedRequests(MaxPages, "browse",
                    PrepareQuery(queryTitle),
                    String.Format("S{0:00}E{1:00}", searchCriteria.SeasonNumber, searchCriteria.EpisodeNumber)));
            }

            return pageableRequests;
        }

        public virtual IndexerPageableRequestChain GetSearchRequests(SeasonSearchCriteria searchCriteria)
        {
            var pageableRequests = new IndexerPageableRequestChain();

            foreach (var queryTitle in searchCriteria.QueryTitles)
            {
                pageableRequests.Add(GetPagedRequests(MaxPages, "archive",
                    PrepareQuery(queryTitle),
                    String.Format("S{0:00}", searchCriteria.SeasonNumber)));

                pageableRequests.Add(GetPagedRequests(MaxPages, "browse",
                    PrepareQuery(queryTitle),
                    String.Format("S{0:00}", searchCriteria.SeasonNumber)));
            }

            return pageableRequests;
        }

        public virtual IndexerPageableRequestChain GetSearchRequests(DailyEpisodeSearchCriteria searchCriteria)
        {
            var pageableRequests = new IndexerPageableRequestChain();

            foreach (var queryTitle in searchCriteria.QueryTitles)
            {
                pageableRequests.Add(GetPagedRequests(MaxPages, "browse",
                    PrepareQuery(queryTitle),
                    String.Format("{0:yyyy-MM-dd}", searchCriteria.AirDate)));
            }

            return pageableRequests;
        }

        public virtual IndexerPageableRequestChain GetSearchRequests(AnimeEpisodeSearchCriteria searchCriteria)
        {
            return new IndexerPageableRequestChain();
        }

        public virtual IndexerPageableRequestChain GetSearchRequests(SpecialEpisodeSearchCriteria searchCriteria)
        {
            var pageableRequests = new IndexerPageableRequestChain();

            foreach (var queryTitle in searchCriteria.EpisodeQueryTitles)
            {
                pageableRequests.Add(GetPagedRequests(MaxPages, "browse",
                    PrepareQuery(queryTitle)));
            }

            return pageableRequests;
        }

        public virtual IndexerPageableRequestChain GetSearchRequests(MovieSearchCriteria searchCriteria)
        {
            var pageableRequests = new IndexerPageableRequestChain();

            pageableRequests.Add(GetPagedRequests(MaxPages, "browse",
                                                  PrepareQuery($"{searchCriteria.Movie.Title} {searchCriteria.Movie.Year}")));

            return pageableRequests;
        }

        private IEnumerable<IndexerRequest> GetPagedRequests(Int32 maxPages, String searchType, params String[] searchParameters)
        {
            String searchString = null;

            if (searchParameters.Any())
            {
                searchString = String.Join(" ", searchParameters).Trim();
            }

            searchString = System.Web.HttpUtility.UrlEncode(searchString);

            for (var page = 0; page < maxPages; page++)
            {
                var request = new IndexerRequest(String.Format("{0}/{1}.php?{2}&search={3}&page={4}", 
                    Settings.BaseUrl.TrimEnd('/'), searchType, MagicMethodStrings[searchType], searchString, page),
                    new HttpAccept("application/x-httpd-php"));
                request.HttpRequest.Cookies["uid"] = Settings.CookieUid;
                request.HttpRequest.Cookies["pass"] = Settings.CookiePass;

                yield return request;
            }

        }

        private IEnumerable<IndexerRequest> GetRssRequests()
        {
            foreach (var category in RssCategories)
            {
                var request = new IndexerRequest(String.Format("{0}/rss.php?feed=dl&cat={1}&passkey={2}",
                    Settings.BaseUrl.TrimEnd('/'), category, Settings.RssKey), HttpAccept.Rss);

                yield return request;
            }
        }

        private String PrepareQuery(String query)
        {
            return query.Replace('+', ' ');
        }
    }
}
