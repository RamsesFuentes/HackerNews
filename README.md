# 🚀 Hacker News - RESTful API (ASP.NET Core)

This project implements a high-performance RESTful API in ASP.NET Core that retrieves the top **n best stories** from the [Hacker News API](https://github.com/HackerNews/API), ordered by score. It is designed to handle high loads while avoiding unnecessary calls to the Hacker News service through intelligent caching and concurrency controls.

## ✅ Features

- **RESTful Endpoint**: `GET /stories/best?quantity=n`
- **Data Source**: Integrates with the public Hacker News API
- **Sorted Output**: Stories returned in descending order by score
- **Efficient Request Handling**:
  - **In-memory caching** with expiration for story IDs and individual stories
  - **Asynchronous parallel fetching** of story details
  - **Concurrency-safe cache population** using custom async mutex with timeout
  - Avoids overloading Hacker News API through request de-duplication
- **Clean Architecture**:
  - Separation of concerns via repository and service layers
  - Typed `HttpClient` with `HttpClientFactory`
  - Strongly-typed DTOs with Unix time conversion
  - Centralized logging using `ILogger<T>`

## 📦 Sample Output Format

```json
[
  {
    "title": "A uBlock Origin update was rejected from the Chrome Web Store",
    "uri": "https://github.com/uBlockOrigin/uBlock-issues/issues/745",
    "postedBy": "ismaildonmez",
    "time": "2019-10-12T13:43:01+00:00",
    "score": 1716,
    "commentCount": 572
  },
  ...
]
