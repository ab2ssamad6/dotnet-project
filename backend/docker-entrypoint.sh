#!/bin/sh
set -e

if [ -z "$ASPNETCORE_URLS" ] && [ -n "$PORT" ]; then
    export ASPNETCORE_URLS="http://+:${PORT}"
fi

exec dotnet Lms.Api.dll "$@"
