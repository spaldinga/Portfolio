FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["SoftwareService/SoftwareService.csproj", "SoftwareService/"]
COPY ["SoftwareService.Tests/SoftwareService.Tests.csproj", "SoftwareService.Tests/"]
RUN dotnet restore "/src/SoftwareService/SoftwareService.csproj"
COPY . .
RUN dotnet build "/src/SoftwareService/SoftwareService.csproj" -c Release -o /app/build

FROM build AS testrunner
WORKDIR /src/SoftwareService.Tests
RUN dotnet tool install dotnet-reportgenerator-globaltool --tool-path /dotnetglobaltools --version 5.3.11
LABEL unittestlayer=true
RUN dotnet test --logger "trx;LogFileName=softwareservicetestresults.xml" /p:CollectCoverage=true /p:CoverletOutputFormat=cobertura /p:CoverletOutput=/out/testresults/coverage/ /p:Exclude="[xunit.*]*" /p:Exclude="[*]SoftwareService.Migrations*" --results-directory /out/testresults
RUN /dotnetglobaltools/reportgenerator "-reports:/out/testresults/coverage/coverage.cobertura.xml" "-targetdir:/out/testresults/coverage/reports" "-reporttypes:HTMLInline;HTMLChart"

FROM build AS efbundlerunner
WORKDIR /src/SoftwareService
RUN dotnet new tool-manifest
RUN dotnet tool install dotnet-ef --version 7.0.0
LABEL efbundlelayer=true
RUN dotnet ef migrations bundle --self-contained -r linux-x64 --project SoftwareService.csproj  --verbose  --prefix-output --output efbundle --force

FROM build AS publish
RUN dotnet publish "/src/SoftwareService/SoftwareService.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
COPY --from=testrunner /out .
COPY --from=efbundlerunner /src/SoftwareService/efbundle .
ENTRYPOINT ["dotnet", "SoftwareService.dll"]
