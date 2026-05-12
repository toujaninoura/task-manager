import { Component } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { provideRouter, UrlTree } from '@angular/router';
import { provideLocationMocks } from '@angular/common/testing';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { noAuthGuard } from './no-auth.guard';
import { ActivatedRouteSnapshot, RouterStateSnapshot } from '@angular/router';

@Component({ template: '', standalone: true })
class DummyComponent {}

function makeJwt(payload: Record<string, unknown>): string {
  const header = btoa(JSON.stringify({ alg: 'HS256', typ: 'JWT' }));
  const body = btoa(JSON.stringify(payload));
  return `${header}.${body}.signature`;
}

describe('noAuthGuard', () => {
  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        provideRouter([{ path: 'login', canActivate: [noAuthGuard], component: DummyComponent }]),
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

  it('noAuthGuard_WhenAuthenticated_ShouldReturnUrlTreeForTasks', () => {
    const futureExp = Math.floor(Date.now() / 1000) + 3600;
    localStorage.setItem('auth_token', makeJwt({ exp: futureExp, email: 'test@test.com' }));
    const result = TestBed.runInInjectionContext(() =>
      noAuthGuard({} as ActivatedRouteSnapshot, {} as RouterStateSnapshot)
    );
    expect(result instanceof UrlTree).toBeTrue();
    expect((result as UrlTree).toString()).toBe('/tasks');
  });

  it('noAuthGuard_WhenNotAuthenticated_ShouldReturnTrue', () => {
    const result = TestBed.runInInjectionContext(() =>
      noAuthGuard({} as ActivatedRouteSnapshot, {} as RouterStateSnapshot)
    );
    expect(result).toBeTrue();
  });
});
