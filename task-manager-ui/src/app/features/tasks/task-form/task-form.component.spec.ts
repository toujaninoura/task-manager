import { ComponentFixture, TestBed } from '@angular/core/testing';
import { RouterTestingModule } from '@angular/router/testing';
import { ActivatedRoute, convertToParamMap } from '@angular/router';
import { of, throwError } from 'rxjs';
import { TaskFormComponent } from './task-form.component';
import { TaskService } from '../../../core/services/task.service';
import { Task } from '../../../core/models/task.model';

describe('TaskFormComponent', () => {
  let component: TaskFormComponent;
  let fixture: ComponentFixture<TaskFormComponent>;
  let taskServiceSpy: jasmine.SpyObj<TaskService>;

  const mockTask: Task = {
    id: 1,
    title: 'Test task',
    description: 'Description test',
    status: 'Todo',
    priority: 'Medium',
    dueDate: '2026-12-31T00:00:00',
    createdAt: '2026-01-01T00:00:00',
    updatedAt: '2026-01-01T00:00:00'
  };

  function createComponent(routeParams: { id?: string } = {}): void {
    TestBed.configureTestingModule({
      imports: [TaskFormComponent, RouterTestingModule],
      providers: [
        { provide: TaskService, useValue: taskServiceSpy },
        {
          provide: ActivatedRoute,
          useValue: {
            snapshot: {
              paramMap: convertToParamMap(routeParams)
            }
          }
        }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(TaskFormComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  }

  beforeEach(() => {
    taskServiceSpy = jasmine.createSpyObj('TaskService', [
      'getTaskById', 'createTask', 'updateTask'
    ]);
    taskServiceSpy.createTask.and.returnValue(of(mockTask));
    taskServiceSpy.updateTask.and.returnValue(of(mockTask));
    taskServiceSpy.getTaskById.and.returnValue(of(mockTask));
  });

  afterEach(() => {
    TestBed.resetTestingModule();
  });

  it('should display task form component', () => {
    createComponent();
    expect(component).toBeTruthy();
  });

  it('should initialize in create mode when no id in route', () => {
    createComponent();
    expect(component.isEditMode).toBeFalse();
    expect(component.taskId).toBeNull();
  });

  it('should initialize in edit mode when id is in route', () => {
    createComponent({ id: '1' });
    expect(component.isEditMode).toBeTrue();
    expect(component.taskId).toBe(1);
  });

  it('should call getTaskById on init in edit mode', () => {
    createComponent({ id: '1' });
    expect(taskServiceSpy.getTaskById).toHaveBeenCalledWith(1);
  });

  it('should not call getTaskById in create mode', () => {
    createComponent();
    expect(taskServiceSpy.getTaskById).not.toHaveBeenCalled();
  });

  it('should have invalid form when title is empty', () => {
    createComponent();
    component.form.get('title')?.setValue('');
    expect(component.form.invalid).toBeTrue();
  });

  it('should have valid form when title is filled', () => {
    createComponent();
    component.form.patchValue({
      title: 'Ma tache',
      status: 'Todo',
      priority: 'Medium'
    });
    expect(component.form.valid).toBeTrue();
  });

  it('should mark all fields as touched when submitting invalid form', () => {
    createComponent();
    component.form.get('title')?.setValue('');
    component.onSubmit();
    expect(component.form.get('title')?.touched).toBeTrue();
  });

  it('should call createTask when submitting in create mode', () => {
    createComponent();
    component.form.patchValue({
      title: 'Nouvelle tache',
      description: '',
      status: 'Todo',
      priority: 'Medium',
      dueDate: null
    });
    component.onSubmit();
    expect(taskServiceSpy.createTask).toHaveBeenCalled();
  });

  it('should call updateTask when submitting in edit mode', () => {
    createComponent({ id: '1' });
    component.form.patchValue({
      title: 'Tache modifiee',
      description: 'desc',
      status: 'Done',
      priority: 'High',
      dueDate: '2026-12-01'
    });
    component.onSubmit();
    expect(taskServiceSpy.updateTask).toHaveBeenCalledWith(1, jasmine.any(Object));
  });

  it('should pre-fill form values in edit mode', () => {
    createComponent({ id: '1' });
    expect(component.form.get('title')?.value).toBe('Test task');
    expect(component.form.get('status')?.value).toBe('Todo');
    expect(component.form.get('priority')?.value).toBe('Medium');
  });

  it('should set errorMessage when getTaskById fails', () => {
    taskServiceSpy.getTaskById.and.returnValue(throwError(() => new Error('Not found')));
    createComponent({ id: '99' });
    expect(component.errorMessage).toBe('Tâche introuvable.');
  });
});
