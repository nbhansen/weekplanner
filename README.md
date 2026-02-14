# Weekplanner

Schedule management app for the GIRAF ecosystem — an app environment for children with autism.

## Architecture

```
Expo mobile app (frontend/)
         │
         ▼
  Weekplanner API (backend/)    ←── Activity-only, .NET 8
         │
         ▼
  GIRAF Core API (external)     ←── Users, orgs, citizens, pictograms
       PostgreSQL
```

The backend stores only **Activities**. All shared entities (users, organizations, citizens, grades, pictograms) are managed by [giraf-core](https://github.com/aau-giraf/giraf-core). JWTs issued by Core are validated locally using a shared secret.

## Quick Start

**Prerequisites:** Docker, Node.js 20+, [giraf-core](https://github.com/aau-giraf/giraf-core) running on port 8000.

```bash
# 1. Start backend + PostgreSQL
docker compose up

# 2. Start frontend (in a separate terminal)
cd frontend
cp ../.env.example .env          # Edit EXPO_PUBLIC_* vars
npm install
npx expo start
```

The backend API will be available at `http://localhost:5171`.

## Project Structure

| Directory | Description |
|-----------|-------------|
| `backend/` | .NET 8 Minimal API — Activity CRUD with Core JWT validation |
| `frontend/` | Expo / React Native app with TanStack Query |
| `docker-compose.yml` | Dev: backend + PostgreSQL |
| `docker-compose.prod.yml` | Prod: env-var driven configuration |

## Testing

```bash
# Backend (21 tests)
cd backend && dotnet test

# Frontend (131 tests)
cd frontend && npm test
```

## CI

- **Backend:** build + test on `backend/**` changes, Docker push on version tags
- **Frontend:** lint + test on `frontend/**` changes
