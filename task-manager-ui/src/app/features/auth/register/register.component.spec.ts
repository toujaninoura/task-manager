import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ReactiveFormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { provideRouter } from '@angular/router';
import { provideLocationMocks } from '@angular/common/testing';
import { of, throwError } from 'rxjs';
import { RegisterComponent } from './register.component';
import { AuthService } from '../../../core/services/auth.service';
import { AuthResponse } from '../../../core/models/user.model';
import { PasswordStrengthService } from '../../../core/services/password-strength.service';

describe('RegisterComponent', () => {
  let component: RegisterComponent;
  let fixture: ComponentFixture<RegisterComponent>;
  let authServiceSpy: jasmine.SpyObj<AuthService>;
  let router: Router;

  const mockAuthResponse: AuthResponse = {
    token: 'mock-token',
    email: 'test@test.com',
    userId: 1,
    expiresAt: '2026-12-31T00:00:00Z'
  };

  const validForm = {
    firstName: 'Jane',
    lastName: 'Doe',
    email: 'jane@test.com',
    password: 'Password1',
    confirmPassword: 'Password1'
  };

  beforeEach(async () => {
    authServiceSpy = jasmine.createSpyObj('AuthService', ['register']);

    await TestBed.configureTestingModule({
      imports: [RegisterComponent, ReactiveFormsModule],
      providers: [
        provideRouter([]),
        provideLocationMocks(),
        { provide: AuthService, useValue: authServiceSpy },
        PasswordStrengthService
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(RegisterComponent);
    component = fixture.componentInstance;
    router = TestBed.inject(Router);
    spyOn(router, 'navigate');
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should have form invalid when empty', () => {
    expect(component.form.invalid).toBeTrue();
  });

  it('should validate required fields', () => {
    ['firstName', 'lastName', 'email', 'password', 'confirmPassword'].forEach(field => {
      expect(component.form.get(field)?.invalid).toBeTrue();
    });
  });

  it('should validate email format', () => {
    component.form.get('email')?.setValue('not-an-email');
    expect(component.form.get('email')?.hasError('email')).toBeTrue();
  });

  it('should validate password minimum length', () => {
    component.form.get('password')?.setValue('short');
    expect(component.form.get('password')?.hasError('minlength')).toBeTrue();
  });

  it('should detect password mismatch', () => {
    component.form.get('password')?.setValue('Password1');
    component.form.get('confirmPassword')?.setValue('Different1');
    expect(component.form.hasError('passwordMismatch')).toBeTrue();
  });

  it('should be valid when all fields correct', () => {
    component.form.setValue(validForm);
    expect(component.form.valid).toBeTrue();
  });

  it('should call register service on valid submit', () => {
    authServiceSpy.register.and.returnValue(of(mockAuthResponse));
    component.form.setValue(validForm);
    component.onSubmit();
    expect(authServiceSpy.register).toHaveBeenCalledWith({
      firstName: 'Jane',
      lastName: 'Doe',
      email: 'jane@test.com',
      password: 'Password1'
    });
  });

  it('should redirect to /login and reset isLoading after successful register', () => {
    authServiceSpy.register.and.returnValue(of(mockAuthResponse));
    component.form.setValue(validForm);
    component.onSubmit();
    expect(router.navigate).toHaveBeenCalledWith(['/login']);
    expect(component.isLoading).toBeFalse();
  });

  it('should set errorMessage on API error', () => {
    authServiceSpy.register.and.returnValue(
      throwError(() => ({ error: { message: 'Email déjà utilisé' } }))
    );
    component.form.setValue(validForm);
    component.onSubmit();
    expect(component.errorMessage).toBe('Email déjà utilisé');
    expect(component.isLoading).toBeFalse();
  });

  it('should set fallback errorMessage when API gives no message', () => {
    authServiceSpy.register.and.returnValue(throwError(() => ({})));
    component.form.setValue(validForm);
    component.onSubmit();
    expect(component.errorMessage).toBe('Une erreur est survenue. Veuillez réessayer.');
  });

  it('should not submit when form is invalid', () => {
    component.onSubmit();
    expect(authServiceSpy.register).not.toHaveBeenCalled();
  });

  it('should not submit when isLoading is true', () => {
    authServiceSpy.register.and.returnValue(of(mockAuthResponse));
    component.form.setValue(validForm);
    component.isLoading = true;
    component.onSubmit();
    expect(authServiceSpy.register).not.toHaveBeenCalled();
  });

  // ================== Password Strength ==================

  it('should return score 0 for empty password', () => {
    component.form.get('password')?.setValue('');
    expect(component.passwordStrengthScore).toBe(0);
  });

  it('should return score 2 for lowercase-only password with length', () => {
    component.form.get('password')?.setValue('abcdefgh');
    expect(component.passwordStrengthScore).toBe(2); // length + lowercase
  });

  it('should return max score 5 for strong password', () => {
    component.form.get('password')?.setValue('Abc1defg!');
    expect(component.passwordStrengthScore).toBe(5);
  });

  it('should return bg-danger for weak password (score <= 1)', () => {
    component.form.get('password')?.setValue('a');
    expect(component.passwordStrengthClass).toBe('bg-danger');
  });

  it('should return bg-warning for medium password (score 2-3)', () => {
    component.form.get('password')?.setValue('abcdefgh');
    expect(component.passwordStrengthClass).toBe('bg-warning');
  });

  it('should return bg-success for strong password (score >= 4)', () => {
    component.form.get('password')?.setValue('Abcdefg1!');
    expect(component.passwordStrengthClass).toBe('bg-success');
  });

  it('should return correct width percentage for score 5', () => {
    component.form.get('password')?.setValue('Abc1defg!');
    expect(component.passwordStrengthWidth).toBe('100%');
  });
});
