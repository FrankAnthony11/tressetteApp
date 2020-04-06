import { Injectable } from '@angular/core';
import { ChatMessage } from 'app/_models/chatMessage';
import { BehaviorSubject } from 'rxjs';
import { Game } from 'app/_models/game';
import { User } from 'app/_models/user';
import { Router } from '@angular/router';
import { ToastrService } from 'ngx-toastr';
import { environment } from 'environments/environment';
import { Card } from 'app/_models/card';
import { TypeOfMessage } from 'app/_models/enums';
import { GameMode } from 'app/_models/enums';
import * as signalR from '@aspnet/signalr';

@Injectable()
export class HubService {

  private _hubConnection: signalR.HubConnection;

  private _buzzPlayerDisabled: boolean = false;
  private _knockPlayerDisabled: boolean = false;
  
  private _allChatMessages: ChatMessage[] = [];
  private _gameChatMessages: ChatMessage[] = [];

  allRunningGamesObservable = new BehaviorSubject<Game[]>(new Array<Game>());
  usersObservable = new BehaviorSubject<User[]>(new Array<User>());
  activeGameObservable = new BehaviorSubject<Game>(null);
  allChatMessagesObservable = new BehaviorSubject<ChatMessage[]>(this._allChatMessages);
  gameChatMessagesObservable = new BehaviorSubject<ChatMessage[]>(this._gameChatMessages);
  currentUserObservable = new BehaviorSubject<User>(null);

  constructor(private _router: Router, private _toastrService: ToastrService) {
    this._hubConnection = new signalR.HubConnectionBuilder().withUrl('/gamehub').build();
    this._hubConnection.start().then(() => {
      let name;
      if (environment.production) {
        do {
          name = localStorage.getItem('name') || prompt('Input your name');
        } while (!name);
      } else {
        let myArray = ['Ante', 'Mate'];
        name = myArray[Math.floor(Math.random() * myArray.length)];
      }
      localStorage.setItem('name', name);

      this._hubConnection.invoke('AddUser', name);
      this._hubConnection.invoke('UpdateAllGames');
    });



    this._hubConnection.on('GetCurrentUser', (user: User) => {
      this.currentUserObservable.next(user);
    });

    this._hubConnection.on('GetAllPlayers', (users: User[]) => {
      this.usersObservable.next(users);
    });
    this._hubConnection.on('DisplayToastMessage', (message: string) => {
      this._toastrService.info(message, '', { timeOut: 8000 });
    });

    this._hubConnection.on('BuzzPlayer', () => {
      if (this._buzzPlayerDisabled) return;

      this._buzzPlayerDisabled = true;

      let alert = new Audio('/sounds/alert.mp3');
      alert.load();
      alert.play();

      setTimeout(() => {
        this._buzzPlayerDisabled = false;
      }, 5000);
    });

    this._hubConnection.on('KnockPlayer', () => {
      if (this._knockPlayerDisabled) return;

      this._knockPlayerDisabled = true;

      let knock = new Audio('/sounds/knock.mp3');
      knock.load();
      knock.play();

      setTimeout(() => {
        this._knockPlayerDisabled = false;
      }, 5000);
    });

    this._hubConnection.on('KickUSerFromGame', () => {
      this.activeGameObservable.next(null);
      this._router.navigateByUrl('home');
    });

    this._hubConnection.on('GameUpdate', (game: Game) => {
      this.activeGameObservable.next(game);
      if(game.gameStarted){
        if (this._router.url != '/game') {
          this._router.navigateByUrl('/game');
        }
      }else{
        if (this._router.url != '/waitingRoom') {
          this._router.navigateByUrl('/waitingRoom');
        }
      }
      
    });

    this._hubConnection.on('SendMessageToAllChat', (message: ChatMessage) => {
      this._allChatMessages.unshift(message);
      this.allChatMessagesObservable.next(this._allChatMessages);
    });

    this._hubConnection.on('UpdateAllGames', (games: Game[]) => {
      this.allRunningGamesObservable.next(games);
    });

    this._hubConnection.on('SendMessageToGameChat', (message: ChatMessage) => {
      this._gameChatMessages.unshift(message);
      this.gameChatMessagesObservable.next(this._gameChatMessages);
    });
  }

  StopConnection() {
    this._hubConnection.stop();
  }

  KickUSerFromGame(user: User): any {
    this._hubConnection.invoke('KickUSerFromGame', user.connectionId, this.activeGameObservable.getValue().gameSetup.id);
  }

  JoinGame(id: string, password:string ): any {
    this._hubConnection.invoke('JoinGame', id, password);
  }

  SetGamePassword(id: string, roomPassword: string): any {
    this._hubConnection.invoke('SetGamePassword', id, roomPassword);
  }

  SetGameTypeOfDeck(id: string, typeOfDeck: number): any {
    this._hubConnection.invoke('SetGameTypeOfDeck', id, typeOfDeck);
  }

  ExitGame(): any {
    if (!this.activeGameObservable.getValue()) return;
    this._hubConnection.invoke('ExitGame', this.activeGameObservable.getValue().gameSetup.id);
    this.activeGameObservable.next(null);
  }

  CreateGame(playUntilPoints:number,  expectedNumberOfPlayers:number, gameMode: GameMode) {
    this._hubConnection.invoke('CreateGame', playUntilPoints, expectedNumberOfPlayers, gameMode);
  }

  AddExtraPoints(cards: Card[]) {
    this._hubConnection.invoke('AddExtraPoints', this.activeGameObservable.getValue().gameSetup.id, cards);
  }

  MakeMove(card: Card) {
    this._hubConnection.invoke('MakeMove', this.activeGameObservable.getValue().gameSetup.id, card);
  }

  StartGame(): any {
    this._hubConnection.invoke('StartGame', this.activeGameObservable.getValue().gameSetup.id);

  }

  SendMessageToAllChat(message: string): any {
    this._hubConnection.invoke('SendMessageToAllChat', this.currentUserObservable.getValue().name, message, TypeOfMessage.chat);
  }

  SendMessageToGameChat(message: string): any {
    this._hubConnection.invoke(
      'SendMessageToGameChat',
      this.activeGameObservable.getValue().gameSetup.id,
      this.currentUserObservable.getValue().name,
      message,
      TypeOfMessage.chat
    );
  }

  CallAction(action: string): any {
    this._hubConnection.invoke('CallAction', action, this.activeGameObservable.getValue().gameSetup.id);
  }

  StartNewRound(): any {
    this._hubConnection.invoke('StartNewRound', this.activeGameObservable.getValue().gameSetup.id);
  }

  get Users() {
    return this.usersObservable.asObservable();
  }
  get ActiveGame() {
    return this.activeGameObservable.asObservable();
  }
  get AllChatMessages() {
    return this.allChatMessagesObservable.asObservable();
  }
  get AllRunningGames() {
    return this.allRunningGamesObservable.asObservable();
  }
  get GameChatMessages() {
    return this.gameChatMessagesObservable.asObservable();
  }
  get CurrentUser() {
    return this.currentUserObservable.asObservable();
  }
}
