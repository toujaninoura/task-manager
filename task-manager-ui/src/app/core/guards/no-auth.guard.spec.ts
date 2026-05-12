import { TestBed } from '@angular/core/testing';
import { provideRouter, Router } from '@angular/router';
import { provideLocationMocks } from '@angular/common/testing';
import { noAuthGuard } from './no-auth.guard';
import { AuthService } from '../services/auth.service';
import { ActivatedRouteSnapshot, RouterStateSnapshot } from '@angular/router';

describe('noAuthGuard', () => {
  let authServiceSpy: jasmine.SpyObj<AuthService>;
  let router: Router;

  beforeEach(() => {
    authServiceSpy = jasmine.createSpyObj('AuthService', ['isAuthenticated']);

    TestBed.configureTestingModule({
      providers: [
        provideRouter([{ path: 'login', canActivate: [noAuthGuard], component: {} as any }]),
        provideLocationMocks(),
        { provide: AuthService, useValue: authServiceSpy }
      ]
    });

    router = TestBed.inject(Router);
    spyOn(router, 'navigate');
  });

  it('noAuthGuard_WhenAuthenticated_ShouldReturnFalseAndNavigateToTasks', () => {
    authServiceSpy.isAuthenticated.and.returnValue(true);
    const result = TestBed.runInInjectionContext(() =>
      noAuthGuard({} as ActivatedRouteSnapshot, {} as RouterStateSnapshot)
    );
    expect(result).toBeFalse();
    expect(router.navigate).toHaveBeenCalledWith(['/tasks']);
  });

  it('noAuthGuard_WhenNotAuthenticated_ShouldReturnTrue', () => {
    authServiceSpy.isAuthenticated.and.returnValue(false);
    const result = TestBed.runInInjectionContext(() =>
      noAuthGuard({} as ActivatedRouteSnapshot, {} as RouterStateSnapshot)
    );
    expect(result).toBeTrue();
    expect(router.navigate).not.toHaveBeenCalled();
  });
});
