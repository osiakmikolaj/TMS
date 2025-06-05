#!/bin/bash

# Zmienne konfiguracyjne
API_URL="https://localhost:7001/api"
TOKEN=""

# Funkcja do logowania
login() {
    echo "Logowanie do systemu..."
    TOKEN=$(curl -s -X POST "$API_URL/auth/login" \
        -H "Content-Type: application/json" \
        -d '{"username":"admin","password":"password"}' | jq -r '.token')
    
    if [ -z "$TOKEN" ] || [ "$TOKEN" == "null" ]; then
        echo "B³¹d logowania!"
        exit 1
    fi
    
    echo "Zalogowano pomyœlnie."
}

# Testowanie endpoint'ów dla projektów
test_projects() {
    echo -e "\n--- Testowanie API projektów ---"
    
    # GET - pobieranie wszystkich projektów
    echo -e "\nPobieranie wszystkich projektów:"
    curl -s -X GET "$API_URL/project" | jq
    
    # POST - dodawanie nowego projektu
    echo -e "\nDodawanie nowego projektu:"
    PROJECT_ID=$(curl -s -X POST "$API_URL/project" \
        -H "Authorization: Bearer $TOKEN" \
        -H "Content-Type: application/json" \
        -d '{"name":"Nowy Projekt","description":"Opis nowego projektu testowego"}' | jq -r '.id')
    
    # GET - pobieranie konkretnego projektu
    echo -e "\nPobieranie projektu o ID $PROJECT_ID:"
    curl -s -X GET "$API_URL/project/$PROJECT_ID" | jq
    
    # PUT - aktualizacja projektu
    echo -e "\nAktualizacja projektu o ID $PROJECT_ID:"
    curl -s -X PUT "$API_URL/project/$PROJECT_ID" \
        -H "Authorization: Bearer $TOKEN" \
        -H "Content-Type: application/json" \
        -d '{"id":'$PROJECT_ID',"name":"Zaktualizowany Projekt","description":"Zaktualizowany opis projektu"}' | jq
    
    # DELETE - usuniêcie projektu
    echo -e "\nUsuniêcie projektu o ID $PROJECT_ID:"
    curl -s -X DELETE "$API_URL/project/$PROJECT_ID" \
        -H "Authorization: Bearer $TOKEN" | jq
}

# Testowanie endpoint'ów dla u¿ytkowników
test_users() {
    echo -e "\n--- Testowanie API u¿ytkowników ---"
    
    # GET - pobieranie wszystkich u¿ytkowników
    echo -e "\nPobieranie wszystkich u¿ytkowników:"
    curl -s -X GET "$API_URL/user" | jq
    
    # POST - dodawanie nowego u¿ytkownika
    echo -e "\nDodawanie nowego u¿ytkownika:"
    USER_ID=$(curl -s -X POST "$API_URL/user" \
        -H "Authorization: Bearer $TOKEN" \
        -H "Content-Type: application/json" \
        -d '{"username":"nowy.uzytkownik","email":"nowy@example.com","firstName":"Nowy","lastName":"U¿ytkownik","role":"Developer"}' | jq -r '.id')
    
    # GET - pobieranie konkretnego u¿ytkownika
    echo -e "\nPobieranie u¿ytkownika o ID $USER_ID:"
    curl -s -X GET "$API_URL/user/$USER_ID" | jq
    
    # PUT - aktualizacja u¿ytkownika
    echo -e "\nAktualizacja u¿ytkownika o ID $USER_ID:"
    curl -s -X PUT "$API_URL/user/$USER_ID" \
        -H "Authorization: Bearer $TOKEN" \
        -H "Content-Type: application/json" \
        -d '{"id":'$USER_ID',"username":"zaktualizowany.uzytkownik","email":"zaktualizowany@example.com","firstName":"Zaktualizowany","lastName":"U¿ytkownik","role":"Developer"}' | jq
    
    # DELETE - usuniêcie u¿ytkownika
    echo -e "\nUsuniêcie u¿ytkownika o ID $USER_ID:"
    curl -s -X DELETE "$API_URL/user/$USER_ID" \
        -H "Authorization: Bearer $TOKEN" | jq
}

# Testowanie endpoint'ów dla zg³oszeñ (tickets)
test_tickets() {
    echo -e "\n--- Testowanie API zg³oszeñ ---"
    
    # GET - pobieranie wszystkich zg³oszeñ
    echo -e "\nPobieranie wszystkich zg³oszeñ:"
    curl -s -X GET "$API_URL/ticket" | jq
    
    # POST - dodawanie nowego zg³oszenia
    echo -e "\nDodawanie nowego zg³oszenia:"
    TICKET_ID=$(curl -s -X POST "$API_URL/ticket" \
        -H "Authorization: Bearer $TOKEN" \
        -H "Content-Type: application/json" \
        -d '{"title":"Nowe zg³oszenie","description":"Opis nowego zg³oszenia testowego","status":"New","projectId":1,"createdById":1}' | jq -r '.id')
    
    # GET - pobieranie konkretnego zg³oszenia
    echo -e "\nPobieranie zg³oszenia o ID $TICKET_ID:"
    curl -s -X GET "$API_URL/ticket/$TICKET_ID" | jq
    
    # PUT - aktualizacja zg³oszenia
    echo -e "\nAktualizacja zg³oszenia o ID $TICKET_ID:"
    curl -s -X PUT "$API_URL/ticket/$TICKET_ID" \
        -H "Authorization: Bearer $TOKEN" \
        -H "Content-Type: application/json" \
        -d '{"id":'$TICKET_ID',"title":"Zaktualizowane zg³oszenie","description":"Zaktualizowany opis zg³oszenia","status":"InProgress","projectId":1,"createdById":1,"assignedToId":2}' | jq
    
    # DELETE - usuniêcie zg³oszenia
    echo -e "\nUsuniêcie zg³oszenia o ID $TICKET_ID:"
    curl -s -X DELETE "$API_URL/ticket/$TICKET_ID" \
        -H "Authorization: Bearer $TOKEN" | jq
}

# Wykonanie testów
login
test_projects
test_users
test_tickets

echo -e "\nTesty API zakoñczone."