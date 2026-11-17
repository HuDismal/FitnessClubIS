# Атлет — информационная система фитнес-клуба

Учебный проект по ПМ.07 «Соадминистрирование и автоматизация баз данных и серверов».
Предметная область: фитнес-клуб (клиенты, абонементы, платежи, тренеры, расписание, записи на занятия).

* СУБД: PostgreSQL 16 (`Database/`)
* Клиент: WPF (.NET 8, MVVM, EF Core + Npgsql, LiveCharts2, Word Interop) — каталог `FitnessClubIS/`
* Документация: `Docs/` (ERD, руководство по стилю, логотип, журналы выполнения)

## Развёртывание базы данных
Вся база (структура и данные) находится в одном файле `Database/fitness_club_full.sql`.

В pgAdmin: создать пустую базу `fitness_club` (UTF8) → Tools → Query Tool → открыть файл → F5.

Из консоли:
```bash
cd Database
./run_all.sh localhost 5432 postgres
```
`01_schema.sql` — только структура (без данных).

## Тестовые учётные записи
| Логин | Пароль | Роль |
|---|---|---|
| admin | Admin#2026 | Администратор |
| manager | Manager#2026 | Менеджер |
| trainer | Trainer#2026 | Тренер |

Строка подключения задаётся переменной окружения `FITNESS_DB`
(по умолчанию `Host=localhost;Port=5432;Database=fitness_club;Username=postgres;Password=postgres`).
