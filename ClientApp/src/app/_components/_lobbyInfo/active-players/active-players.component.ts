import { Component, OnInit, OnDestroy } from '@angular/core';
import { User } from 'app/_models/user';
import { HubService } from 'app/_services/hub.service';
import { takeWhile } from 'rxjs/operators';

@Component({
  selector: 'app-active-players',
  templateUrl: './active-players.component.html',
  styleUrls: ['./active-players.component.css']
})
export class ActivePlayersComponent implements OnInit, OnDestroy {
  private _isAlive = true;

  users: User[] = new Array<User>();

  constructor(private _hubService: HubService) { }
  
  ngOnDestroy(): void {
    this._isAlive = false;
  }

  ngOnInit(): void {
    this._hubService.Users.pipe(takeWhile(() => this._isAlive)).subscribe((users: User[]) => {
      this.users = users;
    });
  }

  buzzPlayer(user: User) {
    this._hubService.SendMessageToAllChat(`/buzz ${user.name}`);
  }
}
