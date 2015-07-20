﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Moq;
using NerdBot.Http;
using NerdBot.Messengers;
using NerdBot.Messengers.GroupMe;
using NerdBot.Mtg;
using NerdBot.Mtg.Prices;
using NerdBot.Parsers;
using NerdBot.UrlShortners;
using NerdBot.Utilities;
using NerdBotCardPrices;
using NUnit.Framework;
using SimpleLogging.Core;

namespace NerdBotCardPricesPlugin_Tests
{
    [TestFixture]
    public class DeckBrewPricePlugin_Tests
    {
        private const string connectionString = "mongodb://localhost";
        private const string databaseName = "mtgdb";

        private IMtgStore mtgStore;
        private DeckBrewPricePlugin plugin;

        private Mock<ICommandParser> commandParserMock;
        private Mock<IHttpClient> httpClientMock;
        private Mock<IUrlShortener> urlShortenerMock;
        private Mock<IMessenger> messengerMock;
        private Mock<ILoggingService> loggingServiceMock;
        private Mock<ICardPriceStore> priceStoreMock;
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
            loggingServiceMock = new Mock<ILoggingService>();
            searchUtilityMock = new Mock<SearchUtility>();

            searchUtilityMock.Setup(s => s.GetSearchValue(It.IsAny<string>()))
                .Returns((string s) => this.GetSearchValue(s));

            searchUtilityMock.Setup(s => s.GetRegexSearchValue(It.IsAny<string>()))
                .Returns((string s) => this.GetRegexSearchValue(s));

            mtgStore = new MtgStore(
                connectionString,
                databaseName,
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

            plugin =  new DeckBrewPricePlugin(
               mtgStore,
               priceStoreMock.Object,
               commandParserMock.Object,
               httpClientMock.Object,
               urlShortenerMock.Object);

            plugin.LoggingService = loggingServiceMock.Object;
        }

        [Test]
        public void GetPrice_SetNotAvailableFallbackToJustName()
        {
            string name = "Inquisition of Kozilek";
            string setCode = "MD1";

            // Setup mock to return NULL for this card and set
            priceStoreMock.Setup(ps => ps.GetCardPrice(name, setCode))
                .Returns(() => null);

            // Setup mock to return a price when falling back to just name
            priceStoreMock.Setup(ps => ps.GetCardPrice(name))
                .Returns(() => new CardPrice()
                {
                    Name = name,
                    SearchName = this.GetSearchValue(name),
                    SetCode = "ROE",
                    PriceFoil = "$100",
                    PriceDiff = "10%"
                });

            var cmd = new Command()
            {
                Cmd = "tcg",
                Arguments = new string[]
                {
                    "Inquisition of Kozilek"   
                }
            };

            var msg = new GroupMeMessage();

            bool handled = plugin.OnCommand(
                    cmd,
                    msg,
                    messengerMock.Object
                    ).Result;

            messengerMock.Verify(m => m.SendMessage(It.Is<string>(s => s.StartsWith("Inquisition of Kozilek [ROE] Foil: $100. 7-Day change: 10%.. Also appears in sets: ROE"))),
                Times.Once);
        }

    }
}
