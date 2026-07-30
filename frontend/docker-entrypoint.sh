#!/bin/sh
set -e

export PORT="${PORT:-80}"

export API_UPSTREAM="${API_UPSTREAM%/}"

if [ -z "$NGINX_RESOLVER" ]; then
    resolvers=""
    for ns in $(awk '/^nameserver/ { print $2 }' /etc/resolv.conf 2>/dev/null); do
        case "$ns" in
            *:*) resolvers="$resolvers [$ns]" ;;
            *)   resolvers="$resolvers $ns" ;;
        esac
    done
    export NGINX_RESOLVER="${resolvers:-1.1.1.1 8.8.8.8}"
fi

exec /docker-entrypoint.sh "$@"
