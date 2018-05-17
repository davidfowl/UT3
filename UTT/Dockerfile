FROM microsoft/dotnet:2.1-sdk-stretch AS build
WORKDIR /app

# Copy csproj and restore as distinct layers
COPY *.csproj ./
RUN dotnet restore

COPY / .
RUN dotnet publish -c Release -o out

FROM microsoft/dotnet:2.1.0-rc1-aspnetcore-runtime-alpine AS runtime
ENV ASPNETCORE_URLS http://+:80
WORKDIR /app
COPY --from=build /app/out ./

ENTRYPOINT ["dotnet", "UTT.dll"]
