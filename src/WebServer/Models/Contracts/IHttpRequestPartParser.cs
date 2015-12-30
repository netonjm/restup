﻿using Devkoes.Restup.WebServer.Http;

namespace Devkoes.Restup.WebServer.Models.Contracts
{
    interface IHttpRequestPartParser
    {
        void HandleRequestPart(byte[] stream, HttpRequest resultThisFar);
        byte[] UnparsedData { get; }
        bool IsFinished { get; }
        bool IsSucceeded { get; }
    }
}