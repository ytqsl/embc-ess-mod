import { Injectable } from '@angular/core';
import {
    ActivatedRouteSnapshot,
    CanActivate,
    Router,
    RouterStateSnapshot,
    UrlTree
} from '@angular/router';
import { ProfileService } from '../../sharedModules/components/profile/profile.service';
import { ProfileMappingService } from '../../sharedModules/components/profile/profile-mapping.service';

@Injectable({ providedIn: 'root' })
export class AllowNavigationGuard implements CanActivate {

    constructor(private router: Router, private profileService: ProfileService, public mappingService: ProfileMappingService) { }

    public async canActivate(route: ActivatedRouteSnapshot, state: RouterStateSnapshot):
        Promise<boolean | UrlTree> {

        // this.profileService.getExistingProfile().subscribe(profile => {
        //     console.log(profile);
        //     this.mappingService.mapUserProfile(profile);
        //     if (state.url === '/verified-registration') {
        //         if (profile.isNewUser) {
        //             this.router.navigate(['/verified-registration/collection-notice']);
        //         } else {
        //             if (!profile.conflicts) {
        //                 this.router.navigate(['/verified-registration/dashboard']);
        //             } else {
        //                 this.router.navigate(['/verified-registration/conflicts']);
        //             }
        //         }
        //     }
        // });
        console.log(state.url)
        this.profileService.profileExists().subscribe((exists: boolean) => {
            this.profileService.getLoginProfile();
            this.profileService.getProfile();
            if (!exists && state.url === '/verified-registration') {
                this.router.navigate(['/verified-registration/collection-notice']);
            } else {
                this.profileService.getConflicts().subscribe(conflicts => {
                    this.mappingService.mapConflicts(conflicts);
                    if (state.url === '/verified-registration') {
                        if (conflicts.length == 0) {
                            this.router.navigate(['/verified-registration/dashboard']);
                        } else {
                            this.router.navigate(['/verified-registration/conflicts']);
                        }
                    }

                })
            }
        });
        return true;
    }
}
