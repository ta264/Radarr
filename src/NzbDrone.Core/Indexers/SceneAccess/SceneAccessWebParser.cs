using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Indexers.Exceptions;
using NzbDrone.Core.Parser.Model;
using HtmlAgilityPack;

namespace NzbDrone.Core.Indexers.SceneAccess
{
    public class SceneAccessWebParser : TorrentWebParser
    {
        public SceneAccessSettings Settings { get; set; }

        protected override bool PreProcess(IndexerResponse indexerResponse)
        {
            if (indexerResponse.HttpResponse.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return false;
            }

            return base.PreProcess(indexerResponse);
        }

        protected override HtmlNodeCollection GetItems(HtmlDocument document)
        {
            var torrentTable = document.DocumentNode.SelectSingleNode("//table[@id='torrents-table']");

            if (torrentTable != null)
            {
                var items = torrentTable.SelectNodes("tr[@class='tt_row']");
                return items;
            }

            return null;
        }

        protected override string GetDownloadUrl(HtmlNode item)
        {
            var linknode = item.SelectSingleNode("td[@class='td_dl']").SelectSingleNode("a[@href]");
            return Settings.BaseUrl + "/" + linknode.Attributes["href"].Value;
        }

        protected override string GetInfoUrl(HtmlNode item)
        {
            var linknode = item.SelectSingleNode("td[@class='ttr_name']").SelectSingleNode("a[@href]"); ;
            return Settings.BaseUrl + "/" + linknode.Attributes["href"].Value;
        }

        protected override string GetTitle(HtmlNode item)
        {        
            var linknode = item.SelectSingleNode("td[@class='ttr_name']").SelectSingleNode("a[@title]"); ;
            return linknode.Attributes["title"].Value;
        }

        protected override string GetSizeString(HtmlNode item)
        {
            var sizeNode = item.SelectSingleNode("td[@class='ttr_size']");
            return sizeNode.InnerText;
        }

        protected override DateTime GetPublishDate(HtmlNode item)
        {
            var pubNode = item.SelectSingleNode("td[@class='ttr_added']");
            var dateTime = pubNode.ChildNodes[0].InnerText + " " + pubNode.ChildNodes[2].InnerText;
            return  XElementExtensions.ParseDate(dateTime);
        }

        protected override Int32? GetSeeders(HtmlNode item)
        {
            var node = item.SelectSingleNode("td[@class='ttr_seeders']");
            return Int32.Parse(node.InnerText);
        }

        protected Int32? GetLeechers(HtmlNode item)
        {
            var node = item.SelectSingleNode("td[@class='ttr_leechers']");
            return Int32.Parse(node.InnerText);
        }

        protected override Int32? GetPeers(HtmlNode item)
        {
            return GetSeeders(item) + GetLeechers(item);
        }

    }
}