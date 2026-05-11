import { TestBed } from '@angular/core/testing';
import { PasswordStrengthService } from './password-strength.service';

describe('PasswordStrengthService', () => {
  let service: PasswordStrengthService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(PasswordStrengthService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  it('should return score 0 for empty string', () => {
    expect(service.evaluate('').score).toBe(0);
  });

  it('should score length >= 8', () => {
    expect(service.evaluate('abcdefgh').score).toBeGreaterThanOrEqual(1);
  });

  it('should score uppercase letter', () => {
    expect(service.evaluate('A').score).toBeGreaterThanOrEqual(1);
  });

  it('should score number', () => {
    expect(service.evaluate('1').score).toBeGreaterThanOrEqual(1);
  });

  it('should score special char', () => {
    expect(service.evaluate('!').score).toBeGreaterThanOrEqual(1);
  });

  it('should return score 5 for very strong password', () => {
    expect(service.evaluate('Abc1defg!').score).toBe(5);
  });

  it('should return bg-danger for score <= 1', () => {
    expect(service.evaluate('a').cssClass).toBe('bg-danger');
  });

  it('should return bg-warning for score 2-3', () => {
    expect(service.evaluate('abcdefgh').cssClass).toBe('bg-warning');
  });

  it('should return bg-success for score >= 4', () => {
    expect(service.evaluate('Abcdefg1!').cssClass).toBe('bg-success');
  });

  it('should return 100% width for score 5', () => {
    expect(service.evaluate('Abc1defg!').widthPercent).toBe('100%');
  });

  it('should return 0% width for empty password', () => {
    expect(service.evaluate('').widthPercent).toBe('0%');
  });
});
