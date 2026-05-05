import { Component, OnInit } from '@angular/core';
import { CommonModule, DatePipe, SlicePipe } from '@angular/common';
import { RouterLink } from '@angular/router';
import { TaskService } from '../../../core/services/task.service';
import { Task, TaskStatus, TaskPriority } from '../../../core/models/task.model';

@Component({
  selector: 'app-task-list',
  standalone: true,
  imports: [CommonModule, RouterLink, DatePipe, SlicePipe],
  templateUrl: './task-list.component.html'
})
export class TaskListComponent implements OnInit {
  tasks: Task[] = [];
  loading = false;

  constructor(private taskService: TaskService) {}

  ngOnInit(): void {
    this.loadTasks();
  }

  loadTasks(): void {
    this.loading = true;
    this.taskService.getTasks().subscribe({
      next: (response) => {
        this.tasks = response.data;
        this.loading = false;
      },
      error: () => {
        this.loading = false;
      }
    });
  }

  deleteTask(id: number): void {
    if (!confirm('Supprimer cette tâche ?')) return;
    this.taskService.deleteTask(id).subscribe(() => this.loadTasks());
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
