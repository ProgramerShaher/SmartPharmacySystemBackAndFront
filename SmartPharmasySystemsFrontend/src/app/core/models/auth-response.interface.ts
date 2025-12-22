import { User } from './user.interface';

export interface AuthResponse {
    id: number;
    username: string;
    token: string;
    expiresAt: Date;
    roles: string[];
}
