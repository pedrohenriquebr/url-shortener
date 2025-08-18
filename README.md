# URL Shortener Service

<p align="center">
  A high-performance URL shortening API built with .NET 8, designed for scalability and observability.
  <br />
  This project is an implementation of the <a href="https://roadmap.sh/projects/url-shortening-service"><strong>URL Shortening Service project from roadmap.sh</strong></a>.
</p>
<p align="center">
  <a href="https://github.com/pedrohenriquebr/url-shortener/issues">Report Bug</a>
  ·
  <a href="https://github.com/pedrohenriquebr/url-shortener/issues">Request Feature</a>
</p>

---

## About The Project

This project provides a complete backend solution for shortening URLs. It's built with a focus on high performance, scalability, and robust observability, following modern software engineering and SRE principles.

Key features include:
* **High-Performance Endpoints**: Fast URL shortening and redirection.
* **Scalable Architecture**: Utilizes Redis for caching and a background service for high-throughput access counting, following CQRS principles.
* **Full Observability Stack**: Comes with a pre-configured Docker Compose environment including Prometheus and Grafana for real-time monitoring of application and database metrics.
* **Load Testing Scripts**: Includes Python scripts to benchmark the API and simulate realistic, concurrent user traffic.

### Built With

This project is built with a modern tech stack:

* **Backend**: C# with .NET 8
* **Database**: Microsoft SQL Server
* **Cache**: Redis
* **Containerization**: Docker & Docker Compose
* **Observability**: Prometheus & Grafana

---

## Getting Started

Follow these steps to get a local copy up and running for development and testing.

### Prerequisites

* **.NET 8 SDK**: [Download](https://dotnet.microsoft.com/download/dotnet/8.0)
* **Docker Desktop**: [Download](https://www.docker.com/products/docker-desktop/)
* **Python 3.10+** (for running benchmark scripts)

### Installation & Setup

1.  **Clone the repository:**
    ```sh
    git clone [https://github.com/pedrohenriquebr/url-shortener.git](https://github.com/pedrohenriquebr/url-shortener.git)
    cd url-shortener
    ```

2.  **Start the infrastructure services:**
    This command will build the custom SQL Server image and start the database, Redis, Prometheus, and Grafana containers in the background.
    ```sh
    docker-compose up --build -d
    ```

3.  **Run the API:**
    You can now run the API project directly from your IDE or via the command line. It will automatically connect to the services running in Docker.
    ```sh
    dotnet run --project /UrlShortener/UrlShortener.csproj
    ```
    The API will be available at `http://localhost:5099`.

---

## Usage

### API Endpoints

Once the application is running, you can explore and interact with the API endpoints through the Swagger UI:

* **Swagger UI**: `http://localhost:5099/swagger`

### Observability Stack

The local environment includes a full monitoring stack to observe the application's performance in real-time.

* **Prometheus**: `http://localhost:9090`
   * *Check the `Targets` page to ensure Prometheus is successfully scraping metrics from the API and the SQL exporter.*

* **Grafana**: `http://localhost:3000`
   * *Default login: `admin` / `admin`*
   * *You will need to configure Prometheus as a data source upon first login. Use `http://prometheus:9090` as the URL.*

---

## Contributing

Contributions are what make the open-source community such an amazing place to learn, inspire, and create. Any contributions you make are **greatly appreciated**.

1.  Fork the Project
2.  Create your Feature Branch (`git checkout -b feature/AmazingFeature`)
3.  Commit your Changes (`git commit -m 'Add some AmazingFeature'`)
4.  Push to the Branch (`git push origin feature/AmazingFeature`)
5.  Open a Pull Request

---

## License

Distributed under the MIT License. See `LICENSE` for more information.

---

## Contact

Project Link: [https://github.com/pedrohenriquebr/url-shortener/](https://github.com/pedrohenriquebr/url-shortener/)