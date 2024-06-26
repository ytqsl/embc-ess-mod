import { Component, OnDestroy, OnInit } from '@angular/core';
import { CacheService } from 'src/app/core/services/cache.service';
import { RouterOutlet } from '@angular/router';

@Component({
  selector: 'app-assigned-community-management',
  templateUrl: './assigned-community-management.component.html',
  styleUrls: ['./assigned-community-management.component.scss'],
  standalone: true,
  imports: [RouterOutlet]
})
export class AssignedCommunityManagementComponent implements OnDestroy {
  constructor(private cacheService: CacheService) {}

  /**
   * Removes cached items
   */
  ngOnDestroy(): void {
    this.cacheService.remove('allTeamCommunityList');
    this.cacheService.remove('teamCommunityList');
  }
}
