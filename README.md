[![license](https://img.shields.io/github/license/lfalck/AzureFunctionsPGPDecrypt.svg)]()
# AzureFunctionsPGPDecrypt
Azure function which performs PGP decryption using [PgpCore](https://github.com/mattosaurus/PgpCore). The private key is stored in Azure Key Vault and accessed using [Managed Service Identity](https://docs.microsoft.com/en-us/azure/app-service/app-service-managed-service-identity).

# Get started
* Enable Managed Service Identity for your Function App
* Add an Access Policy in Key Vault which gives Get permissions for Secrets to your Function App Principal
* Add a Base64 encoded private key as an Azure Key Vault Secret
* Make a request to the function with the encrypted data in the body and the Key Vault Secret Identifier (e.g. https://vaultname.vault.azure.net/secrets/secretname/version) as a query parameter called privatekeysecretid.

## Passphrase (optional)
* Add a Base64 encoded passphrase as a Azure Key Vault Secret
* Pass the Key Vault Secret Identifier as a query parameter called passphrasesecretid.

## Key generation
Option 1: Use a program such as [GPG](https://gnupg.org/) or [GPW4Win](https://www.gpg4win.org/).  
Option 2: Use the console app **PGPEncryptConsoleApp** in the repo [AzureFunctionsPGPEncrypt](https://github.com/lfalck/AzureFunctionsPGPEncrypt)

# Caching
To increase performance the function does some simple caching to avoid fetching from Azure Key Vault on each invocation. The Key Vault Secret Identifier is used as a key in the cache, which means that if you include the secret version in the identifier new versions will always be fetched from Key Vault.
