FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-env

COPY . ./App

WORKDIR /App

RUN dotnet restore

RUN dotnet publish -c Release -o out

FROM mcr.microsoft.com/dotnet/aspnet:8.0

COPY --from=build-env /App/out .

ENTRYPOINT ["dotnet", "ImperativeCompiler.dll"]