#!/bin/sh
echo "Waiting for RabbitMQ Management API..."

while true; do
  curl -s -u taskflow:${RABBITMQ_PASSWORD:-taskflow123} \
    http://rabbitmq:15672/api/health/checks/alarms > /dev/null 2>&1
  if [ $? -eq 0 ]; then
    echo "RabbitMQ Management API is ready"
    break
  fi
  echo "Still waiting..."
  sleep 2
done

echo "Creating exchange..."
curl -u taskflow:${RABBITMQ_PASSWORD:-taskflow123} \
  -X PUT \
  -H "Content-Type: application/json" \
  -d '{"type":"topic","durable":true}' \
  http://rabbitmq:15672/api/exchanges/%2F/taskflow.events

echo "Exchange 'taskflow.events' created"
