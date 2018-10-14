import { Game } from './../../../_models/game';
import { Component, OnInit, Input } from '@angular/core';

@Component({
  selector: 'app-game-info-template',
  templateUrl: './game-info-template.component.html',
  styleUrls: ['./game-info-template.component.css']
})
export class GameInfoTemplateComponent implements OnInit {
  @Input('game')  game: Game;
  constructor() {}

  ngOnInit() {}
}
