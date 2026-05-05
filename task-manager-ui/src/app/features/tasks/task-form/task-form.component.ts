import { Component, DestroyRef, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router, ActivatedRoute, RouterLink } from '@angular/router';
import { HttpErrorResponse } from '@angular/common/http';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { TaskService } from '../../../core/services/task.service';

@Component({
  selector: 'app-task-form',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink],
  templateUrl: './task-form.component.html',
  styleUrl: './task-form.component.css'
})
export class TaskFormComponent implements OnInit {
  private destroyRef = inject(DestroyRef);

  form: FormGroup;
  isEditMode = false;
  taskId: number | null = null;
  errorMessage = '';
  loading = false;

  constructor(
    private fb: FormBuilder,
    private taskService: TaskService,
    private router: Router,
    private route: ActivatedRoute
  ) {
    this.form = this.fb.group({
      title: ['', [Validators.required, Validators.maxLength(200)]],
      description: ['', [Validators.maxLength(1000)]],
      status: ['Todo', Validators.required],
      priority: ['Medium', Validators.required],
      dueDate: [null]
    });
  }

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.isEditMode = true;
      this.taskId = +id;
      this.loading = true;

      this.taskService.getTaskById(this.taskId)
        .pipe(takeUntilDestroyed(this.destroyRef))
        .subscribe({
          next: (task) => {
            this.form.patchValue({
              title: task.title,
              description: task.description ?? '',
              status: task.status,
              priority: task.priority,
              dueDate: task.dueDate ? task.dueDate.substring(0, 10) : null
            });
            this.loading = false;
          },
          error: () => {
            this.errorMessage = 'Tâche introuvable.';
            this.loading = false;
          }
        });
    }
  }

  onSubmit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const v = this.form.value;
    const request = {
      title: v.title as string,
      description: v.description || undefined,
      status: v.status,
      priority: v.priority,
      dueDate: v.dueDate || undefined
    };

    const obs$ = this.isEditMode && this.taskId
      ? this.taskService.updateTask(this.taskId, request)
      : this.taskService.createTask(request);

    obs$.pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: () => this.router.navigate(['/tasks/list']),
      error: (err: HttpErrorResponse) => {
        this.errorMessage = err.error?.message || 'Erreur lors de la sauvegarde.';
      }
    });
  }
}
