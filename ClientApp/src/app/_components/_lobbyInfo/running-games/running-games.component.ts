import { Component, OnInit, OnDestroy } from '@angular/core';
import { Game } from 'app/_models/game';
import { HubService } from 'app/_services/hub.service';
import { takeWhile } from 'rxjs/operators';

@Component({
  selector: 'app-running-games',
  templateUrl: './running-games.component.html',
  styleUrls: ['./running-games.component.css']
})
export class RunningGamesComponent implements OnInit, OnDestroy {

  private _isAlive = true;

  runningGames: Game[] = new Array<Game>();

  constructor(private _hubService: HubService) { }

  ngOnDestroy(): void {
    this._isAlive = false;
  }

  ngOnInit(): void {

    this._hubService.AllRunningGames.pipe(takeWhile(() => this._isAlive)).subscribe((runningGames: Game[]) => {
      this.runningGames = runningGames;
    });
  }

  joinGame(game: Game) {
    let password = '';

    if (game.gameSetup.isPasswordProtected) {
      password = prompt('Input password for this game');
      if (password == null) return;
    }
    this._hubService.JoinGame(game.gameSetup.id, password);
  }

  getBadgeClass(game: Game) {
    if (game.gameStarted) return 'badge-danger';
    return 'badge-primary';
  }

}
