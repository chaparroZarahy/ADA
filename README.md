# CarritoComprasADA - Sistema de Gestión de Compras

Este repositorio contiene el sistema **CarritoComprasADA**, dividido en tres componentes principales:

- `CarritoComprasADA_API/`: Aplicación ASP.NET Core Web API para manejar la lógica de negocio y la comunicación con la base de datos.
- `CarritoComprasADA/`: Aplicación ASP.NET Core MVC (interfaz de usuario) que consume la API.
- `BD/`: Carpeta con los scripts necesarios para crear y poblar la base de datos SQL Server.

---


---

## 🚀 Cómo ejecutar el proyecto

### 1. Requisitos previos

- .NET SDK 6 o superior
- SQL Server (LocalDB o instancia completa)
- Visual Studio 2022 (recomendado)

### 2. Base de Datos

1. Abre SQL Server Management Studio.
2. Ejecuta los scripts dentro de la carpeta `BD/` en el siguiente orden:
   - `01 TABLES.sql`
   - `02 STORED PROCEDURES.sql`
   - `03 INITIAL DATA.sql`

> Asegúrate de que el archivo `appsettings.json` en ambos proyectos tenga la cadena de conexión correcta hacia tu base de datos SQL Server.

### 3. Ejecutar la API

```bash
cd CarritoComprasADA_API
dotnet run

