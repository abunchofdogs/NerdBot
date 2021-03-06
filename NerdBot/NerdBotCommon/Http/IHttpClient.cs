﻿using System.Threading.Tasks;

namespace NerdBotCommon.Http
{
    public interface IHttpClient
    {
        string Post(string url, string json);
        string GetPageSource(string url);
        Task<string> GetAsJson(string url);
        Task<string> GetAsJsonNonCached(string url);
        string GetResponseAsString(string url);
    }
}
