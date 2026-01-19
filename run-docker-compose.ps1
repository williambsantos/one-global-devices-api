Set-Location ./src/one-global-devices-api

docker-compose up --build -d

Start-Process http://localhost:5000/scalar/

Set-Location ../..