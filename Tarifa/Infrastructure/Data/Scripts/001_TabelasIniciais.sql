-- Tabela de tarifacoes
CREATE TABLE IF NOT EXISTS tarifacao (
    id SERIAL PRIMARY KEY,
    numerocontacorrente TEXT NOT NULL,
    idrequisicaotransferencia TEXT NOT NULL UNIQUE,
    valor DECIMAL(18,2) NOT NULL,
    datatarfacao TIMESTAMP NOT NULL
);

-- Tabela de idempotencia para evitar tarifacoes duplicadas
CREATE TABLE IF NOT EXISTS idempotencia (
    chave_idempotencia TEXT PRIMARY KEY,
    requisicao TEXT,
    resultado TEXT
);
