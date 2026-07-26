# LMS Frontend

A production-ready **React 18 + TypeScript + Vite** frontend for the LMS (.NET 8) API. It ships a
complete Learning Management System UI: authentication with JWT + refresh tokens, role-based routing
(Administrator / Trainer / Student), a management back office (categories, trainings, modules,
lessons/exercises/quizzes/exams, trainers), a student learning portal with progress tracking and
graded assessments, and a reusable AI Trainer avatar panel wired to the backend's Anam.ai integration.

## Tech stack

| Concern            | Choice                                             |
| ------------------ | -------------------------------------------------- |
| Framework          | React 18 + TypeScript (strict)                     |
| Build tool         | Vite 6                                             |
| Routing            | React Router 6 (lazy, role-guarded routes)         |
| Styling            | Tailwind CSS 3                                      |
| Forms + validation | React Hook Form + Zod                              |
| HTTP               | Axios (JWT injection + refresh interceptor + retry)|
| Animation          | Framer Motion                                      |
| Notifications      | React Hot Toast + in-app notification center       |
| State              | React Context + hooks (no external state library)  |
| Testing            | Vitest + Testing Library                           |
| Delivery           | Docker (multi-stage) + Nginx (SPA + API proxy)     |

## Project structure

```
src/
  api/          Axios client, token store, error normalization
  app/          (reserved for app-wide providers)
  assets/
  components/
    ui/         Reusable design-system primitives (Button, Modal, DataTable, …)
    common/     Composed building blocks (PageHeader, StatCard, StatusBadge, …)
  constants/    Enum option maps, navigation config, runtime config
  features/
    auth/       AuthContext, Zod schemas
    ai/         AI Trainer panel + Anam SDK loader
    notifications/
  hooks/        useAuth, useAsync, usePagedList, useDisclosure, useLookups, …
  layouts/      AppLayout (sidebar/topbar), AuthLayout
  pages/        One folder per module (auth, dashboard, categories, trainings, …)
  routes/       ProtectedRoute, RoleRoute
  services/     Typed API service objects (one per resource)
  types/        DTO/enum types mirroring the backend
  utils/        cn, formatters, toast helper, zodResolver
```

## Prerequisites

- Node.js 20+ (22 recommended) and npm 10+
- A running instance of the LMS backend API (see `../backend`)

## 1. Local development

```bash
cd frontend
cp .env.example .env         # optional — defaults work out of the box
npm install
npm run dev                  # http://localhost:5173
```

By default the dev server proxies `/api` → `http://localhost:8080` (configurable via
`VITE_PROXY_TARGET`), so the browser stays same-origin and you don't need CORS. The backend already
allow-lists `http://localhost:5173`.

### Demo accounts (seeded by the backend)

| Role          | Email               | Password       |
| ------------- | ------------------- | -------------- |
| Administrator | `admin@lms.local`   | `Admin#12345`  |
| Trainer       | `trainer@lms.local` | `Trainer#12345`|
| Student       | `student@lms.local` | `Student#12345`|

The login page has one-click buttons to fill these in.

### Available scripts

```bash
npm run dev        # start the Vite dev server
npm run build      # type-check (tsc -b) then produce an optimized build in dist/
npm run preview    # serve the production build locally on :3000
npm run lint       # type-check only (tsc --noEmit)
npm test           # run the Vitest suite once
npm run test:watch # watch mode
```

## 2. Configuration

All configuration is via Vite env vars (see `.env.example`):

| Variable             | Default                 | Purpose                                                                 |
| -------------------- | ----------------------- | ----------------------------------------------------------------------- |
| `VITE_API_BASE_URL`  | *(empty)*               | API base. **Leave empty** to call same-origin `/api` (recommended).     |
| `VITE_PROXY_TARGET`  | `http://localhost:8080` | Dev-only: where Vite's `/api` proxy forwards.                           |

In production the app calls same-origin `/api`, which the bundled Nginx reverse-proxies to the
backend — so no CORS configuration or rebuild is needed to point at a different backend host.

## 3. Build & run with Docker

