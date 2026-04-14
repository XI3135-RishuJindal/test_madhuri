package com.platform.usermanagement.service;

import com.platform.usermanagement.dto.*;
import com.platform.usermanagement.entity.User;
import com.platform.usermanagement.exception.UserAlreadyExistsException;
import com.platform.usermanagement.exception.UserNotFoundException;
import com.platform.usermanagement.mapper.UserMapper;
import com.platform.usermanagement.repository.UserRepository;
import org.junit.jupiter.api.BeforeEach;
import org.junit.jupiter.api.Test;
import org.junit.jupiter.api.extension.ExtendWith;
import org.mockito.InjectMocks;
import org.mockito.Mock;
import org.mockito.junit.jupiter.MockitoExtension;
import org.springframework.data.domain.Page;
import org.springframework.data.domain.PageImpl;
import org.springframework.data.domain.Pageable;
import org.springframework.security.crypto.password.PasswordEncoder;

import java.time.LocalDateTime;
import java.util.List;
import java.util.Optional;
import java.util.UUID;

import static org.junit.jupiter.api.Assertions.*;
import static org.mockito.ArgumentMatchers.any;
import static org.mockito.ArgumentMatchers.anyString;
import static org.mockito.Mockito.*;

@ExtendWith(MockitoExtension.class)
class UserManagementServiceTest {

    @Mock
    private UserRepository userRepository;

    @Mock
    private UserMapper userMapper;

    @Mock
    private PasswordEncoder passwordEncoder;

    @Mock
    private EventPublisherService eventPublisherService;

    @Mock
    private UserVerificationService verificationService;

    @InjectMocks
    private UserManagementService userManagementService;

    private User testUser;
    private UserDTO testUserDTO;
    private CreateUserRequest createUserRequest;
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
                .emailVerified(false)
                .active(true)
                .createdAt(LocalDateTime.now())
                .build();

        testUserDTO = UserDTO.builder()
                .id(userId)
                .name("Test User")
                .email("test@example.com")
                .role("USER")
                .emailVerified(false)
                .active(true)
                .createdAt(testUser.getCreatedAt())
                .build();

        createUserRequest = CreateUserRequest.builder()
                .name("Test User")
                .email("test@example.com")
                .password("password123")
                .build();
    }

    @Test
    void createUser_Success() {
        when(userRepository.existsByEmail(anyString())).thenReturn(false);
        when(passwordEncoder.encode(anyString())).thenReturn("encodedPassword");
        when(userMapper.toEntity(any(CreateUserRequest.class), anyString())).thenReturn(testUser);
        when(userRepository.save(any(User.class))).thenReturn(testUser);
        when(userMapper.toDTO(any(User.class))).thenReturn(testUserDTO);

        UserDTO result = userManagementService.createUser(createUserRequest);

        assertNotNull(result);
        assertEquals(testUserDTO.getEmail(), result.getEmail());
        verify(userRepository).save(any(User.class));
        verify(verificationService).sendVerificationCode(any(UUID.class), anyString());
        verify(eventPublisherService).publishUserCreatedEvent(any());
    }

    @Test
    void createUser_UserAlreadyExists() {
        when(userRepository.existsByEmail(anyString())).thenReturn(true);

        assertThrows(UserAlreadyExistsException.class, () -> 
            userManagementService.createUser(createUserRequest)
        );

        verify(userRepository, never()).save(any(User.class));
    }

    @Test
    void getUserById_Success() {
        when(userRepository.findById(userId)).thenReturn(Optional.of(testUser));
        when(userMapper.toDTO(testUser)).thenReturn(testUserDTO);

        UserDTO result = userManagementService.getUserById(userId);

        assertNotNull(result);
        assertEquals(userId, result.getId());
    }

    @Test
    void getUserById_NotFound() {
        when(userRepository.findById(userId)).thenReturn(Optional.empty());

        assertThrows(UserNotFoundException.class, () -> 
            userManagementService.getUserById(userId)
        );
    }

    @Test
    void getAllUsers_Success() {
        Page<User> userPage = new PageImpl<>(List.of(testUser));
        when(userRepository.findAll(any(Pageable.class))).thenReturn(userPage);
        when(userMapper.toDTO(any(User.class))).thenReturn(testUserDTO);

        PaginatedResponse<UserDTO> result = userManagementService.getAllUsers(0, 20);

        assertNotNull(result);
        assertEquals(1, result.getData().size());
        assertEquals(1, result.getPagination().getTotalItems());
    }

    @Test
    void updateUser_Success() {
        UpdateUserRequest updateRequest = UpdateUserRequest.builder()
                .name("Updated Name")
                .build();

        when(userRepository.findById(userId)).thenReturn(Optional.of(testUser));
        when(userRepository.save(any(User.class))).thenReturn(testUser);
        when(userMapper.toDTO(any(User.class))).thenReturn(testUserDTO);

        UserDTO result = userManagementService.updateUser(userId, updateRequest);

        assertNotNull(result);
        verify(userRepository).save(any(User.class));
    }

    @Test
    void updateUser_NotFound() {
        UpdateUserRequest updateRequest = UpdateUserRequest.builder()
                .name("Updated Name")
                .build();

        when(userRepository.findById(userId)).thenReturn(Optional.empty());

        assertThrows(UserNotFoundException.class, () -> 
            userManagementService.updateUser(userId, updateRequest)
        );
    }

    @Test
    void deleteUser_Success() {
        when(userRepository.existsById(userId)).thenReturn(true);

        userManagementService.deleteUser(userId);

        verify(userRepository).deleteById(userId);
    }

    @Test
    void deleteUser_NotFound() {
        when(userRepository.existsById(userId)).thenReturn(false);

        assertThrows(UserNotFoundException.class, () -> 
            userManagementService.deleteUser(userId)
        );

        verify(userRepository, never()).deleteById(any());
    }

    @Test
    void verifyUser_Success() {
        VerificationRequest request = VerificationRequest.builder()
                .userId(userId)
                .verificationCode("123456")
                .build();

        when(userRepository.findById(userId)).thenReturn(Optional.of(testUser));
        when(verificationService.validateVerificationCode(userId, "123456")).thenReturn(true);
        when(userRepository.save(any(User.class))).thenReturn(testUser);

        VerificationResponse result = userManagementService.verifyUser(request);

        assertTrue(result.isVerified());
        verify(userRepository).save(any(User.class));
    }

    @Test
    void verifyUser_InvalidCode() {
        VerificationRequest request = VerificationRequest.builder()
                .userId(userId)
                .verificationCode("invalid")
                .build();

        when(userRepository.findById(userId)).thenReturn(Optional.of(testUser));
        when(verificationService.validateVerificationCode(userId, "invalid")).thenReturn(false);

        VerificationResponse result = userManagementService.verifyUser(request);

        assertFalse(result.isVerified());
        verify(userRepository, never()).save(any(User.class));
    }
}
