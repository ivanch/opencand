# ─── Stage 1: Build ───────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy only the solution and project files first, restore, then copy the rest
COPY ./OpenCand.sln ./
COPY ./OpenCand.ETL/OpenCand.ETL.csproj ./OpenCand.ETL/
COPY ./OpenCand.Core/OpenCand.Core.csproj ./OpenCand.Core/

RUN dotnet restore ./OpenCand.ETL/OpenCand.ETL.csproj

# Now copy the rest of the ETL source code
COPY ./OpenCand.ETL/. ./OpenCand.ETL/
COPY ./OpenCand.Core/. ./OpenCand.Core/

WORKDIR /src/OpenCand.ETL
RUN dotnet publish -c Release -o /app/publish

# ─── Stage 2: Runtime ─────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/runtime:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

ENV ConnectionStrings__DefaultConnection

ENTRYPOINT ["dotnet", "OpenCand.ETL.dll"]
