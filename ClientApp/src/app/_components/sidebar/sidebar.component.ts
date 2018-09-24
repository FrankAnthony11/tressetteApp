import { WaitingRoom } from './../../_models/waitingRoom';
import { User } from './../../_models/user';
import { Component, OnInit } from '@angular/core';
import { HubService } from '../../_services/hub.service';

@Component({
  selector: 'app-sidebar',
  templateUrl: './sidebar.component.html',
  styleUrls: ['./sidebar.component.css']
})
export class SidebarComponent implements OnInit {
  users: User[];
  waitingRooms: WaitingRoom[];

  constructor(private _hubService: HubService) {}

  ngOnInit(): void {
    this._hubService.Users.subscribe((users: User[]) => {
      this.users = users;
    });

    this._hubService.WaitingRooms.subscribe((waitingRooms: WaitingRoom[]) => {
      this.waitingRooms = waitingRooms;
    });
  }
  joinWaitingRoom(waitingRoom: WaitingRoom) {
    let password = '';
    if (waitingRoom.password) {
      password = prompt('Input password for this game');
      if (password == null) return;
    }
    this._hubService.JoinWaitingRoom(waitingRoom.id, password);
  }
}
