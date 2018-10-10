import { TypeOfMessage } from './../_models/enums';
import { environment } from './../../environments/environment';
import { Game } from './../_models/game';
import { WaitingRoom } from './../_models/waitingRoom';
import { Injectable } from '@angular/core';
import * as signalR from '@aspnet/signalr';
import { BehaviorSubject } from 'rxjs';
import { Router } from '@angular/router';
import { ChatMessage } from '../_models/chatMessage';
import { User } from '../_models/user';
import { ToastrService } from 'ngx-toastr';
import { Card } from '../_models/card';

@Injectable()
export class HubService {
  private _hubConnection: signalR.HubConnection;

  private _gameOrWaitingRoomId: string;
  private _allChatMessages: ChatMessage[] = [];
  private _gameChatMessages: ChatMessage[] = [];

  waitingRoomsObservable = new BehaviorSubject<WaitingRoom[]>(null);
  allRunningGamesObservable = new BehaviorSubject<Game[]>(null);
  usersObservable = new BehaviorSubject<User[]>(null);
  activeWaitingRoomObservable = new BehaviorSubject<WaitingRoom>(null);
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
          name = prompt('Input your name');
        } while (!name);
      } else {
        var myArray = ['Ante', 'Mate'];
        name = myArray[Math.floor(Math.random() * myArray.length)];
      }
      this._hubConnection.invoke('AddUser', name);
      this._hubConnection.invoke('UpdateAllWaitingRooms');
      this._hubConnection.invoke('UpdateAllRunningGames');
    });

    this._hubConnection.on('UpdateAllWaitingRooms', (waitingRooms: WaitingRoom[]) => {
      this.waitingRoomsObservable.next(waitingRooms);
    });

    this._hubConnection.on('GetCurrentUser', (user: User) => {
      this.currentUserObservable.next(user);
    });

    this._hubConnection.on('GetAllPlayers', (users: User[]) => {
      this.usersObservable.next(users);
    });
    this._hubConnection.on('DisplayToastMessage', (message: string) => {
      this._toastrService.info(message);
    });

    this._hubConnection.on('BuzzPlayer', () => {
      var alert=new Audio("/sounds/alert.mp3");
      alert.load();
      alert.play();
    });

    this._hubConnection.on('UpdateSingleWaitingRoom', (waitingRoom: WaitingRoom) => {
      this._gameOrWaitingRoomId = waitingRoom.id;
      this.activeWaitingRoomObservable.next(waitingRoom);
    });

    this._hubConnection.on('KickUserFromWaitingRoom', () => {
      this.activeWaitingRoomObservable.next(null);
      this._router.navigateByUrl('home');
    });

    this._hubConnection.on('GameUpdate', (game: Game) => {
      this.activeGameObservable.next(game);
      if (this._router.url != '/game') {
        this.activeWaitingRoomObservable.next(null);
        this._router.navigateByUrl('/game');
      }
    });

    this._hubConnection.on('AddNewMessageToAllChat', (message: ChatMessage) => {
      this._allChatMessages.unshift(message);
      this.allChatMessagesObservable.next(this._allChatMessages);
    });

    this._hubConnection.on('UpdateAllRunningGames', (games: Game[]) => {
      this.allRunningGamesObservable.next(games);
    });

    this._hubConnection.on('AddNewMessageToGameChat', (message: ChatMessage) => {
      this._gameChatMessages.unshift(message);
      this.gameChatMessagesObservable.next(this._gameChatMessages);
    });
  }

  StopConnection() {
    this._hubConnection.stop();
  }

  KickUserFromWaitingRoom(user: User): any {
    this._hubConnection.invoke('KickUserFromWaitingRoom', user.connectionId, this.activeWaitingRoomObservable.getValue().id);
  }

  CreateWaitingRoom(playUntilPoints: number, expectedNumberOfPlayers: number) {
    this._gameChatMessages = [];
    this.gameChatMessagesObservable.next(this._gameChatMessages);

    this._hubConnection.invoke('CreateWaitingRoom', playUntilPoints, expectedNumberOfPlayers).then(() => {
      this._router.navigateByUrl('waitingRoom');
    });
  }

  JoinWaitingRoom(id: string, password: string) {
    this._gameOrWaitingRoomId = id;
    this._gameChatMessages = [];
    this.gameChatMessagesObservable.next(this._gameChatMessages);

    this._hubConnection.invoke('JoinWaitingRoom', id, password).then(() => {
      this._router.navigateByUrl('waitingRoom');
    });
  }

  JoinGameAsPlayerOrSpectator(id: string): any {
    this._gameOrWaitingRoomId = id;
    this._hubConnection.invoke('JoinGameAsPlayerOrSpectator', id);
  }

  SetRoomPassword(id: string, roomPassword: string): any {
    this._hubConnection.invoke('SetRoomPassword', id, roomPassword);
  }

  ExitGame(): any {
    if (!this.activeGameObservable.getValue()) return;
    this._hubConnection.invoke('ExitGame', this.activeGameObservable.getValue().id);
    this._router.navigateByUrl("/");
    this.activeGameObservable.next(null);
  }

  LeaveWaitingRoom() {
    if (!this.activeWaitingRoomObservable.getValue()) return;
    this._hubConnection.invoke('LeaveWaitingRoom', this.activeWaitingRoomObservable.getValue().id);
    this.activeWaitingRoomObservable.next(null);
    this._router.navigateByUrl('/');
  }

  CreateGame() {
    this._hubConnection.invoke('CreateGame', this.activeWaitingRoomObservable.getValue().id);
  }

  AddExtraPoints(cards:Card[]){
    this._hubConnection.invoke('AddExtraPoints', this.activeWaitingRoomObservable.getValue().id, cards);
  }

  MakeMove(card:Card) {
    this._hubConnection.invoke('MakeMove', this.activeGameObservable.getValue().id, card);
  }

  AddNewMessageToAllChat(message: string): any {
    this._hubConnection.invoke('AddNewMessageToAllChat', message, TypeOfMessage.chat);
  }

  AddNewMessageToGameChat(message: string): any {
    this._hubConnection.invoke('AddNewMessageToGameChat', this._gameOrWaitingRoomId, message, TypeOfMessage.chat);
  }

  CallAction(action: string): any {
    this._hubConnection.invoke('CallAction', action, this.activeGameObservable.getValue().id);
  }

  StartNewRound(): any {
    this._hubConnection.invoke('StartNewRound', this.activeGameObservable.getValue().id);
  }

  get Users() {
    return this.usersObservable.asObservable();
  }
  get WaitingRooms() {
    return this.waitingRoomsObservable.asObservable();
  }
  get ActiveWaitingRoom() {
    return this.activeWaitingRoomObservable.asObservable();
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
