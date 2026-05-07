import { Routes } from '@angular/router';
import { DashboardComponent } from './dashboard/dashboard.component';
import { TaskListComponent } from './task-list/task-list.component';
import { TaskFormComponent } from './task-form/task-form.component';
import { SharedTasksComponent } from './shared-tasks/shared-tasks.component';
import { authGuard } from '../../core/guards/auth.guard';

export const TASKS_ROUTES: Routes = [
  { path: '', component: DashboardComponent },
  { path: 'list', component: TaskListComponent, canActivate: [authGuard] },
  { path: 'form', component: TaskFormComponent, canActivate: [authGuard] },
  { path: 'form/:id', component: TaskFormComponent, canActivate: [authGuard] },
  { path: 'shared', component: SharedTasksComponent, canActivate: [authGuard] }
];
