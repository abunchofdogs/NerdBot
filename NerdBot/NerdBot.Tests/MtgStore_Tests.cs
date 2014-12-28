﻿using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moq;
using NerdBot.Mtg;
using NUnit.Framework;

namespace NerdBot.Tests
{
    [TestFixture]
    class MtgStore_Tests
    {
        private List<Card> cardData = new List<Card>();

        [SetUp]
        public void SetUp()
        {
            cardData = new List<Card>()
            {
                new Card()
                {
                    Id = 5891,
                    RelatedCardId = 0,
                    SetNumber =  179,
                    Name = "Boros Charm",
                    SearchName = "Boros Charm",
                    Desc = "Choose one - Boros Charm deals 4 damage to target player; or permanents you control gain indestructible until end of turn; or target creature gains double strike until end of turn.",
                    Flavor = "",
                    Colors = "red;white",
                    Cost = "RW",
                    Cmc = 2,
                    SetName = "Commander 2013 Edition",
                    Type = "Instant",
                    SubType = "",
                    Power = "0",
                    Toughness = "00",
                    Loyalty = "0",
                    Rarity = "Uncommon",
                    Artist = "Zoltan Boros",
                    SetId = "C13",
                    Token = false,
                    Img = "https://api.mtgdb.info/content/card_images/376270.jpeg",
                    ImgHires = "https://api.mtgdb.info/content/hi_res_card_images/376270.jpg",
                    MultiverseId = 376270,
                },
                new Card()
                {
                    Id = 6975,
                    RelatedCardId = 0,
                    SetNumber =  137,
                    Name = "Boros Cluestone",
                    SearchName = "Boros Cluestone",
                    Desc = "{Tap}: Add {Red} or {White} to your mana pool.\n\r" +
                            "{Red}{White}, {Tap}, Sacrifice Boros Cluestone: Draw a card.",
                    Flavor = "",
                    Colors = "None",
                    Cost = "3",
                    Cmc = 3,
                    SetName = "Dragon's Maze",
                    Type = "Artifact",
                    SubType = "",
                    Power = "0",
                    Toughness = "0",
                    Loyalty = "0",
                    Rarity = "Common",
                    Artist = "Raoul Vitale",
                    SetId = "DGM",
                    Token = false,
                    Img = "https://api.mtgdb.info/content/card_images/368997.jpeg",
                    ImgHires = "https://api.mtgdb.info/content/hi_res_card_images/368997.jpg",
                    MultiverseId = 368997,
                },
                new Card()
                {
                    Id = 7213,
                    RelatedCardId = 0,
                    SetNumber =  148,
                    Name = "Boros Charm",
                    SearchName = "Boros Charm",
                    Desc = "Choose one - Boros Charm deals 4 damage to target player; or permanents you control are indestructible this turn; or target creature gains double strike until end of turn.",
                    Flavor = "",
                    Colors = "red;white",
                    Cost = "RW",
                    Cmc = 2,
                    SetName = "Gatecrash",
                    Type = "Instant",
                    SubType = "",
                    Power = "0",
                    Toughness = "0",
                    Loyalty = "0",
                    Rarity = "Uncommon",
                    Artist = "Zoltan Boros",
                    SetId = "GTC",
                    Token = false,
                    Img = "https://api.mtgdb.info/content/card_images/366435.jpeg",
                    ImgHires = "https://api.mtgdb.info/content/hi_res_card_images/366435.jpg",
                    MultiverseId = 366435,
                },
                new Card()
                {
                    Name = "Spore Cloud",
                    Desc = "Tap all blocking creatures. Prevent all combat damage that would be dealt this turn. Each attacking creature and each blocking creature doesn't untap during its controller's next untap step.",
                    SetNumber = 176,
                    Cmc = 3,
                    Cost = "1GG",
                    SetName = "Masters Edition II",
                    SetId = "ME2",
                    Type = "Instant",
                    Img = "https://api.mtgdb.info/content/card_images/184710.jpeg",
                    MultiverseId = 184710
                },
                new Card()
                {
                    Name = "Spore Cloud",
                    Desc ="Tap all blocking creatures. Prevent all combat damage that would be dealt this turn. Each attacking creature and each blocking creature doesn't untap during its controller's next untap step.",
                    SetNumber = 80,
                    Cmc = 3,
                    Cost = "1GG",
                    SetName = "Fallen Empires",
                    SetId = "FEM",
                    Type = "Instant",
                    Img = "https://api.mtgdb.info/content/card_images/1922.jpeg",
                    MultiverseId = 1922
                },
            };
        }
        
        [Test]
        public void Get_Cards_StartingWith_Boros_C()
        {
            // Create a mock set and context
            var cardDbSetMock = new Mock<DbSet<Card>>()
                .SetupData(cardData);

            var contextMock = new Mock<IMtgContext>();
            contextMock.Setup(c => c.Cards).Returns(cardDbSetMock.Object);

            var mtgStore = new MtgStore(contextMock.Object);

            var cards = mtgStore.GetCards("Boros C");
            
            Assert.AreEqual(3, cards.Count());
        }

        [Test]
        public void Get_Cards_StartingWith_Coros()
        {
            // Create a mock set and context
            var cardDbSetMock = new Mock<DbSet<Card>>()
                .SetupData(cardData);

            var contextMock = new Mock<IMtgContext>();
            contextMock.Setup(c => c.Cards).Returns(cardDbSetMock.Object);

            var mtgStore = new MtgStore(contextMock.Object);

            var cards = mtgStore.GetCards("Coros");

            Assert.AreEqual(0, cards.Count());
        }
    }
}
