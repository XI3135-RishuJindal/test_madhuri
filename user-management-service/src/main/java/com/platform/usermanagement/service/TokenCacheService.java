package com.platform.usermanagement.service;

import lombok.RequiredArgsConstructor;
import lombok.extern.slf4j.Slf4j;
import org.springframework.data.redis.core.RedisTemplate;
import org.springframework.stereotype.Service;

import java.time.Duration;
import java.util.UUID;

/**
 * Service for caching JWT tokens in Redis.
 * Provides token storage and validation for session management.
 */
@Service
@RequiredArgsConstructor
@Slf4j
public class TokenCacheService {

    private static final String TOKEN_PREFIX = "token:";
    private static final String BLACKLIST_PREFIX = "blacklist:";

    private final RedisTemplate<String, String> redisTemplate;

    /**
     * Cache a JWT token for a user.
     *
     * @param userId the user ID
     * @param token the JWT token
     * @param expiresInMillis token expiration time in milliseconds
     */
    public void cacheToken(UUID userId, String token, long expiresInMillis) {
        String key = TOKEN_PREFIX + userId.toString();
        redisTemplate.opsForValue().set(key, token, Duration.ofMillis(expiresInMillis));
        log.debug("Token cached for user: {}", userId);
    }

    /**
     * Get the cached token for a user.
     *
     * @param userId the user ID
     * @return the cached token or null if not found
     */
    public String getCachedToken(UUID userId) {
        String key = TOKEN_PREFIX + userId.toString();
        return redisTemplate.opsForValue().get(key);
    }

    /**
     * Invalidate a user's token (logout).
     *
     * @param userId the user ID
     */
    public void invalidateToken(String userId) {
        String key = TOKEN_PREFIX + userId;
        redisTemplate.delete(key);
        log.debug("Token invalidated for user: {}", userId);
    }

    /**
     * Add a token to the blacklist.
     *
     * @param token the token to blacklist
     * @param expiresInMillis time until the token would have expired
     */
    public void blacklistToken(String token, long expiresInMillis) {
        String key = BLACKLIST_PREFIX + token;
        redisTemplate.opsForValue().set(key, "blacklisted", Duration.ofMillis(expiresInMillis));
        log.debug("Token blacklisted");
    }

    /**
     * Check if a token is blacklisted.
     *
     * @param token the token to check
     * @return true if the token is blacklisted
     */
    public boolean isTokenBlacklisted(String token) {
        String key = BLACKLIST_PREFIX + token;
        return Boolean.TRUE.equals(redisTemplate.hasKey(key));
    }
}
