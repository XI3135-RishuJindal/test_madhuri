package com.platform.usermanagement.service;

import com.platform.usermanagement.dto.*;
import com.platform.usermanagement.entity.User;
import com.platform.usermanagement.event.UserCreatedEvent;
import com.platform.usermanagement.exception.UserAlreadyExistsException;
import com.platform.usermanagement.exception.UserNotFoundException;
import com.platform.usermanagement.mapper.UserMapper;
import com.platform.usermanagement.repository.UserRepository;
import lombok.RequiredArgsConstructor;
import lombok.extern.slf4j.Slf4j;
import org.springframework.data.domain.Page;
import org.springframework.data.domain.PageRequest;
import org.springframework.data.domain.Pageable;
import org.springframework.data.domain.Sort;
import org.springframework.security.crypto.password.PasswordEncoder;
import org.springframework.stereotype.Service;
import org.springframework.transaction.annotation.Transactional;

import java.util.List;
import java.util.UUID;
import java.util.stream.Collectors;

/**
 * Service for managing user operations.
 * Handles user CRUD operations and business logic.
 */
@Service
@RequiredArgsConstructor
@Slf4j
public class UserManagementService {

    private final UserRepository userRepository;
    private final UserMapper userMapper;
    private final PasswordEncoder passwordEncoder;
    private final EventPublisherService eventPublisherService;
    private final UserVerificationService verificationService;

    /**
     * Create a new user.
     *
     * @param request the create user request
     * @return the created user DTO
     */
    @Transactional
    public UserDTO createUser(CreateUserRequest request) {
        log.info("Creating user with email: {}", request.getEmail());

        if (userRepository.existsByEmail(request.getEmail())) {
            throw new UserAlreadyExistsException(request.getEmail());
        }

        String encodedPassword = passwordEncoder.encode(request.getPassword());
        User user = userMapper.toEntity(request, encodedPassword);
        User savedUser = userRepository.save(user);

        // Send verification code
        verificationService.sendVerificationCode(savedUser.getId(), savedUser.getEmail());

        // Publish user created event
        UserCreatedEvent event = UserCreatedEvent.from(
                savedUser.getId(),
                savedUser.getEmail(),
                savedUser.getName(),
                savedUser.getCreatedAt()
        );
        eventPublisherService.publishUserCreatedEvent(event);

        log.info("User created successfully with id: {}", savedUser.getId());
        return userMapper.toDTO(savedUser);
    }

    /**
     * Get a user by ID.
     *
     * @param id the user ID
     * @return the user DTO
     */
    @Transactional(readOnly = true)
    public UserDTO getUserById(UUID id) {
        log.debug("Fetching user with id: {}", id);
        User user = userRepository.findById(id)
                .orElseThrow(() -> new UserNotFoundException(id));
        return userMapper.toDTO(user);
    }

    /**
     * Get a user by email.
     *
     * @param email the user email
     * @return the user DTO
     */
    @Transactional(readOnly = true)
    public UserDTO getUserByEmail(String email) {
        log.debug("Fetching user with email: {}", email);
        User user = userRepository.findByEmail(email)
                .orElseThrow(() -> new UserNotFoundException(email));
        return userMapper.toDTO(user);
    }

    /**
     * Get all users with pagination.
     *
     * @param page the page number (0-indexed)
     * @param size the page size
     * @return paginated response of users
     */
    @Transactional(readOnly = true)
    public PaginatedResponse<UserDTO> getAllUsers(int page, int size) {
        log.debug("Fetching all users - page: {}, size: {}", page, size);
        Pageable pageable = PageRequest.of(page, size, Sort.by("createdAt").descending());
        Page<User> userPage = userRepository.findAll(pageable);

        List<UserDTO> users = userPage.getContent().stream()
                .map(userMapper::toDTO)
                .collect(Collectors.toList());

        Pagination pagination = Pagination.builder()
                .totalItems(userPage.getTotalElements())
                .totalPages(userPage.getTotalPages())
                .currentPage(page)
                .itemsPerPage(size)
                .build();

        return PaginatedResponse.<UserDTO>builder()
                .data(users)
                .pagination(pagination)
                .build();
    }

    /**
     * Update an existing user.
     *
     * @param id the user ID
     * @param request the update request
     * @return the updated user DTO
     */
    @Transactional
    public UserDTO updateUser(UUID id, UpdateUserRequest request) {
        log.info("Updating user with id: {}", id);

        User user = userRepository.findById(id)
                .orElseThrow(() -> new UserNotFoundException(id));

        if (request.getName() != null) {
            user.setName(request.getName());
        }
        if (request.getEmail() != null && !request.getEmail().equals(user.getEmail())) {
            if (userRepository.existsByEmail(request.getEmail())) {
                throw new UserAlreadyExistsException(request.getEmail());
            }
            user.setEmail(request.getEmail());
            user.setEmailVerified(false);
            // Send new verification code for new email
            verificationService.sendVerificationCode(user.getId(), request.getEmail());
        }
        if (request.getPassword() != null) {
            user.setPassword(passwordEncoder.encode(request.getPassword()));
        }
        if (request.getRole() != null) {
            user.setRole(request.getRole());
        }
        if (request.getActive() != null) {
            user.setActive(request.getActive());
        }

        User updatedUser = userRepository.save(user);
        log.info("User updated successfully with id: {}", id);
        return userMapper.toDTO(updatedUser);
    }

    /**
     * Delete a user by ID.
     *
     * @param id the user ID
     */
    @Transactional
    public void deleteUser(UUID id) {
        log.info("Deleting user with id: {}", id);

        if (!userRepository.existsById(id)) {
            throw new UserNotFoundException(id);
        }

        userRepository.deleteById(id);
        log.info("User deleted successfully with id: {}", id);
    }

    /**
     * Verify a user's email.
     *
     * @param request the verification request
     * @return verification response
     */
    @Transactional
    public VerificationResponse verifyUser(VerificationRequest request) {
        log.info("Verifying user with id: {}", request.getUserId());

        User user = userRepository.findById(request.getUserId())
                .orElseThrow(() -> new UserNotFoundException(request.getUserId()));

        boolean isValid = verificationService.validateVerificationCode(
                request.getUserId(),
                request.getVerificationCode()
        );

        if (isValid) {
            user.setEmailVerified(true);
            userRepository.save(user);
            log.info("User verified successfully with id: {}", request.getUserId());
            return VerificationResponse.builder()
                    .verified(true)
                    .message("Email verified successfully")
                    .build();
        }

        return VerificationResponse.builder()
                .verified(false)
                .message("Invalid or expired verification code")
                .build();
    }
}
