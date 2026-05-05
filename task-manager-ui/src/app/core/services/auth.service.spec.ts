import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { AuthService } from './auth.service';
import { AuthRequest, AuthResponse } from '../models/user.model';
import { ApiResponse } from '../models/api-response.model';

describe('AuthService', () => {
  let service: AuthService;
  let httpMock: HttpTestingController;

  const mockAuthResponse: AuthResponse = {
    token: 'mock-jwt-token',
    email: 'test@test.com',
    userId: 1,
    expiresAt: '2026-12-31T00:00:00Z'
  };

  const mockApiResponse: ApiResponse<AuthResponse> = {
    success: true,
    data: mockAuthResponse
  };

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        AuthService,
        provideHttpClient(),
        provideHttpClientTesting()
      ]
    });
    service = TestBed.inject(AuthService);
    httpMock = TestBed.inject(HttpTestingController);
    localStorage.clear();
  });

  afterEach(() => {
    httpMock.verify();
    localStorage.clear();
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  it('should call login endpoint and store token', () => {
    const request: AuthRequest = { email: 'test@test.com', password: 'Password123!' };

    service.login(request).subscribe(response => {
      expect(response.token).toBe('mock-jwt-token');
      expect(localStorage.getItem('auth_token')).toBe('mock-jwt-token');
    });

    const req = httpMock.expectOne('http://localhost:5000/api/v1/auth/login');
    expect(req.request.method).toBe('POST');
    req.flush(mockApiResponse);
  });

  it('should call register endpoint and store token', () => {
    const request: AuthRequest = { email: 'new@test.com', password: 'Password123!' };

    service.register(request).subscribe(response => {
      expect(response.token).toBe('mock-jwt-token');
      expect(localStorage.getItem('auth_token')).toBe('mock-jwt-token');
    });

    const req = httpMock.expectOne('http://localhost:5000/api/v1/auth/register');
    expect(req.request.method).toBe('POST');
    req.flush(mockApiResponse);
  });

  it('should return true when token exists in localStorage', () => {
    localStorage.setItem('auth_token', 'valid-token');
    expect(service.isLoggedIn()).toBe(true);
  });

  it('should return false when no token in localStorage', () => {
    expect(service.isLoggedIn()).toBe(false);
  });

  it('should remove token on logout', () => {
    localStorage.setItem('auth_token', 'some-token');
    service.logout();
    expect(localStorage.getItem('auth_token')).toBeNull();
  });

  it('should return token from localStorage', () => {
    localStorage.setItem('auth_token', 'my-token');
    expect(service.getToken()).toBe('my-token');
  });

  it('should return null when no token stored', () => {
    expect(service.getToken()).toBeNull();
  });
});
