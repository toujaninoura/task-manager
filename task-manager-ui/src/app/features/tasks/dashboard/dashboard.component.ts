import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="container mt-4">
      <h1>Dashboard</h1>
    </div>
  `,
  styles: [`:host { display: block; }`]
})
export class DashboardComponent {}
