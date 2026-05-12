import { Component } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { provideRouter, UrlTree } from '@angular/router';
import { provideLocationMocks } from '@angular/common/testing';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { authGuard } from './auth.guard';
import { ActivatedRouteSnapshot, RouterStateSnapshot } from '@angular/router';

@Component({ template: '', standalone: true })
class DummyComponent {}

function makeJwt(payload: Record<string, unknown>): string {
  const header = btoa(JSON.stringify({ alg: 'HS256', typ: 'JWT' }));
  const body = btoa(JSON.stringify(payload));
  return `${header}.${body}.signature`;
}

describe('authGuard', () => {
  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        provideRouter([{ path: 'tasks', canActivate: [authGuard], component: DummyComponent }]),
        provideLocationMocks(),
        provideHttpClient(),
        provideHttpClientTesting()
      ]
    });
    localStorage.clear();
  });

  afterEach(() => {
    localStorage.clear();
  });

  it('authGuard_WhenTokenValid_ShouldReturnTrue', () => {
    const futureExp = Math.floor(Date.now() / 1000) + 3600;
    localStorage.setItem('auth_token', makeJwt({ exp: futureExp, email: 'test@test.com' }));
    const result = TestBed.runInInjectionContext(() =>
      authGuard({} as ActivatedRouteSnapshot, {} as RouterStateSnapshot)
    );
    expect(result).toBeTrue();
  });

  it('authGuard_WhenNoToken_ShouldReturnUrlTreeForLogin', () => {
    const result = TestBed.runInInjectionContext(() =>
      authGuard({} as ActivatedRouteSnapshot, {} as RouterStateSnapshot)
    );
    expect(result instanceof UrlTree).toBeTrue();
    expect((result as UrlTree).toString()).toBe('/login');
  });

  it('authGuard_WhenTokenExpired_ShouldReturnUrlTreeForLogin', () => {
    const pastExp = Math.floor(Date.now() / 1000) - 3600;
    localStorage.setItem('auth_token', makeJwt({ exp: pastExp, email: 'test@test.com' }));
    const result = TestBed.runInInjectionContext(() =>
      authGuard({} as ActivatedRouteSnapshot, {} as RouterStateSnapshot)
    );
    expect(result instanceof UrlTree).toBeTrue();
    expect((result as UrlTree).toString()).toBe('/login');
  });

  it('authGuard_WhenTokenMalformed_ShouldReturnUrlTreeForLogin', () => {
    localStorage.setItem('auth_token', 'not-a-valid-jwt');
    const result = TestBed.runInInjectionContext(() =>
      authGuard({} as ActivatedRouteSnapshot, {} as RouterStateSnapshot)
    );
    expect(result instanceof UrlTree).toBeTrue();
    expect((result as UrlTree).toString()).toBe('/login');
  });
});
