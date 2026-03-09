CREATE TABLE IF NOT EXISTS usuario (
    id SERIAL PRIMARY KEY,
    cpf TEXT UNIQUE NOT NULL,
    nome TEXT NOT NULL,
    senhahash TEXT NOT NULL,
    numeroconta TEXT UNIQUE NOT NULL,
    ativo INTEGER NOT NULL DEFAULT 1,
    datacriacao TIMESTAMP NOT NULL DEFAULT NOW()
);

CREATE TABLE IF NOT EXISTS idempotencia (
    chave_idempotencia TEXT PRIMARY KEY,
    requisicao TEXT,
    resultado TEXT
);
