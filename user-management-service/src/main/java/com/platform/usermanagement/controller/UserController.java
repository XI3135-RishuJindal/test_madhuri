package com.platform.usermanagement.controller;

import com.platform.usermanagement.dto.*;
import com.platform.usermanagement.service.UserManagementService;
import jakarta.validation.Valid;
import lombok.RequiredArgsConstructor;
import lombok.extern.slf4j.Slf4j;
import org.springframework.http.HttpStatus;
import org.springframework.http.ResponseEntity;
import org.springframework.web.bind.annotation.*;

import java.util.UUID;

/**
 * REST controller for user management operations.
 * Provides endpoints for CRUD operations on users.
 */
@RestController
@RequestMapping("/api/v1/users")
@RequiredArgsConstructor
@Slf4j
public class UserController {

    private final UserManagementService userManagementService;

    /**
     * Get all users with pagination.
     * GET /api/v1/users
     *
     * @param page the page number (0-indexed, default 0)
     * @param size the page size (default 20)
     * @return paginated list of users
     */
    @GetMapping
    public ResponseEntity<PaginatedResponse<UserDTO>> getAllUsers(
            @RequestParam(defaultValue = "0") int page,
            @RequestParam(defaultValue = "20") int size) {
        log.info("GET /api/v1/users - page: {}, size: {}", page, size);
        PaginatedResponse<UserDTO> response = userManagementService.getAllUsers(page, size);
        return ResponseEntity.ok(response);
    }

    /**
     * Get a user by ID.
     * GET /api/v1/users/{id}
     *
     * @param id the user ID
     * @return the user details
     */
    @GetMapping("/{id}")
    public ResponseEntity<UserDTO> getUserById(@PathVariable UUID id) {
        log.info("GET /api/v1/users/{}", id);
        UserDTO user = userManagementService.getUserById(id);
        return ResponseEntity.ok(user);
    }

    /**
     * Create a new user.
     * POST /api/v1/users
     *
     * @param request the create user request
     * @return the created user
     */
    @PostMapping
    public ResponseEntity<UserDTO> createUser(@Valid @RequestBody CreateUserRequest request) {
        log.info("POST /api/v1/users - email: {}", request.getEmail());
        UserDTO user = userManagementService.createUser(request);
        return ResponseEntity.status(HttpStatus.CREATED).body(user);
    }

    /**
     * Update an existing user.
     * PUT /api/v1/users/{id}
     *
     * @param id the user ID
     * @param request the update request
     * @return the updated user
     */
    @PutMapping("/{id}")
    public ResponseEntity<UserDTO> updateUser(
            @PathVariable UUID id,
            @Valid @RequestBody UpdateUserRequest request) {
        log.info("PUT /api/v1/users/{}", id);
        UserDTO user = userManagementService.updateUser(id, request);
        return ResponseEntity.ok(user);
    }

    /**
     * Delete a user.
     * DELETE /api/v1/users/{id}
     *
     * @param id the user ID
     * @return no content
     */
    @DeleteMapping("/{id}")
    public ResponseEntity<Void> deleteUser(@PathVariable UUID id) {
        log.info("DELETE /api/v1/users/{}", id);
        userManagementService.deleteUser(id);
        return ResponseEntity.noContent().build();
    }

    /**
     * Verify a user's email.
     * POST /api/v1/users/verify
     *
     * @param request the verification request
     * @return verification response
     */
    @PostMapping("/verify")
    public ResponseEntity<VerificationResponse> verifyUser(@Valid @RequestBody VerificationRequest request) {
        log.info("POST /api/v1/users/verify - userId: {}", request.getUserId());
        VerificationResponse response = userManagementService.verifyUser(request);
        return ResponseEntity.ok(response);
    }
}
