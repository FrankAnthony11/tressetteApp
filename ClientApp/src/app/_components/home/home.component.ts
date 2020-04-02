import { Component, OnInit } from '@angular/core';
import { HubService } from 'app/_services/hub.service';
import { GameMode } from 'app/_models/enums';

@Component({
  selector: 'app-home',
  templateUrl: './home.component.html'
})
export class HomeComponent implements OnInit {
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
