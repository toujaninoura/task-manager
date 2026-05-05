import { Routes } from '@angular/router';
import { DashboardComponent } from './dashboard/dashboard.component';
import { TaskListComponent } from './task-list/task-list.component';

export const TASKS_ROUTES: Routes = [
  { path: '', component: DashboardComponent },
  { path: 'list', component: TaskListComponent }
];
