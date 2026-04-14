package com.platform.usermanagement.exception;

/**
 * Exception thrown when a verification code is invalid or expired.
 */
public class InvalidVerificationCodeException extends RuntimeException {

    public InvalidVerificationCodeException() {
        super("Invalid or expired verification code");
    }

    public InvalidVerificationCodeException(String message) {
        super(message);
    }
}
