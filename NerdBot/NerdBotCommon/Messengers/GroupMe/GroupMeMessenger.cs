﻿using System;
using Nancy.Json;
using NerdBotCommon.Http;
using SimpleLogging.Core;

namespace NerdBotCommon.Messengers.GroupMe
{
    public class GroupMeMessenger : IMessenger
    {
        private readonly IHttpClient mHttpClient;
        private readonly ILoggingService mLogger;
        private readonly string mBotId;
        private readonly string mBotName;
        private readonly string mEndpointUrl;
        private readonly string[] mIgnoreNames;

        #region Properties
        public string BotId
        {
            get { return this.mBotId; }
        }

        public string BotName
        {
            get { return this.mBotName; }
        }

        public string[] IgnoreNames
        {
            get { return this.mIgnoreNames; }
        }
        #endregion

        public GroupMeMessenger(
            string botId,
            string botName,
            string[] ignoreNames,
            string endPointUrl,
            IHttpClient httpClient,
            ILoggingService logger)
        {
            if (string.IsNullOrEmpty(botId))
                throw new ArgumentException("botId");

            if (string.IsNullOrEmpty(botName))
                throw new ArgumentException("botName");

            if (string.IsNullOrEmpty(endPointUrl))
                throw new ArgumentException("endPointUrl");

            if (httpClient == null)
                throw new ArgumentNullException("httpClient");

            if (logger == null)
                throw new ArgumentNullException("logger");

            this.mBotId = botId;
            this.mBotName = botName;
            this.mIgnoreNames = ignoreNames;
            this.mEndpointUrl = endPointUrl;
            this.mHttpClient = httpClient;
            this.mLogger = logger;
        }

        public bool SendMessage(string message)
        {
            if (string.IsNullOrEmpty(message))
                throw new ArgumentException("message");

            string json = new JavaScriptSerializer().Serialize(new
            {
                text = message,
                bot_id = this.mBotId
            });

            try
            {
                this.mLogger.Trace("Sending message '{0}' using botId '{1}'...", message, this.mBotId);

                string result = this.mHttpClient.Post(this.mEndpointUrl, json);

                return true;
            }
            catch (Exception er)
            {
                this.mLogger.Error(er, string.Format("Error sending groupme message: {0}", message));

                return false;
            }
        }

        public bool SendMessageWithMention(string message, string mentionId, int start, int end)
        {
            if (string.IsNullOrEmpty(message))
                throw new ArgumentException("message");

            string json = new JavaScriptSerializer().Serialize(new
            {
                text = message,
                bot_id = this.mBotId,
                attachments = new [] {
                    new
                    {
                        loci = new[]
                        {
                            new []
                            {
                                start, end
                            }
                        },
                        type = "mentions",
                        user_ids = new []
                        {
                            mentionId
                        }
                    }
                }
            });

            try
            {
                this.mLogger.Trace("Sending message '{0}' using botId '{1}'...", message, this.mBotId);

                string result = this.mHttpClient.Post(this.mEndpointUrl, json);

                return true;
            }
            catch (Exception er)
            {
                this.mLogger.Error(er, string.Format("Error sending groupme message: {0}", message));

                return false;
            }
        }
    }
}
