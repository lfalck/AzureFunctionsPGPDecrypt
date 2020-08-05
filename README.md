[![license](https://img.shields.io/github/license/lfalck/AzureFunctionsPGPDecrypt.svg)]()
# AzureFunctionsPGPDecrypt
Azure function which performs PGP decryption using [PgpCore](https://github.com/mattosaurus/PgpCore). The private key can be stored in an environment variable or in Azure Key Vault by using [Key Vault references](https://docs.microsoft.com/en-us/azure/app-service/app-service-key-vault-references).

# Get started
* Store the Base64 encoded private key or a Key Vault reference in an environment variable called pgp-private-key.
* Store a passphrase or a Key Vault reference in an environment variable called pgp-passphrase (optional).
* Make a request to the function with the encrypted data in the body.

## Key generation
Option 1: Use a program such as [GPG](https://gnupg.org/) or [GPW4Win](https://www.gpg4win.org/).  
Option 2: Use the console app **PGPEncryptConsoleApp** in the repo [AzureFunctionsPGPEncrypt](https://github.com/lfalck/AzureFunctionsPGPEncrypt)
