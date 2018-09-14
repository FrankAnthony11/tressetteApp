import { WaitingRoom } from './../../_models/waitingRoom';
import { HubService } from './../../_services/hub.service';
import { Component, OnInit } from '@angular/core';

@Component({
  selector: 'app-waiting-room',
  templateUrl: './waiting-room.component.html',
  styleUrls: ['./waiting-room.component.css']
})
export class WaitingRoomComponent implements OnInit {
  activatedWaitingRoom: WaitingRoom;

  constructor(private _hubService: HubService) {}

  ngOnInit() {
    this._hubService.ActiveWaitingRoom.subscribe(room => {
      this.activatedWaitingRoom = room;
    });
  }

  leaveWaitingRoom() {
    this._hubService.LeaveWaitingRoom();
  }

  CreateGame() {
    this._hubService.CreateGame();
  }
}
