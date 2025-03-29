#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 5003

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["Pong/Pong.csproj", "PongMicroService/"]
RUN dotnet restore "PongMicroService/Pong.csproj"
COPY . .
WORKDIR "/src/Pong"
RUN dotnet build "Pong.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Pong.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Pong.dll"]
