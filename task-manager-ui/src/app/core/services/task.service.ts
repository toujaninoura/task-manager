import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { Task, CreateTaskRequest, UpdateTaskRequest } from '../models/task.model';
import { ApiResponse, PagedResponse } from '../models/api-response.model';
import { environment } from '../../../environments/environment';

@Injectable({ providedIn: 'root' })
export class TaskService {
  private readonly API = `${environment.apiUrl}/api/v1/tasks`;

  constructor(private http: HttpClient) {}

  getTasks(page = 1, pageSize = 10): Observable<PagedResponse<Task>> {
    const params = new HttpParams()
      .set('page', page.toString())
      .set('pageSize', pageSize.toString());

    return this.http
      .get<ApiResponse<PagedResponse<Task>>>(this.API, { params })
      .pipe(map(r => r.data));
  }

  createTask(request: CreateTaskRequest): Observable<Task> {
    return this.http
      .post<ApiResponse<Task>>(this.API, request)
      .pipe(map(r => r.data));
  }

  updateTask(id: number, request: UpdateTaskRequest): Observable<Task> {
    return this.http
      .put<ApiResponse<Task>>(`${this.API}/${id}`, request)
      .pipe(map(r => r.data));
  }

  getTaskById(id: number): Observable<Task> {
    return this.http
      .get<ApiResponse<Task>>(`${this.API}/${id}`)
      .pipe(map(r => r.data));
  }

  deleteTask(id: number): Observable<void> {
    return this.http.delete<void>(`${this.API}/${id}`);
  }
}
