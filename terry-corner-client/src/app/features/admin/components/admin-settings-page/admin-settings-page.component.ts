import { Component, OnInit, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { SupabaseService } from '../../../../core/services/supabase.service';
import { ToastService } from '../../../../core/services/toast.service';

@Component({
  selector: 'tc-admin-settings-page',
  standalone: true,
  imports: [FormsModule],
  templateUrl: './admin-settings-page.component.html',
  styleUrl: './admin-settings-page.component.scss',
})
export class AdminSettingsPageComponent implements OnInit {
  private readonly supabase = inject(SupabaseService);
  private readonly toast = inject(ToastService);

  readonly url = signal('');
  readonly anonKey = signal('');
  readonly isConnected = signal(false);
  readonly testing = signal(false);
  readonly statusMessage = signal('');

  ngOnInit(): void {
    const config = this.supabase.getConfig();
    this.url.set(config.url);
    this.anonKey.set(config.anonKey);
    this.checkStatus();
  }

  checkStatus(): void {
    this.isConnected.set(this.supabase.isConfigured());
  }

  save(): void {
    const u = this.url().trim();
    const k = this.anonKey().trim();

    if (!u || !k) {
      this.toast.error('Please enter both Supabase Project URL and Anon Public Key.');
      return;
    }

    const success = this.supabase.setCredentials(u, k);
    if (success) {
      this.isConnected.set(true);
      this.toast.success('Supabase configuration saved! Testing connection...');
      this.testConnection();
    } else {
      this.toast.error('Invalid Supabase configuration.');
    }
  }

  async testConnection(): Promise<void> {
    this.testing.set(true);
    this.statusMessage.set('Checking database connection...');

    const client = this.supabase.client;
    if (!client) {
      this.testing.set(false);
      this.statusMessage.set('Supabase client is not initialized.');
      this.toast.error('Supabase client not initialized.');
      return;
    }

    try {
      const { data, error } = await client.from('categories').select('count', { count: 'exact', head: true });
      if (error) {
        this.statusMessage.set(`Connection failed: ${error.message}. (Did you run the supabase-schema.sql script in Supabase SQL editor?)`);
        this.toast.error(error.message);
      } else {
        this.statusMessage.set('Connection successful! Database tables are ready.');
        this.toast.success('Connected to Supabase successfully!');
      }
    } catch (err: any) {
      this.statusMessage.set(`Error: ${err.message || err}`);
      this.toast.error('Failed to connect to Supabase.');
    } finally {
      this.testing.set(false);
    }
  }

  disconnect(): void {
    this.supabase.clearCredentials();
    this.url.set('');
    this.anonKey.set('');
    this.isConnected.set(false);
    this.statusMessage.set('Disconnected. Reverting to local static menu fallback.');
    this.toast.info('Disconnected from Supabase.');
  }
}
