import { Component, DestroyRef, EventEmitter, inject, Input, OnChanges, Output, SimpleChanges } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { TaskSharingService } from '../../../core/services/task-sharing.service';
import { Task } from '../../../core/models/task.model';
import { CollaboratorResponse, TaskShareRole } from '../../../core/models/sharing.model';

@Component({
  selector: 'app-task-share-modal',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './task-share-modal.component.html'
})
export class TaskShareModalComponent implements OnChanges {
  private destroyRef = inject(DestroyRef);

  @Input() task: Task | null = null;
  @Output() collaboratorChanged = new EventEmitter<void>();

  inviteForm: FormGroup;
  collaborators: CollaboratorResponse[] = [];
  isLoading = false;
  errorMessage = '';

  constructor(
    private taskSharingService: TaskSharingService,
    private fb: FormBuilder
  ) {
    this.inviteForm = this.fb.group({
      email: ['', [Validators.required, Validators.email]],
      role: ['Editor', Validators.required]
    });
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['task'] && this.task) {
      this.loadCollaborators();
    }
  }

  loadCollaborators(): void {
    if (!this.task) return;
    this.taskSharingService.getCollaborators(this.task.id)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (collaborators) => {
          this.collaborators = collaborators;
        },
        error: () => {
          this.errorMessage = 'Impossible de charger les collaborateurs.';
        }
      });
  }

  invite(): void {
    if (this.inviteForm.invalid || !this.task) return;
    this.isLoading = true;
    this.errorMessage = '';
    this.taskSharingService.inviteCollaborator(this.task.id, this.inviteForm.value)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () => {
          this.inviteForm.reset({ email: '', role: 'Editor' });
          this.isLoading = false;
          this.loadCollaborators();
          this.collaboratorChanged.emit();
        },
        error: () => {
          this.errorMessage = 'Erreur lors de l\'invitation.';
          this.isLoading = false;
        }
      });
  }

  remove(c: CollaboratorResponse): void {
    if (!this.task) return;
    this.taskSharingService.removeCollaborator(this.task.id, c.userId)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () => {
          this.loadCollaborators();
          this.collaboratorChanged.emit();
        },
        error: () => {
          this.errorMessage = 'Impossible de retirer ce collaborateur.';
        }
      });
  }

  getRoleBadgeClass(role: TaskShareRole): string {
    const map: Record<TaskShareRole, string> = {
      Editor: 'bg-primary',
      Viewer: 'bg-secondary',
      Owner: 'bg-dark'
    };
    return map[role] ?? 'bg-secondary';
  }
}
