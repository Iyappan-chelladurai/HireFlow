# Stage 1: Build the application
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy the csproj and restore dependencies
COPY ["HireFlow_API.csproj", "./"]
RUN dotnet restore "HireFlow_API.csproj"

# Copy all files and build the app
COPY . .
RUN dotnet publish "HireFlow_API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Stage 2: Create runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "HireFlow_API.dll"]
