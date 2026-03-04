namespace ContaCorrente.Infrastructure.Security
{
    public interface IPasswordHasher
    {
        string Hash(string password);
        bool Verify(string password, string hashedPassword);
    }

    public class PasswordHasher : IPasswordHasher
    {
        private const int WorkFactor = 12;

        /// <summary>
        /// Gera hash da senha usando BCrypt
        /// </summary>
        public string Hash(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password, WorkFactor);
        }

        /// <summary>
        /// Verifica se a senha corresponde ao hash armazenado
        /// </summary>
        public bool Verify(string password, string hashedPassword)
        {
            if (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(hashedPassword))
                return false;

            try
            {
                return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
            }
            catch
            {
                return false;
            }
        }
    }
}
