import { Component, OnInit } from '@angular/core';
import { HubService } from './_services/hub.service';
import { WaitingRoom } from './_models/waitingRoom';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css']
})
export class AppComponent {

  players: string[];
  waitingRooms: WaitingRoom[];
  activatedRoom: WaitingRoom;

  constructor(private _hubService: HubService) {}
  
  ngOnInit(): void {
    this._hubService.Players.subscribe((players: string[]) => {
      this.players = players;
    });

    this._hubService.WaitingRooms.subscribe((waitingRooms:WaitingRoom[]) => {
      this.waitingRooms = waitingRooms;
    });

    this._hubService.activeWaitingRoomObservable.subscribe((activatedRoom:WaitingRoom) => {
      this.activatedRoom = activatedRoom;
    });
  }
  joinWaitingRoom(id:string){
   this._hubService.JoinWaitingRoom(id);
  }
}
