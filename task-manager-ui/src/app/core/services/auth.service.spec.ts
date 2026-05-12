import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { AuthService } from './auth.service';

function makeJwt(payload: Record<string, unknown>): string {
  const header = btoa(JSON.stringify({ alg: 'HS256', typ: 'JWT' }));
  const body = btoa(JSON.stringify(payload));
  return `${header}.${body}.signature`;
}

describe('AuthService', () => {
  let service: AuthService;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
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

  it('should return false when no token', () => {
    expect(service.isLoggedIn()).toBeFalse();
  });

  it('should return true when valid token exists in localStorage', () => {
    const futureExp = Math.floor(Date.now() / 1000) + 3600;
    localStorage.setItem('auth_token', makeJwt({ exp: futureExp, email: 'test@test.com' }));
    expect(service.isLoggedIn()).toBeTrue();
  });

  it('should clear token on logout', () => {
    const futureExp = Math.floor(Date.now() / 1000) + 3600;
    localStorage.setItem('auth_token', makeJwt({ exp: futureExp }));
    service.logout();
    expect(service.isLoggedIn()).toBeFalse();
    expect(service.getToken()).toBeNull();
  });

  it('should call login endpoint and store token', () => {
    const futureExp = Math.floor(Date.now() / 1000) + 3600;
    const validJwt = makeJwt({ exp: futureExp, email: 'test@test.com' });
    const mockAuthResponse = { token: validJwt, email: 'test@test.com', userId: 1, expiresAt: '2026-12-31T00:00:00Z' };
    const mockApiResponse = { success: true, data: mockAuthResponse };

    service.login({ email: 'test@test.com', password: 'Password123!' }).subscribe(response => {
      expect(response.token).toBe(validJwt);
      expect(localStorage.getItem('auth_token')).toBe(validJwt);
    });

    const req = httpMock.expectOne('https://localhost:7063/api/v1/auth/login');
    expect(req.request.method).toBe('POST');
    req.flush(mockApiResponse);
  });

  it('should call register endpoint and store token', () => {
    const futureExp = Math.floor(Date.now() / 1000) + 3600;
    const validJwt = makeJwt({ exp: futureExp, email: 'new@test.com' });
    const mockAuthResponse = { token: validJwt, email: 'new@test.com', userId: 2, expiresAt: '2026-12-31T00:00:00Z' };
    const mockApiResponse = { success: true, data: mockAuthResponse };

    service.register({ firstName: 'Jane', lastName: 'Doe', email: 'new@test.com', password: 'Password123!' }).subscribe(response => {
      expect(response.token).toBe(validJwt);
      expect(localStorage.getItem('auth_token')).toBe(validJwt);
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
    localStorage.setItem('auth_token', 'not-a-valid-jwt');
    expect(service.getUserId()).toBeNull();
  });

  describe('isAuthenticated', () => {
    it('isAuthenticated_WhenTokenValid_ShouldReturnTrue', () => {
      const futureExp = Math.floor(Date.now() / 1000) + 3600;
      localStorage.setItem('auth_token', makeJwt({ exp: futureExp, email: 'test@test.com' }));
      expect(service.isAuthenticated()).toBeTrue();
    });

    it('isAuthenticated_WhenNoToken_ShouldReturnFalse', () => {
      expect(service.isAuthenticated()).toBeFalse();
    });

    it('isAuthenticated_WhenTokenExpired_ShouldReturnFalse', () => {
      const pastExp = Math.floor(Date.now() / 1000) - 3600;
      localStorage.setItem('auth_token', makeJwt({ exp: pastExp, email: 'test@test.com' }));
      expect(service.isAuthenticated()).toBeFalse();
    });

    it('isAuthenticated_WhenTokenMalformed_ShouldReturnFalse', () => {
      localStorage.setItem('auth_token', 'not-a-valid-jwt');
      expect(service.isAuthenticated()).toBeFalse();
    });
  });
});
