FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ./OpenCand.sln ./
COPY ./OpenCand.ETL/OpenCand.ETL.csproj ./OpenCand.ETL/
COPY ./OpenCand.Core/OpenCand.Core.csproj ./OpenCand.Core/

RUN dotnet restore ./OpenCand.ETL/OpenCand.ETL.csproj

COPY ./OpenCand.ETL/. ./OpenCand.ETL/
COPY ./OpenCand.Core/. ./OpenCand.Core/

WORKDIR /src/OpenCand.ETL
RUN dotnet publish -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/runtime:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

ENV ConnectionStrings__DefaultConnection=

ENTRYPOINT ["dotnet", "OpenCand.ETL.dll"]
