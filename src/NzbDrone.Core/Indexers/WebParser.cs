using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Instrumentation;
using NzbDrone.Core.Indexers.Exceptions;
using NzbDrone.Core.Parser.Model;
using HtmlAgilityPack;

namespace NzbDrone.Core.Indexers
{
    public class WebParser : IParseIndexerResponse
    {
        protected readonly Logger _logger;

        public WebParser()
        {
            _logger = NzbDroneLogger.GetLogger(this);
        }

        public virtual IList<ReleaseInfo> ParseResponse(IndexerResponse indexerResponse)
        {
            var releases = new List<ReleaseInfo>();

            if (!PreProcess(indexerResponse))
            {
                return releases;
            }

            HtmlDocument doc = new HtmlDocument();

            using (var htmlTextReader = new StringReader(indexerResponse.Content))
            {
                doc.Load(htmlTextReader);
            }

            var items = GetItems(doc);

            if (items != null)
            {
                foreach (var item in items)
                {
                    try
                    {
                        var reportInfo = ProcessItem(item);
                        releases.AddIfNotNull(reportInfo);
                    }
                    catch (Exception itemEx)
                    {
                        itemEx.Data.Add("Item", item.InnerHtml);
                        _logger.ErrorException("An error occurred while processing feed item from " + indexerResponse.Request.Url, itemEx);
                    }
                }
            }

            return releases;
        }

        protected virtual ReleaseInfo CreateNewReleaseInfo()
        {
            return new ReleaseInfo();
        }

        protected virtual Boolean PreProcess(IndexerResponse indexerResponse)
        {
            if (indexerResponse.HttpResponse.StatusCode != System.Net.HttpStatusCode.OK)
            {
                throw new IndexerException(indexerResponse, "Indexer API call resulted in an unexpected StatusCode [{0}]", indexerResponse.HttpResponse.StatusCode);
            }

            //if (indexerResponse.HttpResponse.Headers.ContentType != null && indexerResponse.HttpResponse.Headers.ContentType.Contains("text/html") &&
            //    indexerResponse.HttpRequest.Headers.Accept != null && !indexerResponse.HttpRequest.Headers.Accept.Contains("text/html"))
            //{
            //    throw new IndexerException(indexerResponse, "Indexer responded with html content. Site is likely blocked or unavailable.");
            //}

            return true;
        }

        protected ReleaseInfo ProcessItem(HtmlNode item)
        {
            var releaseInfo = CreateNewReleaseInfo();

            releaseInfo = ProcessItem(item, releaseInfo);

            _logger.Trace("Parsed: {0}", releaseInfo.Title);

            return PostProcess(item, releaseInfo);
        }

        protected virtual ReleaseInfo ProcessItem(HtmlNode item, ReleaseInfo releaseInfo)
        {
            releaseInfo.Guid = GetGuid(item);
            releaseInfo.Title = GetTitle(item);
            releaseInfo.PublishDate = GetPublishDate(item);
            releaseInfo.DownloadUrl = GetDownloadUrl(item);
            releaseInfo.InfoUrl = GetInfoUrl(item);
            releaseInfo.CommentUrl = GetCommentUrl(item);

            try
            {
                releaseInfo.Size = GetSize(item);
            }
            catch (Exception)
            {
                throw new SizeParsingException("Unable to parse size from: {0}", releaseInfo.Title);
            }

            return releaseInfo;
        }

        protected virtual ReleaseInfo PostProcess(HtmlNode item, ReleaseInfo releaseInfo)
        {
            return releaseInfo;
        }

        protected virtual String GetGuid(HtmlNode item)
        {
            return Guid.NewGuid().ToString();
        }

        protected virtual String GetTitle(HtmlNode item)
        {
            return String.Empty;
        }

        protected virtual DateTime GetPublishDate(HtmlNode item)
        {
            return DateTime.Now;
        }

        protected virtual string GetDownloadUrl(HtmlNode item)
        {
            return String.Empty;
        }

        protected virtual string GetInfoUrl(HtmlNode item)
        {
            return String.Empty;
        }

        protected virtual string GetCommentUrl(HtmlNode item)
        {
            return String.Empty;
        }

        protected virtual long GetSize(HtmlNode item)
        {
            return ParseSize(GetSizeString(item), true);
        }

        protected virtual String GetSizeString(HtmlNode item)
        {
            return "0B";
        }

        protected virtual HtmlNodeCollection GetItems(HtmlDocument document)
        {
            return document.DocumentNode.SelectNodes("[table]");
        }

        private static readonly Regex ParseSizeRegex = new Regex(@"(?<value>\d+\.\d{1,2}|\d+\,\d+\.\d{1,2}|\d+)\W?(?<unit>[KMG]i?B)",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        public static Int64 ParseSize(String sizeString, Boolean defaultToBinaryPrefix)
        {
            var match = ParseSizeRegex.Matches(sizeString);

            if (match.Count != 0)
            {
                var value = Decimal.Parse(Regex.Replace(match[0].Groups["value"].Value, "\\,", ""), CultureInfo.InvariantCulture);

                var unit = match[0].Groups["unit"].Value.ToLower();

                switch (unit)
                {
                    case "kb":
                        return ConvertToBytes(Convert.ToDouble(value), 1, defaultToBinaryPrefix);
                    case "mb":
                        return ConvertToBytes(Convert.ToDouble(value), 2, defaultToBinaryPrefix);
                    case "gb":
                        return ConvertToBytes(Convert.ToDouble(value), 3, defaultToBinaryPrefix);
                    case "kib":
                        return ConvertToBytes(Convert.ToDouble(value), 1, true);
                    case "mib":
                        return ConvertToBytes(Convert.ToDouble(value), 2, true);
                    case "gib":
                        return ConvertToBytes(Convert.ToDouble(value), 3, true);
                    default:
                        return (Int64)value;
                }
            }
            return 0;
        }

        private static Int64 ConvertToBytes(Double value, Int32 power, Boolean binaryPrefix)
        {
            var prefix = binaryPrefix ? 1024 : 1000;
            var multiplier = Math.Pow(prefix, power);
            var result = value * multiplier;

            return Convert.ToInt64(result);
        }
    }
}
