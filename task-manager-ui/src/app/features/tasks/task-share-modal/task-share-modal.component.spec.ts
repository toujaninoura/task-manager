import { ComponentFixture, TestBed } from '@angular/core/testing';
import { TaskShareModalComponent } from './task-share-modal.component';
import { TaskSharingService } from '../../../core/services/task-sharing.service';
import { ReactiveFormsModule } from '@angular/forms';
import { of, throwError } from 'rxjs';
import { Task } from '../../../core/models/task.model';
import { CollaboratorResponse } from '../../../core/models/sharing.model';
import { SimpleChange } from '@angular/core';

describe('TaskShareModalComponent', () => {
  let component: TaskShareModalComponent;
  let fixture: ComponentFixture<TaskShareModalComponent>;
  let taskSharingServiceSpy: jasmine.SpyObj<TaskSharingService>;

  const mockTask: Task = {
    id: 1,
    title: 'Test Task',
    status: 'Todo',
    priority: 'Medium',
    createdAt: '2026-01-01T00:00:00Z',
    updatedAt: '2026-01-01T00:00:00Z',
    isShared: false,
    collaboratorCount: 0,
    userId: 42
  };

  const mockCollaborators: CollaboratorResponse[] = [
    {
      id: 10,
      userId: 99,
      email: 'collab@test.com',
      role: 'Editor',
      status: 'Accepted',
      invitedAt: '2026-01-01T00:00:00Z'
    }
  ];

  beforeEach(async () => {
    taskSharingServiceSpy = jasmine.createSpyObj('TaskSharingService', [
      'getCollaborators',
      'inviteCollaborator',
      'removeCollaborator'
    ]);

    taskSharingServiceSpy.getCollaborators.and.returnValue(of(mockCollaborators));

    await TestBed.configureTestingModule({
      imports: [TaskShareModalComponent, ReactiveFormsModule],
      providers: [
        { provide: TaskSharingService, useValue: taskSharingServiceSpy }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(TaskShareModalComponent);
    component = fixture.componentInstance;
    component.task = mockTask;
    fixture.detectChanges();
    component.ngOnChanges({
      task: new SimpleChange(null, mockTask, true)
    });
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should initialize inviteForm with empty email and Editor role', () => {
    expect(component.inviteForm.get('email')?.value).toBe('');
    expect(component.inviteForm.get('role')?.value).toBe('Editor');
  });

  it('should load collaborators when task input changes', () => {
    taskSharingServiceSpy.getCollaborators.calls.reset();

    component.task = mockTask;
    component.ngOnChanges({
      task: new SimpleChange(null, mockTask, true)
    });

    expect(taskSharingServiceSpy.getCollaborators).toHaveBeenCalledWith(mockTask.id);
    expect(component.collaborators).toEqual(mockCollaborators);
  });

  it('should not load collaborators when task is null on changes', () => {
    component.task = null;
    taskSharingServiceSpy.getCollaborators.calls.reset();
    component.ngOnChanges({
      task: new SimpleChange(mockTask, null, false)
    });

    expect(taskSharingServiceSpy.getCollaborators).not.toHaveBeenCalled();
  });

  it('should set error message when getCollaborators fails', () => {
    taskSharingServiceSpy.getCollaborators.and.returnValue(throwError(() => new Error('Error')));

    component.task = mockTask;
    component.ngOnChanges({
      task: new SimpleChange(null, mockTask, true)
    });

    expect(component.errorMessage).toBe('Impossible de charger les collaborateurs.');
  });

  it('should call inviteCollaborator and reload collaborators on invite', () => {
    const newCollab: CollaboratorResponse = {
      id: 11,
      userId: 55,
      email: 'new@test.com',
      role: 'Viewer',
      status: 'Pending',
      invitedAt: '2026-01-02T00:00:00Z'
    };
    taskSharingServiceSpy.inviteCollaborator.and.returnValue(of(newCollab));
    component.task = mockTask;

    component.inviteForm.setValue({ email: 'new@test.com', role: 'Viewer' });
    component.invite();

    expect(taskSharingServiceSpy.inviteCollaborator).toHaveBeenCalledWith(mockTask.id, {
      email: 'new@test.com',
      role: 'Viewer'
    });
    expect(taskSharingServiceSpy.getCollaborators).toHaveBeenCalledTimes(2);
    expect(component.isLoading).toBeFalse();
  });

  it('should not invite when form is invalid', () => {
    component.task = mockTask;
    component.inviteForm.setValue({ email: 'not-an-email', role: 'Editor' });
    component.invite();

    expect(taskSharingServiceSpy.inviteCollaborator).not.toHaveBeenCalled();
  });

  it('should not invite when task is null', () => {
    component.task = null;
    component.inviteForm.setValue({ email: 'valid@test.com', role: 'Editor' });
    component.invite();

    expect(taskSharingServiceSpy.inviteCollaborator).not.toHaveBeenCalled();
  });

  it('should set error message when invite fails', () => {
    taskSharingServiceSpy.inviteCollaborator.and.returnValue(throwError(() => new Error('Error')));
    component.task = mockTask;
    component.inviteForm.setValue({ email: 'valid@test.com', role: 'Editor' });

    component.invite();

    expect(component.errorMessage).toBe('Erreur lors de l\'invitation.');
    expect(component.isLoading).toBeFalse();
  });

  it('should emit collaboratorChanged after successful invite', () => {
    const collab: CollaboratorResponse = {
      id: 12, userId: 66, email: 'x@test.com', role: 'Editor', status: 'Pending', invitedAt: ''
    };
    taskSharingServiceSpy.inviteCollaborator.and.returnValue(of(collab));
    component.task = mockTask;
    component.inviteForm.setValue({ email: 'x@test.com', role: 'Editor' });

    spyOn(component.collaboratorChanged, 'emit');
    component.invite();

    expect(component.collaboratorChanged.emit).toHaveBeenCalled();
  });

  it('should call removeCollaborator and reload collaborators on remove', () => {
    taskSharingServiceSpy.removeCollaborator.and.returnValue(of(void 0));
    component.task = mockTask;

    component.remove(mockCollaborators[0]);

    expect(taskSharingServiceSpy.removeCollaborator).toHaveBeenCalledWith(mockTask.id, mockCollaborators[0].userId);
    expect(taskSharingServiceSpy.getCollaborators).toHaveBeenCalledTimes(2);
  });

  it('should set error message when remove fails', () => {
    taskSharingServiceSpy.removeCollaborator.and.returnValue(throwError(() => new Error('Error')));
    component.task = mockTask;

    component.remove(mockCollaborators[0]);

    expect(component.errorMessage).toBe('Impossible de retirer ce collaborateur.');
  });

  it('should emit collaboratorChanged after successful remove', () => {
    taskSharingServiceSpy.removeCollaborator.and.returnValue(of(void 0));
    component.task = mockTask;

    spyOn(component.collaboratorChanged, 'emit');
    component.remove(mockCollaborators[0]);

    expect(component.collaboratorChanged.emit).toHaveBeenCalled();
  });

  it('should emit modalClosed when close is called', () => {
    spyOn(component.modalClosed, 'emit');
    component.close();
    expect(component.modalClosed.emit).toHaveBeenCalled();
  });

  it('should return correct badge class for each role', () => {
    expect(component.getRoleBadgeClass('Editor')).toBe('bg-primary');
    expect(component.getRoleBadgeClass('Viewer')).toBe('bg-secondary');
    expect(component.getRoleBadgeClass('Owner')).toBe('bg-dark');
  });
});
