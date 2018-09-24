import { User } from './../../_models/user';
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
  currentUser:User;

  constructor(private _hubService: HubService) {}

  ngOnInit() {
    this._hubService.ActiveWaitingRoom.subscribe(room => {
      this.activatedWaitingRoom = room;
    });
    
    this._hubService.CurrentUser.subscribe(user => {
      this.currentUser = user;
    });


  }

  leaveWaitingRoom() {
    this._hubService.LeaveWaitingRoom();
  }

  createGame() {
    this._hubService.CreateGame();
  }

  setRoomPassword() {
    if (!this.activatedWaitingRoom.password) return;
    this._hubService.SetRoomPassword(this.activatedWaitingRoom.id, this.activatedWaitingRoom.password);
  }
}
