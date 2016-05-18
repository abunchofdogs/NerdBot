﻿using System;
using System.Collections.Generic;
using System.Linq;
using MongoDB.Bson;
using Nancy.Extensions;
using Nancy.ModelBinding;
using Nancy.Routing;
using NerdBot.Messengers;
using NerdBot.Messengers.GroupMe;
using NerdBot.Mtg;
using NerdBot.Mtg.Prices;
using NerdBot.Parsers;
using NerdBot.Reporters;
using SimpleLogging.Core;

namespace NerdBot
{
    using Nancy;

    public class IndexModule : NancyModule
    {
        public IndexModule(
            BotConfig botConfig,
            IMtgStore mtgStore,
            IMessenger messenger,
            IPluginManager pluginManager,
            ICommandParser commandParser,
            ILoggingService loggingService,
            IReporter reporter,
            ICardPriceStore priceStore)
        {
            Get["/"] = parameters =>
            {
                loggingService.Warning("GET request from {0}: Path '{1}' was invalid.",
                        this.Request.UserHostAddress,
                        this.Request.Path);

                return HttpStatusCode.NotAcceptable;
            };

            // priceincreases route
            Get["/priceincreases/"] = parameters =>
            {
                int limit = 10;

                List<CardPrice> prices = priceStore.GetCardsByPriceIncrease(limit);

                return Response.AsJson<List<CardPrice>>(prices);
            };

            // pricedecreases route
            Get["/pricedecreases/"] = parameters =>
            {
                int limit = 10;

                List<CardPrice> prices = priceStore.GetCardsByPriceDecrease(limit);

                return Response.AsJson<List<CardPrice>>(prices);
            };

            // Ruling route
            Get["/ruling/{id:int}", true] = async (parameters, ct) =>
            {
                int cardMultiverseId = parameters.id;

                var card = await mtgStore.GetCard(cardMultiverseId);

                if (card == null)
                {
                    string msg = string.Format("No card found using multiverseId '{0}'", cardMultiverseId);

                    loggingService.Error(msg);

                    return msg;
                }

                return View["ruling.sshtml", card];
            };

            // Get search results
            Get["/search/{name}", true] = async (parameters, ct) =>
            {
                int limit = 50;

                string name = parameters.name;

                if (string.IsNullOrEmpty(name))
                {
                    return HttpStatusCode.Accepted;
                }

                var cards = await mtgStore.GetCards(name, limit);

                if (cards == null)
                {
                    string msg = string.Format("No cards found using name '{0}'", name);

                    loggingService.Error(msg);

                    return msg;
                }

                return View["search.sshtml", new
                {
                    SearchTerm = name, 
                    Cards = cards
                }];
            };

            Post["/bot/{token}", true] = async (parameters, ct) =>
            {
                try
                {
                    // Get the request's body as a string, for logging
                    string request_string = this.Request.Body.AsString();

                    string sentToken = parameters.token;

                    // If the passed token segment does not match the secret token, return NotAcceptable status
                    if (sentToken != botConfig.SecretToken)
                    {
                        string errMsg = string.Format("POST request from {0}: Token '{1}' was invalid.\nREQUEST = {2}",
                            this.Request.UserHostAddress,
                            sentToken,
                            request_string);

                        loggingService.Warning(errMsg);
                        reporter.Warning(errMsg);

                        return HttpStatusCode.NotAcceptable;
                    }

                    var message = new GroupMeMessage();

                    // Bind and validate the request to GroupMeMessage
                    var msg = this.BindToAndValidate(message);

                    if (!ModelValidationResult.IsValid)
                    {
                        string errMsg = string.Format("POST request from {0}: Message was invalid.\nREQUEST = {1}",
                            this.Request.UserHostAddress,
                            request_string);

                        loggingService.Warning(errMsg);
                        reporter.Warning(errMsg);

                        return HttpStatusCode.NotAcceptable;
                    }

                    // Don't handle messages sent from ourself
                    if (message.name.ToLower() == messenger.BotName.ToLower())
                        return HttpStatusCode.NotAcceptable;

                    if (string.IsNullOrEmpty(message.text))
                    {
                        loggingService.Debug("POST request from {0}: Message text is empty or null.\nREQUEST = {1}",
                            this.Request.UserHostAddress,
                            request_string);

                        return HttpStatusCode.NotAcceptable;
                    }

                    loggingService.Trace("MSG: From: {0} [UID: {1}]; Body: {2}", 
                        message.name,
                        message.user_id,
                        message.text);

                    // Parse the command
                    var command = commandParser.Parse(message.text);
                    if (command != null)
                    {
                        if (!string.IsNullOrEmpty(command.Cmd))
                        {
                            loggingService.Trace("Received command: {0}", command.Cmd);

                            if (command.Cmd.ToLower() == "help")
                            {
                                bool helpHandled = await pluginManager.HandledHelpCommand(command, messenger);
                            }
                            else
                            {
                                // If a message is in a command format '<cmd>\s[message]', 
                                //  have the plugin manager see if any loaded plugins are set to respond to that command
                                bool handled = await pluginManager.HandleCommand(command, message, messenger);

                                if (!handled)
                                    pluginManager.SendMessage(message, messenger);
                            }
                        }
                    }

                    return HttpStatusCode.Accepted;
                }
                catch (Exception er)
                {
                    reporter.Error("MAIN ERROR", er);
                    loggingService.Error(er, string.Format("** MAIN ERROR: {0}", er.Message));

                    return HttpStatusCode.BadGateway;
                }
            };
        }
    }
}