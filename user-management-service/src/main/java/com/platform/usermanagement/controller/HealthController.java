package com.platform.usermanagement.controller;

import lombok.RequiredArgsConstructor;
import lombok.extern.slf4j.Slf4j;
import org.springframework.http.ResponseEntity;
import org.springframework.web.bind.annotation.GetMapping;
import org.springframework.web.bind.annotation.RestController;

import java.time.LocalDateTime;
import java.util.HashMap;
import java.util.Map;

/**
 * REST controller for health check endpoint.
 * Provides service health status for monitoring.
 */
@RestController
@RequiredArgsConstructor
@Slf4j
public class HealthController {

    /**
     * Health check endpoint.
     * GET /health
     *
     * @return health status
     */
    @GetMapping("/health")
    public ResponseEntity<Map<String, Object>> health() {
        log.debug("GET /health");
        Map<String, Object> health = new HashMap<>();
        health.put("status", "UP");
        health.put("service", "user-management-service");
        health.put("timestamp", LocalDateTime.now().toString());
        return ResponseEntity.ok(health);
    }
}
