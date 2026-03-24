#!/bin/bash
set -e

echo "========================================="
echo "TaskFlow - Database Initialization"
echo "========================================="

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

echo "Waiting for databases to be ready..."

# Wait for all databases
$SCRIPT_DIR/wait-for-it.sh localhost:5432 -t 60
$SCRIPT_DIR/wait-for-it.sh localhost:5433 -t 60
$SCRIPT_DIR/wait-for-it.sh localhost:5434 -t 60
$SCRIPT_DIR/wait-for-it.sh localhost:5435 -t 60
$SCRIPT_DIR/wait-for-it.sh localhost:5436 -t 60

echo "All databases are ready!"
echo "Database initialization complete."
