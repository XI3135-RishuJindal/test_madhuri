package com.platform.usermanagement.controller;

import com.platform.usermanagement.dto.LoginRequest;
import com.platform.usermanagement.dto.LoginResponse;
import com.platform.usermanagement.service.AuthenticationService;
import jakarta.validation.Valid;
import lombok.RequiredArgsConstructor;
import lombok.extern.slf4j.Slf4j;
import org.springframework.http.ResponseEntity;
import org.springframework.web.bind.annotation.*;

/**
 * REST controller for authentication operations.
 * Provides endpoints for login and logout.
 */
@RestController
@RequestMapping("/api/v1/auth")
@RequiredArgsConstructor
@Slf4j
public class AuthController {

    private final AuthenticationService authenticationService;

    /**
     * Authenticate a user and return a JWT token.
     * POST /api/v1/auth/login
     *
     * @param request the login request
     * @return login response with JWT token
     */
    @PostMapping("/login")
    public ResponseEntity<LoginResponse> login(@Valid @RequestBody LoginRequest request) {
        log.info("POST /api/v1/auth/login - email: {}", request.getEmail());
        LoginResponse response = authenticationService.login(request);
        return ResponseEntity.ok(response);
    }

    /**
     * Logout a user by invalidating their token.
     * POST /api/v1/auth/logout
     *
     * @param authHeader the Authorization header containing the Bearer token
     * @return no content
     */
    @PostMapping("/logout")
    public ResponseEntity<Void> logout(@RequestHeader("Authorization") String authHeader) {
        log.info("POST /api/v1/auth/logout");
        if (authHeader != null && authHeader.startsWith("Bearer ")) {
            String token = authHeader.substring(7);
            authenticationService.logout(token);
        }
        return ResponseEntity.noContent().build();
    }
}
