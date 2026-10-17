#!/usr/bin/env bash
# ─── OpenCand Build Script (Bash) ───────────────────────────────────────────
set -eo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
cd "$SCRIPT_DIR"

TARGET="All"
DOCKER=false
CONFIG="Release"
CLEAN=false

# Colors
CYAN='\033[0;36m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
RED='\033[0;31m'
NC='\033[0m'

print_usage() {
    cat << EOF
Usage: ./build.sh [OPTIONS]

Options:
  --target <All|API|ETL|Core|UI>  Target project to build (default: All)
  --docker                        Build container images via Docker
  --config <Debug|Release>        .NET configuration (default: Release)
  --clean                         Clean previous build artifacts
  -h, --help                      Show this help message
EOF
}

while [[ $# -gt 0 ]]; do
    case "$1" in
        --target)
            TARGET="$2"
            shift 2
            ;;
        --docker)
            DOCKER=true
            shift
            ;;
        --config)
            CONFIG="$2"
            shift 2
            ;;
        --clean)
            CLEAN=true
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
echo -e "             OpenCand Build Pipeline             "
echo -e "  Target:        ${TARGET}"
echo -e "  Mode:          $([ "$DOCKER" = true ] && echo "Docker Container" || echo "Native Toolchain")"
echo -e "  Configuration: ${CONFIG}"
echo -e "==================================================${NC}"

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
    if ! command -v docker &>/dev/null; then
        echo -e "${RED}[ERROR] docker command not found in PATH.${NC}"
        exit 1
    fi

    COMPOSE_CMD=$(get_compose_cmd)
    if [ -n "$COMPOSE_CMD" ]; then
        if [ "$TARGET" = "All" ]; then
            $COMPOSE_CMD --profile etl build
        else
            case "$TARGET" in
                API|Core) SERVICE="api" ;;
                ETL)      SERVICE="etl" ;;
                UI)       SERVICE="ui" ;;
                *)        SERVICE="api" ;;
            esac
            $COMPOSE_CMD build "$SERVICE"
        fi
    else
        echo -e "${YELLOW}[WARN] docker-compose not found; building directly with docker build.${NC}"
        if [[ "$TARGET" =~ ^(All|API|Core)$ ]]; then
            docker build -t opencand-api:latest -f src/OpenCand.API/Dockerfile .
        fi
        if [[ "$TARGET" =~ ^(All|ETL)$ ]]; then
            docker build -t opencand-etl:latest -f src/OpenCand.ETL/Dockerfile .
        fi
        if [[ "$TARGET" =~ ^(All|UI)$ ]]; then
            docker build -t opencand-ui:latest -f src/OpenCand.UI/Dockerfile src/OpenCand.UI
        fi
    fi
    echo -e "${GREEN}[OK] Docker build finished successfully!${NC}"
    exit 0
fi

# ─── Native Build ────────────────────────────────────────────────────────────
if [[ "$TARGET" =~ ^(All|API|ETL|Core)$ ]]; then
    if ! command -v dotnet &>/dev/null; then
        echo -e "${RED}[ERROR] dotnet SDK not found in PATH. Please install .NET 8 SDK.${NC}"
        exit 1
    fi

    if [ "$CLEAN" = true ]; then
        echo -e "${CYAN}===> Cleaning .NET projects${NC}"
        dotnet clean OpenCand.sln -c "$CONFIG"
    fi

    if [ "$TARGET" = "All" ]; then
        echo -e "${CYAN}===> Building solution: OpenCand.sln ($CONFIG)${NC}"
        dotnet build OpenCand.sln -c "$CONFIG"
    elif [ "$TARGET" = "API" ]; then
        echo -e "${CYAN}===> Building OpenCand.API ($CONFIG)${NC}"
        dotnet build src/OpenCand.API/OpenCand.API.csproj -c "$CONFIG"
    elif [ "$TARGET" = "ETL" ]; then
        echo -e "${CYAN}===> Building OpenCand.ETL ($CONFIG)${NC}"
        dotnet build src/OpenCand.ETL/OpenCand.ETL.csproj -c "$CONFIG"
    elif [ "$TARGET" = "Core" ]; then
        echo -e "${CYAN}===> Building OpenCand.Core ($CONFIG)${NC}"
        dotnet build src/OpenCand.Core/OpenCand.Core.csproj -c "$CONFIG"
    fi
    echo -e "${GREEN}[OK] .NET build succeeded!${NC}"
fi

if [[ "$TARGET" =~ ^(All|UI)$ ]]; then
    if ! command -v npm &>/dev/null; then
        echo -e "${RED}[ERROR] npm not found in PATH. Please install Node.js.${NC}"
        exit 1
    fi

    cd src/OpenCand.UI
    if [ "$CLEAN" = true ] && [ -d "dist" ]; then
        rm -rf dist
    fi

    if [ ! -d "node_modules" ]; then
        echo -e "${CYAN}===> Installing UI dependencies${NC}"
        npm ci || npm install
    fi

    echo -e "${CYAN}===> Building UI (npm run build)${NC}"
    npm run build
    cd "$SCRIPT_DIR"
    echo -e "${GREEN}[OK] UI build succeeded!${NC}"
fi

echo -e "\n${GREEN}=================================================="
echo -e "              All Builds Succeeded!              "
echo -e "==================================================${NC}"
