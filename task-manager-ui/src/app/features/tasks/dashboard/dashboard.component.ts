import { Component, DestroyRef, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { TaskService } from '../../../core/services/task.service';
import { Task } from '../../../core/models/task.model';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './dashboard.component.html'
})
export class DashboardComponent implements OnInit {
  private destroyRef = inject(DestroyRef);
  tasks: Task[] = [];
  stats = { total: 0, todo: 0, inProgress: 0, done: 0 };
  errorMessage = '';

  constructor(private taskService: TaskService) {}

  ngOnInit(): void {
    this.taskService.getTasks(1, 100)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (response) => {
          this.tasks = response.data;
          this.computeStats();
        },
        error: () => {
          this.errorMessage = 'Impossible de charger les statistiques.';
        }
      });
  }

  private computeStats(): void {
    this.stats = {
      total: this.tasks.length,
      todo: this.tasks.filter(t => t.status === 'Todo').length,
      inProgress: this.tasks.filter(t => t.status === 'InProgress').length,
      done: this.tasks.filter(t => t.status === 'Done').length
    };
  }
}
