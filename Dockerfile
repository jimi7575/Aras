FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY Aras.sln ./
COPY Aras.Domain/Aras.Domain.csproj Aras.Domain/
COPY Aras.Application/Aras.Application.csproj Aras.Application/
COPY Aras.Infrastructure/Aras.Infrastructure.csproj Aras.Infrastructure/
COPY Aras.Presentation/Aras.Presentation.csproj Aras.Presentation/
RUN dotnet restore Aras.sln
COPY . .
RUN dotnet publish Aras.Presentation/Aras.Presentation.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:8080
ENV ConnectionStrings__Default="Data Source=/data/aras.db;"
EXPOSE 8080
VOLUME ["/data"]
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Aras.Presentation.dll"]
