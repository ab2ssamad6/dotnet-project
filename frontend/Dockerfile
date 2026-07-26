# ---------- Build stage ----------
FROM node:22-alpine AS build
WORKDIR /app

# Install dependencies (leverage layer caching).
COPY package.json package-lock.json* ./
RUN npm ci || npm install

# Build the production bundle.
COPY . .
# VITE_API_BASE_URL is left empty by default so the app calls same-origin "/api",
# which Nginx reverse-proxies to the backend (see nginx.conf).
ARG VITE_API_BASE_URL=""
ENV VITE_API_BASE_URL=$VITE_API_BASE_URL
RUN npm run build

# ---------- Runtime stage ----------
FROM nginx:1.27-alpine AS runtime

# SPA + reverse-proxy config. API_UPSTREAM is templated at container start.
COPY nginx.conf /etc/nginx/templates/default.conf.template
COPY --from=build /app/dist /usr/share/nginx/html

# Backend API location the /api proxy forwards to (override at run time).
ENV API_UPSTREAM=http://api:8080

EXPOSE 80
# nginx image auto-processes /etc/nginx/templates/*.template with envsubst on start.
CMD ["nginx", "-g", "daemon off;"]
