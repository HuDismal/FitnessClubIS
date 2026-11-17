-- ============================================================
-- Информационная система фитнес-клуба «Атлет»
-- СУБД: PostgreSQL 16
-- Скрипт создания структуры базы данных
-- ============================================================

DROP TABLE IF EXISTS payment, booking, client_membership, membership_type,
    training_session, trainer, workout_type,
    client, account, account_type CASCADE;

-- ---------- Аутентификация ----------
CREATE TABLE account_type (
    id    SERIAL PRIMARY KEY,
    name  VARCHAR(50) NOT NULL UNIQUE
);

CREATE TABLE account (
    id               SERIAL PRIMARY KEY,
    login            VARCHAR(50)  NOT NULL UNIQUE,
    password_hash    CHAR(64)     NOT NULL,            -- SHA-256 в hex
    last_name        VARCHAR(50)  NOT NULL,
    first_name       VARCHAR(50)  NOT NULL,
    account_type_id  INTEGER      NOT NULL REFERENCES account_type(id)
);

-- ---------- Клиенты ----------
CREATE TABLE client (
    id             SERIAL PRIMARY KEY,
    last_name      VARCHAR(50)  NOT NULL,
    first_name     VARCHAR(50)  NOT NULL,
    patronymic     VARCHAR(50),
    birth_date     DATE         NOT NULL CHECK (birth_date < CURRENT_DATE),
    phone          VARCHAR(18)  NOT NULL UNIQUE,
    email          VARCHAR(100) UNIQUE,
    registered_at  DATE         NOT NULL DEFAULT CURRENT_DATE
);

-- ---------- Справочники ----------
CREATE TABLE workout_type (
    id           SERIAL PRIMARY KEY,
    name         VARCHAR(60)  NOT NULL UNIQUE,
    description  VARCHAR(300)
);

CREATE TABLE membership_type (
    id             SERIAL PRIMARY KEY,
    name           VARCHAR(60)    NOT NULL UNIQUE,
    duration_days  INTEGER        NOT NULL CHECK (duration_days > 0),
    visits_limit   INTEGER        CHECK (visits_limit > 0),   -- NULL = безлимит
    price          NUMERIC(10,2)  NOT NULL CHECK (price >= 0)
);

-- ---------- Тренеры ----------
CREATE TABLE trainer (
    id          SERIAL PRIMARY KEY,
    last_name   VARCHAR(50) NOT NULL,
    first_name  VARCHAR(50) NOT NULL,
    patronymic  VARCHAR(50),
    phone       VARCHAR(18) NOT NULL UNIQUE,
    hired_at    DATE        NOT NULL
);

-- ---------- Расписание ----------
CREATE TABLE training_session (
    id               SERIAL PRIMARY KEY,
    workout_type_id  INTEGER     NOT NULL REFERENCES workout_type(id),
    trainer_id       INTEGER     NOT NULL REFERENCES trainer(id),
    starts_at        TIMESTAMP   NOT NULL,
    duration_min     INTEGER     NOT NULL CHECK (duration_min BETWEEN 15 AND 180),
    max_participants INTEGER     NOT NULL CHECK (max_participants > 0)
);

-- ---------- Абонементы и оплата ----------
CREATE TABLE client_membership (
    id                  SERIAL PRIMARY KEY,
    client_id           INTEGER NOT NULL REFERENCES client(id) ON DELETE CASCADE,
    membership_type_id  INTEGER NOT NULL REFERENCES membership_type(id),
    start_date          DATE    NOT NULL,
    end_date            DATE    NOT NULL,
    visits_left         INTEGER CHECK (visits_left >= 0),
    CHECK (end_date > start_date)
);

CREATE TABLE payment (
    id                    SERIAL PRIMARY KEY,
    client_membership_id  INTEGER       NOT NULL REFERENCES client_membership(id) ON DELETE CASCADE,
    amount                NUMERIC(10,2) NOT NULL CHECK (amount > 0),
    paid_at               TIMESTAMP     NOT NULL DEFAULT now(),
    method                VARCHAR(20)   NOT NULL CHECK (method IN ('Наличные','Карта','СБП'))
);

-- ---------- Запись на занятия (клиент ↔ занятие, M:N) ----------
CREATE TABLE booking (
    id          SERIAL PRIMARY KEY,
    session_id  INTEGER NOT NULL REFERENCES training_session(id) ON DELETE CASCADE,
    client_id   INTEGER NOT NULL REFERENCES client(id)           ON DELETE CASCADE,
    attended    BOOLEAN,                           -- NULL = занятие ещё не прошло
    UNIQUE (session_id, client_id)
);

-- ---------- Индексы под частые запросы ----------
CREATE INDEX ix_client_last_name        ON client (last_name);
CREATE INDEX ix_session_starts_at       ON training_session (starts_at);
CREATE INDEX ix_client_membership_client ON client_membership (client_id);
CREATE INDEX ix_payment_paid_at         ON payment (paid_at);
