package com.platform.usermanagement.service;

import com.platform.usermanagement.event.UserCreatedEvent;
import lombok.RequiredArgsConstructor;
import lombok.extern.slf4j.Slf4j;
import org.springframework.beans.factory.annotation.Value;
import org.springframework.kafka.core.KafkaTemplate;
import org.springframework.stereotype.Service;

/**
 * Service for publishing events to Kafka.
 */
@Service
@RequiredArgsConstructor
@Slf4j
public class EventPublisherService {

    private static final String USER_EVENTS_TOPIC = "user-events";

    private final KafkaTemplate<String, Object> kafkaTemplate;

    @Value("${spring.kafka.enabled:false}")
    private boolean kafkaEnabled;

    /**
     * Publish a UserCreatedEvent to Kafka.
     *
     * @param event the user created event
     */
    public void publishUserCreatedEvent(UserCreatedEvent event) {
        if (!kafkaEnabled) {
            log.debug("Kafka is disabled, skipping event publishing for user: {}", event.getUserId());
            return;
        }

        try {
            kafkaTemplate.send(USER_EVENTS_TOPIC, event.getUserId().toString(), event);
            log.info("Published UserCreatedEvent for user: {}", event.getUserId());
        } catch (Exception e) {
            // TODO: Implement retry mechanism or dead letter queue
            log.error("Failed to publish UserCreatedEvent for user: {}", event.getUserId(), e);
        }
    }
}
