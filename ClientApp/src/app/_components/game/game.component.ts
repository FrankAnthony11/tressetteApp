import { Card } from './../../_models/card';
import { Game } from './../../_models/game';
import { HubService } from './../../_services/hub.service';
import { Component, OnInit, ViewChild, ElementRef, HostListener } from '@angular/core';
import { Router } from '@angular/router';
import { User } from '../../_models/user';
import { NgbPopover } from '@ng-bootstrap/ng-bootstrap';

@Component({
  selector: 'app-game',
  templateUrl: './game.component.html',
  styleUrls: ['./game.component.css']
})
export class GameComponent implements OnInit {
  // @HostListener('window:beforeunload', ['$event'])
  // unloadNotification($event: any) {
  //   $event.returnValue = true;
  // }

  @ViewChild('cardsPlayedPopover')
  cardsPlayedPopover: NgbPopover;

  isGameChatSidebarOpen = false;
  gameLocked = false;
  currentUser: User;
  game: Game;
  numberUnreadMessages: number = 0;

  constructor(private _hubService: HubService, private _router: Router) {}

  ngOnInit() {
    this._hubService.ActiveGame.subscribe(game => {
      this.game = game;
      if (game == null) return;
      if (this.game.cardsPlayed.length == game.players.length) {
        this.gameLocked = true;
        setTimeout(() => {
          this.game.cardsDrew = [];
          this.game.cardsPlayed = [];
          this.gameLocked = false;
          if (this.game.gameEnded) {
            let message = 'Game ended! ';
            this.game.teams.forEach(element => {
              message += ` Team ${element.name} points: ${element.calculatedPoints}`;
            });
            alert(message);
          } else if (this.game.roundEnded) {
            this._hubService.StartNewRound();
          }
        }, 3500);
      }
    });

    this._hubService.CurrentUser.subscribe(user => {
      this.currentUser = user;
    });

    this._hubService.GameChatMessages.subscribe(messages => {
      if (messages.length > 0 && messages[0].username != this.currentUser.name && !this.isGameChatSidebarOpen)
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

  toggleGameChatSidebar() {
    this.isGameChatSidebarOpen = !this.isGameChatSidebarOpen;
    this.numberUnreadMessages = 0;
  }

  CallAction(action: string) {
    this._hubService.CallAction(action);
  }

  showCardsPlayedPreviousRound() {
    if (this.game.cardsPlayedPreviousRound.length == this.game.players.length) {
      this.cardsPlayedPopover.toggle();
    }
  }
}
