import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import {
  CollaboratorResponse,
  InvitationResponse,
  InviteCollaboratorRequest
} from '../models/sharing.model';
import { ApiResponse } from '../models/api-response.model';
import { environment } from '../../../environments/environment';

@Injectable({ providedIn: 'root' })
export class TaskSharingService {
  private readonly API = `${environment.apiUrl}/api/v1/tasks`;

  constructor(private http: HttpClient) {}

  inviteCollaborator(taskId: number, request: InviteCollaboratorRequest): Observable<CollaboratorResponse> {
    return this.http
      .post<ApiResponse<CollaboratorResponse>>(`${this.API}/${taskId}/collaborators`, request)
      .pipe(map(r => r.data));
  }

  removeCollaborator(taskId: number, userId: number): Observable<void> {
    return this.http
      .delete<void>(`${this.API}/${taskId}/collaborators/${userId}`)
      .pipe(map(() => void 0));
  }

  getCollaborators(taskId: number): Observable<CollaboratorResponse[]> {
    return this.http
      .get<ApiResponse<CollaboratorResponse[]>>(`${this.API}/${taskId}/collaborators`)
      .pipe(map(r => r.data));
  }

  getSharedTasks(): Observable<InvitationResponse[]> {
    return this.http
      .get<ApiResponse<InvitationResponse[]>>(`${this.API}/shared`)
      .pipe(map(r => r.data));
  }

  getPendingInvitations(): Observable<InvitationResponse[]> {
    return this.http
      .get<ApiResponse<InvitationResponse[]>>(`${this.API}/shared/pending`)
      .pipe(map(r => r.data));
  }

  acceptInvitation(taskId: number): Observable<void> {
    return this.http
      .post<void>(`${this.API}/${taskId}/collaborators/accept`, {})
      .pipe(map(() => void 0));
  }

  rejectInvitation(taskId: number): Observable<void> {
    return this.http
      .post<void>(`${this.API}/${taskId}/collaborators/reject`, {})
      .pipe(map(() => void 0));
  }
}
