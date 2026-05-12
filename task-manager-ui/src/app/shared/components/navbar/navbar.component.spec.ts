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

  it('create_Always_ShouldInstantiateComponent', () => {
    expect(component).toBeTruthy();
  });

  it('userEmail_WhenEmailExists_ShouldReturnEmail', () => {
    authServiceSpy.getUserEmail.and.returnValue('toujani@example.com');
    fixture.detectChanges();
    expect(component.userEmail).toBe('toujani@example.com');
  });

  it('userEmail_WhenEmailIsNull_ShouldReturnEmptyString', () => {
    authServiceSpy.getUserEmail.and.returnValue(null);
    fixture.detectChanges();
    expect(component.userEmail).toBe('');
  });

  it('userInitials_WhenEmailExists_ShouldReturnFirst2LettersUppercase', () => {
    authServiceSpy.getUserEmail.and.returnValue('toujani@example.com');
    fixture.detectChanges();
    expect(component.userInitials).toBe('TO');
  });

  it('userInitials_WhenEmailIsNull_ShouldReturnEmptyString', () => {
    authServiceSpy.getUserEmail.and.returnValue(null);
    fixture.detectChanges();
    expect(component.userInitials).toBe('');
  });

  it('logout_WhenCalled_ShouldCallAuthServiceLogoutAndNavigateToLogin', () => {
    spyOn(router, 'navigate');
    component.logout();
    expect(authServiceSpy.logout).toHaveBeenCalled();
    expect(router.navigate).toHaveBeenCalledWith(['/login']);
  });

  it('template_Always_ShouldRenderNavbarBrandWithAppName', () => {
    const compiled = fixture.nativeElement as HTMLElement;
    const brand = compiled.querySelector('.navbar-brand');
    expect(brand).toBeTruthy();
    expect(brand?.textContent).toContain('TaskManager');
  });

  it('template_Always_ShouldRenderAvatarInitialsElement', () => {
    const compiled = fixture.nativeElement as HTMLElement;
    const avatar = compiled.querySelector('.avatar-initials');
    expect(avatar).toBeTruthy();
  });
});
