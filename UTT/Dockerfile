FROM microsoft/dotnet:2.1-sdk-stretch AS build
WORKDIR /app
COPY / .
RUN dotnet publish -c Release -o out

FROM microsoft/dotnet:2.1-aspnetcore-runtime AS runtime
ENV ASPNETCORE_URLS http://+:80
WORKDIR /app
COPY --from=build /app/out ./

ENTRYPOINT ["dotnet", "UTT.dll"]
