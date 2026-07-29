#!/bin/sh
set -e

# Managed hosts (Railway, Fly.io, Cloud Run, Heroku, ...) tell the container which port to listen
# on through $PORT. Honour it unless the caller pinned the URLs explicitly; with neither set the
# aspnet base image keeps its own default (8080), which is what Docker Compose relies on.
if [ -z "$ASPNETCORE_URLS" ] && [ -n "$PORT" ]; then
    export ASPNETCORE_URLS="http://+:${PORT}"
fi

exec dotnet Lms.Api.dll "$@"
