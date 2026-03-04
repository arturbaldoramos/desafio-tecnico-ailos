namespace Tarifa.Domain.Entities
{
    public class Idempotencia
    {
        public string ChaveIdempotencia { get; set; } = string.Empty;
        public string? Requisicao { get; set; }
        public string? Resultado { get; set; }
    }
}
