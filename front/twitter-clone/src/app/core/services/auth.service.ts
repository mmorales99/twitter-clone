import { Injectable } from '@angular/core';
import { Router } from '@angular/router';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private readonly TOKEN_KEY = 'access_token';
  private readonly REFRESH_TOKEN_KEY = 'refresh_token';
  private readonly USER_KEY = 'user';

  constructor(private router: Router) {}

  // Guarda tokens y usuario tras login
  login(authResponse: { accessToken: string, refreshToken: string, user: any }) {
    localStorage.setItem(this.TOKEN_KEY, authResponse.accessToken);
    localStorage.setItem(this.REFRESH_TOKEN_KEY, authResponse.refreshToken);
    localStorage.setItem(this.USER_KEY, JSON.stringify(authResponse.user));
  }

  // Limpia tokens y usuario (logout)
  logout() {
    localStorage.removeItem(this.TOKEN_KEY);
    localStorage.removeItem(this.REFRESH_TOKEN_KEY);
    localStorage.removeItem(this.USER_KEY);
    this.router.navigate(['/login']);
  }

  // ¿Está autenticado?
  isLoggedIn(): boolean {
    return !!localStorage.getItem(this.TOKEN_KEY);
  }

  // Obtener access token
  getAccessToken(): string | null {
    return localStorage.getItem(this.TOKEN_KEY);
  }

  // Obtener refresh token
  getRefreshToken(): string | null {
    return localStorage.getItem(this.REFRESH_TOKEN_KEY);
  }

  // Obtener usuario actual (decodificado)
  getCurrentUser(): any | null {
    const userJson = localStorage.getItem(this.USER_KEY);
    return userJson ? JSON.parse(userJson) : null;
  }

  // ¿Tiene el usuario un rol concreto?
  hasRole(role: string): boolean {
    const user = this.getCurrentUser();
    return user && user.role && user.role === role;
  }
}
