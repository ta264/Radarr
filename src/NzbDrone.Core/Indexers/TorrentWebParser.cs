using System;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using NzbDrone.Core.Parser.Model;
using HtmlAgilityPack;

namespace NzbDrone.Core.Indexers
{
    public class TorrentWebParser : WebParser
    {
        public TorrentWebParser()
        {

        }

        protected override ReleaseInfo CreateNewReleaseInfo()
        {
            return new TorrentInfo();
        }

        protected override ReleaseInfo ProcessItem(HtmlNode item, ReleaseInfo releaseInfo)
        {
            var result = base.ProcessItem(item, releaseInfo) as TorrentInfo;

            result.InfoHash = GetInfoHash(item);
            result.MagnetUrl = GetMagnetUrl(item);
            result.Seeders = GetSeeders(item);
            result.Peers = GetPeers(item);

            return result;
        }

        protected virtual String GetInfoHash(HtmlNode item)
        {
            return null;
        }

        protected virtual String GetMagnetUrl(HtmlNode item)
        {
            return null;
        }

        protected virtual Int32? GetSeeders(HtmlNode item)
        {
            return null;
        }

        protected virtual Int32? GetPeers(HtmlNode item)
        {
            return null;
        }
    }
}
