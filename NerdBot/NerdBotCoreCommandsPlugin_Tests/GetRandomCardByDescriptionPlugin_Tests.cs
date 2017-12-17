﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Moq;
using NerdBot;
using NerdBot.Parsers;
using NerdBot.Plugin;
using NerdBot.TestsHelper;
using NerdBotCommon.Http;
using NerdBotCommon.Messengers;
using NerdBotCommon.Messengers.GroupMe;
using NerdBotCommon.Mtg;
using NerdBotCommon.Mtg.Prices;
using NerdBotCommon.Parsers;
using NerdBotCommon.Statistics;
using NerdBotCommon.UrlShortners;
using NerdBotCommon.Utilities;
using NerdBotCoreCommands;
using NUnit.Framework;
using SimpleLogging.Core;

namespace NerdBotCoreCommandsPlugin_Tests
{
    [TestFixture]
    class GetRandomCardByDescriptionPlugin_Tests
    {
        private TestConfiguration testConfig;
        private IMtgStore mtgStore;
        private GetRandomCardByDescriptionPlugin plugin;

        private Mock<ICommandParser> commandParserMock;
        private Mock<IHttpClient> httpClientMock;
        private Mock<IUrlShortener> urlShortenerMock;
        private Mock<IMessenger> messengerMock;
        private Mock<ILoggingService> loggingServiceMock;
        private Mock<ICardPriceStore> priceStoreMock;
        private Mock<IBotServices> servicesMock;
        private Mock<IQueryStatisticsStore> queryStatisticsStoreMock;
        private Mock<SearchUtility> searchUtilityMock;

        public string GetSearchValue(string text)
        {
            Regex rgx = new Regex("[^a-zA-Z0-9.^*]");

            string searchValue = text.ToLower();

            // Remove all non a-zA-Z0-9.^ characters
            searchValue = rgx.Replace(searchValue, "");

            // Remove all spaces
            searchValue = searchValue.Replace(" ", "");

            return searchValue;
        }

        public string GetRegexSearchValue(string text)
        {
            Regex rgx = new Regex("[^a-zA-Z0-9.^*]");

            string searchValue = text.ToLower();

            // Replace * and % with a regex '*' char
            searchValue = searchValue.Replace("%", ".*");

            // If the first character of the searchValue is not '*', 
            // meaning the user does not want to do a contains search,
            // explicitly use a starts with regex
            if (!searchValue.StartsWith(".*"))
            {
                searchValue = "^" + searchValue;
            }

            // Remove all non a-zA-Z0-9.^ characters
            searchValue = rgx.Replace(searchValue, "");

            // Remove all spaces
            searchValue = searchValue.Replace(" ", "");

            return searchValue;
        }

        [TestFixtureSetUp]
        public void TestFixtureSetUp()
        {
            testConfig = new ConfigReader().Read();

            loggingServiceMock = new Mock<ILoggingService>();
            searchUtilityMock = new Mock<SearchUtility>();
            queryStatisticsStoreMock = new Mock<IQueryStatisticsStore>();

            searchUtilityMock.Setup(s => s.GetSearchValue(It.IsAny<string>()))
                .Returns((string s) => this.GetSearchValue(s));

            searchUtilityMock.Setup(s => s.GetRegexSearchValue(It.IsAny<string>()))
                .Returns((string s) => this.GetRegexSearchValue(s));

            mtgStore = new MtgStore(
                testConfig.Url,
                testConfig.Database,
                queryStatisticsStoreMock.Object,
                loggingServiceMock.Object,
                searchUtilityMock.Object);
        }

        [SetUp]
        public void SetUp()
        {
            // Setup ICardPriceStore Mocks
            priceStoreMock = new Mock<ICardPriceStore>();

            // Setup ICommandParser Mocks
            commandParserMock = new Mock<ICommandParser>();

            // Setup IHttpClient Mocks
            httpClientMock = new Mock<IHttpClient>();

            // Setup IUrlShortener Mocks
            urlShortenerMock = new Mock<IUrlShortener>();

            // Setup IMessenger Mocks
            messengerMock = new Mock<IMessenger>();

            // Setup IBotServices Mocks
            servicesMock = new Mock<IBotServices>();

            servicesMock.SetupGet(s => s.QueryStatisticsStore)
                .Returns(queryStatisticsStoreMock.Object);

            servicesMock.SetupGet(s => s.Store)
                .Returns(mtgStore);

            servicesMock.SetupGet(s => s.PriceStore)
                .Returns(priceStoreMock.Object);

            servicesMock.SetupGet(s => s.CommandParser)
                .Returns(commandParserMock.Object);

            servicesMock.SetupGet(s => s.HttpClient)
                .Returns(httpClientMock.Object);

            servicesMock.SetupGet(s => s.UrlShortener)
                .Returns(urlShortenerMock.Object);

            plugin = new GetRandomCardByDescriptionPlugin(
                servicesMock.Object,
                new BotConfig());

            plugin.LoggingService = loggingServiceMock.Object;
        }

        [Test]
        public void GetRandomCardByDescription()
        {
            var cmd = new Command()
            {
                Cmd = "desc",
                Arguments = new string[]
                {
                    "shroud"
                }
            };

            var msg = new GroupMeMessage();

            bool handled =
            plugin.OnCommand(
                cmd,
                msg,
                messengerMock.Object
                ).Result;

            messengerMock.Verify(m => m.SendMessage(It.Is<string>(c => c.EndsWith(".jpg"))), Times.Once);
        }

        [Test]
        public void GetRandomCardByDescription_MultipleSets()
        {
            var cmd = new Command()
            {
                Cmd = "desc",
                Arguments = new string[]
                {
                    "a"
                }
            };

            var msg = new GroupMeMessage();

            bool handled =
                plugin.OnCommand(
                    cmd,
                    msg,
                    messengerMock.Object
                ).Result;

            messengerMock.Verify(m => m.SendMessage(It.Is<string>(c => c.EndsWith(".jpg"))), Times.Once);
            messengerMock.Verify(m => m.SendMessage(It.Is<string>(c => c.Contains("Also in sets:"))), Times.Once);
        }
    }
}
