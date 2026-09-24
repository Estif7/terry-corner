import { Injectable, signal } from '@angular/core';
import { createClient, SupabaseClient, User, Session } from '@supabase/supabase-js';
import { environment } from '../../../environments/environment';

const STORAGE_URL_KEY = 'tc_supabase_url';
const STORAGE_KEY_KEY = 'tc_supabase_anon_key';

@Injectable({ providedIn: 'root' })
export class SupabaseService {
  private _client: SupabaseClient | null = null;
  readonly isConfigured = signal(false);
  readonly currentUser = signal<User | null>(null);
  readonly currentSession = signal<Session | null>(null);

  constructor() {
    this.initClient();
  }

  get client(): SupabaseClient | null {
    return this._client;
  }

  private initClient(): void {
    const storedUrl = localStorage.getItem(STORAGE_URL_KEY);
    const storedKey = localStorage.getItem(STORAGE_KEY_KEY);

    const envUrl = (environment as any).supabase?.url;
    const envKey = (environment as any).supabase?.anonKey;

    const url = storedUrl || (envUrl && !envUrl.includes('placeholder') ? envUrl : '');
    const anonKey = storedKey || (envKey && !envKey.includes('placeholder') ? envKey : '');

    if (url && anonKey) {
      try {
        this._client = createClient(url, anonKey, {
          auth: {
            persistSession: true,
            autoRefreshToken: true,
          },
        });
        this.isConfigured.set(true);

        this._client.auth.getSession().then(({ data }) => {
          this.currentSession.set(data.session);
          this.currentUser.set(data.session?.user ?? null);
        });

        this._client.auth.onAuthStateChange((_event, session) => {
          this.currentSession.set(session);
          this.currentUser.set(session?.user ?? null);
        });
      } catch (err) {
        console.error('Failed to initialize Supabase client:', err);
        this._client = null;
        this.isConfigured.set(false);
      }
    } else {
      this._client = null;
      this.isConfigured.set(false);
    }
  }

  setCredentials(url: string, anonKey: string): boolean {
    try {
      const client = createClient(url.trim(), anonKey.trim(), {
        auth: {
          persistSession: true,
          autoRefreshToken: true,
        },
      });

      localStorage.setItem(STORAGE_URL_KEY, url.trim());
      localStorage.setItem(STORAGE_KEY_KEY, anonKey.trim());
      this._client = client;
      this.isConfigured.set(true);

      client.auth.getSession().then(({ data }) => {
        this.currentSession.set(data.session);
        this.currentUser.set(data.session?.user ?? null);
      });

      client.auth.onAuthStateChange((_event, session) => {
        this.currentSession.set(session);
        this.currentUser.set(session?.user ?? null);
      });

      return true;
    } catch (err) {
      console.error('Invalid Supabase configuration:', err);
      return false;
    }
  }

  clearCredentials(): void {
    localStorage.removeItem(STORAGE_URL_KEY);
    localStorage.removeItem(STORAGE_KEY_KEY);
    this._client = null;
    this.isConfigured.set(false);
    this.currentUser.set(null);
    this.currentSession.set(null);
  }

  getConfig(): { url: string; anonKey: string } {
    const storedUrl = localStorage.getItem(STORAGE_URL_KEY) || (environment as any).supabase?.url || '';
    const storedKey = localStorage.getItem(STORAGE_KEY_KEY) || (environment as any).supabase?.anonKey || '';
    return { url: storedUrl, anonKey: storedKey };
  }
}
