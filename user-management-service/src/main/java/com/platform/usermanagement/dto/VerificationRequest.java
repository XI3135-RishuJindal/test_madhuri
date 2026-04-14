package com.platform.usermanagement.dto;

import jakarta.validation.constraints.NotBlank;
import lombok.AllArgsConstructor;
import lombok.Builder;
import lombok.Data;
import lombok.NoArgsConstructor;

import java.util.UUID;

/**
 * Request DTO for email verification.
 */
@Data
@Builder
@NoArgsConstructor
@AllArgsConstructor
public class VerificationRequest {

    @NotBlank(message = "User ID is required")
    private UUID userId;

    @NotBlank(message = "Verification code is required")
    private String verificationCode;
}
