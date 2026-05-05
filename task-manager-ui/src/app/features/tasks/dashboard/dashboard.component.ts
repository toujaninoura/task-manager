import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { TaskService } from '../../../core/services/task.service';
import { Task } from '../../../core/models/task.model';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './dashboard.component.html'
})
export class DashboardComponent implements OnInit {
  tasks: Task[] = [];
  stats = { total: 0, todo: 0, inProgress: 0, done: 0 };

  constructor(private taskService: TaskService) {}

  ngOnInit(): void {
    this.taskService.getTasks(1, 100).subscribe(response => {
      this.tasks = response.data;
      this.computeStats();
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
