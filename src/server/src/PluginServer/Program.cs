﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using JsonRpc;
using JsonRpc.DependencyInjection;
using LanguageServerProtocol;
using LanguageServerProtocol.IPC;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace PluginServer
{
    internal static class Program
    {
        internal static async Task Main(string[] args)
        {
#if DEBUG && WAIT_FOR_DEBUGGER
            while (!Debugger.IsAttached)
            {
                Thread.Sleep(1000);
            }

            Debugger.Break();
#endif
            IConfiguration configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();

            List<int> l = new List<int>();
            
            foreach(int i in l)
            {

            }

            IServiceCollection serviceCollection = new ServiceCollection();

            serviceCollection.AddLogging(builder =>
            {
                builder.AddConfiguration(configuration.GetSection("Logging"));
                builder.AddDebug();
            });

            serviceCollection.AddStdIo();
            serviceCollection.AddJsonRpc();
            serviceCollection.AddLsp();

            IServiceProvider provider = serviceCollection.BuildServiceProvider();
            IRpcService service = provider.GetService<IRpcService>();

            while (true)
            {
                await service.HandleRequest();
            }
        }
    }
}