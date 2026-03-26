import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { AuthService } from '../../../core/services/auth.service';
import { TranslateModule } from '@ngx-translate/core';

@Component({
  selector: 'app-validate-otp',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    RouterModule,
    MatCardModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatProgressSpinnerModule,
    TranslateModule
  ],
  templateUrl: './validate-otp.component.html',
  styleUrls: ['./validate-otp.component.scss']
})
export class ValidateOtpComponent implements OnInit {
  otpForm!: FormGroup;
  loading = false;
  errorMessage = '';
  successMessage = '';
  email = '';
  isPasswordReset = false;

  constructor(
    private fb: FormBuilder,
    private authService: AuthService,
    private router: Router,
    private route: ActivatedRoute
  ) {}

  ngOnInit(): void {
    this.route.queryParams.subscribe(params => {
      this.email = params['email'] || '';
      this.isPasswordReset = params['reset'] === 'true';
    });

    this.otpForm = this.fb.group({
      otpCode: ['', [Validators.required, Validators.pattern(/^[0-9]{6}$/)]]
    });
  }

  onSubmit(): void {
    if (this.otpForm.invalid) {
      return;
    }

    this.loading = true;
    this.errorMessage = '';
    this.successMessage = '';

    const request = {
      email: this.email,
      otpCode: this.otpForm.value.otpCode
    };

    this.authService.validateOtp(request).subscribe({
      next: (response) => {
        this.loading = false;
        this.successMessage = response.message || 'Code validé avec succès !';
        setTimeout(() => {
          if (this.isPasswordReset) {
            // Rediriger vers la page de réinitialisation du mot de passe
            this.router.navigate(['/auth/reset-password'], { 
              queryParams: { email: this.email, code: this.otpForm.value.otpCode } 
            });
          } else {
            // Rediriger vers la page de connexion
            this.router.navigate(['/auth/login']);
          }
        }, 1500);
      },
      error: (error) => {
        this.loading = false;
        this.errorMessage = error.error?.message || 'Code invalide ou expiré.';
      }
    });
  }

  resendOtp(): void {
    if (!this.email) {
      this.errorMessage = 'Email manquant.';
      return;
    }

    this.loading = true;
    this.errorMessage = '';
    this.successMessage = '';

    this.authService.resendOtp(this.email).subscribe({
      next: (response) => {
        this.loading = false;
        this.successMessage = response.message || 'Un nouveau code a été envoyé.';
      },
      error: (error) => {
        this.loading = false;
        this.errorMessage = error.error?.message || 'Impossible de renvoyer le code.';
      }
    });
  }
}
