﻿using AspNetCoreRateLimit;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Chandler
{
    /// <summary>
    /// Program
    /// </summary>
    public class Program
    {
        /// <summary>
        /// Main
        /// </summary>
        /// <param name="args"></param>
        public static async Task Main(string[] args)
        {
            if(args.Contains("-setup"))
            {
                Console.WriteLine("doot doot setup");
                return;
            }

            var webhost = CreateWebHostBuilder(args).Build();

            using var scope = webhost.Services.CreateScope();
            var ipPolicyStore = scope.ServiceProvider.GetRequiredService<IIpPolicyStore>();
            await ipPolicyStore.SeedAsync();

            await webhost.RunAsync();
        }

        /// <summary>
        /// CreateWebHostBuilder
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
            .UseUrls("http://*:420");
    }
}