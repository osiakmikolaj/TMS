#!/bin/bash
# Test TMS API Endpoints

# Variables
API_URL="https://localhost:7180/api"
TOKEN=""

# Login to get JWT token
echo "Logging in..."
TOKEN=$(curl -s -X POST $API_URL/auth/login \
  -H "Content-Type: application/json" \
  -d '{"username":"admin","password":"password"}' | jq -r .token)

echo "Token: $TOKEN"

# Get all projects
echo -e "\nGetting all projects..."
curl -s -X GET $API_URL/projects \
  -H "Authorization: Bearer $TOKEN" | jq

# Create a new project
echo -e "\nCreating a new project..."
PROJECT_ID=$(curl -s -X POST $API_URL/projects \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $TOKEN" \
  -d '{"name":"Mobile App","description":"Mobile application for customers"}' | jq)

echo "Created project with ID: $PROJECT_ID"

# Get tickets for a project
echo -e "\nGetting tickets for project..."
curl -s -X GET "$API_URL/tickets?projectId=$PROJECT_ID" \
  -H "Authorization: Bearer $TOKEN" | jq

# Create a new ticket
echo -e "\nCreating a new ticket..."
TICKET_ID=$(curl -s -X POST $API_URL/tickets \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $TOKEN" \
  -d '{"title":"Implement login screen","description":"Create login screen for mobile app","projectId":'"$PROJECT_ID"',"createdById":1}' | jq)

echo "Created ticket with ID: $TICKET_ID"

# Get ticket details
echo -e "\nGetting ticket details..."
curl -s -X GET "$API_URL/tickets/$TICKET_ID" \
  -H "Authorization: Bearer $TOKEN" | jq

# Update ticket
echo -e "\nUpdating ticket..."
curl -s -X PUT "$API_URL/tickets/$TICKET_ID" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $TOKEN" \
  -d '{"title":"Implement login & registration screen","status":1,"assignedToId":2}' | jq

# Add comment to ticket
echo -e "\nAdding comment to ticket..."
curl -s -X POST "$API_URL/tickets/$TICKET_ID/comments" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $TOKEN" \
  -d '{"content":"This needs to be completed by next week","userId":1}' | jq

# Get all comments for a ticket
echo -e "\nGetting all comments for a ticket..."
curl -s -X GET "$API_URL/tickets/$TICKET_ID/comments" \
  -H "Authorization: Bearer $TOKEN" | jq

echo -e "\nAll tests completed!"