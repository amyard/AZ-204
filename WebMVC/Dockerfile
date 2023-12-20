FROM mcr.microsoft.com/dotnet/sdk:7.0 as build
WORKDIR /source

COPY *.csproj ./
RUN dotnet restore

COPY . .
RUN dotnet publish -c Release -o out

FROM mcr.microsoft.com/dotnet/aspnet:7.0
WORKDIR /App
COPY --from=build /source/out .
EXPOSE 80
ENTRYPOINT ["dotnet", "WebMVC.dll"]