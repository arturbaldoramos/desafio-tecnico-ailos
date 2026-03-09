CREATE TABLE IF NOT EXISTS deposito (
    id SERIAL PRIMARY KEY,
    idrequisicao TEXT NOT NULL UNIQUE,
    numeroconta TEXT NOT NULL,
    valor DECIMAL(18,2) NOT NULL,
    datadeposito TIMESTAMP NOT NULL,
    status TEXT NOT NULL DEFAULT 'PENDENTE',
    mensagemerro TEXT
);

CREATE TABLE IF NOT EXISTS saque (
    id SERIAL PRIMARY KEY,
    idrequisicao TEXT NOT NULL UNIQUE,
    numeroconta TEXT NOT NULL,
    valor DECIMAL(18,2) NOT NULL,
    datasaque TIMESTAMP NOT NULL,
    status TEXT NOT NULL DEFAULT 'PENDENTE',
    mensagemerro TEXT
);

CREATE TABLE IF NOT EXISTS idempotencia (
    chave_idempotencia TEXT PRIMARY KEY,
    requisicao TEXT,
    resultado TEXT
);
