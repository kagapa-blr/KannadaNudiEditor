FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY KannadaNudiWeb.csproj .
RUN dotnet restore
COPY . .
RUN dotnet publish -c Release -o /app/publish

FROM nginx:alpine
WORKDIR /usr/share/nginx/html
EXPOSE 5000
COPY nginx.conf /etc/nginx/conf.d/default.conf
COPY --from=build /app/publish/wwwroot .
