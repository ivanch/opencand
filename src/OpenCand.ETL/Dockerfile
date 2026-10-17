FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution and project files for layer caching
COPY OpenCand.sln ./
COPY src/OpenCand.ETL/OpenCand.ETL.csproj src/OpenCand.ETL/
COPY src/OpenCand.Core/OpenCand.Core.csproj src/OpenCand.Core/

RUN dotnet restore src/OpenCand.ETL/OpenCand.ETL.csproj

# Copy source code and build
COPY src/OpenCand.ETL/. src/OpenCand.ETL/
COPY src/OpenCand.Core/. src/OpenCand.Core/

WORKDIR /src/src/OpenCand.ETL
RUN dotnet publish -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/runtime:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

ENV DatabaseSettings__ConnectionString=
ENV BasePath=/app/sample

ENTRYPOINT ["dotnet", "OpenCand.ETL.dll"]
