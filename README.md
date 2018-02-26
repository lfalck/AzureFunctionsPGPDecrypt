[![license](https://img.shields.io/github/license/lfalck/AzureFunctionsPGPDecrypt.svg)]()
# AzureFunctionsPGPDecrypt
Azure function which performs PGP decryption using [PgpCore](https://github.com/mattosaurus/PgpCore). The private key is stored in Azure Key Vault and accessed using [Managed Service Identity](https://docs.microsoft.com/en-us/azure/app-service/app-service-managed-service-identity).

# Get started
* Enable Managed Service Identity for your Function App
* Add an Access Policy in Key Vault which gives Get permissions for Secrets to your Function App Principal
* Add a Base64 encoded private key as an Azure Key Vault Secret
* Make a request to the function with the encrypted data in the body and the Key Vault Secret Identifier (e.g. https://vaultname.vault.azure.net/secrets/secretname/) as a query parameter called privatekeysecretid.
