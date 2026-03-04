namespace ContaCorrente.Application.Validators
{
    public static class CpfValidator
    {
        public static bool IsValid(string cpf)
        {
            if (string.IsNullOrWhiteSpace(cpf))
                return false;

            // Remove caracteres não numéricos
            cpf = new string(cpf.Where(char.IsDigit).ToArray());

            // 11 dígitos
            if (cpf.Length != 11)
                return false;

            // Verifica se todos os dígitos são iguais (ex: 111.111.111-11)
            if (cpf.Distinct().Count() == 1)
                return false;

            // Calcula primeiro dígito verificador
            int soma = 0;
            for (int i = 0; i < 9; i++)
                soma += (cpf[i] - '0') * (10 - i);

            int resto = soma % 11;
            int primeiroDigito = resto < 2 ? 0 : 11 - resto;

            if (cpf[9] - '0' != primeiroDigito)
                return false;

            // Calcula segundo dígito verificador
            soma = 0;
            for (int i = 0; i < 10; i++)
                soma += (cpf[i] - '0') * (11 - i);

            resto = soma % 11;
            int segundoDigito = resto < 2 ? 0 : 11 - resto;

            if (cpf[10] - '0' != segundoDigito)
                return false;

            return true;
        }
    }
}
