import { Game } from './../_models/game';
import { WaitingRoom } from './../_models/waitingRoom';
import { Injectable } from '@angular/core';
import * as signalR from '@aspnet/signalr';
import { BehaviorSubject } from 'rxjs';
import { Router } from '@angular/router';
import { ChatMessage } from '../_models/chatMessage';
import { User } from '../_models/user';

@Injectable()
export class HubService {
  private _hubConnection: signalR.HubConnection;

  private _messages: ChatMessage[] = [];

  waitingRoomsObservable = new BehaviorSubject<WaitingRoom[]>(null);
  usersObservable = new BehaviorSubject<User[]>(null);
  activeWaitingRoomObservable = new BehaviorSubject<WaitingRoom>(null);
  activeGameObservable = new BehaviorSubject<Game>(null);
  messagesObservable = new BehaviorSubject<ChatMessage[]>(this._messages);
  currentUserObservable = new BehaviorSubject<User>(null);

  constructor(private _router: Router) {
    this._hubConnection = new signalR.HubConnectionBuilder().withUrl('/gamehub').build();
    this._hubConnection.start().then(() => {
      let name;
      // do {
      //   name = prompt('Input your name');
      // } while (!name);
      // name = name+Math.floor((Math.random() * 100) + 1);;
      var myArray = ['Ante', 'Mate', 'Jure', 'Lola', 'Mile'];
      name = myArray[Math.floor(Math.random() * myArray.length)] + Math.floor(Math.random() * 100 + 1);
      this._hubConnection.invoke('AddUser', name);
    });

    this._hubConnection.on('AllWaitingRoomsUpdate', (waitingRooms: WaitingRoom[]) => {
      this.waitingRoomsObservable.next(waitingRooms);
    });

    this._hubConnection.on('GetCurrentUser', (user: User) => {
      this.currentUserObservable.next(user);
    });

    this._hubConnection.on('GetAllPlayers', (users: User[]) => {
      this.usersObservable.next(users);
    });

    this._hubConnection.on('SingleWaitingRoomUpdate', (waitingRoom: WaitingRoom) => {
      this.activeWaitingRoomObservable.next(waitingRoom);
    });

    this._hubConnection.on('GameUpdate', (game: Game) => {
      this.activeGameObservable.next(game);
    });

    this._hubConnection.on('GameStarted', (game: Game) => {
      this.activeGameObservable.next(game);
      this._router.navigateByUrl('game');
    });

    this._hubConnection.on('AddNewMessage', (message: ChatMessage) => {
      this._messages.unshift(message);
      this.messagesObservable.next(this._messages);
    });

    this._hubConnection.on('ExitGame', () => {
      alert('Game exits. One of the players has left the game');
      this._router.navigateByUrl('/');
      //   this.activeGameObservable.next(null);
    });
  }

  StopConnection() {
    this._hubConnection.stop();
  }

  CreateWaitingRoom(playUntilPoints: number, expectedNumberOfPlayers: number) {
    this._hubConnection.invoke('CreateWaitingRoom', playUntilPoints, expectedNumberOfPlayers).then(() => {
      this._router.navigateByUrl('waitingRoom');
    });
  }

  JoinWaitingRoom(id: string) {
    this._hubConnection.invoke('JoinWaitingRoom', id).then(() => {
      this._router.navigateByUrl('waitingRoom');
    });
  }

  ExitGame(): any {
    this._hubConnection.invoke('ExitGame', this.activeGameObservable.getValue().id);
  }

  LeaveWaitingRoom() {
    this._hubConnection.invoke('LeaveWaitingRoom', this.activeWaitingRoomObservable.getValue().id);
    this.activeWaitingRoomObservable.next(null);
    this._router.navigateByUrl('/');
  }

  CreateGame() {
    this._hubConnection.invoke('CreateGame', this.activeWaitingRoomObservable.getValue().id);
  }

  MakeMove(card) {
    this._hubConnection.invoke('MakeMove', this.activeGameObservable.getValue().id, card);
  }

  SendMessage(message: string): any {
    this._hubConnection.invoke('AddNewMessage', message);
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
  get Messages() {
    return this.messagesObservable.asObservable();
  }
  get CurrentUser() {
    return this.currentUserObservable.asObservable();
  }
}
