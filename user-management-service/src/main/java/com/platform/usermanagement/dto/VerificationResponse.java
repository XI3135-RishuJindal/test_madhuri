package com.platform.usermanagement.dto;

import lombok.AllArgsConstructor;
import lombok.Builder;
import lombok.Data;
import lombok.NoArgsConstructor;

/**
 * Response DTO for email verification.
 */
@Data
@Builder
@NoArgsConstructor
@AllArgsConstructor
public class VerificationResponse {

    private boolean verified;
    private String message;
}
