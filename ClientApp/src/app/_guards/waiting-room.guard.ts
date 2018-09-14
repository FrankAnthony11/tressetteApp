import { HubService } from './../_services/hub.service';
import { CanActivate, ActivatedRouteSnapshot, RouterStateSnapshot } from '@angular/router';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
@Injectable()
export class WaitingRoomGuard implements CanActivate {
  constructor(private _hubService: HubService) {}
  canActivate(route: ActivatedRouteSnapshot, state: RouterStateSnapshot): Observable<boolean> | Promise<boolean> | boolean {
    let waitingRoom = this._hubService.activeWaitingRoomObservable.getValue();
    
    if(!waitingRoom){
        return false;
    }
    return true;
  }
}
