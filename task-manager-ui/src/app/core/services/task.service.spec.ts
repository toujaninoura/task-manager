import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TaskService } from './task.service';
import { Task, CreateTaskRequest } from '../models/task.model';
import { ApiResponse, PagedResponse } from '../models/api-response.model';

describe('TaskService', () => {
  let service: TaskService;
  let httpMock: HttpTestingController;

  const mockTask: Task = {
    id: 1,
    title: 'Test Task',
    description: 'Test description',
    status: 'Todo',
    priority: 'Medium',
    createdAt: '2026-01-01T00:00:00Z',
    updatedAt: '2026-01-01T00:00:00Z'
  };

  const mockPagedResponse: ApiResponse<PagedResponse<Task>> = {
    success: true,
    data: {
      data: [mockTask],
      page: 1,
      pageSize: 10,
      totalCount: 1,
      totalPages: 1,
      hasNext: false,
      hasPrev: false
    }
  };

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        TaskService,
        provideHttpClient(),
        provideHttpClientTesting()
      ]
    });
    service = TestBed.inject(TaskService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  it('should call getTasks endpoint with pagination params', () => {
    service.getTasks(1, 10).subscribe(response => {
      expect(response.data.length).toBe(1);
      expect(response.data[0].title).toBe('Test Task');
    });

    const req = httpMock.expectOne(r => r.url === 'http://localhost:5000/api/v1/tasks');
    expect(req.request.method).toBe('GET');
    expect(req.request.params.get('page')).toBe('1');
    expect(req.request.params.get('pageSize')).toBe('10');
    req.flush(mockPagedResponse);
  });

  it('should call createTask endpoint with POST', () => {
    const createRequest: CreateTaskRequest = {
      title: 'New Task',
      status: 'Todo',
      priority: 'High'
    };

    const mockCreateResponse: ApiResponse<Task> = { success: true, data: mockTask };

    service.createTask(createRequest).subscribe(task => {
      expect(task.id).toBe(1);
      expect(task.title).toBe('Test Task');
    });

    const req = httpMock.expectOne('http://localhost:5000/api/v1/tasks');
    expect(req.request.method).toBe('POST');
    req.flush(mockCreateResponse);
  });

  it('should call updateTask endpoint with PUT', () => {
    const updateRequest: CreateTaskRequest = {
      title: 'Updated Task',
      status: 'InProgress',
      priority: 'Low'
    };
    const mockUpdateResponse: ApiResponse<Task> = {
      success: true,
      data: { ...mockTask, title: 'Updated Task', status: 'InProgress' }
    };

    service.updateTask(1, updateRequest).subscribe(task => {
      expect(task.title).toBe('Updated Task');
    });

    const req = httpMock.expectOne('http://localhost:5000/api/v1/tasks/1');
    expect(req.request.method).toBe('PUT');
    req.flush(mockUpdateResponse);
  });

  it('should call deleteTask endpoint with DELETE', () => {
    service.deleteTask(1).subscribe();

    const req = httpMock.expectOne('http://localhost:5000/api/v1/tasks/1');
    expect(req.request.method).toBe('DELETE');
    req.flush(null);
  });
});
