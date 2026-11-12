FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["HotelBookingAPI/HotelBookingAPI/HotelBookingAPI.csproj", "HotelBookingAPI/HotelBookingAPI/"]
RUN dotnet restore "HotelBookingAPI/HotelBookingAPI/HotelBookingAPI.csproj"

COPY . .
WORKDIR /src/HotelBookingAPI/HotelBookingAPI
RUN dotnet publish "HotelBookingAPI.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://0.0.0.0:10000
EXPOSE 10000
ENTRYPOINT ["sh", "-c", "dotnet HotelBookingAPI.dll --urls http://0.0.0.0:${PORT:-10000}"]
