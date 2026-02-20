# ============================
#   BUILD STAGE
# ============================
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project file and restore first (layer caching)
COPY CAT.AID.Web.csproj ./
RUN dotnet restore

# Copy everything else
COPY . ./

# Publish (NO --no-restore)
RUN dotnet publish CAT.AID.Web.csproj -c Release -o /app/publish


# ============================
#   RUNTIME STAGE
# ============================
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Install fonts for QuestPDF
RUN apt-get update && apt-get install -y \
    fontconfig \
    fonts-dejavu \
    && rm -rf /var/lib/apt/lists/*

COPY --from=build /app/publish ./

RUN mkdir -p /app/wwwroot/Images \
    /app/wwwroot/data \
    /app/wwwroot/uploads

ENV ASPNETCORE_URLS=http://+:8080
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false

EXPOSE 8080
ENTRYPOINT ["dotnet", "CAT.AID.Web.dll"]
