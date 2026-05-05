import { ComponentFixture, TestBed } from '@angular/core/testing';
import { RouterTestingModule } from '@angular/router/testing';
import { of } from 'rxjs';
import { DashboardComponent } from './dashboard.component';
import { TaskService } from '../../../core/services/task.service';
import { PagedResponse } from '../../../core/models/api-response.model';
import { Task } from '../../../core/models/task.model';

describe('DashboardComponent', () => {
  let component: DashboardComponent;
  let fixture: ComponentFixture<DashboardComponent>;
  let taskServiceSpy: jasmine.SpyObj<TaskService>;

  const emptyPagedResponse: PagedResponse<Task> = {
    data: [],
    page: 1,
    pageSize: 100,
    totalCount: 0,
    totalPages: 0,
    hasNext: false,
    hasPrev: false
  };

  beforeEach(async () => {
    taskServiceSpy = jasmine.createSpyObj('TaskService', ['getTasks']);
    taskServiceSpy.getTasks.and.returnValue(of(emptyPagedResponse));

    await TestBed.configureTestingModule({
      imports: [DashboardComponent, RouterTestingModule],
      providers: [
        { provide: TaskService, useValue: taskServiceSpy }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(DashboardComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should display the dashboard component', () => {
    expect(component).toBeTruthy();
  });

  it('should have stats at 0 by default when no tasks', () => {
    expect(component.stats.total).toBe(0);
    expect(component.stats.todo).toBe(0);
    expect(component.stats.inProgress).toBe(0);
    expect(component.stats.done).toBe(0);
  });

  it('should call taskService on init', () => {
    expect(taskServiceSpy.getTasks).toHaveBeenCalledWith(1, 100);
  });

  it('should compute stats correctly from tasks', () => {
    const mockTasks: Task[] = [
      { id: 1, title: 'T1', status: 'Todo', priority: 'Low', createdAt: '', updatedAt: '' },
      { id: 2, title: 'T2', status: 'InProgress', priority: 'Medium', createdAt: '', updatedAt: '' },
      { id: 3, title: 'T3', status: 'Done', priority: 'High', createdAt: '', updatedAt: '' },
      { id: 4, title: 'T4', status: 'Todo', priority: 'Low', createdAt: '', updatedAt: '' }
    ];
    taskServiceSpy.getTasks.and.returnValue(of({ ...emptyPagedResponse, data: mockTasks, totalCount: 4 }));
    component.ngOnInit();
    expect(component.stats.total).toBe(4);
    expect(component.stats.todo).toBe(2);
    expect(component.stats.inProgress).toBe(1);
    expect(component.stats.done).toBe(1);
  });
});
