#!/usr/bin/env bash
# Сбор журналов выполнения запросов (используются в отчёте). Запуск: ./collect_logs.sh <host> <port> <user>
H=${1:-localhost}; P=${2:-5432}; U=${3:-postgres}
Q="psql -h $H -p $P -U $U -d fitness_club"; L=../Docs/logs; mkdir -p $L
$Q -c "\dt" > $L/dt.txt
$Q -c "SELECT 'account_type' AS table_name,count(*) FROM account_type UNION ALL SELECT 'account',count(*) FROM account UNION ALL SELECT 'client',count(*) FROM client UNION ALL SELECT 'trainer',count(*) FROM trainer UNION ALL SELECT 'workout_type',count(*) FROM workout_type UNION ALL SELECT 'membership_type',count(*) FROM membership_type UNION ALL SELECT 'client_membership',count(*) FROM client_membership UNION ALL SELECT 'payment',count(*) FROM payment UNION ALL SELECT 'training_session',count(*) FROM training_session UNION ALL SELECT 'booking',count(*) FROM booking" > $L/counts.txt
$Q -c "SELECT a.login, t.name AS role, left(a.password_hash,16)||'…' AS hash FROM account a JOIN account_type t ON t.id=a.account_type_id" > $L/accounts.txt
$Q -c "SELECT id,last_name,first_name,phone,registered_at FROM client ORDER BY id LIMIT 8" > $L/clients8.txt
{
echo "-- Проверка серверной части авторизации (тот же алгоритм SHA-256, что и в C#)"
for pair in "admin:Admin#2026" "manager:Manager#2026" "trainer:Trainer#2026" "admin:wrong"; do
  l=${pair%%:*}; p=${pair#*:}
  echo "-- логин $l, пароль $p"
  $Q -c "SELECT a.login, t.name AS role FROM account a JOIN account_type t ON t.id=a.account_type_id WHERE a.login='$l' AND a.password_hash = encode(sha256(convert_to('$p','UTF8')),'hex')"
done
} > $L/auth_check.log 2>&1
{
echo "-- 5.1 Поиск: фамилия, имя или телефон содержит «ов»"
$Q -c "SELECT id, last_name, first_name, phone FROM client WHERE last_name ILIKE '%ов%' OR first_name ILIKE '%ов%' OR phone ILIKE '%ов%' ORDER BY last_name LIMIT 5"
echo "-- 5.1 Фильтр: клиенты с действующим абонементом на 09.11.2026"
$Q -c "SELECT count(*) AS with_active FROM client c WHERE EXISTS (SELECT 1 FROM client_membership m WHERE m.client_id=c.id AND m.start_date<=DATE '2026-11-09' AND m.end_date>=DATE '2026-11-09')"
$Q -c "SELECT count(*) AS without_active FROM client c WHERE NOT EXISTS (SELECT 1 FROM client_membership m WHERE m.client_id=c.id AND m.start_date<=DATE '2026-11-09' AND m.end_date>=DATE '2026-11-09')"
echo "-- 5.2 Сортировка: по дате регистрации (новые сверху), первые 5"
$Q -c "SELECT id, last_name, registered_at FROM client ORDER BY registered_at DESC LIMIT 5"
echo "-- 5.2 Сортировка: по фамилии (А–Я), первые 5"
$Q -c "SELECT id, last_name, first_name FROM client ORDER BY last_name, first_name LIMIT 5"
} > $L/search_filter_sort.log 2>&1
{
echo "-- 6. Удаление клиента (в транзакции, затем откат, чтобы не потерять данные)"
$Q <<'EOF'
BEGIN;
SELECT c.id, c.last_name,
  (SELECT count(*) FROM client_membership m WHERE m.client_id=c.id) AS memberships,
  (SELECT count(*) FROM payment p JOIN client_membership m ON m.id=p.client_membership_id WHERE m.client_id=c.id) AS payments,
  (SELECT count(*) FROM booking b WHERE b.client_id=c.id) AS bookings
FROM client c WHERE c.id=5;
DELETE FROM client WHERE id=5;
SELECT (SELECT count(*) FROM client WHERE id=5) AS client_left,
       (SELECT count(*) FROM client_membership WHERE client_id=5) AS memberships_left,
       (SELECT count(*) FROM booking WHERE client_id=5) AS bookings_left;
ROLLBACK;
SELECT count(*) AS clients_after_rollback FROM client;
EOF
echo "-- 6. Защита справочников от удаления: тренер, у которого есть занятия"
$Q -c "DELETE FROM trainer WHERE id=1" 2>&1
} > $L/delete_demo.log 2>&1
{
echo "-- 7. Добавление и изменение клиента, проверка ограничений"
$Q <<'EOF'
BEGIN;
INSERT INTO client(last_name,first_name,patronymic,birth_date,phone,email)
VALUES ('Тестова','Алиса','Игоревна','2001-03-14','+7 (900) 555-44-33','alice@example.com') RETURNING id, last_name, registered_at;
UPDATE client SET phone='+7 (900) 555-44-34' WHERE email='alice@example.com' RETURNING id, last_name, phone;
INSERT INTO client(last_name,first_name,birth_date,phone) VALUES ('Дубль','Тест','2000-01-01','+7 (900) 555-44-34');
ROLLBACK;
INSERT INTO client(last_name,first_name,birth_date,phone) VALUES ('Будущий','Тест','2030-01-01','+7 (900) 000-00-00');
EOF
} > $L/add_edit_demo.log 2>&1
{
echo "-- 8. Выручка по месяцам (данные для графика)"
$Q -c "SELECT to_char(date_trunc('month',paid_at),'MM.YYYY') AS month, count(*) AS payments, sum(amount) AS revenue FROM payment WHERE paid_at >= DATE '2026-06-01' AND paid_at < DATE '2026-11-10' GROUP BY date_trunc('month',paid_at) ORDER BY date_trunc('month',paid_at)"
echo "-- 8. Посещения по направлениям (данные для круговой диаграммы)"
$Q -c "SELECT w.name AS workout, count(*) AS visits FROM booking b JOIN training_session s ON s.id=b.session_id JOIN workout_type w ON w.id=s.workout_type_id WHERE b.attended IS TRUE AND s.starts_at >= DATE '2026-09-01' AND s.starts_at < DATE '2026-11-10' GROUP BY w.name ORDER BY visits DESC"
echo "-- 8. Расписание на ближайшие дни"
$Q -c "SELECT to_char(s.starts_at,'DD.MM.YYYY HH24:MI') AS starts, w.name AS workout, t.last_name AS trainer, s.max_participants - count(b.id) AS free FROM training_session s JOIN workout_type w ON w.id=s.workout_type_id JOIN trainer t ON t.id=s.trainer_id LEFT JOIN booking b ON b.session_id=s.id WHERE s.starts_at >= DATE '2026-11-09' AND s.starts_at < DATE '2026-11-11' GROUP BY s.id,w.name,t.last_name ORDER BY s.starts_at LIMIT 6"
} > $L/report_queries.log 2>&1
