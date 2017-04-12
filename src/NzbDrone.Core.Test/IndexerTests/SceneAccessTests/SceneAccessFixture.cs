using Moq;
using NUnit.Framework;
using NzbDrone.Common.Http;
using NzbDrone.Core.Indexers;
using NzbDrone.Core.Indexers.SceneAccess;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Test.Common;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Specialized;
using FluentAssertions;

namespace NzbDrone.Core.Test.IndexerTests.SceneAccessTests
{
    [TestFixture]
    public class SceneAccessFixture : CoreTest<SceneAccess>
    {
        [SetUp]
        public void Setup()
        {
            Subject.Definition = new IndexerDefinition()
                {
                    Name = "SceneAcesss",
                    Settings = new SceneAccessSettings() { BaseUrl = "https://sceneaccess.eu" }
                };
        }

        [Test]
        public void should_parse_series_search_from_SceneAccess()
        {
            var recentFeed = ReadAllText(@"Files/Indexers/SceneAccess/breakingbad.html");

            recentFeed.Length.Should().BeGreaterThan(0);

            Mocker.GetMock<IHttpClient>()
                .Setup(o => o.Execute(It.Is<HttpRequest>(v => v.Method == HttpMethod.GET)))
                .Returns<HttpRequest>(r =>  new HttpResponse(r, new HttpHeader(new NameValueCollection {
                    {"Content-Type", "text/html"},
                    {"Accept", "text/html"}
                }), recentFeed));

            var releases = Subject.FetchRecent();

            releases.Count.Should().Be(25);

        }

        [Test]
        public void should_run_test()
        {
            Boolean dummy = true;
            dummy.Should().Be(true);
        }
    }
}
