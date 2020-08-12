// Copyright 2020 Heath Stewart.
// Licensed under the MIT License.See LICENSE.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Azure.KeyVault;
using Microsoft.Azure.KeyVault.Models;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.Rest;
using Microsoft.Rest.Azure;
using Microsoft.Rest.Tracing.Etw;

namespace KeyVaultClientLogging
{
    class Program
    {
        static async Task Main()
        {
            // Add the ETW trace logger. Listen for events from the Microsoft.Rest provider.
            ServiceClientTracing.AddTracingInterceptor(new EtwTracingInterceptor());
            ServiceClientTracing.IsEnabled = true;

            // To authenticate with Managed Identity or a service principal,
            // see https://docs.microsoft.com/azure/key-vault/general/service-to-service-authentication.

            string vaultUri = Environment.GetEnvironmentVariable("AZURE_KEYVAULT_URL") ??
                throw new InvalidOperationException("AZURE_KEYVAULT_URL environment variable not defined");

            string clientId = Environment.GetEnvironmentVariable("AZURE_CLIENT_ID") ??
                throw new InvalidOperationException("AZURE_CLIENT_ID environment variable not defined");

            string clientSecret = Environment.GetEnvironmentVariable("AZURE_CLIENT_SECRET") ??
                throw new InvalidOperationException("AZURE_CLIENT_SECRET environment variable not defined");

            // Show the process ID and pause to let users attach a trace listener.
            using (Process p = Process.GetCurrentProcess())
            {
                Console.Write($"Attach trace listener to process {p.Id} and press Enter to continue...");
                Console.ReadLine();
            }

            KeyVaultClient client = new KeyVaultClient(async (authority, resource, scope) =>
            {
                string id = Guid.NewGuid().ToString("n");

                // Log information helpful for diagnostics.
                ServiceClientTracing.Enter(id, null, "Authenticate", new Dictionary<string, object>
                {
                    [nameof(authority)] = authority,
                    [nameof(resource)] = resource,
                    [nameof(scope)] = scope,
                });

                ClientCredential credential = new ClientCredential(clientId, clientSecret);

                // Normally should use TokenCache.DefaultShared, but need to trace possible authentication issues.
                AuthenticationContext context = new AuthenticationContext(authority, null);

                ServiceClientTracing.Information($"Authenticating client: {clientId}");
                AuthenticationResult result = await context.AcquireTokenAsync(resource, credential);

                ServiceClientTracing.Exit(id, $"{result.AccessToken.Substring(8)}...");
                return result.AccessToken;
            });

            IPage<SecretItem> secrets = await client.GetSecretsAsync(vaultUri);
            do
            {
                foreach (SecretItem item in secrets)
                {
                    SecretBundle secret = await client.GetSecretAsync(item.Id);
                    Console.WriteLine($"{secret.SecretIdentifier.Name} = {secret.Value}");
                }

                secrets = secrets.NextPageLink != null
                    ? await client.GetSecretsNextAsync(secrets.NextPageLink)
                    : null;

            } while (secrets != null);
        }
    }
}
