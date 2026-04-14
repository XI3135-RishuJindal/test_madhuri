package com.platform.usermanagement.controller;

import com.fasterxml.jackson.databind.ObjectMapper;
import com.platform.usermanagement.dto.LoginRequest;
import com.platform.usermanagement.dto.LoginResponse;
import com.platform.usermanagement.dto.UserDTO;
import com.platform.usermanagement.exception.InvalidCredentialsException;
import com.platform.usermanagement.security.JwtAuthenticationEntryPoint;
import com.platform.usermanagement.security.JwtAuthenticationFilter;
import com.platform.usermanagement.security.JwtTokenProvider;
import com.platform.usermanagement.service.AuthenticationService;
import com.platform.usermanagement.service.TokenCacheService;
import org.junit.jupiter.api.BeforeEach;
import org.junit.jupiter.api.Test;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.boot.test.autoconfigure.web.servlet.WebMvcTest;
import org.springframework.boot.test.mock.mockito.MockBean;
import org.springframework.http.MediaType;
import org.springframework.security.test.context.support.WithMockUser;
import org.springframework.test.web.servlet.MockMvc;

import java.time.LocalDateTime;
import java.util.UUID;

import static org.mockito.ArgumentMatchers.any;
import static org.mockito.Mockito.when;
import static org.springframework.security.test.web.servlet.request.SecurityMockMvcRequestPostProcessors.csrf;
import static org.springframework.test.web.servlet.request.MockMvcRequestBuilders.post;
import static org.springframework.test.web.servlet.result.MockMvcResultMatchers.*;

@WebMvcTest(AuthController.class)
class AuthControllerTest {

    @Autowired
    private MockMvc mockMvc;

    @Autowired
    private ObjectMapper objectMapper;

    @MockBean
    private AuthenticationService authenticationService;

    @MockBean
    private JwtTokenProvider jwtTokenProvider;

    @MockBean
    private TokenCacheService tokenCacheService;

    @MockBean
    private JwtAuthenticationFilter jwtAuthenticationFilter;

    @MockBean
    private JwtAuthenticationEntryPoint jwtAuthenticationEntryPoint;

    private LoginRequest loginRequest;
    private LoginResponse loginResponse;

    @BeforeEach
    void setUp() {
        UUID userId = UUID.randomUUID();

        loginRequest = LoginRequest.builder()
                .email("test@example.com")
                .password("password123")
                .build();

        UserDTO userDTO = UserDTO.builder()
                .id(userId)
                .name("Test User")
                .email("test@example.com")
                .role("USER")
                .active(true)
                .createdAt(LocalDateTime.now())
                .build();

        loginResponse = LoginResponse.builder()
                .accessToken("jwt-token")
                .tokenType("Bearer")
                .expiresIn(86400000)
                .user(userDTO)
                .build();
    }

    @Test
    void login_Success() throws Exception {
        when(authenticationService.login(any(LoginRequest.class))).thenReturn(loginResponse);

        mockMvc.perform(post("/api/v1/auth/login")
                        .with(csrf())
                        .contentType(MediaType.APPLICATION_JSON)
                        .content(objectMapper.writeValueAsString(loginRequest)))
                .andExpect(status().isOk())
                .andExpect(jsonPath("$.accessToken").value("jwt-token"))
                .andExpect(jsonPath("$.tokenType").value("Bearer"))
                .andExpect(jsonPath("$.user.email").value("test@example.com"));
    }

    @Test
    void login_InvalidCredentials() throws Exception {
        when(authenticationService.login(any(LoginRequest.class)))
                .thenThrow(new InvalidCredentialsException());

        mockMvc.perform(post("/api/v1/auth/login")
                        .with(csrf())
                        .contentType(MediaType.APPLICATION_JSON)
                        .content(objectMapper.writeValueAsString(loginRequest)))
                .andExpect(status().isUnauthorized());
    }

    @Test
    void login_ValidationError() throws Exception {
        LoginRequest invalidRequest = LoginRequest.builder()
                .email("invalid-email")
                .password("")
                .build();

        mockMvc.perform(post("/api/v1/auth/login")
                        .with(csrf())
                        .contentType(MediaType.APPLICATION_JSON)
                        .content(objectMapper.writeValueAsString(invalidRequest)))
                .andExpect(status().isBadRequest());
    }

    @Test
    @WithMockUser
    void logout_Success() throws Exception {
        mockMvc.perform(post("/api/v1/auth/logout")
                        .with(csrf())
                        .header("Authorization", "Bearer jwt-token"))
                .andExpect(status().isNoContent());
    }
}
