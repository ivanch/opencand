FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ./OpenCand.sln ./
COPY ./OpenCand.API/OpenCand.API.csproj ./OpenCand.API/
COPY ./OpenCand.Core/OpenCand.Core.csproj ./OpenCand.Core/

RUN dotnet restore ./OpenCand.API/OpenCand.API.csproj

COPY ./OpenCand.API/. ./OpenCand.API/
COPY ./OpenCand.Core/. ./OpenCand.Core/

WORKDIR /src/OpenCand.API
RUN dotnet publish -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

ENV ConnectionStrings__DefaultConnection=
EXPOSE 8080

ENTRYPOINT ["dotnet", "OpenCand.API.dll"]
