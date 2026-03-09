using System.Security.Cryptography;

namespace Autenticacao.Infrastructure.Security
{
    public interface IRsaKeyService
    {
        RSA GetPrivateKey();
        RSA GetPublicKey();
    }

    public class RsaKeyService : IRsaKeyService
    {
        private readonly RSA _privateKey;
        private readonly RSA _publicKey;

        public RsaKeyService(IConfiguration configuration)
        {
            var privateKeyPath = configuration.GetValue<string>("RsaKeys:PrivateKeyPath")!;
            var publicKeyPath = configuration.GetValue<string>("RsaKeys:PublicKeyPath")!;

            _privateKey = RSA.Create();
            _privateKey.ImportFromPem(File.ReadAllText(privateKeyPath));

            _publicKey = RSA.Create();
            _publicKey.ImportFromPem(File.ReadAllText(publicKeyPath));
        }

        public RSA GetPrivateKey() => _privateKey;
        public RSA GetPublicKey() => _publicKey;
    }
}
