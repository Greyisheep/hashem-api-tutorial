#!/bin/bash

echo "Starting Squad.API with Docker..."
echo

echo "Building and starting containers..."
docker-compose up --build -d

echo
echo "Waiting for services to be ready..."
sleep 10

echo
echo "Checking service status..."
docker-compose ps

echo
echo "Squad.API should be available at:"
echo "- HTTP:  http://localhost:5000"
echo "- HTTPS: https://localhost:5001"
echo "- Swagger: http://localhost:5000/swagger"
echo
echo "Database is available at:"
echo "- Host: localhost"
echo "- Port: 5432"
echo "- Database: squad_db"
echo "- Username: squad_user"
echo "- Password: squad_password"
echo
echo "To view logs: docker-compose logs -f squad-api"
echo "To stop: docker-compose down"
echo 