FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["S3PresignApi.csproj", "./"]
RUN dotnet restore "./S3PresignApi.csproj"
COPY . .
RUN dotnet publish "S3PresignApi.csproj" -c Release -o /out

FROM base AS final
WORKDIR /app
COPY --from=build /out .
ENTRYPOINT ["dotnet", "S3PresignApi.dll"]