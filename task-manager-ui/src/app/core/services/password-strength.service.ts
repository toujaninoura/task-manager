import { Injectable } from '@angular/core';

export interface PasswordStrength {
  score: number;
  cssClass: string;
  widthPercent: string;
}

@Injectable({ providedIn: 'root' })
export class PasswordStrengthService {
  evaluate(password: string): PasswordStrength {
    const score = this.computeScore(password);
    return {
      score,
      cssClass: score <= 1 ? 'bg-danger' : score <= 3 ? 'bg-warning' : 'bg-success',
      widthPercent: `${(score / 5) * 100}%`
    };
  }

  private computeScore(pwd: string): number {
    if (!pwd) return 0;
    let score = 0;
    if (pwd.length >= 8) score++;
    if (/[A-Z]/.test(pwd)) score++;
    if (/[a-z]/.test(pwd)) score++;
    if (/[0-9]/.test(pwd)) score++;
    if (/[^A-Za-z0-9]/.test(pwd)) score++;
    return score;
  }
}
