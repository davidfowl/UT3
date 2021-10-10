FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /app

# Copy csproj and restore as distinct layers
COPY /UTT/*.csproj ./
RUN dotnet restore

COPY /UTT .
RUN dotnet publish -c Release -r linux-musl-x64 --self-contained -o out /p:PublishSingleFile=true /p:PublishTrimmed=true

FROM mcr.microsoft.com/dotnet/runtime-deps:6.0-alpine AS runtime
ENV ASPNETCORE_URLS http://+:80
WORKDIR /app
COPY --from=build /app/out ./

ENTRYPOINT ["./UTT"]
