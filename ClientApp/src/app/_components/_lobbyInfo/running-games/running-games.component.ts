import { Component, OnInit } from '@angular/core';
import { Game } from 'app/_models/game';
import { HubService } from 'app/_services/hub.service';

@Component({
  selector: 'app-running-games',
  templateUrl: './running-games.component.html',
  styleUrls: ['./running-games.component.css']
})
export class RunningGamesComponent implements OnInit {

  runningGames: Game[] = new Array<Game>();

  constructor(private _hubService: HubService) {}

  ngOnInit(): void {

    this._hubService.AllRunningGames.subscribe((runningGames: Game[]) => {
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

}
