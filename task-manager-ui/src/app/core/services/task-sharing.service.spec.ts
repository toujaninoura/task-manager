import { TestBed } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { TaskSharingService } from './task-sharing.service';
import { environment } from '../../../environments/environment';
import {
  CollaboratorResponse,
  InvitationResponse,
  InviteCollaboratorRequest
} from '../models/sharing.model';
import { ApiResponse } from '../models/api-response.model';

describe('TaskSharingService', () => {
  let service: TaskSharingService;
  let httpMock: HttpTestingController;

  const BASE_URL = `${environment.apiUrl}/api/v1/tasks`;

  const mockCollaborator: CollaboratorResponse = {
    id: 1,
    userId: 99,
    email: 'collab@test.com',
    role: 'Editor',
    status: 'Accepted',
    invitedAt: '2026-01-01T00:00:00Z'
  };

  const mockInvitation: InvitationResponse = {
    id: 2,
    taskId: 10,
    taskTitle: 'Task Alpha',
    invitedByEmail: 'owner@test.com',
    role: 'Viewer',
    invitedAt: '2026-01-02T00:00:00Z'
  };

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule]
    });
    service = TestBed.inject(TaskSharingService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => httpMock.verify());

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  describe('getCollaborators', () => {
    it('should GET collaborators for a task', () => {
      const mockResponse: ApiResponse<CollaboratorResponse[]> = {
        success: true,
        data: [mockCollaborator]
      };

      service.getCollaborators(10).subscribe(result => {
        expect(result).toEqual([mockCollaborator]);
      });

      const req = httpMock.expectOne(`${BASE_URL}/10/collaborators`);
      expect(req.request.method).toBe('GET');
      req.flush(mockResponse);
    });
  });

  describe('inviteCollaborator', () => {
    it('should POST to invite a collaborator', () => {
      const inviteRequest: InviteCollaboratorRequest = { email: 'new@test.com', role: 'Editor' };
      const mockResponse: ApiResponse<CollaboratorResponse> = {
        success: true,
        data: mockCollaborator
      };

      service.inviteCollaborator(10, inviteRequest).subscribe(result => {
        expect(result).toEqual(mockCollaborator);
      });

      const req = httpMock.expectOne(`${BASE_URL}/10/collaborators`);
      expect(req.request.method).toBe('POST');
      expect(req.request.body).toEqual(inviteRequest);
      req.flush(mockResponse);
    });
  });

  describe('removeCollaborator', () => {
    it('should DELETE a collaborator from a task', () => {
      service.removeCollaborator(10, 99).subscribe(result => {
        expect(result).toBeUndefined();
      });

      const req = httpMock.expectOne(`${BASE_URL}/10/collaborators/99`);
      expect(req.request.method).toBe('DELETE');
      req.flush(null);
    });
  });

  describe('getSharedTasks', () => {
    it('should GET shared tasks for the current user', () => {
      const mockResponse: ApiResponse<InvitationResponse[]> = {
        success: true,
        data: [mockInvitation]
      };

      service.getSharedTasks().subscribe(result => {
        expect(result).toEqual([mockInvitation]);
      });

      const req = httpMock.expectOne(`${BASE_URL}/shared`);
      expect(req.request.method).toBe('GET');
      req.flush(mockResponse);
    });
  });

  describe('getPendingInvitations', () => {
    it('should GET pending invitations for the current user', () => {
      const mockResponse: ApiResponse<InvitationResponse[]> = {
        success: true,
        data: [mockInvitation]
      };

      service.getPendingInvitations().subscribe(result => {
        expect(result).toEqual([mockInvitation]);
      });

      const req = httpMock.expectOne(`${BASE_URL}/invitations/pending`);
      expect(req.request.method).toBe('GET');
      req.flush(mockResponse);
    });
  });

  describe('acceptInvitation', () => {
    it('should POST to accept an invitation', () => {
      service.acceptInvitation(10).subscribe(result => {
        expect(result).toBeUndefined();
      });

      const req = httpMock.expectOne(`${BASE_URL}/10/invitations/accept`);
      expect(req.request.method).toBe('POST');
      req.flush(null);
    });
  });

  describe('rejectInvitation', () => {
    it('should POST to reject an invitation', () => {
      service.rejectInvitation(10).subscribe(result => {
        expect(result).toBeUndefined();
      });

      const req = httpMock.expectOne(`${BASE_URL}/10/invitations/reject`);
      expect(req.request.method).toBe('POST');
      req.flush(null);
    });
  });
});
