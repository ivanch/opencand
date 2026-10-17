#!/usr/bin/env bash
# ─── OpenCand Run Script (Bash) ─────────────────────────────────────────────
set -eo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
cd "$SCRIPT_DIR"

MODE="API"
DOCKER=false
NO_BUILD=false
NO_UI=false
DB_HOST="localhost"
DB_PORT=5432

# Colors
CYAN='\033[0;36m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
RED='\033[0;31m'
NC='\033[0m'

print_usage() {
    cat << EOF
Usage: ./run.sh [OPTIONS]

Options:
  --mode <API|ETL>    Execution mode: API (default) or ETL
  --etl               Convenience flag to run in ETL mode
  --api               Convenience flag to run in API mode (default)
  --docker            Run using Docker Compose
  --no-build          Skip build step before running
  --no-ui             In native API mode, skip starting the UI dev server
  -h, --help          Show this help message
EOF
}

while [[ $# -gt 0 ]]; do
    case "$1" in
        --mode)
            MODE="$2"
            shift 2
            ;;
        --etl)
            MODE="ETL"
            shift
            ;;
        --api)
            MODE="API"
            shift
            ;;
        --docker)
            DOCKER=true
            shift
            ;;
        --no-build)
            NO_BUILD=true
            shift
            ;;
        --no-ui)
            NO_UI=true
            shift
            ;;
        -h|--help)
            print_usage
            exit 0
            ;;
        *)
            echo -e "${RED}[ERROR] Unknown option: $1${NC}"
            print_usage
            exit 1
            ;;
    esac
done

echo -e "${CYAN}=================================================="
echo -e "             OpenCand Local Runner               "
echo -e "  Execution Mode: ${YELLOW}${MODE}${CYAN}"
echo -e "  Runtime:        $([ "$DOCKER" = true ] && echo "Docker Compose" || echo "Native (dotnet / npm)")"
echo -e "==================================================${NC}"

# Build step
if [ "$NO_BUILD" = false ]; then
    echo -e "${CYAN}===> Building required projects...${NC}"
    BUILD_ARGS=()
    if [ "$MODE" = "ETL" ]; then
        BUILD_ARGS+=(--target ETL)
    else
        BUILD_ARGS+=(--target All)
    fi
    if [ "$DOCKER" = true ]; then
        BUILD_ARGS+=(--docker)
    fi
    ./build.sh "${BUILD_ARGS[@]}"
fi

get_compose_cmd() {
    if command -v docker &>/dev/null && docker compose version &>/dev/null; then
        echo "docker compose"
    elif command -v docker-compose &>/dev/null; then
        echo "docker-compose"
    else
        echo ""
    fi
}

if [ "$DOCKER" = true ]; then
    COMPOSE_CMD=$(get_compose_cmd)
    if [ -z "$COMPOSE_CMD" ]; then
        echo -e "${RED}[ERROR] docker-compose is required for --docker mode.${NC}"
        exit 1
    fi

    if [ "$MODE" = "API" ]; then
        echo -e "${GREEN}===> Starting OpenCand stack (Postgres + API + UI)...${NC}"
        $COMPOSE_CMD up
    else
        echo -e "${GREEN}===> Ensuring Postgres is up for ETL...${NC}"
        $COMPOSE_CMD up -d postgres
        echo -e "${GREEN}===> Running OpenCand ETL container...${NC}"
        $COMPOSE_CMD --profile etl run --rm etl
    fi
    exit 0
fi

# ─── Native Execution ────────────────────────────────────────────────────────
# Check DB connectivity
if nc -z "$DB_HOST" "$DB_PORT" &>/dev/null; then
    echo -e "${GREEN}[OK] PostgreSQL detected on $DB_HOST:$DB_PORT${NC}"
else
    echo -e "${YELLOW}[WARN] PostgreSQL is not responding on $DB_HOST:$DB_PORT.${NC}"
    echo -e "${YELLOW}You can start a temporary local database via Docker:${NC}"
    echo -e "${CYAN}  docker run -d --name opencand-db -p 5432:5432 -e POSTGRES_USER=root -e POSTGRES_PASSWORD=root -e POSTGRES_DB=opencand postgres:16-alpine${NC}\n"
fi

if [ "$MODE" = "API" ]; then
    mkdir -p src/OpenCand.API/fotos_cand

    if [ "$NO_UI" = false ]; then
        echo -e "${CYAN}===> Launching UI Vite dev server in background...${NC}"
        (cd src/OpenCand.UI && npm run dev) &
        UI_PID=$!
        echo -e "${GREEN}Frontend running at: http://localhost:5173${NC}"
        trap "kill $UI_PID 2>/dev/null || true" EXIT
    fi

    echo -e "${CYAN}===> Starting OpenCand API...${NC}"
    echo -e "${GREEN}API Base URL: http://localhost:5299${NC}"
    echo -e "${GREEN}Swagger UI:   http://localhost:5299/swagger${NC}"
    dotnet run --project src/OpenCand.API/OpenCand.API.csproj --launch-profile "http"
else
    echo -e "${CYAN}===> Starting OpenCand ETL (Data Importer)...${NC}"
    dotnet run --project src/OpenCand.ETL/OpenCand.ETL.csproj
fi
