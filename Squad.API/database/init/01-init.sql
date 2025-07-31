-- Initialize Squad.co transaction database
-- This script creates tables for storing payment transactions and webhook events

-- Create transactions table
CREATE TABLE IF NOT EXISTS squad_transactions (
    id SERIAL PRIMARY KEY,
    transaction_ref VARCHAR(255) UNIQUE NOT NULL,
    email VARCHAR(255) NOT NULL,
    amount INTEGER NOT NULL,
    currency VARCHAR(10) DEFAULT 'NGN',
    status VARCHAR(50) DEFAULT 'pending',
    checkout_url TEXT,
    merchant_id VARCHAR(255),
    customer_name VARCHAR(255),
    metadata JSONB,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Create webhook_events table
CREATE TABLE IF NOT EXISTS squad_webhook_events (
    id SERIAL PRIMARY KEY,
    event_type VARCHAR(100) NOT NULL,
    transaction_ref VARCHAR(255) NOT NULL,
    signature VARCHAR(500),
    payload JSONB NOT NULL,
    processed_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    status VARCHAR(50) DEFAULT 'received'
);

-- Create payment_verifications table
CREATE TABLE IF NOT EXISTS squad_payment_verifications (
    id SERIAL PRIMARY KEY,
    transaction_ref VARCHAR(255) UNIQUE NOT NULL,
    verification_status VARCHAR(50) NOT NULL,
    verification_response JSONB,
    verified_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Create indexes for better performance
CREATE INDEX IF NOT EXISTS idx_squad_transactions_ref ON squad_transactions(transaction_ref);
CREATE INDEX IF NOT EXISTS idx_squad_transactions_status ON squad_transactions(status);
CREATE INDEX IF NOT EXISTS idx_squad_transactions_email ON squad_transactions(email);
CREATE INDEX IF NOT EXISTS idx_webhook_events_ref ON squad_webhook_events(transaction_ref);
CREATE INDEX IF NOT EXISTS idx_webhook_events_type ON squad_webhook_events(event_type);

-- Create function to update updated_at timestamp
CREATE OR REPLACE FUNCTION update_updated_at_column()
RETURNS TRIGGER AS $$
BEGIN
    NEW.updated_at = CURRENT_TIMESTAMP;
    RETURN NEW;
END;
$$ language 'plpgsql';

-- Create trigger for updated_at
CREATE TRIGGER update_squad_transactions_updated_at 
    BEFORE UPDATE ON squad_transactions 
    FOR EACH ROW EXECUTE FUNCTION update_updated_at_column();

-- Insert sample data for testing (optional)
INSERT INTO squad_transactions (transaction_ref, email, amount, currency, status, customer_name) 
VALUES 
    ('TEST_REF_001', 'test@example.com', 10000, 'NGN', 'pending', 'Test User'),
    ('TEST_REF_002', 'demo@example.com', 25000, 'NGN', 'success', 'Demo User')
ON CONFLICT (transaction_ref) DO NOTHING; 