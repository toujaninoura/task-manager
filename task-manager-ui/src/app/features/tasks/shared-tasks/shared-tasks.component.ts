import { Component, DestroyRef, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { EMPTY, forkJoin, Subject } from 'rxjs';
import { catchError, switchMap } from 'rxjs/operators';
import { TaskSharingService } from '../../../core/services/task-sharing.service';
import { AuthService } from '../../../core/services/auth.service';
import { InvitationResponse, TaskShareRole } from '../../../core/models/sharing.model';

@Component({
  selector: 'app-shared-tasks',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './shared-tasks.component.html',
  styleUrl: './shared-tasks.component.css'
})
export class SharedTasksComponent implements OnInit {
  private readonly destroyRef = inject(DestroyRef);
  private readonly loadTrigger$ = new Subject<void>();

  sharedTasks: InvitationResponse[] = [];
  pendingInvitations: InvitationResponse[] = [];
  isLoading = false;
  errorMessage = '';
  currentUserId: number | null = null;

  constructor(
    private taskSharingService: TaskSharingService,
    private authService: AuthService
  ) {}

  ngOnInit(): void {
    this.currentUserId = this.authService.getUserId();

    this.loadTrigger$
      .pipe(
        switchMap(() => {
          this.isLoading = true;
          this.errorMessage = '';
          return forkJoin({
            shared: this.taskSharingService.getSharedTasks(),
            pending: this.taskSharingService.getPendingInvitations()
          }).pipe(
            catchError(() => {
              this.errorMessage = 'Impossible de charger les tâches partagées.';
              this.isLoading = false;
              return EMPTY;
            })
          );
        }),
        takeUntilDestroyed(this.destroyRef)
      )
      .subscribe(({ shared, pending }) => {
        this.sharedTasks = shared;
        this.pendingInvitations = pending;
        this.isLoading = false;
      });

    this.loadTrigger$.next();
  }

  loadData(): void {
    this.loadTrigger$.next();
  }

  accept(inv: InvitationResponse): void {
    this.taskSharingService.acceptInvitation(inv.taskId)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () => this.loadData(),
        error: () => { this.errorMessage = 'Impossible d\'accepter l\'invitation.'; }
      });
  }

  reject(inv: InvitationResponse): void {
    this.taskSharingService.rejectInvitation(inv.taskId)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () => this.loadData(),
        error: () => { this.errorMessage = 'Impossible de refuser l\'invitation.'; }
      });
  }

  leaveTask(t: InvitationResponse): void {
    if (!this.currentUserId) return;
    this.taskSharingService.removeCollaborator(t.taskId, this.currentUserId)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () => this.loadData(),
        error: () => { this.errorMessage = 'Impossible de quitter cette tâche.'; }
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
