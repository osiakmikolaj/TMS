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
        echo "B��d logowania!"
        exit 1
    fi
    
    echo "Zalogowano pomy�lnie."
}

# Testowanie endpoint'�w dla projekt�w
test_projects() {
    echo -e "\n--- Testowanie API projekt�w ---"
    
    # GET - pobieranie wszystkich projekt�w
    echo -e "\nPobieranie wszystkich projekt�w:"
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
    
    # DELETE - usuni�cie projektu
    echo -e "\nUsuni�cie projektu o ID $PROJECT_ID:"
    curl -s -X DELETE "$API_URL/project/$PROJECT_ID" \
        -H "Authorization: Bearer $TOKEN" | jq
}

# Testowanie endpoint'�w dla u�ytkownik�w
test_users() {
    echo -e "\n--- Testowanie API u�ytkownik�w ---"
    
    # GET - pobieranie wszystkich u�ytkownik�w
    echo -e "\nPobieranie wszystkich u�ytkownik�w:"
    curl -s -X GET "$API_URL/user" | jq
    
    # POST - dodawanie nowego u�ytkownika
    echo -e "\nDodawanie nowego u�ytkownika:"
    USER_ID=$(curl -s -X POST "$API_URL/user" \
        -H "Authorization: Bearer $TOKEN" \
        -H "Content-Type: application/json" \
        -d '{"username":"nowy.uzytkownik","email":"nowy@example.com","firstName":"Nowy","lastName":"U�ytkownik","role":"Developer"}' | jq -r '.id')
    
    # GET - pobieranie konkretnego u�ytkownika
    echo -e "\nPobieranie u�ytkownika o ID $USER_ID:"
    curl -s -X GET "$API_URL/user/$USER_ID" | jq
    
    # PUT - aktualizacja u�ytkownika
    echo -e "\nAktualizacja u�ytkownika o ID $USER_ID:"
    curl -s -X PUT "$API_URL/user/$USER_ID" \
        -H "Authorization: Bearer $TOKEN" \
        -H "Content-Type: application/json" \
        -d '{"id":'$USER_ID',"username":"zaktualizowany.uzytkownik","email":"zaktualizowany@example.com","firstName":"Zaktualizowany","lastName":"U�ytkownik","role":"Developer"}' | jq
    
    # DELETE - usuni�cie u�ytkownika
    echo -e "\nUsuni�cie u�ytkownika o ID $USER_ID:"
    curl -s -X DELETE "$API_URL/user/$USER_ID" \
        -H "Authorization: Bearer $TOKEN" | jq
}

# Testowanie endpoint'�w dla zg�osze� (tickets)
test_tickets() {
    echo -e "\n--- Testowanie API zg�osze� ---"
    
    # GET - pobieranie wszystkich zg�osze�
    echo -e "\nPobieranie wszystkich zg�osze�:"
    curl -s -X GET "$API_URL/ticket" | jq
    
    # POST - dodawanie nowego zg�oszenia
    echo -e "\nDodawanie nowego zg�oszenia:"
    TICKET_ID=$(curl -s -X POST "$API_URL/ticket" \
        -H "Authorization: Bearer $TOKEN" \
        -H "Content-Type: application/json" \
        -d '{"title":"Nowe zg�oszenie","description":"Opis nowego zg�oszenia testowego","status":"New","projectId":1,"createdById":1}' | jq -r '.id')
    
    # GET - pobieranie konkretnego zg�oszenia
    echo -e "\nPobieranie zg�oszenia o ID $TICKET_ID:"
    curl -s -X GET "$API_URL/ticket/$TICKET_ID" | jq
    
    # PUT - aktualizacja zg�oszenia
    echo -e "\nAktualizacja zg�oszenia o ID $TICKET_ID:"
    curl -s -X PUT "$API_URL/ticket/$TICKET_ID" \
        -H "Authorization: Bearer $TOKEN" \
        -H "Content-Type: application/json" \
        -d '{"id":'$TICKET_ID',"title":"Zaktualizowane zg�oszenie","description":"Zaktualizowany opis zg�oszenia","status":"InProgress","projectId":1,"createdById":1,"assignedToId":2}' | jq
    
    # DELETE - usuni�cie zg�oszenia
    echo -e "\nUsuni�cie zg�oszenia o ID $TICKET_ID:"
    curl -s -X DELETE "$API_URL/ticket/$TICKET_ID" \
        -H "Authorization: Bearer $TOKEN" | jq
}

# Wykonanie test�w
login
test_projects
test_users
test_tickets

echo -e "\nTesty API zako�czone."