using System.IO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using PgpCore;
using System.Threading.Tasks;
using System;
using System.Text;
using Microsoft.Extensions.Logging;
using Org.BouncyCastle.Bcpg.OpenPgp;

namespace AzureFunctionsPGPDecrypt
{
    public static class PGPDecryptAndVerify
    {
        [FunctionName(nameof(PGPDecryptAndVerify))]
        public static async Task<IActionResult> RunAsync([HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequest req, ILogger log)
        {
            log.LogInformation($"C# HTTP trigger function {nameof(PGPDecryptAndVerify)} processed a request.");

            string privateKeyBase64 = Environment.GetEnvironmentVariable("pgp-private-key");
            string passPhrase = Environment.GetEnvironmentVariable("pgp-passphrase");
            string publicKeyVerifyBase64 = Environment.GetEnvironmentVariable("pgp-public-key-verify");

            if (string.IsNullOrEmpty(privateKeyBase64))
            {
                return new BadRequestObjectResult($"Please add a base64 encoded private key to an environment variable called pgp-private-key");
            }

            if (string.IsNullOrEmpty(publicKeyVerifyBase64))
            {
                return new BadRequestObjectResult($"Please add a base64 encoded public key to an environment variable called pgp-public-key-verify");
            }

            byte[] privateKeyBytes = Convert.FromBase64String(privateKeyBase64);
            string privateKey = Encoding.UTF8.GetString(privateKeyBytes);

            byte[] publicKeyVerifyBytes = Convert.FromBase64String(publicKeyVerifyBase64);
            string publicKeyVerify = Encoding.UTF8.GetString(publicKeyVerifyBytes);

            try
            {
                Stream decryptedData = await DecryptAndVerifyAsync(req.Body, privateKey, publicKeyVerify, passPhrase);
                return new OkObjectResult(decryptedData);
            }
            catch (PgpException pgpException)
            {
                return new BadRequestObjectResult(pgpException.Message);
            }
        }

        private static async Task<Stream> DecryptAndVerifyAsync(Stream inputStream, string privateKey, string publicKeyVerify, string passPhrase)
        {
            using (PGP pgp = new PGP())
            {
                Stream outputStream = new MemoryStream();

                using (inputStream)
                using (Stream privateKeyStream = privateKey.ToStream())
                using (Stream publicKeyVerifyStream = publicKeyVerify.ToStream())
                {
                    await pgp.DecryptStreamAndVerifyAsync(inputStream, outputStream, publicKeyVerifyStream, privateKeyStream, passPhrase);
                    outputStream.Seek(0, SeekOrigin.Begin);
                    return outputStream;
                }
            }
        }
    }
}
