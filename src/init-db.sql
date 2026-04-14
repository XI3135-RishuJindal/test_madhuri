-- Initialize User Management Service database

-- Create users table
CREATE TABLE IF NOT EXISTS users (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    name VARCHAR(255) NOT NULL,
    email VARCHAR(255) NOT NULL UNIQUE,
    password_hash VARCHAR(255) NOT NULL,
    email_verified BOOLEAN DEFAULT FALSE,
    role VARCHAR(50) DEFAULT 'User',
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE
);

-- Create index on email for faster lookups
CREATE INDEX IF NOT EXISTS idx_users_email ON users(email);

-- Create index on created_at for sorting
CREATE INDEX IF NOT EXISTS idx_users_created_at ON users(created_at);

-- Insert a default admin user (password: admin123)
-- SHA256 hash of 'admin123' = 'JAvlGPq9JyTdtvBO6x2llnRI1+gxwIyPqCKAn3THIKk='
INSERT INTO users (id, name, email, password_hash, email_verified, role, created_at)
VALUES (
    'a0eebc99-9c0b-4ef8-bb6d-6bb9bd380a11',
    'Admin User',
    'admin@example.com',
    'JAvlGPq9JyTdtvBO6x2llnRI1+gxwIyPqCKAn3THIKk=',
    TRUE,
    'Admin',
    CURRENT_TIMESTAMP
) ON CONFLICT (email) DO NOTHING;
