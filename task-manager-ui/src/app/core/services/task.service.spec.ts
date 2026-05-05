import { TestBed } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { TaskService } from './task.service';

describe('TaskService', () => {
  let service: TaskService;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule]
    });
    service = TestBed.inject(TaskService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => httpMock.verify());

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  it('getTasks should call GET /api/v1/tasks', () => {
    const mockResponse = {
      success: true,
      data: {
        data: [],
        page: 1,
        pageSize: 10,
        totalCount: 0,
        totalPages: 0,
        hasNext: false,
        hasPrev: false
      }
    };

    service.getTasks().subscribe(result => {
      expect(result.data).toEqual([]);
      expect(result.totalCount).toBe(0);
    });

    const req = httpMock.expectOne(r => r.url.includes('/api/v1/tasks'));
    expect(req.request.method).toBe('GET');
    req.flush(mockResponse);
  });

  it('should call createTask endpoint with POST', () => {
    const mockTask = {
      id: 1,
      title: 'New Task',
      description: '',
      status: 'Todo',
      priority: 'High',
      createdAt: '2026-01-01T00:00:00Z',
      updatedAt: '2026-01-01T00:00:00Z'
    };
    const mockApiResponse = { success: true, data: mockTask };

    service.createTask({ title: 'New Task', status: 'Todo', priority: 'High' }).subscribe(task => {
      expect(task.id).toBe(1);
      expect(task.title).toBe('New Task');
    });

    const req = httpMock.expectOne('http://localhost:5000/api/v1/tasks');
    expect(req.request.method).toBe('POST');
    req.flush(mockApiResponse);
  });

  it('should call deleteTask endpoint with DELETE', () => {
    service.deleteTask(1).subscribe();

    const req = httpMock.expectOne('http://localhost:5000/api/v1/tasks/1');
    expect(req.request.method).toBe('DELETE');
    req.flush(null);
  });
});
