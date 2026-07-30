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

## 4. Deploy to Railway (GHCR image)

Same shape as the backend: GitHub Actions builds the image, pushes it to the **GitHub Container
Registry**, and Railway only *runs* it — nothing is compiled on Railway. Deploy the backend first
(`../backend/README.md` → "Deploy to Railway"), because you need its hostname here.

The image is platform-ready: Nginx binds to `$PORT`, `/health` answers locally, and the SPA calls
same-origin `/api` which Nginx reverse-proxies to `$API_UPSTREAM`. **The backend host is a run-time
variable, not baked into the build** — so the same image works against any API, and no CORS
configuration is needed at all.

### What you need first

- Push access to this repository (the image publishes to `ghcr.io/ab2ssamad6/lms-frontend`).
- A [Railway](https://railway.com) account, with the **backend API service already deployed**.

### Step 1 — Publish the image to GHCR

**Option A — GitHub Actions (recommended).** `.github/workflows/frontend-image.yml` builds
`frontend/` and pushes `ghcr.io/<owner>/lms-frontend:latest` plus a `sha-<short>` tag on every push
to `master` that touches `frontend/`. It uses the built-in `GITHUB_TOKEN`, so there is no secret to
configure. To publish without pushing code: **Actions → Publish frontend image → Run workflow**.

**Option B — from your machine.** Needs a [classic PAT](https://github.com/settings/tokens) with the
`write:packages` scope:

```bash
cd frontend
echo "$GHCR_PAT" | docker login ghcr.io -u <your-github-username> --password-stdin
docker build --platform linux/amd64 -t ghcr.io/ab2ssamad6/lms-frontend:v1 .
docker push ghcr.io/ab2ssamad6/lms-frontend:v1
```

`--platform linux/amd64` matters: Railway runs x86-64, and an image built on Apple Silicon without
it crashes with `exec format error`.

**Then set the package's visibility.** A freshly published GHCR package is **private**. Either make
it public — repo → **Packages** → `lms-frontend` → **Package settings** → *Change visibility →
Public* — or keep it private and hand Railway a PAT with `read:packages` in step 2.

### Step 2 — Add the frontend service from the image

In the **same Railway project as the API** (this is what puts them on one private network):
**New → Docker Image** → paste:

```
ghcr.io/ab2ssamad6/lms-frontend:latest
```

If the package is private, Railway prompts for registry credentials — username = your GitHub
username, password = a PAT with `read:packages`.

### Step 3 — Set the frontend service variables

Frontend service → **Variables** → **Raw editor**:

```env
PORT=8080
API_UPSTREAM=http://${{lms-api.RAILWAY_PRIVATE_DOMAIN}}:8080
```

That is the whole configuration. Notes:

- **Replace `lms-api` with your API service's actual name** as it appears in the Railway project.
  The reference resolves to `<service>.railway.internal`; if you'd rather not use a reference, paste
  the literal hostname. The `:8080` must match the API's `PORT`.
- **Private networking is IPv6-only and its DNS is not up the instant the container boots.** That is
  why `nginx.conf` holds the upstream in a `set $api_upstream` variable and `docker-entrypoint.sh`
  derives a `resolver` from `/etc/resolv.conf`: nginx then resolves per request instead of once at
  config-load, so it cannot crash-loop waiting for DNS. Don't inline the hostname into `proxy_pass`.
- **A public upstream also works** if you prefer it — `API_UPSTREAM=https://<api>.up.railway.app`
  (no trailing slash, no port). Traffic then leaves Railway's network and bills as egress; the
  `Host $proxy_host` / `proxy_ssl_server_name` settings in `nginx.conf` are what make it route
  correctly. Private is the better default.
- **Do not set `VITE_API_BASE_URL`.** It is a *build-time* Vite variable — setting it on Railway does
  nothing, and baking it in would bypass the proxy and require CORS.

### Step 4 — Expose it and add the health check

1. Frontend service → **Settings → Networking → Generate Domain**, target port **8080** (matching
   `PORT`). You get `https://<something>.up.railway.app`.
2. **Settings → Deploy → Health Check Path**: `/health`. The default timeout is fine — this endpoint
   is served by Nginx itself and does not wait on the backend.

### Step 5 — Deploy and verify

```bash
FE=https://<your-frontend>.up.railway.app

curl -s $FE/health                       # -> {"status":"healthy"}  (Nginx itself)
curl -si $FE/dashboard | head -1         # -> 200: SPA history fallback works
curl -s $FE/api/trainings | head -c 200  # -> JSON from the backend through the proxy
```

Then open the domain and log in with a [demo account](#demo-accounts-seeded-by-the-backend). If the
page loads but every API call fails, the proxy is the problem, not the SPA — check `API_UPSTREAM`.

### Step 6 — CORS (only if you skip the proxy)

With this setup the browser only ever talks to the frontend's own origin, so the backend's
`Cors__AllowedOrigins` is irrelevant. It matters **only** if you build with `VITE_API_BASE_URL`
pointing at the API's public domain — then add the frontend origin to the backend's
`Cors__AllowedOrigins` (comma-separated, **no trailing slash**, never `*`, which is compared as a
literal origin and matches nothing).

### Step 7 — Ship a new version

Railway does not poll the registry. After a new image is published:

- **Same tag (`latest`)** — frontend service → **Deployments** → **Redeploy** to pull the new digest.
- **New tag (`sha-abc1234`)** — update **Settings → Source → Image**, which deploys on its own. This
  is the safer habit: the running version stays identifiable.

### Troubleshooting

| Symptom | Cause |
|---|---|
| Deploy healthy but the domain 502s | Domain's target port ≠ `PORT`. Set both to 8080. |
| Container restarts with `host not found in upstream` | `proxy_pass` was given a literal hostname instead of the `$api_upstream` variable — nginx resolves literals at startup and exits. |
| `exec format error` in logs | Image built on ARM without `--platform linux/amd64`. |
| `denied` / `manifest unknown` when Railway pulls | GHCR package still private with no `read:packages` credentials attached. |
| SPA loads, all `/api` calls 502 | `API_UPSTREAM` wrong service name, wrong port, or the API is in a different Railway project (no shared private network). |
| `/api` calls 404 against a *public* upstream | `Host` forwarded as the frontend's own host. Fixed by `Host $proxy_host` — don't revert it. |
| Deep links 404 on refresh | SPA fallback missing — only happens if `nginx.conf` was replaced. |
| Browser CORS error | You baked `VITE_API_BASE_URL` and bypassed the proxy — see step 6. |

## 5. Other production targets

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
