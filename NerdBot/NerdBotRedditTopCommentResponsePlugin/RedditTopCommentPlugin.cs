﻿using NerdBot.Http;
using NerdBot.Messengers;
using NerdBot.Mtg;
using NerdBot.Mtg.Prices;
using NerdBot.Parsers;
using NerdBot.Plugin;
using NerdBot.UrlShortners;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NerdBotRedditTopCommentResponsePlugin
{
    public class RedditTopCommentPlugin : MessagePluginBase
    {
        private const string cSubReddit = "r/RoastMe";
        private const int cReplyChance = 2;

        private RedditTopFetcher mFetcher;
        private Random mRandom;

        public override string Name
        {
            get { return "Reddit Top Reply"; }
        }

        public override string Description
        {
            get { return "Randomly reply to message with top comment in a given subreddit."; }
        }

        public override string ShortDescription
        {
            get { return "Randomly reply to message with top comment in a given subreddit."; }
        }

        public RedditTopCommentPlugin(
                IMtgStore store,
                ICardPriceStore priceStore,
                ICommandParser commandParser,
                IHttpClient httpClient,
                IUrlShortener urlShortener)
            : base(
                store,
                priceStore,
                commandParser,
                httpClient,
                urlShortener)
        {
        }

        public override void OnLoad()
        {
            this.mFetcher = new RedditTopFetcher(this.mHttpClient);
            this.mRandom = new Random();
        }

        public override void OnUnload()
        {
        }

        public override async Task<bool> OnMessage(IMessage message, IMessenger messenger)
        {
            // Exit if message was sent by the bot
            if (message.name.ToLower() == this.BotName.ToLower())
                return false;

            // If a message contains 'roast me', get a random r/roastme top comment
            if (message.text.ToLower().Contains("roast me"))
            {
                string reply = await this.mFetcher.GetTopCommentFromSubreddit(cSubReddit);

                // This is lame, but check if the reply conatins url syntax. 
                // If it does, get a new one.
                if (reply.Contains("["))
                {
                    reply = await this.mFetcher.GetTopCommentFromSubreddit(cSubReddit);
                }

                if (!string.IsNullOrEmpty(reply))
                {
                    messenger.SendMessage(string.Format("@{0} {1}", message.name, reply));
                }
            }
            else
            {
                if (this.mRandom.Next(0, 100) < cReplyChance)
                {
                    string reply = await this.mFetcher.GetTopCommentFromSubreddit(cSubReddit);

                    // This is lame, but check if the reply conatins url syntax. 
                    // If it does, get a new one.
                    if (reply.Contains("["))
                    {
                        reply = await this.mFetcher.GetTopCommentFromSubreddit(cSubReddit);
                    }

                    if (!string.IsNullOrEmpty(reply))
                    {
                        messenger.SendMessage(string.Format("@{0} {1}", message.name, reply));
                    }
                }
            }

            return false;
        }
    }
}