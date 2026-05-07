import { Routes } from '@angular/router';
import { DashboardComponent } from './dashboard/dashboard.component';
import { TaskListComponent } from './task-list/task-list.component';
import { TaskFormComponent } from './task-form/task-form.component';
import { SharedTasksComponent } from './shared-tasks/shared-tasks.component';
import { authGuard } from '../../core/guards/auth.guard';

export const TASKS_ROUTES: Routes = [
  { path: '', component: DashboardComponent },
  { path: 'list', component: TaskListComponent },
  { path: 'form', component: TaskFormComponent },
  { path: 'form/:id', component: TaskFormComponent },
  { path: 'shared', component: SharedTasksComponent, canActivate: [authGuard] }
];
