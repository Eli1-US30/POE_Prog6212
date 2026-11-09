# syntax=docker/dockerfile:1

# ---------- Build stage: compile and publish ----------
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy only the project file first, so the restore layer is cached until the dependencies change
COPY RaceDay.MVC.csproj ./
RUN dotnet restore

# Copy the rest of the source and publish a release build
COPY . ./
RUN dotnet publish -c Release -o /app/publish --no-restore /p:UseAppHost=false

# ---------- Runtime stage: small image with only what is needed to run ----------
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

# Run as a non-root user (built into Microsoft's ASP.NET images)
USER $APP_UID

COPY --from=build /app/publish .

ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "RaceDay.MVC.dll"]