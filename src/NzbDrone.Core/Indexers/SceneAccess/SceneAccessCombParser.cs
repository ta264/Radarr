using System;
using System.Collections.Generic;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Indexers.Exceptions;

namespace NzbDrone.Core.Indexers.SceneAccess
{
    public class SceneAccessCombParser : IParseIndexerResponse
    {
        public SceneAccessSettings Settings { get; set; }

        private SceneAccessRssParser RssParser = new SceneAccessRssParser() { ParseSizeInDescription = true, ParseSeedersInDescription = true };
        private SceneAccessWebParser WebParser = new SceneAccessWebParser();

        public IList<ReleaseInfo> ParseResponse(IndexerResponse indexerResponse)
        {
            var releases = new List<ReleaseInfo>();

            if (!PreProcess(indexerResponse))
            {
                return releases;
            }

            if (indexerResponse.HttpResponse.Headers.ContentType != null && indexerResponse.HttpResponse.Headers.ContentType.Contains("text/html") &&
                indexerResponse.HttpRequest.Headers.Accept != null && !indexerResponse.HttpRequest.Headers.Accept.Contains("text/html"))
            {
                WebParser.Settings = Settings;
                return WebParser.ParseResponse(indexerResponse);
            }
            else
            {
                return RssParser.ParseResponse(indexerResponse);
            }
        }

        protected virtual Boolean PreProcess(IndexerResponse indexerResponse)
        {
            if (indexerResponse.HttpResponse.StatusCode != System.Net.HttpStatusCode.OK)
            {
                throw new IndexerException(indexerResponse, "Indexer API call resulted in an unexpected StatusCode [{0}]", indexerResponse.HttpResponse.StatusCode);
            }

            return true;
        }
    }
}
