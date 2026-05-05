import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-task-list',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="container mt-4">
      <h1>Task list</h1>
    </div>
  `,
  styles: [`:host { display: block; }`]
})
export class TaskListComponent {}
