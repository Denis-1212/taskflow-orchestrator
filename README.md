# TaskFlow Orchestrator

Распределенная система управления проектами и задачами, построенная на микросервисной архитектуре.

## Возможности

- Управление пользователями и аутентификация (JWT)
- Создание проектов и управление участниками
- Создание задач, назначение исполнителей, отслеживание статусов
- Уведомления о назначении задач (in-app)
- Аудит действий пользователей
- API Gateway с маршрутизацией и rate limiting
- Асинхронное взаимодействие через RabbitMQ с гарантированной доставкой (Outbox pattern)
- Синхронное взаимодействие через gRPC
- Наблюдаемость: логи (Seq), метрики (Prometheus + Grafana), трассировка (Jaeger)

## Архитектура
```
┌───────────────────────────────────────────────────┐
│                 API Gateway (YARP)                │
│                    Port: 8080                     │
└───────────────────────────────────────────────────┘
                         │
     ┌───────────────────┼─────────────────────┐
     ▼                   ▼                     ▼
┌───────────────┐ ┌───────────────┐ ┌───────────────┐
│ Auth Service  │ │Project Service│ │ Task Service  │
│ Port: 5001    │ │ Port: 5002    │ │ Port: 5003    │
└───────────────┘ └───────────────┘ └───────────────┘
     │                   │                     │
     ▼                   ▼                     ▼
┌───────────────┐ ┌───────────────┐ ┌───────────────┐
│Notification   │ │ Audit Service │ │ PostgreSQL    │
│ Port: 5004    │ │ Port: 5005    │ │ (5 БД)        │
└───────────────┘ └───────────────┘ └───────────────┘
```

### Сервисы

| Сервис | Порт | Назначение |
|--------|------|------------|
| **API Gateway** | 8080 | Единая точка входа, JWT аутентификация, маршрутизация |
| **Auth Service** | 5001 | Регистрация, логин, JWT, refresh tokens (Redis) |
| **Project Service** | 5002 | CRUD проектов, управление участниками |
| **Task Service** | 5003 | CRUD задач, статусы, назначения, outbox |
| **Notification Service** | 5004 | Уведомления (in-app, email) |
| **Audit Service** | 5005 | Логирование действий |

### Инфраструктура

| Компонент | Назначение |
|-----------|------------|
| **PostgreSQL** | Реляционная БД (отдельный инстанс на сервис) |
| **Redis** | Хранение refresh tokens, кэш, дедубликация |
| **RabbitMQ** | Асинхронные события (уведомления, аудит) |
| **Seq** | Централизованное логирование |
| **Jaeger** | Распределенная трассировка |
| **Prometheus + Grafana** | Мониторинг и метрики |

### Взаимодействие

- **Синхронное (gRPC)** — проверки существования, валидация участников
- **Асинхронное (RabbitMQ)** — уведомления, аудит, email
- **REST API (Gateway)** — внешние клиенты



**Коммуникация:**
- Синхронная: gRPC (Auth → Project, Task → Auth/Project)
- Асинхронная: RabbitMQ (Task → Notification, Task → Audit)



## Быстрый старт

### Предварительные требования

- Docker 24.0+
- Docker Compose 2.20+
- .NET 10 SDK (для локальной разработки)

### Запуск

# .env
JWT_SECRET=your-super-secret-key-at-least-32-characters-long
POSTGRES_PASSWORD=taskflow123
RABBITMQ_PASSWORD=taskflow123
REDIS_PASSWORD=taskflow123

# Запустить инфраструктуру (БД, Redis, RabbitMQ, мониторинг)
docker-compose -f docker-compose.infrastructure.yml up -d

# Запустить все сервисы
docker-compose up -d

# Проверить статус
docker-compose ps

# Доступ к сервисам
Сервис	URL	Назначение
- API Gateway	http://localhost:8080	Единая точка входа
- Auth Service	http://localhost:5001/swagger	Аутентификация
- Project Service	http://localhost:5002/swagger	Управление проектами
- Task Service	http://localhost:5003/swagger	Управление задачами
- Notification Service	http://localhost:5004/swagger	Уведомления
- Audit Service	http://localhost:5005/swagger	Аудит
- Seq	http://localhost:5341	Логи
- Jaeger	http://localhost:16686	Трассировка
- Prometheus	http://localhost:9090	Метрики
- Grafana	http://localhost:3000	Дашборды (admin/admin123)
- RabbitMQ	http://localhost:15672	Очереди (taskflow/taskflow123)

