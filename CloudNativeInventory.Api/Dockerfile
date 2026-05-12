# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# --- STAGE 1: BASE ---
# This stage set up the basic runtime environment.
# We use the ASP.NET runtime image because it's small and optimized for running apps.
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
# Define a non-root user to run the application for better security.
ENV ASPNETCORE_HTTP_PORTS=8080
# Security: Run as a non-privileged user (Rootless) to reduce the attack surface.
USER $APP_UID
# Set the working directory inside the container.
WORKDIR /app
# Document that the app listens on ports 8080 (HTTP) and 8081 (HTTPS).
EXPOSE 8080



# --- STAGE 2: BUILD ---
# This stage uses the full .NET SDK to compile the source code.
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
# Define whether to build in Debug or Release mode (defaults to Release).
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# CACHE OPTIMIZATION: Copy only the project file first. 
# Docker will cache this layer. As long as you don't change your dependencies (NuGet packages), 
# Docker will skip the 'dotnet restore' step in future builds.
COPY ["CloudNativeInventory.Api/CloudNativeInventory.Api.csproj", "CloudNativeInventory.Api/"]

# Download/restore the required NuGet packages based on the .csproj file.
RUN dotnet restore "./CloudNativeInventory.Api/CloudNativeInventory.Api.csproj"

# Copy all the remaining source code files into the container.
# This layer will be re-run every time you change a line of code in your Controllers or Models.
COPY . .

# Move into the project folder and compile the application.
WORKDIR "/src/CloudNativeInventory.Api"
RUN dotnet build "./CloudNativeInventory.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build


# --- STAGE 3: PUBLISH ---
# This stage prepares the application for production by "publishing" it (optimizing files).
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
# Create the final, ready-to-run files in the /app/publish folder.
RUN dotnet publish "./CloudNativeInventory.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false


# --- STAGE 4: FINAL ---
# This is the actual image that will be deployed.
# We go back to the small 'base' image to keep the final size as small as possible.
FROM base AS final
WORKDIR /app
# Copy only the finished files from the publish stage. 
# We leave the heavy SDK and source code behind to stay lightweight and secure.
COPY --from=publish /app/publish .
# Define the command to start the API when the container launches.
ENTRYPOINT ["dotnet", "CloudNativeInventory.Api.dll"]