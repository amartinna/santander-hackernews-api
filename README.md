# Santander Developer Coding Test - Hacker News Best Stories API

This is a production-ready RESTful API built with **ASP.NET Core (.NET 8/9)** that retrieves the details of the best *n* stories from the Hacker News API, ordered by score in descending order.

## Architectural Strategy & Performance Optimization

To satisfy the core requirement of **efficiently servicing large numbers of requests without overloading the Hacker News API**, the solution deliberately avoids making real-time, sequential HTTP requests to the external API for every incoming client call. Instead, it implements a highly resilient, asynchronous caching infrastructure:

### 1. Asynchronous Background Worker (`IHostedService`)
- A managed background worker (`HackerNewsCacheRefresher`) runs as a singleton service in the application lifecycle.
- Every 60 seconds, it polls the external Firebase endpoint to fetch the list of best story IDs.
- It utilizes **`Task.WhenAll`** to fetch the details of individual stories concurrently across multiple threads, completely eliminating sequential HTTP latency and maximizing throughput.

### 2. Thread-Safe In-Memory Cache Optimization
- Once the background worker aggregates and processes the stories, it sorts them in descending order by score.
- The materialized array is stored in an optimized, thread-safe memory storage cache (`IMemoryCache`).
- **0 ms Latency Service:** When a consumer calls our REST endpoint, the data is served instantly from memory. The external Hacker News API is *never* hit during client requests, guaranteeing absolute protection against API throttling or rate-limiting overloads.

### 3. Clean Architecture Implementation
The solution is decoupled following strict separation of concerns:
- **Core / Domain:** Contains the internal entity models (e.g., `Story`) and abstraction interfaces.
- **Infrastructure:** Implements the resilient `HttpClient` using transient fault handling to manage external infrastructure boundaries.
- **Presentation (API):** Clean controllers serving the structured JSON contract.

---

## Technical Stack & Requirements
- **Runtime:** .NET 8.0 SDK / .NET 9.0 SDK
- **Testing Layer:** xUnit, Moq (for controller and cache boundary isolation testing)

## How to Run the Application

1. **Clone the repository:**
   ```bash
   git clone <your-public-repo-url>
   cd santander-hackernews-api
   ```

2. **Restore dependencies and build:**
   ```bash
   dotnet restore
   dotnet build
   ```

3. **Run the Web API:**
   ```bash
   dotnet run --project src/Santander.HackerNews.Api
   ```

4. **Consume the endpoint:**
   Send a `GET` request specifying the desired number of top stories (*n*):
   ```bash
   GET http://localhost:5297/api/stories/best?n=10
   ```

---

## Assumptions Made
- **Data Freshness:** A 60-second synchronization window with the Hacker News API is assumed to be an optimal balance between near-real-time accuracy and infrastructure safety.
- **Graceful Fallbacks:** If the external Hacker News API encounters downtime during a synchronization tick, the system gracefully continues to serve the last successfully cached snapshot to ensure high availability.

## Future Enhancements (Given More Time)
- **Distributed Caching:** Upgrade from `IMemoryCache` to a distributed cache cluster (**Redis**) to enable seamless horizontal scaling across multiple container instances.
- **Resilient Circuit Breakers:** Inject an advanced Polly Circuit Breaker policy onto the external HTTP client wrapper to handle prolonged network partitions defensively.