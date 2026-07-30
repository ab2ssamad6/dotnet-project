#!/bin/sh
set -e

# Managed hosts (Railway, Fly.io, Cloud Run, Heroku, ...) tell the container which port to listen
# on through $PORT. nginx.conf is a template, so exporting a default here is enough — the base
# image's envsubst pass substitutes ${PORT} before nginx starts. Compose relies on the 80 default.
export PORT="${PORT:-80}"

# proxy_pass is built as "$api_upstream$request_uri", so a trailing slash here would double up.
export API_UPSTREAM="${API_UPSTREAM%/}"

# The /api proxy_pass uses a *variable*, which makes nginx resolve the upstream per request instead
# of once at config-load time. That needs an explicit resolver (nginx does not read resolv.conf on
# its own), and it is what lets the upstream be a name that is not resolvable the instant the
# container boots — e.g. Railway's private-network `*.railway.internal`, which is IPv6-only.
if [ -z "$NGINX_RESOLVER" ]; then
    resolvers=""
    for ns in $(awk '/^nameserver/ { print $2 }' /etc/resolv.conf 2>/dev/null); do
        # nginx requires IPv6 literals in brackets.
        case "$ns" in
            *:*) resolvers="$resolvers [$ns]" ;;
            *)   resolvers="$resolvers $ns" ;;
        esac
    done
    # Fall back to a public resolver only if the container has no nameserver at all.
    export NGINX_RESOLVER="${resolvers:-1.1.1.1 8.8.8.8}"
fi

# Hand over to the official nginx entrypoint: it runs /docker-entrypoint.d/, which includes the
# envsubst pass over /etc/nginx/templates/*.template, then execs the CMD.
exec /docker-entrypoint.sh "$@"
