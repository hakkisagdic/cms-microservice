#!/bin/bash

# CMS Microservice - Start All Services in Separate Terminals

# Function to check if port is available
check_port() {
    local port=$1
    if lsof -ti :$port > /dev/null 2>&1; then
        lsof -ti :$port | xargs kill -9 2>/dev/null || true
        sleep 2
    fi
}

# Stop any existing services
pkill -f "IdentityService" 2>/dev/null || true
pkill -f "UserService" 2>/dev/null || true
pkill -f "ContentService" 2>/dev/null || true
pkill -f "ApiGateway" 2>/dev/null || true
sleep 3

# Check and clean ports
check_port 5000
check_port 5001
check_port 5002
check_port 5128

# Get the project root directory
PROJECT_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"

osascript -e "tell application \"Terminal\" to do script \"cd '$PROJECT_ROOT/src/IdentityService/IdentityService.API' && dotnet run --urls='http://localhost:5128'\""

osascript -e "tell application \"Terminal\" to do script \"cd '$PROJECT_ROOT/src/UserService/UserService.API' && dotnet run --urls='http://localhost:5001'\""

osascript -e "tell application \"Terminal\" to do script \"cd '$PROJECT_ROOT/src/ContentService/ContentService.API' && dotnet run --urls='http://localhost:5002'\""

osascript -e "tell application \"Terminal\" to do script \"cd '$PROJECT_ROOT/src/ApiGateway' && dotnet run --urls='http://localhost:5000'\"" 

sleep 15

echo ""
echo "🎉 All service terminals opened!"
echo ""
echo "📋 Service URLs:"
echo "   • API Gateway:     http://localhost:5000"
echo "   • Identity Service: http://localhost:5128"
echo "   • User Service:    http://localhost:5001"
echo "   • Content Service: http://localhost:5002"
echo ""
echo "📊 Swagger Documentation:"
echo "   • Identity:  http://localhost:5128/swagger"
echo "   • Users:     http://localhost:5001/swagger"
echo "   • Contents:  http://localhost:5002/swagger"
echo "   • Gateway:   http://localhost:5000/swagger"
echo ""
echo "� Service Health Check:"
echo "   • Identity:  curl http://localhost:5128/api/Test/public"
echo "   • Gateway:   curl http://localhost:5000/status"
echo ""
echo "🛑 To stop all services: ./scripts/stop.sh"
echo ""
echo "💡 Each service is now running in its own terminal window."
echo "   You can monitor logs and output directly in each terminal."
