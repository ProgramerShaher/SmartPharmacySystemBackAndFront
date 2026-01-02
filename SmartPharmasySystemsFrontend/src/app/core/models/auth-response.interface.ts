/**
 * Login Request DTO - matches backend LoginRequestDto
 */
export interface LoginRequest {
    username: string;
    password: string;
}

/**
 * Login Response DTO - matches backend LoginResponseDto
 */
export interface LoginResponse {
    token: string;
    userId: number;
    username: string;
    fullName: string;
    roleName: string;
    email?: string;
}

/**
 * Change Password DTO - matches backend ChangePasswordDto
 */
export interface ChangePasswordRequest {
    oldPassword: string;
    newPassword: string;
    confirmPassword: string;
}

/**
 * Current User Response from /api/Auth/me
 * Extended to match User interface requirements
 */
export interface CurrentUserResponse {
    userId: number;
    username: string;
    role: string;
    roleId: number;
    isAdmin: boolean;
    isPharmacist: boolean;
    // Additional fields to match User interface
    id?: number;  // Alias for userId
    fullName?: string;
    roleName?: string;  // Alias for role
    email?: string;
    phoneNumber?: string;
    status?: number;
    createdAt?: string;
    isDeleted?: boolean;
}

// Legacy - keep for backward compatibility
export interface AuthResponse {
    id: number;
    username: string;
    token: string;
    expiresAt: Date;
    roles: string[];
}
