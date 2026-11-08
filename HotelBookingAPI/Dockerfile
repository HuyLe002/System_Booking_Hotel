FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY HotelBookingAPI/HotelBookingAPI.csproj HotelBookingAPI/
RUN dotnet restore HotelBookingAPI/HotelBookingAPI.csproj

COPY . .
RUN dotnet publish HotelBookingAPI/HotelBookingAPI.csproj \
    --configuration Release \
    --output /app/publish \
    --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
ENV ASPNETCORE_URLS=http://0.0.0.0:10000
EXPOSE 10000

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "HotelBookingAPI.dll"]
