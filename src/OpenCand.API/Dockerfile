FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution and project files for layer caching
COPY OpenCand.sln ./
COPY src/OpenCand.API/OpenCand.API.csproj src/OpenCand.API/
COPY src/OpenCand.Core/OpenCand.Core.csproj src/OpenCand.Core/

RUN dotnet restore src/OpenCand.API/OpenCand.API.csproj

# Copy source code and build
COPY src/OpenCand.API/. src/OpenCand.API/
COPY src/OpenCand.Core/. src/OpenCand.Core/
# embedded by OpenCand.Core (migrations applied on startup + materialized view definitions)
COPY db/migrations/ db/migrations/
COPY db/mv.sql db/mv.sql

WORKDIR /src/src/OpenCand.API
RUN dotnet publish -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

# Ensure fotos_cand directory exists even if no volume is mounted
RUN mkdir -p /app/fotos_cand

ENV DatabaseSettings__ConnectionString=
EXPOSE 8080

ENTRYPOINT ["dotnet", "OpenCand.API.dll"]
