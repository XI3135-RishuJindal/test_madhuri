package com.platform.usermanagement.service;

import com.platform.usermanagement.dto.LoginRequest;
import com.platform.usermanagement.dto.LoginResponse;
import com.platform.usermanagement.dto.UserDTO;
import com.platform.usermanagement.entity.User;
import com.platform.usermanagement.exception.InvalidCredentialsException;
import com.platform.usermanagement.mapper.UserMapper;
import com.platform.usermanagement.repository.UserRepository;
import com.platform.usermanagement.security.JwtTokenProvider;
import org.junit.jupiter.api.BeforeEach;
import org.junit.jupiter.api.Test;
import org.junit.jupiter.api.extension.ExtendWith;
import org.mockito.InjectMocks;
import org.mockito.Mock;
import org.mockito.junit.jupiter.MockitoExtension;
import org.springframework.security.crypto.password.PasswordEncoder;

import java.time.LocalDateTime;
import java.util.Optional;
import java.util.UUID;

import static org.junit.jupiter.api.Assertions.*;
import static org.mockito.ArgumentMatchers.any;
import static org.mockito.ArgumentMatchers.anyString;
import static org.mockito.Mockito.*;

@ExtendWith(MockitoExtension.class)
class AuthenticationServiceTest {

    @Mock
    private UserRepository userRepository;

    @Mock
    private PasswordEncoder passwordEncoder;

    @Mock
    private JwtTokenProvider jwtTokenProvider;

    @Mock
    private UserMapper userMapper;

    @Mock
    private TokenCacheService tokenCacheService;

    @InjectMocks
    private AuthenticationService authenticationService;

    private User testUser;
    private UserDTO testUserDTO;
    private LoginRequest loginRequest;
    private UUID userId;

    @BeforeEach
    void setUp() {
        userId = UUID.randomUUID();
        testUser = User.builder()
                .id(userId)
                .name("Test User")
                .email("test@example.com")
                .password("encodedPassword")
                .role("USER")
                .emailVerified(true)
                .active(true)
                .createdAt(LocalDateTime.now())
                .build();

        testUserDTO = UserDTO.builder()
                .id(userId)
                .name("Test User")
                .email("test@example.com")
                .role("USER")
                .emailVerified(true)
                .active(true)
                .createdAt(testUser.getCreatedAt())
                .build();

        loginRequest = LoginRequest.builder()
                .email("test@example.com")
                .password("password123")
                .build();
    }

    @Test
    void login_Success() {
        when(userRepository.findByEmail(anyString())).thenReturn(Optional.of(testUser));
        when(passwordEncoder.matches(anyString(), anyString())).thenReturn(true);
        when(jwtTokenProvider.generateToken(any(User.class))).thenReturn("jwt-token");
        when(jwtTokenProvider.getExpirationTime()).thenReturn(86400000L);
        when(userMapper.toDTO(any(User.class))).thenReturn(testUserDTO);

        LoginResponse result = authenticationService.login(loginRequest);

        assertNotNull(result);
        assertEquals("jwt-token", result.getAccessToken());
        assertEquals("Bearer", result.getTokenType());
        assertEquals(testUserDTO.getEmail(), result.getUser().getEmail());
        verify(tokenCacheService).cacheToken(any(UUID.class), anyString(), anyLong());
    }

    @Test
    void login_UserNotFound() {
        when(userRepository.findByEmail(anyString())).thenReturn(Optional.empty());

        assertThrows(InvalidCredentialsException.class, () -> 
            authenticationService.login(loginRequest)
        );
    }

    @Test
    void login_InvalidPassword() {
        when(userRepository.findByEmail(anyString())).thenReturn(Optional.of(testUser));
        when(passwordEncoder.matches(anyString(), anyString())).thenReturn(false);

        assertThrows(InvalidCredentialsException.class, () -> 
            authenticationService.login(loginRequest)
        );
    }

    @Test
    void login_InactiveUser() {
        testUser.setActive(false);
        when(userRepository.findByEmail(anyString())).thenReturn(Optional.of(testUser));
        when(passwordEncoder.matches(anyString(), anyString())).thenReturn(true);

        assertThrows(InvalidCredentialsException.class, () -> 
            authenticationService.login(loginRequest)
        );
    }

    @Test
    void logout_Success() {
        String token = "jwt-token";
        when(jwtTokenProvider.getUserIdFromToken(token)).thenReturn(userId.toString());

        authenticationService.logout(token);

        verify(tokenCacheService).invalidateToken(userId.toString());
    }
}
