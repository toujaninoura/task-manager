import { ComponentFixture, TestBed } from '@angular/core/testing';
import { RouterTestingModule } from '@angular/router/testing';
import { of } from 'rxjs';
import { TaskListComponent } from './task-list.component';
import { TaskService } from '../../../core/services/task.service';
import { PagedResponse } from '../../../core/models/api-response.model';
import { Task } from '../../../core/models/task.model';

describe('TaskListComponent', () => {
  let component: TaskListComponent;
  let fixture: ComponentFixture<TaskListComponent>;
  let taskServiceSpy: jasmine.SpyObj<TaskService>;

  const emptyPagedResponse: PagedResponse<Task> = {
    data: [],
    page: 1,
    pageSize: 10,
    totalCount: 0,
    totalPages: 0,
    hasNext: false,
    hasPrev: false
  };

  beforeEach(async () => {
    taskServiceSpy = jasmine.createSpyObj('TaskService', ['getTasks', 'deleteTask']);
    taskServiceSpy.getTasks.and.returnValue(of(emptyPagedResponse));

    await TestBed.configureTestingModule({
      imports: [TaskListComponent, RouterTestingModule],
      providers: [
        { provide: TaskService, useValue: taskServiceSpy }
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
});
