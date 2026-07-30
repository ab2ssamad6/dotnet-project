FROM node:22-alpine AS build
WORKDIR /app

COPY package.json package-lock.json* ./
RUN npm ci || npm install

COPY . .
ARG VITE_API_BASE_URL=""
ENV VITE_API_BASE_URL=$VITE_API_BASE_URL
RUN npm run build

FROM nginx:1.27-alpine AS runtime

COPY nginx.conf /etc/nginx/templates/default.conf.template
COPY --from=build /app/dist /usr/share/nginx/html

ENV API_UPSTREAM=http://api:8080

COPY docker-entrypoint.sh /lms-entrypoint.sh
RUN chmod +x /lms-entrypoint.sh

EXPOSE 80
ENTRYPOINT ["/lms-entrypoint.sh"]
CMD ["nginx", "-g", "daemon off;"]
