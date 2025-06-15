#!/bin/bash

BASE_URL="http://localhost:5160/api"
JWT_TOKEN="Jx9j6vJ6FhJq4m1ZQzQp3A8l0wYk2rP7uTzB1sVn4xE="

login_and_get_token() {
    echo "Attempting to log in..."
    LOGIN_RESPONSE=$(curl -s -X POST "${BASE_URL}/Auth/login" \
        -H "Content-Type: application/json" \
        -d '{
              "username": "admin",
              "password": "admin"
            }')

    echo "Login response: $LOGIN_RESPONSE"
    TOKEN=$(echo "$LOGIN_RESPONSE" | jq -r .token)

    if [ "$TOKEN" != "null" ] && [ ! -z "$TOKEN" ]; then
        JWT_TOKEN=$TOKEN
        echo "Login successful. Token obtained: $JWT_TOKEN"
    else
        echo "Login failed or token not found in response."
        exit 1
    fi
}

echo "-----------------------------------"
echo "TMS API Test Script"
echo "-----------------------------------"
echo "Base URL: $BASE_URL"
echo ""

echo "[AUTH] Logging in to get JWT token..."

if [ -z "$JWT_TOKEN" ]; then
    echo "JWT_TOKEN is not set. Please log in manually or set the token."
    echo "Example manual login command:"
    echo "curl -s -X POST \"${BASE_URL}/Auth/login\" -H \"Content-Type: application/json\" -d '{\"username\": \"admin\", \"password\": \"password\"}' | jq"
    echo "Then copy the token and set JWT_TOKEN variable in this script."
fi
echo ""


echo "[PROJECTS] Getting all projects (requires Admin token for some operations)..."
curl -s -X GET "${BASE_URL}/Project" -H "Authorization: Bearer $JWT_TOKEN" | jq
echo ""

echo "[PROJECTS] Getting a specific project (e.g., ID 1)..."
curl -s -X GET "${BASE_URL}/Project/1" -H "Authorization: Bearer $JWT_TOKEN" | jq 
echo ""

echo "[PROJECTS] Creating a new project (requires Admin token)..."
curl -s -X POST "${BASE_URL}/Project" \
    -H "Content-Type: application/json" \
    -H "Authorization: Bearer $JWT_TOKEN" \
    -d '{
          "name": "New Project from cURL",
          "description": "This is a test project created via cURL."
        }' | jq
echo ""

echo "[PROJECTS] Updating an existing project (e.g., ID 1, requires Admin token)..."
curl -s -X PUT "${BASE_URL}/Project/1" \
    -H "Content-Type: application/json" \
    -H "Authorization: Bearer $JWT_TOKEN" \
    -d '{
          "id": 1, 
          "name": "Updated Project Name via cURL",
          "description": "Updated description."
        }' | jq
echo ""

echo "[PROJECTS] Deleting a project (e.g., ID 1, requires Admin token)..."
echo ""


echo "[TICKETS] Getting all tickets..."
curl -s -X GET "${BASE_URL}/Ticket" -H "Authorization: Bearer $JWT_TOKEN" | jq
echo ""

echo "[TICKETS] Getting a specific ticket (e.g., ID 1)..."
curl -s -X GET "${BASE_URL}/Ticket/1" -H "Authorization: Bearer $JWT_TOKEN" | jq
echo ""

echo "[TICKETS] Creating a new ticket..."
curl -s -X POST "${BASE_URL}/Ticket" \
    -H "Content-Type: application/json" \
    -H "Authorization: Bearer $JWT_TOKEN" \
    -d '{
          "title": "New Ticket from cURL",
          "description": "Issue reported via cURL script.",
          "status": 0,
          "createdById": 1, 
          "assignedToId": 2, 
          "projectId": 1    
        }' | jq
echo ""

echo "[TICKETS] Updating an existing ticket (e.g., ID 1)..."
curl -s -X PUT "${BASE_URL}/Ticket/1" \
    -H "Content-Type: application/json" \
    -H "Authorization: Bearer $JWT_TOKEN" \
    -d '{
          "id": 1,
          "title": "Updated Ticket Title via cURL",
          "description": "Updated ticket description.",
          "status": 1,
          "assignedToId": 2,
          "projectId": 1
        }' | jq
echo ""

echo "[TICKETS] Deleting a ticket (e.g., ID 1)..."
echo ""


echo "[USERS] Getting all users (requires Admin token for some operations)..."
curl -s -X GET "${BASE_URL}/User" -H "Authorization: Bearer $JWT_TOKEN" | jq
echo ""

echo "[USERS] Getting a specific user (e.g., ID 1)..."
curl -s -X GET "${BASE_URL}/User/1" -H "Authorization: Bearer $JWT_TOKEN" | jq
echo ""

echo "[USERS] Creating a new user (requires Admin token)..."
curl -s -X POST "${BASE_URL}/User" \
    -H "Content-Type: application/json" \
    -H "Authorization: Bearer $JWT_TOKEN" \
    -d '{
          "username": "curluser",
          "email": "curluser@example.com",
          "firstName": "cURL",
          "lastName": "User",
          "role": "Developer"
        }' | jq
echo ""

echo "[USERS] Updating an existing user (e.g., ID 1, requires Admin token)..."
curl -s -X PUT "${BASE_URL}/User/1" \
    -H "Content-Type: application/json" \
    -H "Authorization: Bearer $JWT_TOKEN" \
    -d '{
          "id": 1,
          "username": "updatedcurluser",
          "email": "updatedcurluser@example.com",
          "firstName": "UpdatedcURL",
          "lastName": "UpdatedUser",
          "role": "Developer"
        }' | jq
echo ""

echo "[USERS] Deleting a user (e.g., ID 1, requires Admin token)..."
echo ""

echo "-----------------------------------"
echo "API Test Script Finished"
echo "-----------------------------------"