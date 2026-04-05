#!/bin/sh
echo "Waiting for RabbitMQ..."
while ! nc -z rabbitmq 15672; do
  sleep 1
done

echo "Creating exchange..."
curl -u taskflow:${RABBITMQ_PASSWORD:-taskflow123} \
  -X PUT \
  -H "Content-Type: application/json" \
  -d '{"type":"topic","durable":true}' \
  http://rabbitmq:15672/api/exchanges/%2F/taskflow.events

echo "Exchange created"
