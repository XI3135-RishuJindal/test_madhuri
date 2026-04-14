package com.platform.usermanagement.mapper;

import com.platform.usermanagement.dto.CreateUserRequest;
import com.platform.usermanagement.dto.UserDTO;
import com.platform.usermanagement.entity.User;
import org.springframework.stereotype.Component;

/**
 * Mapper for converting between User entity and DTOs.
 */
@Component
public class UserMapper {

    /**
     * Convert User entity to UserDTO.
     *
     * @param user the user entity
     * @return UserDTO
     */
    public UserDTO toDTO(User user) {
        if (user == null) {
            return null;
        }
        return UserDTO.builder()
                .id(user.getId())
                .name(user.getName())
                .email(user.getEmail())
                .role(user.getRole())
                .emailVerified(user.isEmailVerified())
                .active(user.isActive())
                .createdAt(user.getCreatedAt())
                .updatedAt(user.getUpdatedAt())
                .build();
    }

    /**
     * Convert CreateUserRequest to User entity.
     *
     * @param request the create user request
     * @param encodedPassword the encoded password
     * @return User entity
     */
    public User toEntity(CreateUserRequest request, String encodedPassword) {
        if (request == null) {
            return null;
        }
        return User.builder()
                .name(request.getName())
                .email(request.getEmail())
                .password(encodedPassword)
                .role(request.getRole() != null ? request.getRole() : "USER")
                .emailVerified(false)
                .active(true)
                .build();
    }
}
