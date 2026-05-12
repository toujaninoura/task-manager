import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map, tap } from 'rxjs/operators';
import { AuthRequest, AuthResponse, RegisterRequest } from '../models/user.model';
import { ApiResponse } from '../models/api-response.model';
import { environment } from '../../../environments/environment';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly TOKEN_KEY = 'auth_token';
  private readonly API = `${environment.apiUrl}/api/v1/auth`;

  constructor(private http: HttpClient) {}

  login(request: AuthRequest): Observable<AuthResponse> {
    return this.http
      .post<ApiResponse<AuthResponse>>(`${this.API}/login`, request)
      .pipe(
        map(r => r.data),
        tap(r => this.setToken(r.token))
      );
  }

  register(request: RegisterRequest): Observable<AuthResponse> {
    return this.http
      .post<ApiResponse<AuthResponse>>(`${this.API}/register`, request)
      .pipe(
        map(r => r.data),
        tap(r => this.setToken(r.token))
      );
  }

  logout(): void {
    localStorage.removeItem(this.TOKEN_KEY);
  }

  getToken(): string | null {
    return localStorage.getItem(this.TOKEN_KEY);
  }

  isLoggedIn(): boolean {
    return !!this.getToken();
  }

  setToken(token: string): void {
    localStorage.setItem(this.TOKEN_KEY, token);
  }

  getUserEmail(): string | null {
    const p = this.decodePayload();
    return p ? (p['email'] as string ?? null) : null;
  }

  getUserId(): number | null {
    const p = this.decodePayload();
    if (!p) return null;
    const id = p['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier'];
    return id ? parseInt(id as string, 10) : null;
  }

  private decodePayload(): Record<string, unknown> | null {
    const token = this.getToken();
    if (!token) return null;
    try {
      const payload = JSON.parse(atob(token.split('.')[1])) as Record<string, unknown>;
      const now = Math.floor(Date.now() / 1000);
      if (typeof payload['exp'] === 'number' && payload['exp'] < now) return null;
      return payload;
    } catch {
      return null;
    }
  }
}
