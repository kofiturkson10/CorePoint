# Projekt: Företagsportal (övning)

Detta är en övningsportal för att lära mig använda Claude Code. Jag är student och lär mig samtidigt – förklara kort vad du gör och varför när du föreslår eller genomför något.

## Tech stack
- Backend: ASP.NET Core Web API (senaste LTS), C#, Controllers (MVC-mönster) – returnerar endast JSON, inga Views/Razor
- Frontend: React, i mappen /client
- Datalagring: Entity Framework Core

## Struktur
- Backend och frontend i samma repo, separata mappar: /server (API) och /client (React)
- Frontend anropar backend via REST-endpoints

## Konventioner
- Använd async/await genomgående i backend
- Följ standard ASP.NET Core-mappstruktur i /server (Controllers, Models, Services, Data)
- Tydliga, beskrivande namn på klasser och metoder – engelska i koden

## Arbetssätt
- Bygg en sak i taget, i små steg
- Föreslå en plan innan större ändringar