package com.platform.usermanagement.service;

import com.platform.usermanagement.dto.LoginRequest;
import com.platform.usermanagement.dto.LoginResponse;
import com.platform.usermanagement.dto.UserDTO;
import com.platform.usermanagement.entity.User;
import com.platform.usermanagement.exception.InvalidCredentialsException;
import com.platform.usermanagement.mapper.UserMapper;
import com.platform.usermanagement.repository.UserRepository;
import com.platform.usermanagement.security.JwtTokenProvider;
import lombok.RequiredArgsConstructor;
import lombok.extern.slf4j.Slf4j;
import org.springframework.security.crypto.password.PasswordEncoder;
import org.springframework.stereotype.Service;

/**
 * Service for handling user authentication.
 */
@Service
@RequiredArgsConstructor
@Slf4j
public class AuthenticationService {

    private final UserRepository userRepository;
    private final PasswordEncoder passwordEncoder;
    private final JwtTokenProvider jwtTokenProvider;
    private final UserMapper userMapper;
    private final TokenCacheService tokenCacheService;

    /**
     * Authenticate a user and return a JWT token.
     *
     * @param request the login request
     * @return login response with JWT token
     */
    public LoginResponse login(LoginRequest request) {
        log.info("Login attempt for email: {}", request.getEmail());

        User user = userRepository.findByEmail(request.getEmail())
                .orElseThrow(InvalidCredentialsException::new);

        if (!passwordEncoder.matches(request.getPassword(), user.getPassword())) {
            log.warn("Invalid password for email: {}", request.getEmail());
            throw new InvalidCredentialsException();
        }

        if (!user.isActive()) {
            log.warn("Login attempt for inactive user: {}", request.getEmail());
            throw new InvalidCredentialsException("Account is deactivated");
        }

        String token = jwtTokenProvider.generateToken(user);
        long expiresIn = jwtTokenProvider.getExpirationTime();

        // Cache the token in Redis
        tokenCacheService.cacheToken(user.getId(), token, expiresIn);

        UserDTO userDTO = userMapper.toDTO(user);

        log.info("Login successful for user: {}", user.getId());

        return LoginResponse.builder()
                .accessToken(token)
                .tokenType("Bearer")
                .expiresIn(expiresIn)
                .user(userDTO)
                .build();
    }

    /**
     * Logout a user by invalidating their token.
     *
     * @param token the JWT token to invalidate
     */
    public void logout(String token) {
        String userId = jwtTokenProvider.getUserIdFromToken(token);
        tokenCacheService.invalidateToken(userId);
        log.info("User logged out: {}", userId);
    }
}
