package com.platform.usermanagement.repository;

import com.platform.usermanagement.entity.User;
import org.springframework.data.domain.Page;
import org.springframework.data.domain.Pageable;
import org.springframework.data.jpa.repository.JpaRepository;
import org.springframework.stereotype.Repository;

import java.util.Optional;
import java.util.UUID;

/**
 * Repository for User entity.
 * Provides CRUD operations and custom queries for PostgreSQL.
 */
@Repository
public interface UserRepository extends JpaRepository<User, UUID> {

    /**
     * Find a user by email address.
     *
     * @param email the email address
     * @return Optional containing the user if found
     */
    Optional<User> findByEmail(String email);

    /**
     * Check if a user exists with the given email.
     *
     * @param email the email address
     * @return true if user exists
     */
    boolean existsByEmail(String email);

    /**
     * Find all active users with pagination.
     *
     * @param active the active status
     * @param pageable pagination parameters
     * @return Page of users
     */
    Page<User> findByActive(boolean active, Pageable pageable);

    /**
     * Find users by role with pagination.
     *
     * @param role the user role
     * @param pageable pagination parameters
     * @return Page of users
     */
    Page<User> findByRole(String role, Pageable pageable);
}
