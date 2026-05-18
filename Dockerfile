# Stage 1: Build Stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project files and restore dependencies
COPY ["Vanguard Engine.csproj", "./"]
RUN dotnet restore "./Vanguard Engine.csproj"

# Copy remaining source code and build
COPY . .
RUN dotnet build "Vanguard Engine.csproj" -c Release -o /app/build

# Publish the compiled app
FROM build AS publish
RUN dotnet publish "Vanguard Engine.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Stage 2: Runtime Stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Set dynamic port binding for cloud providers (Render, Railway, Fly.io)
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "Vanguard Engine.dll"]
