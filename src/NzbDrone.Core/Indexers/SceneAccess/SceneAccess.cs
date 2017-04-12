using System;
using NLog;
using NzbDrone.Common.Http;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Parser;

namespace NzbDrone.Core.Indexers.SceneAccess
{
    public class SceneAccess : HttpIndexerBase<SceneAccessSettings>
    {
        public override string Name
        {
            get
            {
                return "SceneAccess";
            }
        }
        public override DownloadProtocol Protocol { get { return DownloadProtocol.Torrent; } }
        public override Int32 PageSize { get { return 25; } }

        public SceneAccess(IHttpClient httpClient, IIndexerStatusService indexerStatusService, IConfigService configService, IParsingService parsingService, Logger logger)
            : base(httpClient, indexerStatusService, configService, parsingService, logger)
        {
            
        }

        public override IIndexerRequestGenerator GetRequestGenerator()
        {
            return new SceneAccessRequestGenerator() { Settings = Settings, PageSize = PageSize };
        }

        public override IParseIndexerResponse GetParser()
        {
            return new SceneAccessCombParser() { Settings = Settings };
        }

    }
}