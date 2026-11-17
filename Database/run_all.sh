#!/usr/bin/env bash
# Консольное развёртывание: ./run_all.sh <host> <port> <user>
# В pgAdmin то же самое делается в Query Tool: открыть fitness_club_full.sql и нажать F5.
set -e
H=${1:-localhost}; P=${2:-5432}; U=${3:-postgres}
psql -h $H -p $P -U $U -c "DROP DATABASE IF EXISTS fitness_club" -c "CREATE DATABASE fitness_club ENCODING 'UTF8'"
psql -h $H -p $P -U $U -d fitness_club -v ON_ERROR_STOP=1 -q -f fitness_club_full.sql
