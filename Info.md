# MailHandlingService API endpoints

# // Mail handling
GET    /api/emails                          - Lista e-postmeddelanden (med filtrering)
GET    /api/emails/{id}                     - Hämta e-postdetaljer
POST   /api/emails                          - Skicka/skapa nytt e-postmeddelande
PUT    /api/emails/{id}                     - Uppdatera e-post (markera som läst/oläst)
DELETE /api/emails/{id}                     - Ta bort/flytta till papperskorgen
GET    /api/folders/{folder}/emails         - Hämta e-post efter mapp (inkorg/skickat/papperskorg)
POST   /api/emails/{id}/reply               - Svara på e-post
POST   /api/emails/{id}/forward             - Vidarebefordra e-post

# // Star handling
POST   /api/emails/{id}/star                - Stjärnmärk en e-post
DELETE /api/emails/{id}/star                - Ta bort stjärnmärkning från e-post
GET    /api/emails/starred                  - Hämta alla stjärnmärkta e-postmeddelanden

# // Label handling
GET    /api/labels                          - Lista alla labels
POST   /api/labels                          - Skapa ny label
PUT    /api/labels/{id}                     - Uppdatera label (namn/färg)
DELETE /api/labels/{id}                     - Ta bort label
POST   /api/emails/{id}/labels/{labelId}    - Lägg till label på e-post
DELETE /api/emails/{id}/labels/{labelId}    - Ta bort label från e-post


### // Extra
# // Draft handling
POST   /api/drafts                          - Spara utkast
PUT    /api/drafts/{id}                     - Uppdatera utkast
GET    /api/drafts                          - Lista alla utkast
DELETE /api/drafts/{id}                     - Ta bort utkast
POST   /api/drafts/{id}/send                - Skicka utkast

# // Search handling
GET /api/emails/search                      - Sök e-postmeddelanden (med query parameters)

# // Attachment handling
GET    /api/emails/{id}/attachments         - Lista bilagor för en e-post
GET    /api/attachments/{id}                - Hämta en specifik bilaga
POST   /api/emails/{id}/attachments         - Ladda upp bilaga till e-post
DELETE /api/attachments/{id}                - Ta bort bilaga

# // Batch handling
POST /api/emails/batch/read                 - Markera flera e-postmeddelanden som lästa
POST /api/emails/batch/unread               - Markera flera som olästa
POST /api/emails/batch/delete               - Ta bort flera e-postmeddelanden
POST /api/emails/batch/move                 - Flytta flera e-postmeddelanden till annan mapp

# // Azure
"SqlServer": "Server=tcp:ventixe-project-sqlserver.database.windows.net,1433;Initial Catalog=database;Persist Security Info=False;User ID=SqlAdmin;Password=Kolbulle1!;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"

# // local
"SqlServer": "Server=localhost,1433;Database=ventixe_database;User Id=sa;Password=Kolbulle1!;MultipleActiveResultSets=True;Encrypt=False;TrustServerCertificate=True;Connection Timeout=30;"
