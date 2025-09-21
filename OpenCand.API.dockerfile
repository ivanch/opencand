# ─── Stage 1: Build ───────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy only the solution and project files first, restore, then copy the rest
COPY ./OpenCand.sln ./
COPY ./OpenCand.API/OpenCand.API.csproj ./OpenCand.API/
COPY ./OpenCand.Core/OpenCand.Core.csproj ./OpenCand.Core/

RUN dotnet restore ./OpenCand.API/OpenCand.API.csproj

# Now copy the rest of the API source code
COPY ./OpenCand.API/. ./OpenCand.API/
COPY ./OpenCand.Core/. ./OpenCand.Core/

WORKDIR /src/OpenCand.API
RUN dotnet publish -c Release -o /app/publish

# ─── Stage 2: Runtime ─────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

ENV ConnectionStrings__DefaultConnection
EXPOSE 8080

ENTRYPOINT ["dotnet", "OpenCand.API.dll"]
