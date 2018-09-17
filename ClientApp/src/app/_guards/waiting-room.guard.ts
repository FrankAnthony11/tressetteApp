import { HubService } from './../_services/hub.service';
import { CanActivate, ActivatedRouteSnapshot, RouterStateSnapshot, Router } from '@angular/router';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
@Injectable()
export class WaitingRoomGuard implements CanActivate {
  constructor(private _hubService: HubService, private _router:Router) {}
  canActivate(route: ActivatedRouteSnapshot, state: RouterStateSnapshot): Observable<boolean> | Promise<boolean> | boolean {
    let waitingRoom = this._hubService.activeWaitingRoomObservable.getValue();
    
    if(!waitingRoom){
      {
        this._router.navigate(['/']);
        return false;
      }
    }
    return true;
  }
}
