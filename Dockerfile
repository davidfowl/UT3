FROM microsoft/dotnet:2.1-sdk-stretch AS build
WORKDIR /app

# Copy csproj and restore as distinct layers
COPY /UTT/*.csproj ./
RUN dotnet restore

COPY /UTT .
RUN dotnet publish -c Release -r linux-musl-x64 -o out

FROM microsoft/dotnet:2.1-runtime-deps-alpine AS runtime
ENV ASPNETCORE_URLS http://+:80
WORKDIR /app
COPY --from=build /app/out ./

ENTRYPOINT ["./UTT"]