The multi-stage `Dockerfile` builds the SPA and serves it with Nginx, reverse-proxying `/api`
to the backend (host set by the `API_UPSTREAM` env var at run time).

```bash
cd frontend

# Build the image
docker build -t lms-frontend:latest .

# Run it, pointing /api at your backend
docker run -d --name lms-frontend -p 3000:80 \
  -e API_UPSTREAM=http://host.docker.internal:8080 \
  --add-host host.docker.internal:host-gateway \
  lms-frontend:latest

# open http://localhost:3000
```

### Docker Compose

A `docker-compose.yml` is included:

```bash
# Backend reachable on the host at :8080
API_UPSTREAM=http://host.docker.internal:8080 docker compose up --build -d
open http://localhost:3000
```

To run **alongside the backend compose** on the same Docker network, add this service to
`../backend/docker-compose.yml` (or attach it to `backend_default`) and set
`API_UPSTREAM=http://api:8080`.

## 4. Production deployment

The `dist/` output is a static SPA and can be deployed anywhere. Two common paths:

### A. Container (recommended — self-contained proxy)

The image already contains Nginx configured for SPA history fallback, gzip, asset caching,
security headers, and the `/api` reverse proxy. Deploy it to any container platform (ECS, Cloud Run,
Kubernetes, Fly.io, etc.) and set `API_UPSTREAM` to your backend's internal URL:

```bash
docker run -d -p 80:80 -e API_UPSTREAM=https://api.internal.example.com lms-frontend:latest
```

Behind Kubernetes, set `API_UPSTREAM` to the backend Service DNS (e.g. `http://lms-api.default.svc:8080`).

### B. Static hosting (CDN / S3 / Netlify / Vercel)

```bash
VITE_API_BASE_URL=https://api.example.com npm run build
# upload dist/ to your static host
```

When hosting statically you must:

1. Set `VITE_API_BASE_URL` at build time to the backend's public URL, **and**
2. Add the frontend origin to the backend's `Cors:AllowedOrigins`, **and**
3. Configure your host to rewrite all unknown routes to `index.html` (SPA history fallback).

> The container approach avoids CORS entirely by keeping the browser same-origin, which is why it's
> recommended.

## Feature overview

- **Authentication** — Login, Register (Student/Trainer), Forgot/Reset password, Email verification,
  Change password. JWT access token + rotating refresh token, transparently refreshed by an Axios
  interceptor; a failed refresh forces a clean logout.
- **Role-based UI** — The sidebar, routes and actions adapt to the user's roles. Route guards
  (`ProtectedRoute`, `RoleRoute`) enforce access and mirror the backend's authorization policies.
- **Dashboard** — Role-tailored: admins see platform KPIs, category breakdowns and recent activity;
  trainers see content stats; students see progress and "continue learning".
- **Content management** — Full CRUD for Categories, Trainings (grid with search/filter/publish),
  Modules (nested, reorderable, AI-avatar toggle) and Activities (Lessons, Exercises, Quizzes and
  Exams with a multi-question / multi-answer builder).
- **Trainers** — CRUD with avatar, biography, expertise and contact details.
- **Student portal** — Browse the published catalog, enroll, a course player that tracks module
  completion, and interactive quizzes/exams with a timer, scoring and results.
- **AI Trainer** — A reusable panel that fetches a streaming session token from the API and connects
  the Anam.ai avatar to a video element, with start/stop, ask-a-question, transcript, mic/speaker
  controls, connection status and error handling. Embedded automatically in AI-enabled modules.
- **Reusable UI** — DataTable (sort/search/paginate/bulk select), Modal, imperative ConfirmDialog,
  Skeletons, EmptyState, Toaster, and a full form kit — all responsive and accessible.

## Notes on API alignment

- The backend serializes enums as **numbers**; the frontend types and `constants/enums.ts` mirror
  those numeric values exactly.
- List endpoints return a `PagedResult<T>` (`items`, `page`, `totalCount`, …) consumed by
  `usePagedList`.
- Errors are RFC7807 Problem Details, normalized into a friendly `ApiError` by `api/errors.ts`.
- There is no student-directory endpoint, so the admin "Students" view summarizes engagement from the
  dashboard aggregates.
```
