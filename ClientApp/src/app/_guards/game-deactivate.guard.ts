import { GameComponent } from './../_components/game/game.component';
import { Injectable } from '@angular/core';
import { CanDeactivate, ActivatedRouteSnapshot, RouterStateSnapshot } from '@angular/router';
import { Observable } from 'rxjs';

@Injectable()
export class GameDeactivateGuard implements CanDeactivate<GameComponent> {
  canDeactivate(
    component: GameComponent,
    currentRoute: ActivatedRouteSnapshot,
    currentState: RouterStateSnapshot,
    nextState?: RouterStateSnapshot
  ): boolean | Observable<boolean> | Promise<boolean> {
    return confirm('Are you sure you want to quit this game?');
  }
}
