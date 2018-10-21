import { Component, OnInit } from '@angular/core';
import { Game } from 'app/_models/game';
import { HubService } from 'app/_services/hub.service';

@Component({
  selector: 'app-game-spectators',
  templateUrl: './game-spectators.component.html',
  styleUrls: ['./game-spectators.component.css']
})
export class GameSpectatorsComponent implements OnInit {
  game: Game;

  constructor(private _hubService: HubService) {}

  ngOnInit() {
    this._hubService.ActiveGame.subscribe(game => {
      this.game = game;
    });
  }
}
