import { Component, OnInit } from '@angular/core';
import { HubService } from 'app/_services/hub.service';
import { GameMode } from 'app/_models/enums';

@Component({
  selector: 'app-home',
  templateUrl: './home.component.html'
})
export class HomeComponent implements OnInit {
  standardDescription = `This is plain Tressette where the goal is to maximize the score.
  Aces account for 1 point, and all cards not in range [4-6] account for 1/3 points.`;

  evasionDescription = `In this variation, each player plays alone, and the goal is to minimize the score.
  The first player reaching the given upper score loses, the game ends, and a new game without that player starts.
  Card points are like in standard mode. Depending on the region, the Ace of Clubs might account
  for more points than the other Aces. In the variation implemented here, it accounts for 4 points rather than 1.
  At the end of each round, the player getting the last point will also get all spurious cards
  from the other players. However, if a player scores all points at the end of a round,
  these points will go to the other players instead.`;
  
  ngOnInit(): void {}
  constructor(private _hubService: HubService) {}

  createGame(playUntilPoints: number, expectedNumberOfPlayers: number) {
    this._hubService.CreateGame(playUntilPoints, expectedNumberOfPlayers, GameMode.plain);
  }

  createGameEvasion(playUntilPoints: number, expectedNumberOfPlayers: number) {
    this._hubService.CreateGame(playUntilPoints, expectedNumberOfPlayers,GameMode.evasion);
  }

  rename() {
    let name = prompt('Input your name');
    if (!name) return;
    localStorage.setItem('name', name);
    window.location.reload();
  }
}
