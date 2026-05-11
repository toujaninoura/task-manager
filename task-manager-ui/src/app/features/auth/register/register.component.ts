import { Component, DestroyRef, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { AbstractControl, ReactiveFormsModule, FormBuilder, FormGroup, Validators, ValidationErrors } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { AuthService } from '../../../core/services/auth.service';

function passwordMatchValidator(group: AbstractControl): ValidationErrors | null {
  const password = group.get('password')?.value;
  const confirm = group.get('confirmPassword')?.value;
  return password && confirm && password !== confirm ? { passwordMismatch: true } : null;
}

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink],
  templateUrl: './register.component.html',
  styleUrl: './register.component.css'
})
export class RegisterComponent {
  private destroyRef = inject(DestroyRef);
  form: FormGroup;
  isLoading = false;
  errorMessage = '';

  constructor(
    private fb: FormBuilder,
    private authService: AuthService,
    private router: Router
  ) {
    this.form = this.fb.group(
      {
        firstName: ['', [Validators.required]],
        lastName: ['', [Validators.required]],
        email: ['', [Validators.required, Validators.email]],
        password: ['', [Validators.required, Validators.minLength(8)]],
        confirmPassword: ['', [Validators.required]]
      },
      { validators: passwordMatchValidator }
    );
  }

  field(name: string) {
    return this.form.get(name);
  }

  isInvalid(name: string): boolean {
    const c = this.field(name);
    return !!(c?.invalid && c?.touched);
  }

  get passwordMismatch(): boolean {
    return !!(this.form.hasError('passwordMismatch') && this.field('confirmPassword')?.touched);
  }

  get passwordStrengthScore(): number {
    const pwd: string = this.field('password')?.value ?? '';
    if (!pwd) return 0;
    let score = 0;
    if (pwd.length >= 8) score++;
    if (/[A-Z]/.test(pwd)) score++;
    if (/[a-z]/.test(pwd)) score++;
    if (/[0-9]/.test(pwd)) score++;
    if (/[^A-Za-z0-9]/.test(pwd)) score++;
    return score;
  }

  get passwordStrengthClass(): string {
    const s = this.passwordStrengthScore;
    if (s <= 1) return 'bg-danger';
    if (s <= 3) return 'bg-warning';
    return 'bg-success';
  }

  get passwordStrengthWidth(): string {
    return `${(this.passwordStrengthScore / 5) * 100}%`;
  }

  onSubmit(): void {
    if (this.form.invalid || this.isLoading) return;
    this.isLoading = true;
    this.errorMessage = '';

    const { firstName, lastName, email, password } = this.form.value;
    this.authService.register({ firstName, lastName, email, password })
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () => { this.isLoading = false; this.router.navigate(['/login']); },
        error: (err: HttpErrorResponse) => {
          this.isLoading = false;
          this.errorMessage = err.error?.message || 'Une erreur est survenue. Veuillez réessayer.';
        }
      });
  }
}
