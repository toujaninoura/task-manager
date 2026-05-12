import { Component } from '@angular/core';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter, Router } from '@angular/router';
import { provideLocationMocks } from '@angular/common/testing';
import { NavbarComponent } from './navbar.component';
import { AuthService } from '../../../core/services/auth.service';

@Component({ template: '', standalone: true })
class DummyComponent {}

describe('NavbarComponent', () => {
  let component: NavbarComponent;
  let fixture: ComponentFixture<NavbarComponent>;
  let authServiceSpy: jasmine.SpyObj<AuthService>;
  let router: Router;

  async function setup(emailReturn: string | null = 'test@example.com'): Promise<void> {
    authServiceSpy = jasmine.createSpyObj('AuthService', ['getUserEmail', 'logout']);
    authServiceSpy.getUserEmail.and.returnValue(emailReturn);

    await TestBed.configureTestingModule({
      imports: [NavbarComponent],
      providers: [
        provideRouter([
          { path: 'tasks', component: DummyComponent },
          { path: 'tasks/list', component: DummyComponent }
        ]),
        provideLocationMocks(),
        { provide: AuthService, useValue: authServiceSpy }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(NavbarComponent);
    component = fixture.componentInstance;
    router = TestBed.inject(Router);
    fixture.detectChanges();
  }

  afterEach(() => TestBed.resetTestingModule());

  it('create_Always_ShouldInstantiateComponent', async () => {
    await setup();
    expect(component).toBeTruthy();
  });

  it('userEmail_WhenEmailExists_ShouldReturnEmail', async () => {
    await setup('toujani@example.com');
    expect(component.userEmail).toBe('toujani@example.com');
  });

  it('userEmail_WhenEmailIsNull_ShouldReturnEmptyString', async () => {
    await setup(null);
    expect(component.userEmail).toBe('');
  });

  it('userInitials_WhenEmailExists_ShouldReturnFirst2LettersUppercase', async () => {
    await setup('toujani@example.com');
    expect(component.userInitials).toBe('TO');
  });

  it('userInitials_WhenEmailIsNull_ShouldReturnEmptyString', async () => {
    await setup(null);
    expect(component.userInitials).toBe('');
  });

  it('logout_WhenCalled_ShouldCallAuthServiceLogoutAndNavigateToLogin', async () => {
    await setup();
    spyOn(router, 'navigate');
    component.logout();
    expect(authServiceSpy.logout).toHaveBeenCalled();
    expect(router.navigate).toHaveBeenCalledWith(['/login']);
  });

  it('template_Always_ShouldRenderNavbarBrandWithAppName', async () => {
    await setup();
    const compiled = fixture.nativeElement as HTMLElement;
    const brand = compiled.querySelector('.navbar-brand');
    expect(brand).toBeTruthy();
    expect(brand?.textContent).toContain('TaskManager');
  });

  it('template_Always_ShouldRenderAvatarInitialsElement', async () => {
    await setup();
    const compiled = fixture.nativeElement as HTMLElement;
    expect(compiled.querySelector('.avatar-initials')).toBeTruthy();
  });

  it('template_Dashboard_WhenRouteActive_ShouldApplyActiveClass', async () => {
    await setup();
    await router.navigate(['/tasks']);
    await fixture.whenStable();
    fixture.detectChanges();
    const navLinks = fixture.nativeElement.querySelectorAll('.navbar-nav a[routerlink="/tasks"]') as NodeListOf<HTMLElement>;
    const dashboardLink = Array.from(navLinks).find(el => el.classList.contains('nav-link'));
    expect(dashboardLink?.classList).toContain('active');
  });

  it('template_MyTasks_WhenRouteActive_ShouldApplyActiveClass', async () => {
    await setup();
    await router.navigate(['/tasks/list']);
    await fixture.whenStable();
    fixture.detectChanges();
    const myTasksLink = fixture.nativeElement.querySelector('.navbar-nav a[routerlink="/tasks/list"]') as HTMLElement;
    expect(myTasksLink?.classList).toContain('active');
  });

  it('template_SharedTasks_ShouldBeSpanWithDisabledClass', async () => {
    await setup();
    const compiled = fixture.nativeElement as HTMLElement;
    const sharedSpan = compiled.querySelector('span.nav-link.disabled');
    expect(sharedSpan).toBeTruthy();
    expect(sharedSpan?.textContent).toContain('Tâches Partagées');
  });

  it('template_SharedTasks_ShouldHaveAriaDisabledTrue', async () => {
    await setup();
    const compiled = fixture.nativeElement as HTMLElement;
    const sharedSpan = compiled.querySelector('span.nav-link.disabled');
    expect(sharedSpan?.getAttribute('aria-disabled')).toBe('true');
  });

  it('template_SharedTasks_ShouldContainSoonBadge', async () => {
    await setup();
    const compiled = fixture.nativeElement as HTMLElement;
    const badge = compiled.querySelector('span.nav-link.disabled .badge');
    expect(badge).toBeTruthy();
    expect(badge?.textContent?.trim()).toBe('Bientôt');
  });
});
