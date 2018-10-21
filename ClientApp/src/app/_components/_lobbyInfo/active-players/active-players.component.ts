import { Component, OnInit } from '@angular/core';
import { User } from 'app/_models/user';
import { HubService } from 'app/_services/hub.service';

@Component({
  selector: 'app-active-players',
  templateUrl: './active-players.component.html',
  styleUrls: ['./active-players.component.css']
})
export class ActivePlayersComponent implements OnInit {
  users: User[] = new Array<User>();

  constructor(private _hubService: HubService) {}

  ngOnInit(): void {
    this._hubService.Users.subscribe((users: User[]) => {
      this.users = users;
    });
  }

  buzzPlayer(user: User) {
    this._hubService.SendMessageToAllChat(`/buzz ${user.name}`);
  }
}
