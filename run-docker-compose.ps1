cd ./src/one-global-devices-api

docker-compose up --build -d

Start-Process http://localhost:5000/swagger/index.html

cd ../..