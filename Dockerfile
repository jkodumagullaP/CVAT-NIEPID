# ============================
#   BUILD STAGE
# ============================
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY CAT.AID.Web.csproj ./
RUN dotnet restore --disable-parallel

COPY . ./
RUN dotnet publish CAT.AID.Web.csproj -c Release -o /app/publish --no-restore


# ============================
#   RUNTIME STAGE
# ============================
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Fonts for QuestPDF
RUN apt-get update && apt-get install -y \
    fontconfig \
    fonts-dejavu \
    && rm -rf /var/lib/apt/lists/*

# Copy published output
COPY --from=build /app/publish ./

# Ensure runtime folders exist
RUN mkdir -p /app/wwwroot/Images \
    /app/wwwroot/data \
    /app/wwwroot/uploads

ENV ASPNETCORE_URLS=http://+:8080
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false

EXPOSE 8080
ENTRYPOINT ["dotnet", "CAT.AID.Web.dll"]
