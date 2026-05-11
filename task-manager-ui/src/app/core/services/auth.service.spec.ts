import { TestBed } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { AuthService } from './auth.service';

describe('AuthService', () => {
  let service: AuthService;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule]
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

  it('should return false when no token', () => {
    expect(service.isLoggedIn()).toBeFalse();
  });

  it('should return true after setToken', () => {
    service.setToken('test-token');
    expect(service.isLoggedIn()).toBeTrue();
    expect(service.getToken()).toBe('test-token');
  });

  it('should clear token on logout', () => {
    service.setToken('test-token');
    service.logout();
    expect(service.isLoggedIn()).toBeFalse();
    expect(service.getToken()).toBeNull();
  });

  it('should call login endpoint and store token', () => {
    const mockAuthResponse = {
      token: 'mock-jwt-token',
      email: 'test@test.com',
      userId: 1,
      expiresAt: '2026-12-31T00:00:00Z'
    };
    const mockApiResponse = { success: true, data: mockAuthResponse };

    service.login({ email: 'test@test.com', password: 'Password123!' }).subscribe(response => {
      expect(response.token).toBe('mock-jwt-token');
      expect(localStorage.getItem('auth_token')).toBe('mock-jwt-token');
    });

    const req = httpMock.expectOne('https://localhost:7063/api/v1/auth/login');
    expect(req.request.method).toBe('POST');
    req.flush(mockApiResponse);
  });

  it('should call register endpoint and store token', () => {
    const mockAuthResponse = {
      token: 'mock-jwt-token',
      email: 'new@test.com',
      userId: 2,
      expiresAt: '2026-12-31T00:00:00Z'
    };
    const mockApiResponse = { success: true, data: mockAuthResponse };

    service.register({ firstName: 'Jane', lastName: 'Doe', email: 'new@test.com', password: 'Password123!' }).subscribe(response => {
      expect(response.token).toBe('mock-jwt-token');
      expect(localStorage.getItem('auth_token')).toBe('mock-jwt-token');
    });

    const req = httpMock.expectOne('https://localhost:7063/api/v1/auth/register');
    expect(req.request.method).toBe('POST');
    req.flush(mockApiResponse);
  });

  it('should return null when no token stored', () => {
    expect(service.getToken()).toBeNull();
  });

  it('should return null from getUserId when no token', () => {
    expect(service.getUserId()).toBeNull();
  });

  it('should return null from getUserId when token is malformed', () => {
    service.setToken('not-a-valid-jwt');
    expect(service.getUserId()).toBeNull();
  });
});
