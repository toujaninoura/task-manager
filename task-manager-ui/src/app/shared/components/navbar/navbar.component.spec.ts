import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter, Router } from '@angular/router';
import { provideLocationMocks } from '@angular/common/testing';
import { NavbarComponent } from './navbar.component';
import { AuthService } from '../../../core/services/auth.service';

describe('NavbarComponent', () => {
  let component: NavbarComponent;
  let fixture: ComponentFixture<NavbarComponent>;
  let authServiceSpy: jasmine.SpyObj<AuthService>;
  let router: Router;

  beforeEach(async () => {
    authServiceSpy = jasmine.createSpyObj('AuthService', ['getUserEmail', 'logout']);
    authServiceSpy.getUserEmail.and.returnValue('test@example.com');

    await TestBed.configureTestingModule({
      imports: [NavbarComponent],
      providers: [
        provideRouter([]),
        provideLocationMocks(),
        { provide: AuthService, useValue: authServiceSpy }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(NavbarComponent);
    component = fixture.componentInstance;
    router = TestBed.inject(Router);
    fixture.detectChanges();
  });

  it('should create the navbar component', () => {
    expect(component).toBeTruthy();
  });

  it('should return userEmail from AuthService', () => {
    authServiceSpy.getUserEmail.and.returnValue('toujani@example.com');
    expect(component.userEmail).toBe('toujani@example.com');
  });

  it('should return empty string when userEmail is null', () => {
    authServiceSpy.getUserEmail.and.returnValue(null);
    expect(component.userEmail).toBe('');
  });

  it('should return userInitials as first 2 letters uppercase', () => {
    authServiceSpy.getUserEmail.and.returnValue('toujani@example.com');
    expect(component.userInitials).toBe('TO');
  });

  it('should return empty string for userInitials when email is null', () => {
    authServiceSpy.getUserEmail.and.returnValue(null);
    expect(component.userInitials).toBe('');
  });

  it('should call authService.logout and navigate to /login on logout()', () => {
    spyOn(router, 'navigate');
    component.logout();
    expect(authServiceSpy.logout).toHaveBeenCalled();
    expect(router.navigate).toHaveBeenCalledWith(['/login']);
  });

  it('should render the navbar brand link', () => {
    const compiled = fixture.nativeElement as HTMLElement;
    const brand = compiled.querySelector('.navbar-brand');
    expect(brand).toBeTruthy();
    expect(brand?.textContent).toContain('TaskManager');
  });

  it('should render the dropdown avatar initials in the template', () => {
    const compiled = fixture.nativeElement as HTMLElement;
    const avatar = compiled.querySelector('.avatar-initials');
    expect(avatar).toBeTruthy();
  });
});
