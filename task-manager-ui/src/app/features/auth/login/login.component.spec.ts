import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ReactiveFormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { of, throwError } from 'rxjs';
import { LoginComponent } from './login.component';
import { AuthService } from '../../../core/services/auth.service';
import { AuthResponse } from '../../../core/models/user.model';

describe('LoginComponent', () => {
  let component: LoginComponent;
  let fixture: ComponentFixture<LoginComponent>;
  let authServiceSpy: jasmine.SpyObj<AuthService>;
  let routerSpy: jasmine.SpyObj<Router>;

  const mockAuthResponse: AuthResponse = {
    token: 'mock-token',
    email: 'test@test.com',
    userId: 1,
    expiresAt: '2026-12-31T00:00:00Z'
  };

  beforeEach(async () => {
    authServiceSpy = jasmine.createSpyObj('AuthService', ['login', 'register']);
    routerSpy = jasmine.createSpyObj('Router', ['navigate']);

    await TestBed.configureTestingModule({
      imports: [LoginComponent, ReactiveFormsModule],
      providers: [
        { provide: AuthService, useValue: authServiceSpy },
        { provide: Router, useValue: routerSpy }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(LoginComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should display the login component', () => {
    expect(component).toBeTruthy();
  });

  it('should have form invalid when empty', () => {
    expect(component.form.invalid).toBeTrue();
  });

  it('should have email control invalid when empty', () => {
    const emailControl = component.form.get('email');
    expect(emailControl?.invalid).toBeTrue();
  });

  it('should have password control invalid when too short', () => {
    component.form.get('password')?.setValue('short');
    expect(component.form.get('password')?.invalid).toBeTrue();
  });

  it('should start in login mode', () => {
    expect(component.isLoginMode).toBeTrue();
  });

  it('should toggle to register mode', () => {
    component.toggle();
    expect(component.isLoginMode).toBeFalse();
  });

  it('should call login service on submit in login mode', () => {
    authServiceSpy.login.and.returnValue(of(mockAuthResponse));
    component.form.setValue({ email: 'test@test.com', password: 'password123' });
    component.onSubmit();
    expect(authServiceSpy.login).toHaveBeenCalled();
  });

  it('should redirect to /tasks after successful login', () => {
    authServiceSpy.login.and.returnValue(of(mockAuthResponse));
    component.form.setValue({ email: 'test@test.com', password: 'password123' });
    component.onSubmit();
    expect(routerSpy.navigate).toHaveBeenCalledWith(['/tasks']);
  });

  it('should set errorMessage on login failure', () => {
    authServiceSpy.login.and.returnValue(
      throwError(() => ({ error: { message: 'Identifiants incorrects' } }))
    );
    component.form.setValue({ email: 'test@test.com', password: 'password123' });
    component.onSubmit();
    expect(component.errorMessage).toBe('Identifiants incorrects');
  });

  it('should call register service on submit in register mode', () => {
    authServiceSpy.register.and.returnValue(of(mockAuthResponse));
    component.toggle();
    component.form.setValue({ email: 'new@test.com', password: 'password123' });
    component.onSubmit();
    expect(authServiceSpy.register).toHaveBeenCalled();
  });

  it('should not submit if form is invalid', () => {
    component.onSubmit();
    expect(authServiceSpy.login).not.toHaveBeenCalled();
    expect(authServiceSpy.register).not.toHaveBeenCalled();
  });
});
