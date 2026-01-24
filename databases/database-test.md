## DATABASES

* create database development enviroment

```
cd ./src/one-global-devices-api
docker-compose down
docker-compose up --build -d
docker-compose up sqlserver
```

* if your system doesn't have a docker compose installed

## (powershell)

```
cd ./databases

docker run -e "ACCEPT_EULA=Y" -e "MSSQL_SA_PASSWORD=SqlServer2022!" `
  -p 1433:1433 `
  --name sqlserver `
  --hostname sqlserver `
   -v "sqlserver_data:/var/opt/mssql" `
   -d `
   mcr.microsoft.com/mssql/server:2022-latest

-- change appSettings to localhost
```

## bash

```
cd ./databases

docker run -e "ACCEPT_EULA=Y" -e "MSSQL_SA_PASSWORD=SqlServer2022!" \
  -p 1433:1433 \
  --name sqlserver \
  --hostname sqlserver \
   -v "$(pwd)/mssql-data:/var/opt/mssql/data" \
   -d \
   mcr.microsoft.com/mssql/server:2022-latest

-- change appSettings to localhost
```
