package com.platform.usermanagement.service;

import lombok.RequiredArgsConstructor;
import lombok.extern.slf4j.Slf4j;
import org.springframework.data.redis.core.RedisTemplate;
import org.springframework.stereotype.Service;

import java.security.SecureRandom;
import java.time.Duration;
import java.util.UUID;

/**
 * Service for handling user verification codes.
 * Uses Redis for storing verification codes with TTL.
 */
@Service
@RequiredArgsConstructor
@Slf4j
public class UserVerificationService {

    private static final String VERIFICATION_CODE_PREFIX = "verification:";
    private static final Duration CODE_EXPIRATION = Duration.ofMinutes(15);
    private static final int CODE_LENGTH = 6;

    private final RedisTemplate<String, String> redisTemplate;
    private final SecureRandom secureRandom = new SecureRandom();

    /**
     * Generate and send a verification code for a user.
     *
     * @param userId the user ID
     * @param email the user's email address
     * @return the generated verification code
     */
    public String sendVerificationCode(UUID userId, String email) {
        String code = generateVerificationCode();
        String key = VERIFICATION_CODE_PREFIX + userId.toString();

        redisTemplate.opsForValue().set(key, code, CODE_EXPIRATION);
        log.info("Verification code generated for user: {}", userId);

        // TODO: Integrate with email service to send the verification code
        // For now, we just log it (in production, this should be sent via email)
        log.debug("Verification code for {}: {}", email, code);

        return code;
    }

    /**
     * Validate a verification code for a user.
     *
     * @param userId the user ID
     * @param code the verification code to validate
     * @return true if the code is valid
     */
    public boolean validateVerificationCode(UUID userId, String code) {
        String key = VERIFICATION_CODE_PREFIX + userId.toString();
        String storedCode = redisTemplate.opsForValue().get(key);

        if (storedCode != null && storedCode.equals(code)) {
            // Delete the code after successful validation
            redisTemplate.delete(key);
            log.info("Verification code validated successfully for user: {}", userId);
            return true;
        }

        log.warn("Invalid verification code attempt for user: {}", userId);
        return false;
    }

    /**
     * Resend a verification code for a user.
     *
     * @param userId the user ID
     * @param email the user's email address
     * @return the new verification code
     */
    public String resendVerificationCode(UUID userId, String email) {
        // Delete any existing code
        String key = VERIFICATION_CODE_PREFIX + userId.toString();
        redisTemplate.delete(key);

        // Generate and send new code
        return sendVerificationCode(userId, email);
    }

    /**
     * Generate a random numeric verification code.
     *
     * @return the generated code
     */
    private String generateVerificationCode() {
        StringBuilder code = new StringBuilder();
        for (int i = 0; i < CODE_LENGTH; i++) {
            code.append(secureRandom.nextInt(10));
        }
        return code.toString();
    }
}
