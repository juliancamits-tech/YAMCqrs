# YAMCqrs.Core

Libreria CORE del proyecto usando como base los conceptos de CQRS y Mediator como orquestador.

## ⚙️ Instalación

```bash
dotnet add package YAMCqrs.Core
```

## 🚀 Uso Rápido
Para registrar la en tu contenedor de dependencias:

```csharp
 builder.Services.AddCqrs();
```

## 🛠️ Detalles de Implementación

- **ICommand y IQuery**: Se usa una implementacion doble para separar flujo de escritura del solo lectura. 
- **ICommandHandler y IQueryHandler**: Procesamiento de los comandos ejecutados.
- **ICommandInterceptor y IQueryInterceptor**: Logica adicional que se agrega a nivel pipeline.
- **IDispatcher**: Interface para ejecutar los ICommand y IQuery.
- **Result**: Estandar para no utilizar Exceptions para salir por errores de negocios.

> [!TIP]
> Se recomienda ver el ejemplo de implementacion basica para mas detalles [Ejemplo](../examples/CommandWorkFlow_spa.md)

## 📋 Dependencias

- Este proyecto no tiene dependencias.

> [!IMPORTANT]
> Este proyecto utiliza SourceGeneration para generar la inyeccion de dependencia de los Handlers y generar un Dispatcher que no requiere la utilizacion de Reflection para su funcionamiento.