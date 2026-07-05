# 📋 Task Manager

Sistema full stack de gestión de tareas y usuarios para equipos, con autenticación segura, control de roles, actualizaciones en tiempo real y auditoría completa de actividad.

Construido con **Clean Architecture** en .NET, autenticación **JWT con refresh tokens**, comunicación en tiempo real con **SignalR**, y un frontend moderno en **React + Tailwind**, todo contenerizado con **Docker Compose**.

![.NET](https://img.shields.io/badge/.NET-10-512BD4?logo=dotnet)
![React](https://img.shields.io/badge/React-19-61DAFB?logo=react)
![PostgreSQL](https://img.shields.io/badge/PostgreSQL-16-4169E1?logo=postgresql)
![Docker](https://img.shields.io/badge/Docker-Compose-2496ED?logo=docker)

## ✨ Características

- 🔐 **Autenticación JWT** con refresh tokens rotativos y hash de contraseñas con BCrypt
- 👥 **Gestión de usuarios** con roles (Admin / Usuario), paginación, búsqueda y activación/desactivación
- ✅ **Gestión de tareas** con estados, asignación entre usuarios, filtros y control de permisos granular
- ⚡ **Tiempo real** con SignalR: ve quién está conectado al instante, sin recargar la página
- 📝 **Auditoría completa**: cada acción relevante queda registrada con usuario, IP y timestamp
- 🐳 **Totalmente contenerizado** con Docker Compose (un solo comando para levantar todo)
- 🎨 **UI moderna y responsive** construida con React y Tailwind CSS

## 🖼️ Vista previa



## 🏗️ Arquitectura

El backend sigue **Clean Architecture**, separando responsabilidades en 4 capas independientes:

src/
├── Domain/          # Entidades y reglas de negocio puras, sin dependencias externas
├── Application/      # Casos de uso, DTOs, interfaces de servicios
├── Infrastructure/   # EF Core, JWT, SignalR, email — implementaciones concretas
└── API/               # Controllers, middleware, punto de entrada

La dirección de las dependencias siempre apunta hacia adentro (`API → Infrastructure → Application → Domain`), lo que permite testear la lógica de negocio de forma aislada y cambiar detalles de infraestructura (por ejemplo, la base de datos) sin tocar las reglas de negocio.

## 🛠️ Stack tecnológico

| Capa | Tecnologías |
|---|---|
| Backend | .NET 10, ASP.NET Core, Entity Framework Core, JWT, BCrypt, SignalR |
| Base de datos | PostgreSQL |
| Frontend | React 19, Vite, Tailwind CSS, React Router, Axios |
| Infraestructura | Docker, Docker Compose, Nginx, Mailhog |

## 🚀 Cómo ejecutarlo

### Con Docker (recomendado)

```bash
git clone https://github.com/ValestDev/task-manager.git
cd task-manager
cp .env.example .env
```

Edita `.env` y completa `JWT_SECRET` (una cadena aleatoria de al menos 32 caracteres) y `POSTGRES_PASSWORD`.

```bash
docker-compose up --build
```

Listo. Accede a:
- **App**: http://localhost:5173
- **API**: http://localhost:5169
- **Correos de prueba (Mailhog)**: http://localhost:8025

Al iniciar por primera vez, el sistema crea automáticamente un usuario administrador con una contraseña aleatoria — revisa Mailhog o los logs del contenedor `backend` para obtenerla.

### Desarrollo local (sin Docker)

<details>
<summary>Ver instrucciones</summary>

**Requisitos**: .NET 8+, Node.js 18+, PostgreSQL

```bash
# Backend
cd src/API
dotnet ef database update --project ../Infrastructure --startup-project .
dotnet run

# Frontend (en otra terminal)
cd frontend
npm install
npm run dev
```
</details>

## 📁 Colección de Postman

Incluida en [`/postman`](./postman) — impórtala para probar todos los endpoints de la API.

## 🔒 Decisiones de seguridad destacadas

- Contraseñas hasheadas con BCrypt (nunca en texto plano)
- JWT firmado con HMAC-SHA256, access tokens de vida corta (15 min) + refresh tokens con rotación
- Mismo mensaje de error para credenciales inválidas y usuarios inexistentes, previniendo enumeración de cuentas
- Middleware global de manejo de excepciones que nunca expone stack traces al cliente
- Secretos gestionados vía variables de entorno, nunca hardcodeados
- Permisos a nivel de recurso (una tarea solo la puede editar su creador, el usuario asignado, o un administrador)


