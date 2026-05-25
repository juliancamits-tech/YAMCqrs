# YAMCqrs
Yet another Cqrs + Mediator framework for .NET


> [!TIP] Click in package name for see the Doc

| Package    | Description | Nuget Version | Nuget Downloads |
|:------------|:-------------|:---------------:|:-----------------:|
|[YAMCqrs.BackgroundWorker](docs/packages/YAMCqrs.BackgroundWorker.Core_en.md)| BackGround Service estandarizer|[![NuGet Version](https://img.shields.io/nuget/v/YAMCqrs.Core.svg?style=flat-square)](https://www.nuget.org/packages/YAMCqrs.BackgroundWorker/)|[![NuGet Downloads](https://img.shields.io/nuget/dt/YAMCqrs.Core.svg?style=flat-square)](https://www.nuget.org/packages/YAMCqrs.BackgroundWorker)|
|[YAMCqrs.BackgroundWorker.Storage.MongoDb](docs/packages/YAMCqrs.BackgroundWorker.Storage.MongoDb_en.md)|MongoDb Storage for BackGround Workers|[![NuGet Version](https://img.shields.io/nuget/v/YAMCqrs.Core.svg?style=flat-square)](https://www.nuget.org/packages/YAMCqrs.BackgroundWorker.Storage.MongoDb/)|[![NuGet Downloads](https://img.shields.io/nuget/dt/YAMCqrs.Core.svg?style=flat-square)](https://www.nuget.org/packages/YAMCqrs.BackgroundWorker.Storage.MongoDb/)|
|[YAMCqrs.Core](docs/packages/YAMCqrs.Core_en.md)|Core Package|[![NuGet Version](https://img.shields.io/nuget/v/YAMCqrs.Core.svg?style=flat-square)](https://www.nuget.org/packages/YAMCqrs.Core/)|[![NuGet Downloads](https://img.shields.io/nuget/dt/YAMCqrs.Core.svg?style=flat-square)](https://www.nuget.org/packages/YAMCqrs.Core/)|
|[YAMCqrs.EventBus.Core](docs/packages/YAMCqrs.EventBus.Core_en.md)|Event Bus Core Package and Domain Event|[![NuGet Version](https://img.shields.io/nuget/v/YAMCqrs.Core.svg?style=flat-square)](https://www.nuget.org/packages/YAMCqrs.EventBus.Core/)|[![NuGet Downloads](https://img.shields.io/nuget/dt/YAMCqrs.Core.svg?style=flat-square)](https://www.nuget.org/packages/YAMCqrs.EventBus.Core/)|
|[YAMCqrs.EventBus.Provider.Kafka](docs/packages/YAMCqrs.EventBus.Provider.Kafka_en.md)|Kafka Event Bus Provider|[![NuGet Version](https://img.shields.io/nuget/v/YAMCqrs.Core.svg?style=flat-square)](https://www.nuget.org/packages/YAMCqrs.EventBus.Provider.Kafka/)|[![NuGet Downloads](https://img.shields.io/nuget/dt/YAMCqrs.Core.svg?style=flat-square)](https://www.nuget.org/packages/YAMCqrs.EventBus.Provider.Kafka/)|
|[YAMCqrs.EventBus.Storage.MongoDb](docs/packages/YAMCqrs.EventBus.Storage.MongoDb_en.md)|MongoDb Storage for Events|[![NuGet Version](https://img.shields.io/nuget/v/YAMCqrs.Core.svg?style=flat-square)](https://www.nuget.org/packages/YAMCqrs.EventBus.Storage.MongoDb/)|[![NuGet Downloads](https://img.shields.io/nuget/dt/YAMCqrs.Core.svg?style=flat-square)](https://www.nuget.org/packages/YAMCqrs.EventBus.Storage.MongoDb/)|

(I am going to translate this to english when i have a more usable version)

## 📚 Documentation / Documentación

*   **[Docs](./docs/):** Documentacion.
*   **[ADRs (Architecture Decision Records)](./docs/adr):** Registro de las decisiones arquitectónicas clave y el "porqué" de las soluciones técnicas.
*   **[Packages](./docs/packages):** Documentación sobre cómo implementar y configurar cada paquete.
*   **[Example](./docs/examples/):** Ejemplos de uso.


## 🚀 Roadmap

[X] Core Package

[X] EventBus Package

[X] DotNet.Aspire for local testing the framework

[X] Add Mongo support as extension for storage information

[X] Add Kafka as extension for EventBus

[X] Create Nugget.org Profile

[X] First relase un Nugget.org

[X] Pipeline CI/CD

[X] More English Documentaion

[X] Add Some Analzers

[-] Unit Tests (Working on this).

[-] Check for implemente more *record*, *record struct* for thinks that are just `primitive container class` and define ADR for the use.

[-] Check if enums can be used in more perfomant way.

[-] Add RabbitMq

[-] Define for Relational database (SqlServer, Postgre) we are using EF, Dapper or the direct driver for each.