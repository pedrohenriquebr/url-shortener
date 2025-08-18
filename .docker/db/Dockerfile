FROM mcr.microsoft.com/mssql/server:2017-latest
ENV ACCEPT_EULA y
ENV SA_PASSWORD HakunaM4t4t4@171
ENV MSSQL_PID Express

# Create a config directory
RUN mkdir -p /usr/config
WORKDIR /usr/config

# Bundle config source
COPY . /usr/config