import { ComponentFixture, TestBed } from '@angular/core/testing';
import { SharedTasksComponent } from './shared-tasks.component';
import { TaskSharingService } from '../../../core/services/task-sharing.service';
import { AuthService } from '../../../core/services/auth.service';
import { RouterModule } from '@angular/router';
import { of, throwError } from 'rxjs';
import { InvitationResponse } from '../../../core/models/sharing.model';

describe('SharedTasksComponent', () => {
  let component: SharedTasksComponent;
  let fixture: ComponentFixture<SharedTasksComponent>;
  let taskSharingServiceSpy: jasmine.SpyObj<TaskSharingService>;
  let authServiceSpy: jasmine.SpyObj<AuthService>;

  const mockSharedTasks: InvitationResponse[] = [
    {
      id: 1,
      taskId: 10,
      taskTitle: 'Task Alpha',
      invitedByEmail: 'owner@test.com',
      role: 'Editor',
      invitedAt: '2026-01-01T00:00:00Z'
    }
  ];

  const mockPendingInvitations: InvitationResponse[] = [
    {
      id: 2,
      taskId: 20,
      taskTitle: 'Task Beta',
      invitedByEmail: 'admin@test.com',
      role: 'Viewer',
      invitedAt: '2026-01-02T00:00:00Z'
    }
  ];

  beforeEach(async () => {
    taskSharingServiceSpy = jasmine.createSpyObj('TaskSharingService', [
      'getSharedTasks',
      'getPendingInvitations',
      'acceptInvitation',
      'rejectInvitation',
      'removeCollaborator'
    ]);
    authServiceSpy = jasmine.createSpyObj('AuthService', ['getUserId']);

    taskSharingServiceSpy.getSharedTasks.and.returnValue(of(mockSharedTasks));
    taskSharingServiceSpy.getPendingInvitations.and.returnValue(of(mockPendingInvitations));
    authServiceSpy.getUserId.and.returnValue(42);

    await TestBed.configureTestingModule({
      imports: [SharedTasksComponent, RouterModule.forRoot([])],
      providers: [
        { provide: TaskSharingService, useValue: taskSharingServiceSpy },
        { provide: AuthService, useValue: authServiceSpy }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(SharedTasksComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should load shared tasks and pending invitations on init', () => {
    expect(taskSharingServiceSpy.getSharedTasks).toHaveBeenCalled();
    expect(taskSharingServiceSpy.getPendingInvitations).toHaveBeenCalled();
    expect(component.sharedTasks).toEqual(mockSharedTasks);
    expect(component.pendingInvitations).toEqual(mockPendingInvitations);
    expect(component.isLoading).toBeFalse();
  });

  it('should set currentUserId from authService on init', () => {
    expect(authServiceSpy.getUserId).toHaveBeenCalled();
    expect(component.currentUserId).toBe(42);
  });

  it('should set error message when loading data fails', () => {
    taskSharingServiceSpy.getSharedTasks.and.returnValue(throwError(() => new Error('Network error')));
    taskSharingServiceSpy.getPendingInvitations.and.returnValue(of([]));

    component.loadData();

    expect(component.errorMessage).toBe('Impossible de charger les tâches partagées.');
    expect(component.isLoading).toBeFalse();
  });

  it('should call acceptInvitation and reload data on accept', () => {
    taskSharingServiceSpy.acceptInvitation.and.returnValue(of(void 0));

    component.accept(mockPendingInvitations[0]);

    expect(taskSharingServiceSpy.acceptInvitation).toHaveBeenCalledWith(20);
    expect(taskSharingServiceSpy.getSharedTasks).toHaveBeenCalledTimes(2);
  });

  it('should set error message when acceptInvitation fails', () => {
    taskSharingServiceSpy.acceptInvitation.and.returnValue(throwError(() => new Error('Forbidden')));

    component.accept(mockPendingInvitations[0]);

    expect(component.errorMessage).toBe('Impossible d\'accepter l\'invitation.');
  });

  it('should call rejectInvitation and reload data on reject', () => {
    taskSharingServiceSpy.rejectInvitation.and.returnValue(of(void 0));

    component.reject(mockPendingInvitations[0]);

    expect(taskSharingServiceSpy.rejectInvitation).toHaveBeenCalledWith(20);
    expect(taskSharingServiceSpy.getSharedTasks).toHaveBeenCalledTimes(2);
  });

  it('should set error message when rejectInvitation fails', () => {
    taskSharingServiceSpy.rejectInvitation.and.returnValue(throwError(() => new Error('Forbidden')));

    component.reject(mockPendingInvitations[0]);

    expect(component.errorMessage).toBe('Impossible de refuser l\'invitation.');
  });

  it('should call removeCollaborator with currentUserId and reload on leaveTask', () => {
    taskSharingServiceSpy.removeCollaborator.and.returnValue(of(void 0));

    component.leaveTask(mockSharedTasks[0]);

    expect(taskSharingServiceSpy.removeCollaborator).toHaveBeenCalledWith(10, 42);
    expect(taskSharingServiceSpy.getSharedTasks).toHaveBeenCalledTimes(2);
  });

  it('should set error message when leaveTask fails', () => {
    taskSharingServiceSpy.removeCollaborator.and.returnValue(throwError(() => new Error('Error')));

    component.leaveTask(mockSharedTasks[0]);

    expect(component.errorMessage).toBe('Impossible de quitter cette tâche.');
  });

  it('should not call removeCollaborator when currentUserId is null', () => {
    component.currentUserId = null;
    component.leaveTask(mockSharedTasks[0]);

    expect(taskSharingServiceSpy.removeCollaborator).not.toHaveBeenCalled();
  });

  it('should return correct badge class for each role', () => {
    expect(component.getRoleBadgeClass('Editor')).toBe('bg-primary');
    expect(component.getRoleBadgeClass('Viewer')).toBe('bg-secondary');
    expect(component.getRoleBadgeClass('Owner')).toBe('bg-dark');
  });
});
