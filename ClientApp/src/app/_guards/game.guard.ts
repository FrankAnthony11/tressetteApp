import { HubService } from './../_services/hub.service';
import { CanActivate, ActivatedRouteSnapshot, RouterStateSnapshot, Router } from '@angular/router';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
@Injectable()
export class GameGuard implements CanActivate {
  constructor(private _hubService: HubService, private _router: Router) {}
  canActivate(route: ActivatedRouteSnapshot, state: RouterStateSnapshot): Observable<boolean> | Promise<boolean> | boolean {
    let game = this._hubService.activeGameObservable.getValue();

    if (!game) {
      {
        this._router.navigate(['/']);
        return false;
      }
    }
    return true;
  }
}
