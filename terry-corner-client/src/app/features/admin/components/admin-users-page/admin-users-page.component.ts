import { Component, OnInit, inject, signal } from '@angular/core';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { DatePipe } from '@angular/common';
import { UsersAdminService } from '../../services/users-admin.service';
import { AdminUser, UserStats } from '../../models/admin.models';
import { ToastService } from '../../../../core/services/toast.service';
import { AuthService } from '../../../../core/services/auth.service';

@Component({
  selector: 'tc-admin-users-page',
  standalone: true,
  imports: [ReactiveFormsModule, DatePipe],
  templateUrl: './admin-users-page.component.html',
  styleUrl: './admin-users-page.component.scss',
})
export class AdminUsersPageComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly usersAdmin = inject(UsersAdminService);
  private readonly toast = inject(ToastService);
  private readonly auth = inject(AuthService);

  readonly users = signal<AdminUser[]>([]);
  readonly stats = signal<UserStats | null>(null);
  readonly loading = signal(true);
  readonly editingId = signal<string | null>(null);
  readonly saving = signal(false);
  readonly busyId = signal<string | null>(null);
  readonly previewUrls = signal<Record<string, string>>({});
  readonly creatingManager = signal(false);
  readonly savingManager = signal(false);

  readonly currentUserId = this.auth.user()?.id;

  readonly form = this.fb.nonNullable.group({
    fullName: ['', [Validators.required, Validators.maxLength(150)]],
    email: ['', [Validators.required, Validators.email]],
    phoneNumber: [''],
  });

  readonly managerForm = this.fb.nonNullable.group({
    fullName: ['', [Validators.required, Validators.maxLength(150)]],
    email: ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required, Validators.minLength(10)]],
  });

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.loading.set(true);
    this.usersAdmin.getUsers().subscribe({
      next: (users) => {
        this.users.set(users);
        this.loading.set(false);
        users.filter((u) => u.hasProfilePicture).forEach((u) => this.loadPreview(u.id));
      },
      error: () => this.loading.set(false),
    });
    this.usersAdmin.getStats().subscribe({ next: (s) => this.stats.set(s), error: () => {} });
  }

  private loadPreview(userId: string): void {
    this.usersAdmin.getProfilePictureUrl(userId).subscribe({
      next: (url) => this.previewUrls.update((map) => ({ ...map, [userId]: url })),
      error: () => {},
    });
  }

  startEdit(user: AdminUser): void {
    this.editingId.set(user.id);
    this.form.reset({ fullName: user.fullName, email: user.email, phoneNumber: user.phoneNumber ?? '' });
  }

  cancel(): void {
    this.editingId.set(null);
  }

  save(user: AdminUser): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const { fullName, email, phoneNumber } = this.form.getRawValue();
    this.saving.set(true);

    this.usersAdmin.updateUser(user.id, fullName, email, phoneNumber).subscribe({
      next: () => {
        this.toast.success('User updated.');
        this.saving.set(false);
        this.editingId.set(null);
        this.load();
      },
      error: () => this.saving.set(false),
    });
  }

  restrict(user: AdminUser): void {
    if (!confirm(`Restrict ${user.fullName}? They'll be signed out immediately and can't sign back in.`)) return;

    this.busyId.set(user.id);
    this.usersAdmin.restrictUser(user.id).subscribe({
      next: () => {
        this.toast.success(`${user.fullName} has been restricted.`);
        this.load();
        this.busyId.set(null);
      },
      error: () => this.busyId.set(null),
    });
  }

  unrestrict(user: AdminUser): void {
    this.busyId.set(user.id);
    this.usersAdmin.unrestrictUser(user.id).subscribe({
      next: () => {
        this.toast.success(`${user.fullName} can sign in again.`);
        this.load();
        this.busyId.set(null);
      },
      error: () => this.busyId.set(null),
    });
  }

  startCreateManager(): void {
    this.creatingManager.set(true);
    this.managerForm.reset({ fullName: '', email: '', password: '' });
  }

  cancelCreateManager(): void {
    this.creatingManager.set(false);
  }

  saveManager(): void {
    if (this.managerForm.invalid) {
      this.managerForm.markAllAsTouched();
      return;
    }

    const { fullName, email, password } = this.managerForm.getRawValue();
    this.savingManager.set(true);

    this.usersAdmin.createManager(fullName, email, password).subscribe({
      next: () => {
        this.toast.success(`Manager account created for ${fullName}.`);
        this.savingManager.set(false);
        this.creatingManager.set(false);
        this.load();
      },
      error: () => this.savingManager.set(false),
    });
  }

  changeRole(user: AdminUser, newRole: string): void {
    if (!newRole || newRole === user.roles[0]) return;
    if (!confirm(`Change ${user.fullName}'s role to ${newRole}?`)) return;

    this.busyId.set(user.id);
    this.usersAdmin.changeRole(user.id, newRole).subscribe({
      next: () => {
        this.toast.success(`${user.fullName} is now ${newRole}.`);
        this.load();
        this.busyId.set(null);
      },
      error: () => this.busyId.set(null),
    });
  }
}
