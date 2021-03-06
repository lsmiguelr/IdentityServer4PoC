﻿// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using IdentityModel.Client;
using Newtonsoft.Json.Linq;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Client
{
    public class Program
    {
        public static void Main(string[] args) => MainAsync().GetAwaiter().GetResult();
        
        private static async Task MainAsync()
        {
            // discover endpoints from metadata
            var disco = await DiscoveryClient.GetAsync("http://localhost:5000");
            if (disco.IsError)
            {
                Console.WriteLine(disco.Error);
                return;
            }

            // request token
            var tokenClient = new TokenClient(disco.TokenEndpoint, "cc.client", "secret");
            var tokenResponse = await tokenClient.RequestClientCredentialsAsync("api.full_access");

            if (tokenResponse.IsError)
            {
                Console.WriteLine(tokenResponse.Error);
                return;
            }

            //Console.WriteLine(tokenResponse.Json);
            Console.WriteLine($"START: {DateTime.Now}");
            Console.WriteLine("\n\n");

            // call api
            var client = new HttpClient();
            client.SetBearerToken(tokenResponse.AccessToken);

            for (; ;)
            {
                var response = await client.GetAsync("http://localhost:5001/identity");
                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine(response.StatusCode);
                    Console.WriteLine($"ERROR: {DateTime.Now}");

                    if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                        break;
                }
                else
                {
                    var content = await response.Content.ReadAsStringAsync();
                    //Console.WriteLine(JArray.Parse(content));
                    Console.WriteLine($"ITERATION: {DateTime.Now}");
                }

                await Task.Delay(60000);
            }
            
            Console.ReadKey();
        }
    }
}