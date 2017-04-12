using System;
using System.Linq;
using System.Xml.Linq;
using System.Text.RegularExpressions;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Indexers.Exceptions;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.Indexers.SceneAccess
{
    public class SceneAccessRssParser : TorrentRssParser
    {
        protected override bool PreProcess(IndexerResponse indexerResponse)
        {
            if (indexerResponse.HttpResponse.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return false;
            }

            return base.PreProcess(indexerResponse);
        }

        protected override DateTime GetPublishDate(XElement item)
        {
            var matchTime = ParsePublishDateRegex.Match(item.Element("description").Value);
            if (matchTime.Success)
            {
                return XElementExtensions.ParseDate(matchTime.Groups["time"].Value);
            }
            else
            {
                return System.DateTime.Now;
            }
        }

        //eg Category: TV/HD-x264 Pre: 6 minutes and 19 seconds after pre Size: 899.54 MB Status: 19 seeders and 0 leechers Added: 2015-01-31 12:29:30
        private static readonly Regex ParsePublishDateRegex = new Regex(@"Added:\s+(?<time>\d{4}-\d\d-\d\d\s+\d\d:\d\d:\d\d)", RegexOptions.IgnoreCase | RegexOptions.Compiled);

    }
}