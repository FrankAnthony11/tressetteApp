import { HubService } from './../_services/hub.service';
import { GameComponent } from './../_components/game/game.component';
import { Injectable } from '@angular/core';
import { CanDeactivate, ActivatedRouteSnapshot, RouterStateSnapshot } from '@angular/router';
import { Observable } from 'rxjs';

@Injectable()
export class GameDeactivateGuard implements CanDeactivate<GameComponent> {
  constructor(private _hubService: HubService) {}
  canDeactivate(
    component: GameComponent,
    currentRoute: ActivatedRouteSnapshot,
    currentState: RouterStateSnapshot,
    nextState?: RouterStateSnapshot
  ): boolean | Observable<boolean> | Promise<boolean> {
    var cfrm = confirm('Really exit game?');
    if (!cfrm) return false;
    this._hubService.ExitGame();
    return true;
  }
}
