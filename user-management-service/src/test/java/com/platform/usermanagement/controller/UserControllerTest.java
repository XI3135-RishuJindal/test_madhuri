package com.platform.usermanagement.controller;

import com.fasterxml.jackson.databind.ObjectMapper;
import com.platform.usermanagement.dto.*;
import com.platform.usermanagement.exception.UserNotFoundException;
import com.platform.usermanagement.security.JwtAuthenticationEntryPoint;
import com.platform.usermanagement.security.JwtAuthenticationFilter;
import com.platform.usermanagement.security.JwtTokenProvider;
import com.platform.usermanagement.service.TokenCacheService;
import com.platform.usermanagement.service.UserManagementService;
import org.junit.jupiter.api.BeforeEach;
import org.junit.jupiter.api.Test;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.boot.test.autoconfigure.web.servlet.WebMvcTest;
import org.springframework.boot.test.mock.mockito.MockBean;
import org.springframework.http.MediaType;
import org.springframework.security.test.context.support.WithMockUser;
import org.springframework.test.web.servlet.MockMvc;

import java.time.LocalDateTime;
import java.util.List;
import java.util.UUID;

import static org.mockito.ArgumentMatchers.any;
import static org.mockito.ArgumentMatchers.anyInt;
import static org.mockito.Mockito.when;
import static org.springframework.security.test.web.servlet.request.SecurityMockMvcRequestPostProcessors.csrf;
import static org.springframework.test.web.servlet.request.MockMvcRequestBuilders.*;
import static org.springframework.test.web.servlet.result.MockMvcResultMatchers.*;

@WebMvcTest(UserController.class)
class UserControllerTest {

    @Autowired
    private MockMvc mockMvc;

    @Autowired
    private ObjectMapper objectMapper;

    @MockBean
    private UserManagementService userManagementService;

    @MockBean
    private JwtTokenProvider jwtTokenProvider;

    @MockBean
    private TokenCacheService tokenCacheService;

    @MockBean
    private JwtAuthenticationFilter jwtAuthenticationFilter;

    @MockBean
    private JwtAuthenticationEntryPoint jwtAuthenticationEntryPoint;

    private UserDTO testUserDTO;
    private UUID userId;

    @BeforeEach
    void setUp() {
        userId = UUID.randomUUID();
        testUserDTO = UserDTO.builder()
                .id(userId)
                .name("Test User")
                .email("test@example.com")
                .role("USER")
                .emailVerified(false)
                .active(true)
                .createdAt(LocalDateTime.now())
                .build();
    }

    @Test
    @WithMockUser
    void getAllUsers_Success() throws Exception {
        PaginatedResponse<UserDTO> response = PaginatedResponse.<UserDTO>builder()
                .data(List.of(testUserDTO))
                .pagination(Pagination.builder()
                        .totalItems(1)
                        .totalPages(1)
                        .currentPage(0)
                        .itemsPerPage(20)
                        .build())
                .build();

        when(userManagementService.getAllUsers(anyInt(), anyInt())).thenReturn(response);

        mockMvc.perform(get("/api/v1/users")
                        .param("page", "0")
                        .param("size", "20"))
                .andExpect(status().isOk())
                .andExpect(jsonPath("$.data").isArray())
                .andExpect(jsonPath("$.data[0].email").value("test@example.com"))
                .andExpect(jsonPath("$.pagination.totalItems").value(1));
    }

    @Test
    @WithMockUser
    void getUserById_Success() throws Exception {
        when(userManagementService.getUserById(userId)).thenReturn(testUserDTO);

        mockMvc.perform(get("/api/v1/users/{id}", userId))
                .andExpect(status().isOk())
                .andExpect(jsonPath("$.id").value(userId.toString()))
                .andExpect(jsonPath("$.email").value("test@example.com"));
    }

    @Test
    @WithMockUser
    void getUserById_NotFound() throws Exception {
        when(userManagementService.getUserById(userId))
                .thenThrow(new UserNotFoundException(userId));

        mockMvc.perform(get("/api/v1/users/{id}", userId))
                .andExpect(status().isNotFound());
    }

    @Test
    void createUser_Success() throws Exception {
        CreateUserRequest request = CreateUserRequest.builder()
                .name("Test User")
                .email("test@example.com")
                .password("password123")
                .build();

        when(userManagementService.createUser(any(CreateUserRequest.class))).thenReturn(testUserDTO);

        mockMvc.perform(post("/api/v1/users")
                        .with(csrf())
                        .contentType(MediaType.APPLICATION_JSON)
                        .content(objectMapper.writeValueAsString(request)))
                .andExpect(status().isCreated())
                .andExpect(jsonPath("$.email").value("test@example.com"));
    }

    @Test
    void createUser_ValidationError() throws Exception {
        CreateUserRequest request = CreateUserRequest.builder()
                .name("")  // Invalid: empty name
                .email("invalid-email")  // Invalid: not a valid email
                .password("short")  // Invalid: too short
                .build();

        mockMvc.perform(post("/api/v1/users")
                        .with(csrf())
                        .contentType(MediaType.APPLICATION_JSON)
                        .content(objectMapper.writeValueAsString(request)))
                .andExpect(status().isBadRequest());
    }

    @Test
    @WithMockUser
    void updateUser_Success() throws Exception {
        UpdateUserRequest request = UpdateUserRequest.builder()
                .name("Updated Name")
                .build();

        UserDTO updatedUser = UserDTO.builder()
                .id(userId)
                .name("Updated Name")
                .email("test@example.com")
                .role("USER")
                .active(true)
                .build();

        when(userManagementService.updateUser(any(UUID.class), any(UpdateUserRequest.class)))
                .thenReturn(updatedUser);

        mockMvc.perform(put("/api/v1/users/{id}", userId)
                        .with(csrf())
                        .contentType(MediaType.APPLICATION_JSON)
                        .content(objectMapper.writeValueAsString(request)))
                .andExpect(status().isOk())
                .andExpect(jsonPath("$.name").value("Updated Name"));
    }

    @Test
    @WithMockUser
    void deleteUser_Success() throws Exception {
        mockMvc.perform(delete("/api/v1/users/{id}", userId)
                        .with(csrf()))
                .andExpect(status().isNoContent());
    }
}
