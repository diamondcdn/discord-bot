FROM mcr.microsoft.com/dotnet/runtime:7.0 AS base
WORKDIR /app

RUN apt-get update
RUN apt-get install -y locales
RUN sed -i -e 's/# en_US.UTF-8 UTF-8/en_US.UTF-8 UTF-8/' /etc/locale.gen && \
    locale-gen
ENV LC_ALL en_US.UTF-8 
ENV LANG en_US.UTF-8  
ENV LANGUAGE en_US:en     

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["DiamondCDN.Bot/DiamondCDN.Bot.csproj", "DiamondCDN.Bot/"]
COPY ["DiamondCDN.Bot.Common/DiamondCDN.Bot.Common.csproj", "DiamondCDN.Bot.Common/"]
COPY ["DiamondCDN.Bot.Commands/DiamondCDN.Bot.Commands.csproj", "DiamondCDN.Bot.Commands/"]
COPY ["DiamondCDN.Bot.Services/DiamondCDN.Bot.Services.csproj", "DiamondCDN.Bot.Services/"]
RUN dotnet restore "DiamondCDN.Bot/DiamondCDN.Bot.csproj"
COPY . .
WORKDIR "/src/DiamondCDN.Bot"
RUN dotnet build "DiamondCDN.Bot.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "DiamondCDN.Bot.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "DiamondCDN.Bot.dll"]
