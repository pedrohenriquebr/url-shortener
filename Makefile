include .env
export $(shell sed 's/=.*//' .env)

stop-all:
	docker compose stop

reset-db:
	docker compose stop db
	docker compose rm -v db

stop-db:
	docker compose stop db

init-db:
	docker compose build db
	docker compose up -d db 
	sleep 15
	docker compose exec db  /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P $$SA_PASSWORD -Q "CREATE DATABASE [UrlShortenerDb]"
	docker compose exec db  /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P $$SA_PASSWORD -i setup.sql