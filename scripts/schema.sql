-- ============================================================
-- BancoSol API — Script de creación de base de datos
-- ============================================================

-- Crear la base de datos
CREATE DATABASE bancosol_core_db;

-- Conectarse a ella
\c bancosol_core_db;

-- Tabla de parámetros del sistema
CREATE TABLE parameters (
    id SERIAL PRIMARY KEY,
    category VARCHAR(50) NOT NULL,
    code VARCHAR(20) NOT NULL,
    description VARCHAR(100) NOT NULL,
    is_active BOOLEAN NOT NULL DEFAULT true,
    created_at TIMESTAMP NOT NULL DEFAULT NOW(),
    CONSTRAINT uq_parameters_category_code UNIQUE (category, code)
);

-- Tabla de clientes
CREATE TABLE customers (
    id BIGSERIAL PRIMARY KEY,
    first_name VARCHAR(150) NOT NULL,
    second_name VARCHAR(150),
    first_last_name VARCHAR(150) NOT NULL,
    second_last_name VARCHAR(150),
    ci VARCHAR(20) NOT NULL,
    email VARCHAR(100) NOT NULL,
    phone VARCHAR(20),
    created_at TIMESTAMP NOT NULL DEFAULT NOW(),
    CONSTRAINT uq_customers_ci UNIQUE (ci),
    CONSTRAINT uq_customers_email UNIQUE (email)
);

-- Tabla de cuentas (billeteras)
CREATE TABLE accounts (
    id BIGSERIAL PRIMARY KEY,
    account_number VARCHAR(20) NOT NULL,
    customer_id BIGINT NOT NULL,
    currency VARCHAR(3) NOT NULL,
    balance DECIMAL(18,2) NOT NULL DEFAULT 0,
    status VARCHAR(20) NOT NULL DEFAULT 'Active',
    created_at TIMESTAMP NOT NULL DEFAULT NOW(),
    CONSTRAINT uq_accounts_account_number UNIQUE (account_number),
    CONSTRAINT fk_accounts_customer FOREIGN KEY (customer_id) REFERENCES customers(id)
);

-- Tabla de movimientos (depósitos, retiros, transferencias)
CREATE TABLE transactions (
    id BIGSERIAL PRIMARY KEY,
    account_id BIGINT NOT NULL,
    type VARCHAR(30) NOT NULL,
    amount DECIMAL(18,2) NOT NULL,
    previous_balance DECIMAL(18,2) NOT NULL,
    new_balance DECIMAL(18,2) NOT NULL,
    description VARCHAR(255),
    exchange_rate DECIMAL(18,6),
    created_at TIMESTAMP NOT NULL DEFAULT NOW(),
    CONSTRAINT fk_transactions_account FOREIGN KEY (account_id) REFERENCES accounts(id)
);

-- Tabla de transferencias entre cuentas
CREATE TABLE transfers (
    id BIGSERIAL PRIMARY KEY,
    source_account_id BIGINT NOT NULL,
    destination_account_id BIGINT NOT NULL,
    source_amount DECIMAL(18,2) NOT NULL,
    destination_amount DECIMAL(18,2) NOT NULL,
    source_currency VARCHAR(3) NOT NULL,
    destination_currency VARCHAR(3) NOT NULL,
    exchange_rate DECIMAL(18,6),
    idempotency_key VARCHAR(100) NOT NULL,
    status VARCHAR(20) NOT NULL DEFAULT 'Completed',
    created_at TIMESTAMP NOT NULL DEFAULT NOW(),
    CONSTRAINT uq_transfers_idempotency_key UNIQUE (idempotency_key),
    CONSTRAINT fk_transfers_source FOREIGN KEY (source_account_id) REFERENCES accounts(id),
    CONSTRAINT fk_transfers_destination FOREIGN KEY (destination_account_id) REFERENCES accounts(id)
);

-- Dato inicial requerido por el sistema para numeración de cuentas
INSERT INTO parameters (category, code, description, is_active)
VALUES ('ACCOUNT', 'INITIAL_NUMBER', '10000000000001', true);
