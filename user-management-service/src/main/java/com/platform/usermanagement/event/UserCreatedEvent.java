package com.platform.usermanagement.event;

import lombok.AllArgsConstructor;
import lombok.Builder;
import lombok.Data;
import lombok.NoArgsConstructor;

import java.time.LocalDateTime;
import java.util.UUID;

/**
 * Event published when a new user is created.
 * Published to Kafka for analytics and downstream processing.
 */
@Data
@Builder
@NoArgsConstructor
@AllArgsConstructor
public class UserCreatedEvent {

    private UUID userId;
    private String email;
    private String name;
    private LocalDateTime createdAt;
    private String eventType;

    public static UserCreatedEvent from(UUID userId, String email, String name, LocalDateTime createdAt) {
        return UserCreatedEvent.builder()
                .userId(userId)
                .email(email)
                .name(name)
                .createdAt(createdAt)
                .eventType("USER_CREATED")
                .build();
    }
}
