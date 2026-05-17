# YAMCqrs
Yet another Cqrs + Mediator framework for .NET

| Package    | Description | Nuget Version | Nuget Downloads |
|:------------|:-------------|:---------------:|:-----------------:|
|YAMCqrs.BackgroundWorker| BackGround Service estandarizer|[![NuGet Version](https://img.shields.io/nuget/v/YAMCqrs.Core.svg?style=flat-square)](https://www.nuget.org/packages/YAMCqrs.BackgroundWorker/)|[![NuGet Downloads](https://img.shields.io/nuget/dt/YAMCqrs.Core.svg?style=flat-square)](https://www.nuget.org/packages/YAMCqrs.BackgroundWorker)|
|YAMCqrs.BackgroundWorker.Storage.MongoDb|MongoDb Storage for BackGround Workers|[![NuGet Version](https://img.shields.io/nuget/v/YAMCqrs.Core.svg?style=flat-square)](https://www.nuget.org/packages/YAMCqrs.BackgroundWorker.Storage.MongoDb/)|[![NuGet Downloads](https://img.shields.io/nuget/dt/YAMCqrs.Core.svg?style=flat-square)](https://www.nuget.org/packages/YAMCqrs.BackgroundWorker.Storage.MongoDb/)|
|YAMCqrs.Core|Core Package|[![NuGet Version](https://img.shields.io/nuget/v/YAMCqrs.Core.svg?style=flat-square)](https://www.nuget.org/packages/YAMCqrs.Core/)|[![NuGet Downloads](https://img.shields.io/nuget/dt/YAMCqrs.Core.svg?style=flat-square)](https://www.nuget.org/packages/YAMCqrs.Core/)|
|YAMCqrs.EventBus.Core|Event Bus Core Package and Domain Event|[![NuGet Version](https://img.shields.io/nuget/v/YAMCqrs.Core.svg?style=flat-square)](https://www.nuget.org/packages/YAMCqrs.EventBus.Core/)|[![NuGet Downloads](https://img.shields.io/nuget/dt/YAMCqrs.Core.svg?style=flat-square)](https://www.nuget.org/packages/YAMCqrs.EventBus.Core/)|
|YAMCqrs.EventBus.Provider.Kafka|Kafka Event Bus Provider|[![NuGet Version](https://img.shields.io/nuget/v/YAMCqrs.Core.svg?style=flat-square)](https://www.nuget.org/packages/YAMCqrs.EventBus.Provider.Kafka/)|[![NuGet Downloads](https://img.shields.io/nuget/dt/YAMCqrs.Core.svg?style=flat-square)](https://www.nuget.org/packages/YAMCqrs.EventBus.Provider.Kafka/)|
|YAMCqrs.EventBus.Storage.MongoDb|MongoDb Storage for Events|[![NuGet Version](https://img.shields.io/nuget/v/YAMCqrs.Core.svg?style=flat-square)](https://www.nuget.org/packages/YAMCqrs.EventBus.Storage.MongoDb/)|[![NuGet Downloads](https://img.shields.io/nuget/dt/YAMCqrs.Core.svg?style=flat-square)](https://www.nuget.org/packages/YAMCqrs.EventBus.Storage.MongoDb/)|

(I am going to translate this to english when i have a more usable version)

El objetivo de este proyecto es crear un Framework que utiliza como CORE Cqrs + Mediator la idea principal es usar "Source Generators" para evitar el uso de reflection y Analyzers para reforzar buenas practicas.
Tambien la idea es poder crear extensiones o paquetes adicionales que permitan brindar mas funcionalidades el paquete principal solamente contempla los conceptos de Cqrs + Mediator
Pero por ejemplo el paquete de ServiceBus plantea el incorporar "Evento de dominio" como si fuera un "evento de integracion" en memoria para que luego cambiar ese "evento de dominio" a "evento de integracion" sea mucho mas facil.
Tambien la idea es que el paquete de ServiceBus sea agnostico al bus a utilizar y se creen extensiones del mismo para implementaciones mas directas

## 📚 Documentación

Para profundizar en los detalles técnicos y el uso del framework, consulta los siguientes recursos:
*   **[Documentacion](./docs/):** Documentacion.
*   **[ADRs (Architecture Decision Records)](./docs/adr):** Registro de las decisiones arquitectónicas clave y el "porqué" de las soluciones técnicas.
*   **[Documentacion de los Paquetes](./docs/packages):** Documentación sobre cómo implementar y configurar cada paquete.
*   **[Ejemplos](./docs/examples/):** Ejemplos de uso.


## 🚀 Roadmap

[X] Paquete Core

[X] Paquete Core de ServiceBus

[X] Integracion con DotNet.Aspire para facilitar levantar el proyecto local con dependencias como Mongo

[X] Integracion con Mongo para demostrar como las cosas "En memoria" se percistirian en una BD

[X] Integracion con Kafka

[X] Crear perfile en Nugget.org

[X] Primera version oficial en Nugget.org

[-] Pipeline CI/CD

[-] Documentacion en ingles con diagramas

[-] Configuracion de Analyzers

[-] Unit Tests