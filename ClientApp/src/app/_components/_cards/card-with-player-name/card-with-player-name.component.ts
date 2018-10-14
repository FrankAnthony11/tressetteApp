import { Component, OnInit, Input } from '@angular/core';
import { CardAndUser } from 'app/_models/cardAndUser';

@Component({
  selector: 'app-card-with-player-name',
  templateUrl: './card-with-player-name.component.html',
  styleUrls: ['./card-with-player-name.component.css']
})
export class CardWithPlayerNameComponent implements OnInit {
  @Input('cardAndUser')
  cardAndUser: CardAndUser;
  @Input('isPlayedCard')
  isPlayedCard: boolean = false;

  constructor() {}

  ngOnInit() {}

  getCardClass() {
    if (this.isPlayedCard) return ['played-card'];
    return ['drew-card'];
  }
}
