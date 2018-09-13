import { WaitingRoom } from './../_models/waitingRoom';
import { Injectable } from '@angular/core';
import * as signalR from '@aspnet/signalr';
import { BehaviorSubject } from 'rxjs';

@Injectable()
export class HubService {
  private _hubConnection: signalR.HubConnection;

  private _waitingRoomsObservable = new BehaviorSubject<WaitingRoom[]>(null);
  private _playersObservable = new BehaviorSubject<string[]>(null);
  private _activeWaitingRoomObservable = new BehaviorSubject<WaitingRoom>(null);

  constructor() {
    this._hubConnection = new signalR.HubConnectionBuilder().withUrl('/gamehub').build();
    this._hubConnection.start().then(() => {
      this._hubConnection.invoke('AllWaitingRoomsUpdate');
    });

    this._hubConnection.on('AllWaitingRoomsUpdate', (waitingRooms: WaitingRoom[]) => {
      this._waitingRoomsObservable.next(waitingRooms);
    });

    this._hubConnection.on('GetAllPlayers', (players: string[]) => {
      this._playersObservable.next(players);
    });

    this._hubConnection.on('SingleWaitingRoomUpdate', (waitingRoom: WaitingRoom) => {
      this._activeWaitingRoomObservable.next(waitingRoom);
    });
  }

  StopConnection() {
    this._hubConnection.stop();
  }

  CreateWaitingRoom() {
    this._hubConnection.invoke('CreateWaitingRoom');
  }

  JoinWaitingRoom(id: string) {
    this._hubConnection.invoke('JoinWaitingRoom', id);
  }

  LeaveWaitingRoom(id: string) {
    this._activeWaitingRoomObservable.next(null);
    this._hubConnection.invoke('LeaveWaitingRoom', id);
  }

  get Players() {
    return this._playersObservable.asObservable();
  }
  get WaitingRooms() {
    return this._waitingRoomsObservable.asObservable();
  }
  get ActiveWaitingRoom() {
    return this._activeWaitingRoomObservable.asObservable();
  }
}
