using System.IO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using PgpCore;
using System.Threading.Tasks;
using System.Net.Http;
using System;
using System.Text;
using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;

namespace AzureFunctionsPGPDecrypt
{
    public static class PGPDecrypt
    {
        private const string PrivateKeyEnvironmentVariable = "pgp-private-key";

        [FunctionName(nameof(PGPDecrypt))]
        public static async Task<IActionResult> RunAsync(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)]
        HttpRequest req, ILogger log)
        {
            log.LogInformation($"C# HTTP trigger function {nameof(PGPDecrypt)} processed a request.");

            string privateKeyBase64 = Environment.GetEnvironmentVariable(PrivateKeyEnvironmentVariable);

            if (string.IsNullOrEmpty(privateKeyBase64))
            {
                return new BadRequestObjectResult($"Please add a base64 encoded private key to an environment variable called {PrivateKeyEnvironmentVariable}");
            }

            byte[] privateKeyBytes = Convert.FromBase64String(privateKeyBase64);
            string privateKey = Encoding.UTF8.GetString(privateKeyBytes);

            Stream decryptedData = await DecryptAsync(req.Body, privateKey);

            return new OkObjectResult(decryptedData);
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
