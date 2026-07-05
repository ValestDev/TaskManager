FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src


COPY ["src/API/TaskManager.API.csproj", "src/API/"]
COPY ["src/Application/TaskManager.Application.csproj", "src/Application/"]
COPY ["src/Infrastructure/TaskManager.Infrastructure.csproj", "src/Infrastructure/"]
COPY ["src/Domain/TaskManager.Domain.csproj", "src/Domain/"]

RUN dotnet restore "src/API/TaskManager.API.csproj"


COPY . .

WORKDIR /src/src/API
RUN dotnet publish "TaskManager.API.csproj" -c Release -o /app/publish


FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

EXPOSE 8080
ENTRYPOINT ["dotnet", "TaskManager.API.dll"]