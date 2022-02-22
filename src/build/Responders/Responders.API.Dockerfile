FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://*:8080
ENV ASPNETCORE_FORWARDEDHEADERS_ENABLED=true
ENV DOTNET_gcServer=1

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
# install diagnostics tools
RUN dotnet tool install --tool-path /tools dotnet-trace
RUN dotnet tool install --tool-path /tools dotnet-counters
RUN dotnet tool install --tool-path /tools dotnet-dump

WORKDIR /src
COPY ["EMBC.Responders.API/EMBC.Responders.API.csproj", "EMBC.Responders.API/"]
COPY ["EMBC.ESS.Shared.Contracts/EMBC.ESS.Shared.Contracts.csproj", "EMBC.ESS.Shared.Contracts/"]
COPY ["EMBC.Utilities/EMBC.Utilities.csproj", "EMBC.Utilities/"]
COPY ["EMBC.Utilities.Hosting/EMBC.Utilities.Hosting.csproj", "EMBC.Utilities.Hosting/"]
COPY ["EMBC.Utilities.Messaging/EMBC.Utilities.Messaging.csproj", "EMBC.Utilities.Messaging/"]
COPY ["EMBC.Tests.Unit.Responders.API/EMBC.Tests.Unit.Responders.API.csproj", "EMBC.Tests.Unit.Responders.API/"]
COPY ["EMBC.Responders.sln", "stylecop.json", ".editorconfig", "./"]
RUN dotnet restore .
COPY . .

# run unit tests
RUN dotnet test EMBC.Responders.sln -c Release

# build
FROM build AS publish
RUN dotnet publish "EMBC.Responders.API/EMBC.Responders.API.csproj" -c Release -o /app/publish --runtime linux-musl-x64 --no-self-contained

FROM base AS final
# copy diagnostics tools
WORKDIR /tools
COPY --from=build /tools .
# copy app
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "EMBC.Responders.API.dll"]