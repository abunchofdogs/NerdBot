﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NerdBot.Http;
using NerdBot.Messengers;
using NerdBot.Mtg;
using NerdBot.Parsers;
using NerdBot.Plugin;
using NerdBot.UrlShortners;

namespace NerdBotUrbanDictionary
{
    public class UrbanDictionaryPlugin : PluginBase
    {
        public override string Name
        {
            get { return "UrbanDictionary command"; }
        }

        public override string Description
        {
            get { return "Returns the top rated definition from UrbanDictionary";  }
        }

        public override string ShortDescription
        {
            get { return "Returns the top rated definition from UrbanDictionary"; }
        }

        public override string Command
        {
            get { return "wtf"; }
        }

        public override string HelpCommand
        {
            get { return "help wtf"; }
        }

        public override string HelpDescription
        {
            get { return string.Format("{0} example usage: 'wtf is a box?'", this.Command); }
        }

        public UrbanDictionaryPlugin(
                IMtgStore store,
                ICommandParser commandParser,
                IHttpClient httpClient,
                IUrlShortener urlShortener)
            : base(
                store,
                commandParser,
                httpClient,
                urlShortener)
        {
        }

        public override void OnLoad()
        {
        }

        public override void OnUnload()
        {
        }

        public override async Task<bool> OnMessage(IMessage message, IMessenger messenger)
        {
            return false;
        }

        public override async Task<bool> OnCommand(Command command, IMessage message, IMessenger messenger)
        {
            if (command == null)
                throw new ArgumentNullException("command");

            if (message == null)
                throw new ArgumentNullException("message");

            if (messenger == null)
                throw new ArgumentNullException("messenger");

            string url = "http://api.urbandictionary.com/v0/define?page=http://ikonzeh.org&term={0}";

            var urbanDict = new UrbanDictionaryFetcher(url, base.HttpClient);

            if (command.Arguments.Any())
            {
                UrbanDictionaryData defData = null;

                // wtf is a <text>?
                if (command.Arguments.Length == 1)
                {
                    if (command.Arguments[0].StartsWith("is a"))
                    {
                        string word = command.Arguments[0].Replace("is a", "").Trim();
                        word = word.Replace("?", ""); // Remove any question marks

                        defData = await urbanDict.GetDefinition(word);
                    }
                    else if (command.Arguments[0].StartsWith("is"))
                    {
                        string word = command.Arguments[0].Replace("is", "").Trim();
                        word = word.Replace("?", ""); // Remove any question marks

                        defData = await urbanDict.GetDefinition(word);
                    }
                }

                if (defData != null)
                {
                    if (defData.ResultType == "no_results")
                        return false;

                    if (defData.Definitions.Any())
                    {
                        var definition = defData.Definitions.FirstOrDefault();
                        if (definition != null)
                        {
                            messenger.SendMessage(definition.Definition);

                            if (defData.Tags.Any())
                            {
                                string tags = string.Join(", ", defData.Tags.Take(5).ToArray());

                                messenger.SendMessage(string.Format("Perhaps you meant {0}.", tags));
                            }
                        }
                    }
                }
            }

            return false;
        }
    }
}
