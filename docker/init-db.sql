-- Initialize databases for both services
CREATE DATABASE "UserServiceDb";
CREATE DATABASE "ContentServiceDb";

-- Create separate users for each service (optional, for better security)
CREATE USER userservice_user WITH PASSWORD 'userservice_pass';
CREATE USER contentservice_user WITH PASSWORD 'contentservice_pass';

-- Grant privileges
GRANT ALL PRIVILEGES ON DATABASE "UserServiceDb" TO userservice_user;
GRANT ALL PRIVILEGES ON DATABASE "ContentServiceDb" TO contentservice_user;
