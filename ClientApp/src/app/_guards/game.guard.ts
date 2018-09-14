import { HubService } from './../_services/hub.service';
import { CanActivate, ActivatedRouteSnapshot, RouterStateSnapshot } from '@angular/router';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
@Injectable()
export class GameGuard implements CanActivate {
  constructor(private _hubService: HubService) {}
  canActivate(route: ActivatedRouteSnapshot, state: RouterStateSnapshot): Observable<boolean> | Promise<boolean> | boolean {
    let game = this._hubService.activeGameObservable.getValue();
    
    if(!game){
        return false;
    }
    return true;
  }
}
