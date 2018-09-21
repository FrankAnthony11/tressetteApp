import { Card } from './../../_models/card';
import { Game } from './../../_models/game';
import { HubService } from './../../_services/hub.service';
import { Component, OnInit, ViewChild, ElementRef } from '@angular/core';
import { Router } from '@angular/router';
import { User } from '../../_models/user';
import { NgbPopover } from '@ng-bootstrap/ng-bootstrap';

@Component({
  selector: 'app-game',
  templateUrl: './game.component.html',
  styleUrls: ['./game.component.css']
})
export class GameComponent implements OnInit {
  @ViewChild('gameChatPopover')
  gameChatPopover: NgbPopover;

  gameLocked = false;

  currentUser: User;
  game: Game;
  numberUnreadMessages: number = 0;

  constructor(private _hubService: HubService, private _router: Router) {}

  ngOnInit() {
    this._hubService.ActiveGame.subscribe(game => {
      this.game = game;
      if (this.game.cardsPlayed.length == game.players.length) {
        this.gameLocked = true;
        setTimeout(() => {
          this.game.cardsDrew = [];
          this.game.cardsPlayed = [];
          this.gameLocked = false;
        }, 3500);
      }
      if (this.game.gameEnded) {
        alert(`Game ended!`);
      }
    });

    this._hubService.CurrentUser.subscribe(user => {
      this.currentUser = user;
    });

    this._hubService.GameChatMessages.subscribe(messages => {
      if (messages.length > 0 && messages[0].user.connectionId != this.currentUser.connectionId && !this.gameChatPopover.isOpen())
        this.numberUnreadMessages++;
    });
  }

  makeMove(card) {
    if (this.gameLocked) return;
    this._hubService.MakeMove(card);
  }

  exitGame() {
    var cfrm = confirm('Really exit game?');
    if (cfrm) {
      this._hubService.ExitGame();
    }
  }

  markMessagesAsRead() {
    this.numberUnreadMessages = 0;
  }
}
