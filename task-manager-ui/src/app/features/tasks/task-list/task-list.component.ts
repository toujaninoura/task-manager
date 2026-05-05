import { Component, DestroyRef, inject, OnInit } from '@angular/core';
import { CommonModule, DatePipe, SlicePipe } from '@angular/common';
import { RouterLink } from '@angular/router';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { TaskService } from '../../../core/services/task.service';
import { Task, TaskStatus, TaskPriority } from '../../../core/models/task.model';

@Component({
  selector: 'app-task-list',
  standalone: true,
  imports: [CommonModule, RouterLink, DatePipe, SlicePipe],
  templateUrl: './task-list.component.html'
})
export class TaskListComponent implements OnInit {
  private destroyRef = inject(DestroyRef);
  tasks: Task[] = [];
  loading = false;
  errorMessage = '';

  constructor(private taskService: TaskService) {}

  ngOnInit(): void {
    this.loadTasks();
  }

  loadTasks(): void {
    this.loading = true;
    this.taskService.getTasks()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (response) => {
          this.tasks = response.data;
          this.loading = false;
        },
        error: () => {
          this.errorMessage = 'Erreur lors du chargement.';
          this.loading = false;
        }
      });
  }

  deleteTask(id: number): void {
    if (!confirm('Supprimer cette tâche ?')) return;
    this.taskService.deleteTask(id)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () => this.loadTasks(),
        error: () => { this.errorMessage = 'Impossible de supprimer cette tâche.'; }
      });
  }

  trackById(_index: number, task: Task): number {
    return task.id;
  }

  getStatusBadge(status: TaskStatus): string {
    const map: Record<TaskStatus, string> = {
      Todo: 'secondary',
      InProgress: 'warning',
      Done: 'success'
    };
    return map[status] ?? 'secondary';
  }

  getPriorityBadge(priority: TaskPriority): string {
    const map: Record<TaskPriority, string> = {
      Low: 'info',
      Medium: 'warning',
      High: 'danger'
    };
    return map[priority] ?? 'secondary';
  }
}
