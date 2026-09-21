import { Component, inject, signal } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { AuthService } from '../../../../core/services/auth.service';
import { AccountService } from '../../services/account.service';

const ALLOWED_TYPES = ['image/jpeg', 'image/png'];
const MAX_SIZE_BYTES = 2 * 1024 * 1024;

@Component({
  selector: 'tc-register-page',
  standalone: true,
  imports: [ReactiveFormsModule, RouterLink],
  templateUrl: './register-page.component.html',
  styleUrl: './register-page.component.scss',
})
export class RegisterPageComponent {
  private readonly fb = inject(FormBuilder);
  private readonly auth = inject(AuthService);
  private readonly accountService = inject(AccountService);
  private readonly router = inject(Router);

  readonly submitting = signal(false);
  readonly selectedPicture = signal<File | null>(null);
  readonly pictureError = signal<string | null>(null);

  readonly form = this.fb.nonNullable.group({
    fullName: ['', [Validators.required, Validators.maxLength(150)]],
    email: ['', [Validators.required, Validators.email]],
    phoneNumber: ['', [Validators.required, Validators.maxLength(30)]],
    password: ['', [Validators.required, Validators.minLength(10)]],
  });

  onPictureSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0] ?? null;

    if (!file) {
      this.selectedPicture.set(null);
      return;
    }

    if (!ALLOWED_TYPES.includes(file.type)) {
      this.pictureError.set('Unsupported file type. Please choose a JPG or PNG.');
      this.selectedPicture.set(null);
      return;
    }

    if (file.size > MAX_SIZE_BYTES) {
      this.pictureError.set('File is too large. Maximum size is 2 MB.');
      this.selectedPicture.set(null);
      return;
    }

    this.pictureError.set(null);
    this.selectedPicture.set(file);
  }

  submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.submitting.set(true);
    this.auth.register(this.form.getRawValue()).subscribe({
      next: () => {
        const picture = this.selectedPicture();
        if (!picture) {
          this.router.navigateByUrl('/');
          return;
        }

        // Optional — if this fails, the account still exists and works fine; the person
        // can just add a picture later from My Account. Don't block navigation on it.
        this.accountService.uploadProfilePicture(picture).subscribe({
          next: () => this.router.navigateByUrl('/'),
          error: () => this.router.navigateByUrl('/'),
        });
      },
      error: () => this.submitting.set(false),
    });
  }
}
