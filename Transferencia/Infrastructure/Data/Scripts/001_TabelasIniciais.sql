-- Tabela de transferências
CREATE TABLE IF NOT EXISTS transferencia (
    id SERIAL PRIMARY KEY,
    idrequisicao TEXT NOT NULL,
    numerocontaorigem TEXT NOT NULL,
    numerocontadestino TEXT NOT NULL,
    datamovimento TIMESTAMP NOT NULL,
    valor DECIMAL(18,2) NOT NULL,
    status TEXT NOT NULL DEFAULT 'PENDENTE',
    mensagemerro TEXT
);

-- Tabela de idempotência para evitar transferências duplicadas
CREATE TABLE IF NOT EXISTS idempotencia (
    chave_idempotencia TEXT PRIMARY KEY,
    requisicao TEXT,
    resultado TEXT
);
