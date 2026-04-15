FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["Training.Api/Training.Api.csproj", "Training.Api/"]
COPY ["Training.Application/Training.Application.csproj", "Training.Application/"]
COPY ["Training.Domain/Training.Domain.csproj", "Training.Domain/"]
COPY ["Training.Infrastructure/Training.Infrastructure.csproj", "Training.Infrastructure/"]
RUN dotnet restore "Training.Api/Training.Api.csproj"

COPY . .
WORKDIR "/src/Training.Api"
RUN dotnet publish "Training.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Training.Api.dll"]
