﻿using System;
using System.Threading;
using System.Threading.Tasks;
using JsonRpc;
using JsonRpc.HandleResult;
using JsonRpc.Messages;
using Newtonsoft.Json.Linq;

namespace TestingApp
{
    internal class A
    {
        public string Name { get; set; }
    }

    internal class Input : IInput
    {
        public Task<JToken> ReadAsync(CancellationToken cancellationToken = default)
        {
            Request request = new Request(new MessageId(123), "abc", new { Name = "add", K = 5 });

            return Task.FromResult(JToken.FromObject(
                request
            ));
        }
    }

    [RemoteMethodHandler("abc")]
    internal class Handler
    {
        public static Task<ErrorResult> Handle(string name, int k)
        {
            Console.WriteLine($"Handled: {k}");

            return Task.FromResult(
                new ErrorResult(new Error(ErrorCode.InternalError, "abcdefg", new { ABC = 5000 }))
            );
        }
    }

    internal class Output : IOutput
    {
        public Task WriteAsync(JToken response, CancellationToken cancellationToken = default)
        {
            Console.WriteLine(response);
            return Task.CompletedTask;
        }
    }

    internal class Program
    {
        private static void Main(string[] args)
        {
            MainAsync(args).Wait();
        }

        private static async Task MainAsync(string[] args)
        {
            RpcService rpcService = new RpcService();
            rpcService.RegisterHandler(new Handler());
            await rpcService.HandleRequest(new Input(), new Output());
        }
    }
}