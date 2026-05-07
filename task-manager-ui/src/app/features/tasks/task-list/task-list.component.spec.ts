import { ComponentFixture, TestBed } from '@angular/core/testing';
import { RouterTestingModule } from '@angular/router/testing';
import { of, throwError } from 'rxjs';
import { TaskListComponent } from './task-list.component';
import { TaskService } from '../../../core/services/task.service';
import { AuthService } from '../../../core/services/auth.service';
import { PagedResponse } from '../../../core/models/api-response.model';
import { Task } from '../../../core/models/task.model';

describe('TaskListComponent', () => {
  let component: TaskListComponent;
  let fixture: ComponentFixture<TaskListComponent>;
  let taskServiceSpy: jasmine.SpyObj<TaskService>;
  let authServiceSpy: jasmine.SpyObj<AuthService>;

  const mockTasks: Task[] = [
    {
      id: 1, title: 'Task Todo Low', status: 'Todo', priority: 'Low',
      createdAt: '2026-01-01T00:00:00', updatedAt: '2026-01-01T00:00:00',
      isShared: false, collaboratorCount: 0, userId: 1
    },
    {
      id: 2, title: 'Task InProgress Medium', status: 'InProgress', priority: 'Medium',
      createdAt: '2026-01-02T00:00:00', updatedAt: '2026-01-02T00:00:00',
      isShared: false, collaboratorCount: 0, userId: 1
    },
    {
      id: 3, title: 'Task Done High', status: 'Done', priority: 'High',
      createdAt: '2026-01-03T00:00:00', updatedAt: '2026-01-03T00:00:00',
      isShared: false, collaboratorCount: 0, userId: 1
    },
    {
      id: 4, title: 'Task Todo High', status: 'Todo', priority: 'High',
      createdAt: '2026-01-04T00:00:00', updatedAt: '2026-01-04T00:00:00',
      isShared: false, collaboratorCount: 0, userId: 1
    }
  ];

  const emptyPagedResponse: PagedResponse<Task> = {
    data: [],
    page: 1, pageSize: 10, totalCount: 0, totalPages: 0,
    hasNext: false, hasPrev: false
  };

  const filledPagedResponse: PagedResponse<Task> = {
    data: mockTasks,
    page: 1, pageSize: 10, totalCount: 4, totalPages: 1,
    hasNext: false, hasPrev: false
  };

  beforeEach(async () => {
    taskServiceSpy = jasmine.createSpyObj('TaskService', ['getTasks', 'deleteTask']);
    authServiceSpy = jasmine.createSpyObj('AuthService', ['getUserId']);
    taskServiceSpy.getTasks.and.returnValue(of(emptyPagedResponse));
    taskServiceSpy.deleteTask.and.returnValue(of(void 0));
    authServiceSpy.getUserId.and.returnValue(1);

    await TestBed.configureTestingModule({
      imports: [TaskListComponent, RouterTestingModule],
      providers: [
        { provide: TaskService, useValue: taskServiceSpy },
        { provide: AuthService, useValue: authServiceSpy }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(TaskListComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should display the task list component', () => {
    expect(component).toBeTruthy();
  });

  it('should call taskService on init', () => {
    expect(taskServiceSpy.getTasks).toHaveBeenCalled();
  });

  it('should return "success" badge for Done status', () => {
    expect(component.getStatusBadge('Done')).toBe('success');
  });

  it('should return "warning" badge for InProgress status', () => {
    expect(component.getStatusBadge('InProgress')).toBe('warning');
  });

  it('should return "secondary" badge for Todo status', () => {
    expect(component.getStatusBadge('Todo')).toBe('secondary');
  });

  it('should return "danger" badge for High priority', () => {
    expect(component.getPriorityBadge('High')).toBe('danger');
  });

  it('should return "warning" badge for Medium priority', () => {
    expect(component.getPriorityBadge('Medium')).toBe('warning');
  });

  it('should return "info" badge for Low priority', () => {
    expect(component.getPriorityBadge('Low')).toBe('info');
  });

  it('should have empty tasks array initially', () => {
    expect(component.tasks.length).toBe(0);
  });

  it('should show loading false after tasks loaded', () => {
    expect(component.loading).toBeFalse();
  });

  // --- Tests filtres issue #11 ---

  it('applyFilters_WithStatusTodo_ReturnsOnlyTodoTasks', () => {
    component.allTasks = mockTasks;
    component.filterStatus = 'Todo';
    component.filterPriority = '';
    component.applyFilters();

    expect(component.tasks.length).toBe(2);
    component.tasks.forEach(t => expect(t.status).toBe('Todo'));
    expect(component.filteredCount).toBe(2);
  });

  it('applyFilters_WithPriorityHigh_ReturnsOnlyHighPriorityTasks', () => {
    component.allTasks = mockTasks;
    component.filterStatus = '';
    component.filterPriority = 'High';
    component.applyFilters();

    expect(component.tasks.length).toBe(2);
    component.tasks.forEach(t => expect(t.priority).toBe('High'));
    expect(component.filteredCount).toBe(2);
  });

  it('applyFilters_WithStatusAndPriority_ReturnsCombinedFilter', () => {
    component.allTasks = mockTasks;
    component.filterStatus = 'Todo';
    component.filterPriority = 'High';
    component.applyFilters();

    expect(component.tasks.length).toBe(1);
    expect(component.tasks[0].id).toBe(4);
    expect(component.filteredCount).toBe(1);
  });

  it('resetFilters_ClearsAllFilters', () => {
    component.allTasks = mockTasks;
    component.filterStatus = 'Todo';
    component.filterPriority = 'High';
    component.applyFilters();
    expect(component.tasks.length).toBe(1);

    component.resetFilters();

    expect(component.filterStatus).toBe('');
    expect(component.filterPriority).toBe('');
    expect(component.tasks.length).toBe(4);
    expect(component.filteredCount).toBe(4);
  });

  it('should show error message when getTasks fails', () => {
    taskServiceSpy.getTasks.and.returnValue(throwError(() => new Error('Network error')));
    component.loadTasks();
    expect(component.errorMessage).toBe('Erreur lors du chargement.');
  });

  it('should load all tasks and apply filters after loadTasks', () => {
    taskServiceSpy.getTasks.and.returnValue(of(filledPagedResponse));
    component.loadTasks();

    expect(component.allTasks.length).toBe(4);
    expect(component.filteredCount).toBe(4);
  });
});
