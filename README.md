# YAMCqrs
Yet another Cqrs + Mediator framework for Net


(I am going to translate this to english when i have a more usable version)

El objetivo de este proyecto es crear un Framework que utiliza como CORE Cqrs + Mediator la idea principal es usar "Source Generators" para evitar el uso de reflection y Analyzers para reforzar buenas practicas.
Tambien la idea es poder crear extensiones o paquetes adicionales que permitan brindar mas funcionalidades el paquete principal solamente contempla los conceptos de Cqrs + Mediator
Pero por ejemplo el paquete de ServiceBus plantea el incorporar "Evento de dominio" como si fuera un "evento de integracion" en memoria para que luego cambiar ese "evento de dominio" a "evento de integracion" sea mucho mas facil.
Tambien la idea es que el paquete de ServiceBus sea agnostico al bus a utilizar y se creen extensiones del mismo para implementaciones mas directas

Hay muchas definiciones sobre esto en los ADR que estan en la carpeta "docs"

El roadmap que tengo en mente seria.

[X] Paquete Core
[X] Paquete Core de ServiceBus
[-] Integracion con DotNet.Aspire para facilitar levantar el proyecto local con dependencias como Mongo
[-] Integracion con Mongo para demostrar como las cosas "En memoria" se percistirian en una BD
[-] Integracion con Kafka y RabbitMq
[-] Crear perfile en Nugget.org
[-] Pipeline CI/CD
[-] Primera version oficial en Nugget.org
[-] Configuracion del repo para buenas practicas
[-] Documentacion fuerte en ingles con diagramas
