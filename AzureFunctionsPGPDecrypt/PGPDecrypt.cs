using System.IO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs.Host;
using PgpCore;
using System.Threading.Tasks;
using Microsoft.Azure.KeyVault.Models;
using System.Net.Http;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Azure.KeyVault;
using System;
using System.Text;
using System.Collections.Concurrent;

namespace AzureFunctionsPGPDecrypt
{
    public static class PGPDecrypt
    {
        private static HttpClient client = new HttpClient();
        private static ConcurrentDictionary<string, string> secrects = new ConcurrentDictionary<string, string>();

        [FunctionName(nameof(PGPDecrypt))]
        public static async Task<IActionResult> RunAsync(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)]
        HttpRequest req, TraceWriter log)
        {
            log.Info($"C# HTTP trigger function {nameof(PGPDecrypt)} processed a request.");

            string privateKeySecretId = req.Query["privatekeysecretid"];

            if (privateKeySecretId == null)
            {
                return new BadRequestObjectResult("Please pass a private key secret identifier on the query string");
            }

            string privateKey;
            try
            {
                privateKey = await GetPrivateKeyAsync(privateKeySecretId);
            }
            catch (KeyVaultErrorException e) when (e.Body.Error.Code == "SecretNotFound")
            {
                return new NotFoundResult();
            }
            catch (KeyVaultErrorException e) when (e.Body.Error.Code == "Forbidden")
            {
                return new UnauthorizedResult();
            }

            Stream decryptedData = await DecryptAsync(req.Body, privateKey);

            return new OkObjectResult(decryptedData);
        }

        private static async Task<string> GetPrivateKeyAsync(string secretIdentifier)
        {
            if (!secrects.ContainsKey(secretIdentifier))
            {
                var azureServiceTokenProvider = new AzureServiceTokenProvider();
                var authenticationCallback = new KeyVaultClient.AuthenticationCallback(azureServiceTokenProvider.KeyVaultTokenCallback);
                var kvClient = new KeyVaultClient(authenticationCallback, client);

                SecretBundle secretBundle = await kvClient.GetSecretAsync(secretIdentifier);
                byte[] data = Convert.FromBase64String(secretBundle.Value);
                secrects[secretIdentifier] = Encoding.UTF8.GetString(data);
            }
            return secrects[secretIdentifier];
        }

        private static async Task<Stream> DecryptAsync(Stream inputStream, string privateKey)
        {
            using (PGP pgp = new PGP())
            {
                Stream outputStream = new MemoryStream();

                using (inputStream)
                using (Stream privateKeyStream = GenerateStreamFromString(privateKey))
                {
                    await pgp.DecryptStreamAsync(inputStream, outputStream, privateKeyStream, null);
                    outputStream.Seek(0, SeekOrigin.Begin);
                    return outputStream;
                }
            }
        }

        private static Stream GenerateStreamFromString(string s)
        {
            MemoryStream stream = new MemoryStream();
            StreamWriter writer = new StreamWriter(stream);
            writer.Write(s);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }
    }
}
