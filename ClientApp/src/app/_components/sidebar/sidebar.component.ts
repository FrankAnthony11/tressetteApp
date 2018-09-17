import { Component, OnInit } from '@angular/core';
import { WaitingRoom } from '../../_models/waitingRoom';
import { HubService } from '../../_services/hub.service';

@Component({
  selector: 'app-sidebar',
  templateUrl: './sidebar.component.html',
  styleUrls: ['./sidebar.component.css']
})
export class SidebarComponent implements OnInit {

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
