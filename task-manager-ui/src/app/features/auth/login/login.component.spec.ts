import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ReactiveFormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { provideRouter } from '@angular/router';
import { provideLocationMocks } from '@angular/common/testing';
import { of, throwError } from 'rxjs';
import { LoginComponent } from './login.component';
import { AuthService } from '../../../core/services/auth.service';
import { AuthResponse } from '../../../core/models/user.model';

describe('LoginComponent', () => {
  let component: LoginComponent;
  let fixture: ComponentFixture<LoginComponent>;
  let authServiceSpy: jasmine.SpyObj<AuthService>;
  let router: Router;

  const mockAuthResponse: AuthResponse = {
    token: 'mock-token',
    email: 'test@test.com',
    userId: 1,
    expiresAt: '2026-12-31T00:00:00Z'
  };

  beforeEach(async () => {
    authServiceSpy = jasmine.createSpyObj('AuthService', ['login']);

    await TestBed.configureTestingModule({
      imports: [LoginComponent, ReactiveFormsModule],
      providers: [
        provideRouter([]),
        provideLocationMocks(),
        { provide: AuthService, useValue: authServiceSpy }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(LoginComponent);
    component = fixture.componentInstance;
    router = TestBed.inject(Router);
    spyOn(router, 'navigate');
    fixture.detectChanges();
  });

  it('should display the login component', () => {
    expect(component).toBeTruthy();
  });

  it('should have form invalid when empty', () => {
    expect(component.form.invalid).toBeTrue();
  });

  it('should have email control invalid when empty', () => {
    expect(component.form.get('email')?.invalid).toBeTrue();
  });

  it('should have password control invalid when too short', () => {
    component.form.get('password')?.setValue('short');
    expect(component.form.get('password')?.invalid).toBeTrue();
  });

  it('should call login service on submit', () => {
    authServiceSpy.login.and.returnValue(of(mockAuthResponse));
    component.form.setValue({ email: 'test@test.com', password: 'password123' });
    component.onSubmit();
    expect(authServiceSpy.login).toHaveBeenCalled();
  });

  it('should redirect to /tasks and reset isLoading after successful login', () => {
    authServiceSpy.login.and.returnValue(of(mockAuthResponse));
    component.form.setValue({ email: 'test@test.com', password: 'password123' });
    component.onSubmit();
    expect(router.navigate).toHaveBeenCalledWith(['/tasks']);
    expect(component.isLoading).toBeFalse();
  });

  it('should set errorMessage on login failure', () => {
    authServiceSpy.login.and.returnValue(
      throwError(() => ({ error: { message: 'Identifiants incorrects' } }))
    );
    component.form.setValue({ email: 'test@test.com', password: 'password123' });
    component.onSubmit();
    expect(component.errorMessage).toBe('Identifiants incorrects');
    expect(component.isLoading).toBeFalse();
  });

  it('should not submit if form is invalid', () => {
    component.onSubmit();
    expect(authServiceSpy.login).not.toHaveBeenCalled();
  });

  it('should not submit if isLoading is true', () => {
    authServiceSpy.login.and.returnValue(of(mockAuthResponse));
    component.form.setValue({ email: 'test@test.com', password: 'password123' });
    component.isLoading = true;
    component.onSubmit();
    expect(authServiceSpy.login).not.toHaveBeenCalled();
  });
});
