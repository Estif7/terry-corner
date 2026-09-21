import { Component, OnInit, inject, signal } from '@angular/core';
import { DecimalPipe } from '@angular/common';
import { AdminService } from '../../services/admin.service';
import { AdminOverview } from '../../models/admin.models';

@Component({
  selector: 'tc-admin-overview-page',
  standalone: true,
  imports: [DecimalPipe],
  templateUrl: './admin-overview-page.component.html',
  styleUrl: './admin-overview-page.component.scss',
})
export class AdminOverviewPageComponent implements OnInit {
  private readonly adminService = inject(AdminService);

  readonly overview = signal<AdminOverview | null>(null);
  readonly loading = signal(true);

  ngOnInit(): void {
    this.adminService.getOverview().subscribe({
      next: (overview) => {
        this.overview.set(overview);
        this.loading.set(false);
      },
      error: () => this.loading.set(false),
    });
  }
}
