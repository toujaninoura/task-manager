import { Component } from '@angular/core';
import { Router, RouterOutlet } from '@angular/router';
import { NgIf } from '@angular/common';
import { NavbarComponent } from './shared/components/navbar/navbar.component';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet, NgIf, NavbarComponent],
  template: `
    <app-navbar *ngIf="showNavbar" />
    <router-outlet />
  `
})
export class App {
  constructor(private router: Router) {}

  get showNavbar(): boolean {
    return !['login', 'register'].some(path => this.router.url.includes('/' + path));
  }
}
