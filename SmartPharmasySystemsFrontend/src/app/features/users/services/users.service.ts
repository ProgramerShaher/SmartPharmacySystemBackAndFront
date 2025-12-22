import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, map } from 'rxjs';
import { ApiResponse, PagedResult, User, UserQueryDto, UserCreateDto, UserUpdateDto, Role, RoleCreateDto, RoleUpdateDto } from '../../../core/models';
import { environment } from '../../../../environments/environment';

@Injectable({ providedIn: 'root' })
export class UsersService {
    constructor(private http: HttpClient) { }

    search(query: UserQueryDto): Observable<PagedResult<User>> {
        return this.http.get<ApiResponse<PagedResult<User>>>(`${environment.apiUrl}/Users`, { params: query as any }).pipe(map(res => res.data));
    }
    getById(id: number): Observable<User> {
        return this.http.get<ApiResponse<User>>(`${environment.apiUrl}/Users/${id}`).pipe(map(res => res.data));
    }
    create(user: UserCreateDto): Observable<User> {
        return this.http.post<ApiResponse<User>>(`${environment.apiUrl}/Users`, user).pipe(map(res => res.data));
    }
    update(id: number, user: UserUpdateDto): Observable<User> {
        return this.http.put<ApiResponse<User>>(`${environment.apiUrl}/Users/${id}`, user).pipe(map(res => res.data));
    }
    delete(id: number): Observable<void> {
        return this.http.delete<ApiResponse<void>>(`${environment.apiUrl}/Users/${id}`).pipe(map(res => res.data));
    }
}
