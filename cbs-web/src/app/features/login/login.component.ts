import { CommonModule } from '@angular/common';
import { Component, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../../core/auth/auth.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './login.component.html',
  styleUrl: './login.component.scss'
})
export class LoginComponent {
  username = '';
  password = '';
  readonly submitting = signal(false);
  readonly errorMessage = signal<string | null>(null);

  constructor(private readonly authService: AuthService, private readonly router: Router) {}

  submit(): void {
    if (!this.username || !this.password) {
      this.errorMessage.set('Username and password are required.');
      return;
    }

    this.submitting.set(true);
    this.errorMessage.set(null);

    this.authService.login({ username: this.username, plainTextPassword: this.password }).subscribe({
      next: (result) => {
        this.submitting.set(false);
        if (result.succeeded && result.token) {
          this.authService.setToken(result.token);
          this.router.navigateByUrl('/');
        } else {
          this.errorMessage.set(result.failureReason ?? 'Login failed.');
        }
      },
      error: () => {
        this.submitting.set(false);
        this.errorMessage.set('Unable to reach the IDCOL CBS API. Is the backend running?');
      }
    });
  }
}
